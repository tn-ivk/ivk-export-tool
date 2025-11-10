# Оптимизация экспорта больших таблиц (> 1 GB)

## Проблема

Текущая реализация `SqlExportService` имеет серьёзные проблемы с потреблением памяти и производительностью при экспорте больших таблиц (> 1 GB).

## Текущие проблемы

### 1. Высокое потребление памяти (КРИТИЧНО)

**Файл**: `src/IvkExportTool.Infrastructure/Services/SqlExportService.cs:141-177`

**Проблема**:
- `List<string> batchRows` накапливает до 1000 строк в памяти перед записью
- Для каждой строки создаётся `List<string> values` со всеми значениями колонок
- При больших строках (BLOB/TEXT поля) батч может занять сотни MB памяти

**Пример**:
```csharp
var batchRows = new List<string>();  // накопление в памяти
while (await reader.ReadAsync(cancellationToken))
{
    var values = new List<string>();  // аллокация для каждой строки
    // ...
    batchRows.Add($"({string.Join(", ", values)})");  // ещё одна аллокация
}
```

### 2. String concatenation (КРИТИЧНО)

**Файл**: `src/IvkExportTool.Infrastructure/Services/SqlExportService.cs:167`

**Проблема**:
- `string.Join(",\n", batchRows)` создаёт огромную строку в памяти перед записью
- Для батча в 1000 строк по 10 KB каждая = ~10 MB на один `string.Join`
- При 100+ таблицах это приводит к постоянным аллокациям и GC паузам

**Код**:
```csharp
await writer.WriteLineAsync(string.Join(",\n", batchRows) + ";");  // огромная аллокация
```

### 3. Отсутствие потоковой записи (КРИТИЧНО)

**Проблема**:
- Данные аккумулируются в памяти (`batchRows`) перед записью на диск
- Нет немедленной записи строк через `StreamWriter`
- Неэффективное использование I/O буферов

### 4. Прогресс экспорта только на уровне таблиц (ВАЖНО)

**Файл**: `src/IvkExportTool.Infrastructure/Services/SqlExportService.cs:53`

**Проблема**:
- Progress показывается только между таблицами
- При экспорте одной гигабайтной таблицы пользователь не видит прогресса минутами
- Невозможно оценить оставшееся время для больших таблиц

**Код**:
```csharp
progress?.Report((int)((double)currentTable / totalTables * 100));  // только между таблицами
```

### 5. Фиксированный BatchSize (СРЕДНЕ)

**Файл**: `src/IvkExportTool.Core/Models/ExportOptions.cs:43`

**Проблема**:
- `BatchSize = 1000` не учитывает размер строк
- Для таблиц с узкими строками (4-5 колонок) можно использовать батчи по 5000-10000
- Для таблиц с широкими строками (50+ колонок, BLOB) нужно 100-500
- Отсутствие адаптации приводит к неоптимальному использованию памяти

### 6. MySqlDataReader buffer (НИЗКО)

**Проблема**:
- `SELECT * FROM table` загружает все данные через reader
- Внутренние буферы MySqlConnector могут расти
- Для очень больших таблиц (> 10M строк) лучше использовать chunked reading

## Предлагаемые решения

### Приоритет 1: КРИТИЧНО (необходимо для работы с таблицами > 1 GB)

#### 1.1. Потоковая запись без накопления в памяти

**Что делать**:
Переписать `ExportTableDataAsync` с немедленной записью строк без промежуточного `List<string>`.

**Псевдокод**:
```csharp
private async Task ExportTableDataAsync(...)
{
    // ...
    var rowCount = 0;
    var currentBatchRow = 0;

    while (await reader.ReadAsync(cancellationToken))
    {
        // Начало нового INSERT батча
        if (currentBatchRow == 0)
        {
            if (rowCount > 0)
                await writer.WriteLineAsync(";");  // завершаем предыдущий INSERT

            await writer.WriteLineAsync($"{insertHeader}");
        }

        // Запись строки напрямую в writer
        if (currentBatchRow > 0)
            await writer.WriteLineAsync(",");

        await writer.WriteAsync("(");

        for (int i = 0; i < columnCount; i++)
        {
            if (i > 0)
                await writer.WriteAsync(", ");

            var sqlValue = reader.IsDBNull(i)
                ? "NULL"
                : ConvertToSqlValue(reader.GetValue(i));

            await writer.WriteAsync(sqlValue);
        }

        await writer.WriteAsync(")");

        rowCount++;
        currentBatchRow++;

        if (currentBatchRow >= batchSize)
            currentBatchRow = 0;
    }

    if (rowCount > 0)
        await writer.WriteLineAsync(";");
}
```

**Преимущества**:
- Устраняет аллокацию `List<string> batchRows`
- Устраняет `string.Join` на больших массивах
- Память растёт только на размер одной строки
- Немедленная запись на диск через `StreamWriter` буфер

#### 1.2. Использование StringBuilder вместо string concatenation

**Что делать**:
Использовать `StringBuilder` для построения SQL значений и экранирования строк.

**Где применить**:
- В `ConvertToSqlValue` для построения строковых литералов
- В `EscapeSqlString` для экранирования

**Пример**:
```csharp
private string ConvertToSqlValue(object value)
{
    return value switch
    {
        string str => BuildEscapedString(str),  // используем StringBuilder внутри
        DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'",
        bool b => b ? "1" : "0",
        byte[] bytes => BuildHexString(bytes),  // используем StringBuilder
        null => "NULL",
        _ => value.ToString() ?? "NULL"
    };
}

private string BuildEscapedString(string value)
{
    var sb = new StringBuilder(value.Length + 20);
    sb.Append('\'');

    foreach (var ch in value)
    {
        switch (ch)
        {
            case '\\': sb.Append("\\\\"); break;
            case '\'': sb.Append("\\'"); break;
            case '"': sb.Append("\\\""); break;
            case '\n': sb.Append("\\n"); break;
            case '\r': sb.Append("\\r"); break;
            case '\t': sb.Append("\\t"); break;
            case '\0': sb.Append("\\0"); break;
            default: sb.Append(ch); break;
        }
    }

    sb.Append('\'');
    return sb.ToString();
}
```

### Приоритет 2: ВАЖНО (улучшает UX для больших таблиц)

#### 2.1. Детальный прогресс внутри таблиц

**Что делать**:
- Добавить новый класс `ExportProgress` с детальной информацией
- Обновлять прогресс каждые N строк (например, каждые 10000 строк)
- Передавать информацию о текущей таблице и количестве обработанных строк

**Новые модели**:
```csharp
// src/IvkExportTool.Core/Models/ExportProgress.cs
public class ExportProgress
{
    public string CurrentTable { get; set; } = string.Empty;
    public long RowsProcessed { get; set; }
    public long TotalRows { get; set; }  // если известно
    public int CurrentTableIndex { get; set; }
    public int TotalTables { get; set; }
    public int PercentComplete { get; set; }
    public TimeSpan Elapsed { get; set; }
}
```

**Изменения в ExportAsync**:
```csharp
public async Task<ExportResult> ExportAsync(
    ConnectionConfig config,
    ExportOptions options,
    IProgress<ExportProgress>? detailedProgress = null,  // новый параметр
    CancellationToken cancellationToken = default)
{
    var stopwatch = Stopwatch.StartNew();

    foreach (var (tableName, index) in options.Tables.Select((t, i) => (t, i)))
    {
        var tableProgress = new ExportProgress
        {
            CurrentTable = tableName,
            CurrentTableIndex = index,
            TotalTables = totalTables,
            Elapsed = stopwatch.Elapsed
        };

        await ExportTableAsync(
            connection,
            writer,
            tableName,
            options,
            tableProgress,
            detailedProgress,
            cancellationToken);
    }
}
```

**В ExportTableDataAsync**:
```csharp
const int progressReportInterval = 10000;

while (await reader.ReadAsync(cancellationToken))
{
    // ... экспорт строки ...

    rowCount++;

    if (rowCount % progressReportInterval == 0)
    {
        progress.RowsProcessed = rowCount;
        progress.PercentComplete = CalculatePercent(progress);
        detailedProgress?.Report(progress);
    }
}
```

**Обновления UI**:
- В `MainWindowViewModel` отобразить детальный прогресс:
  - "Экспорт таблицы users: 150000/500000 строк (30%)"
  - Progress bar с процентом выполнения
  - Прошедшее время

### Приоритет 3: ПОЛЕЗНО (оптимизация производительности)

#### 3.1. Адаптивный BatchSize

**Что делать**:
Динамически вычислять оптимальный размер батча на основе:
- Количества колонок в таблице
- Типов данных (наличие BLOB/TEXT)
- Целевого размера батча в памяти (например, 5-10 MB)

**Реализация**:
```csharp
private int CalculateAdaptiveBatchSize(MySqlDataReader reader, int defaultBatchSize)
{
    const int targetBatchMemoryMB = 10;
    const int minBatchSize = 100;
    const int maxBatchSize = 10000;

    var estimatedRowSize = 0;

    for (int i = 0; i < reader.FieldCount; i++)
    {
        var fieldType = reader.GetFieldType(i);
        estimatedRowSize += fieldType.Name switch
        {
            "String" => 200,      // средний размер строки
            "Byte[]" => 1000,     // BLOB
            "DateTime" => 20,
            "Int32" => 10,
            "Int64" => 20,
            "Decimal" => 30,
            _ => 50
        };
    }

    var adaptiveBatchSize = (targetBatchMemoryMB * 1024 * 1024) / estimatedRowSize;
    return Math.Clamp(adaptiveBatchSize, minBatchSize, maxBatchSize);
}
```

**Использование**:
```csharp
var adaptiveBatchSize = CalculateAdaptiveBatchSize(reader, options.BatchSize);
// использовать adaptiveBatchSize вместо options.BatchSize
```

#### 3.2. Настройка MySqlConnector для больших выборок

**Что делать**:
```csharp
selectCommand.CommandTimeout = 3600;  // 1 час для очень больших таблиц

// Опционально: использовать server-side cursor для больших результатов
// (требует MySqlConnector >= 2.0)
```

### Приоритет 4: ОПЦИОНАЛЬНО (для экстремальных случаев)

#### 4.1. Chunked reading для сверхбольших таблиц (> 10M строк)

**Когда применять**:
- Таблицы с десятками миллионов строк
- Ограниченная память на клиенте
- Нестабильное сетевое соединение (можно возобновить с chunk'а)

**Реализация**:
```csharp
private async Task ExportTableDataChunked(
    MySqlConnection connection,
    StreamWriter writer,
    string tableName,
    int chunkSize,
    CancellationToken cancellationToken)
{
    const int chunkSize = 100000;

    for (long offset = 0; ; offset += chunkSize)
    {
        var query = $"SELECT * FROM `{tableName}` LIMIT {chunkSize} OFFSET {offset}";
        await using var command = new MySqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!reader.HasRows)
            break;  // достигли конца таблицы

        // экспорт chunk'а
        var rowsInChunk = await ExportChunk(reader, writer, ...);

        if (rowsInChunk < chunkSize)
            break;  // последний chunk
    }
}
```

**Недостатки**:
- OFFSET медленный для больших смещений
- Лучше использовать WHERE id > lastId AND id <= lastId + chunkSize

**Альтернатива (быстрее)**:
```csharp
// Получить первичный ключ таблицы
var pkColumn = await GetPrimaryKeyColumn(tableName);

long? lastId = null;

while (true)
{
    var query = lastId == null
        ? $"SELECT * FROM `{tableName}` ORDER BY `{pkColumn}` LIMIT {chunkSize}"
        : $"SELECT * FROM `{tableName}` WHERE `{pkColumn}` > {lastId} ORDER BY `{pkColumn}` LIMIT {chunkSize}";

    // ... обработка chunk'а ...

    if (rowsInChunk < chunkSize)
        break;
}
```

## План реализации

### Этап 1: Критичные исправления (MUST HAVE) ✅ ВЫПОЛНЕНО
- [x] Переписать `ExportTableDataAsync` с потоковой записью (п. 1.1)
- [x] Внедрить `StringBuilder` для экранирования строк (п. 1.2)
- [x] Добавить unit-тесты для проверки памяти (мокировать большие таблицы)

**Метрика успеха**: Экспорт таблицы 1 GB занимает < 500 MB RAM

**Дата выполнения**: 2025-11-10

**Реализованные изменения**:
1. Переписан метод `ExportTableDataAsync` с потоковой записью без накопления `List<string>` в памяти
2. Добавлен метод `BuildEscapedString` с использованием `StringBuilder` для эффективного экранирования строк
3. Добавлен метод `BuildHexString` с использованием `StringBuilder` для эффективной конвертации byte[] в hex
4. Увеличен размер буфера `StreamWriter` до 64KB для оптимизации I/O
5. Добавлен `CommandTimeout = 3600` секунд для поддержки больших таблиц
6. Созданы unit-тесты для проверки всех методов конвертации и экранирования

### Этап 2: Улучшение UX (SHOULD HAVE)
- [ ] Добавить модель `ExportProgress` (п. 2.1)
- [ ] Обновить интерфейс `IExportService` для поддержки детального прогресса
- [ ] Обновить `MainWindowViewModel` для отображения детального прогресса
- [ ] Добавить отображение количества строк в UI

**Метрика успеха**: Пользователь видит прогресс каждые 5 секунд при экспорте большой таблицы

### Этап 3: Оптимизация производительности (NICE TO HAVE)
- [ ] Реализовать адаптивный `BatchSize` (п. 3.1)
- [ ] Добавить настройки timeout для MySqlCommand (п. 3.2)
- [ ] Провести бенчмарки на таблицах разного размера

**Метрика успеха**: Экспорт на 20-30% быстрее при автоматическом BatchSize

### Этап 4: Экстремальные случаи (FUTURE)
- [ ] Реализовать chunked reading (п. 4.1)
- [ ] Добавить опцию в UI для включения chunked mode
- [ ] Добавить возможность возобновления экспорта после сбоя

**Метрика успеха**: Возможность экспортировать таблицы > 100M строк

## Риски и ограничения

### Риски
1. **Производительность I/O**: Частая запись в `StreamWriter` может быть медленнее
   - Митигация: `StreamWriter` имеет внутренний буфер (обычно 1024 байта)
   - Можно увеличить буфер: `new StreamWriter(fileStream, Encoding.UTF8, bufferSize: 64 * 1024)`

2. **Сетевые таймауты**: Большие таблицы могут экспортироваться часами
   - Митигация: Увеличить `CommandTimeout` или использовать chunked reading

3. **Chunked reading**: OFFSET медленный на больших смещениях
   - Митигация: Использовать WHERE с первичным ключом вместо OFFSET

### Ограничения
1. **Размер SQL файла**: Файлы > 10 GB могут быть проблемой для импорта
   - Рекомендация: Экспортировать таблицы в отдельные файлы

2. **Типы данных**: Некоторые типы (например, JSON, геометрия) могут требовать специальной обработки
   - Текущая реализация: базовая поддержка через `ToString()`

3. **Кодировка**: Проблемы с экзотическими символами
   - Текущая реализация: UTF-8 с экранированием спецсимволов

## Тестирование

### Unit-тесты
```csharp
[Test]
public async Task ExportAsync_LargeTable_ShouldNotExceedMemoryLimit()
{
    // Arrange: мокировать таблицу с 1M строк
    var mockReader = CreateMockReaderWithLargeDataset(rowCount: 1_000_000);

    // Act
    var memoryBefore = GC.GetTotalMemory(true);
    await service.ExportAsync(...);
    var memoryAfter = GC.GetTotalMemory(true);

    // Assert: рост памяти должен быть < 100 MB
    var memoryGrowth = memoryAfter - memoryBefore;
    memoryGrowth.Should().BeLessThan(100 * 1024 * 1024);
}
```

### Интеграционные тесты
- Создать тестовую БД с таблицей 1 GB
- Экспортировать и проверить:
  - Размер файла корректный
  - Все строки экспортированы
  - SQL синтаксически корректен (можно импортировать обратно)

### Performance тесты
- Бенчмарки для таблиц разного размера (100 MB, 500 MB, 1 GB, 5 GB)
- Сравнение потребления памяти до/после оптимизации
- Сравнение времени экспорта

## Дополнительные улучшения (вне скоупа)

1. **Параллельный экспорт таблиц**: Экспортировать несколько таблиц одновременно
2. **Компрессия на лету**: Писать в .sql.gz вместо .sql
3. **Инкрементальный экспорт**: Экспортировать только новые/изменённые строки
4. **Resume capability**: Возобновление прерванного экспорта
5. **Экспорт в другие форматы**: CSV, JSON, Parquet для больших таблиц

## Ссылки

- [MySqlConnector Performance Tips](https://mysqlconnector.net/performance/)
- [StreamWriter Buffering](https://learn.microsoft.com/en-us/dotnet/api/system.io.streamwriter)
- [Memory Management in .NET](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/)
