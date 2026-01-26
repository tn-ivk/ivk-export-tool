# Code Review: IvkExportTool

**Дата анализа:** 2026-01-14
**Версия приложения:** v1.0.x
**Анализируемые слои:** Core, Infrastructure, Desktop, Tests

---

## Содержание

- [Критические проблемы (P0)](#-критические-проблемы-p0)
- [Высокий приоритет (P1)](#-высокий-приоритет-p1)
- [Средний приоритет (P2)](#-средний-приоритет-p2)
- [Низкий приоритет (P3)](#-низкий-приоритет-p3)
- [Покрытие тестами](#-покрытие-тестами)
- [План действий](#-план-действий)
- [Итоговая оценка](#-итоговая-оценка)

---

## 🔴 Критические проблемы (P0)

### 1. Безопасность: Hardcoded ключ шифрования

**Файл:** `src/IvkExportTool.Core/Security/CredentialProtector.cs`

**Описание:**
Ключ шифрования AES-256 хранится в исходном коде, разбитый на 8 частей (`KeyPart1`...`KeyPart8`). При декомпиляции приложения (ILSpy, dnSpy) или анализе hex-дампа ключ легко восстанавливается, что делает шифрование credentials бесполезным.

**Код:**
```csharp
private static readonly byte[] KeyPart1 = { 0x49, 0x76, 0x6B, 0x45 }; // "IvkE"
private static readonly byte[] KeyPart2 = { 0x78, 0x70, 0x6F, 0x72 }; // "xpor"
// ... всего 8 частей, складывающихся в "IvkExportTool_2025_Key!@#$%^&*()"
```

**Риск:** Любой пользователь может расшифровать все сохранённые credentials.

**Рекомендация:**
- Windows: использовать DPAPI (`ProtectedData` с `DataProtectionScope.CurrentUser`)
- macOS: использовать KeyChain через `Security.framework`
- Linux: ключ на основе хардварных идентификаторов + user-specific salt

---

### 2. SQL-инъекция в именах таблиц

**Файл:** `src/IvkExportTool.Infrastructure/Services/SqlExportService.cs`
**Строки:** 111, 149, 165, 179

**Описание:**
Имена таблиц подставляются в SQL-запросы через string interpolation. Backticks (`` ` ``) не защищают полностью от инъекций.

**Код:**
```csharp
var selectCommand = new MySqlCommand($"SELECT * FROM `{tableName}`", connection);
var countCommand = new MySqlCommand($"SELECT COUNT(*) FROM `{tableName}`", connection);
```

**Пример атаки:** `tableName = "users`; DROP TABLE users; --"`

**Рекомендация:**
Добавить строгую валидацию имён таблиц:
```csharp
private static void ValidateTableName(string tableName)
{
    if (string.IsNullOrWhiteSpace(tableName))
        throw new ArgumentException("Table name cannot be empty");

    if (!Regex.IsMatch(tableName, @"^[a-zA-Z0-9_]+$"))
        throw new ArgumentException($"Invalid table name: {tableName}");
}
```

---

### 3. Утечка памяти: подписки на события

**Файлы:**
- `src/IvkExportTool.Desktop/ViewModels/MainWindowViewModel.cs:188-193`
- `src/IvkExportTool.Desktop/App.axaml.cs:88-89`
- `src/IvkExportTool.Desktop/Views/StartWindow.axaml.cs`
- `src/IvkExportTool.Desktop/Views/AutoConnectionWindow.axaml.cs`

**Описание:**
При фильтрации таблиц в `MainWindowViewModel` создаётся новая коллекция `FilteredTables`, но элементы, исчезнувшие из фильтра, остаются подписаны на `PropertyChanged`. При частой смене фильтра накапливаются "мёртвые" подписки.

В `App.axaml.cs` при переключении окон подписки на события ViewModel (`ManualConnectionRequested`, `ConnectionSucceeded` и др.) не отписываются.

**Код проблемы:**
```csharp
// MainWindowViewModel.cs
private void ApplyFilters()
{
    FilteredTables = new ObservableCollection<TableItemViewModel>(filtered);
    foreach (var table in FilteredTables)
    {
        table.PropertyChanged -= Table_PropertyChanged;  // Только для НОВЫХ элементов!
        table.PropertyChanged += Table_PropertyChanged;
    }
    // Элементы, исчезнувшие из фильтра, НЕ отписываются!
}
```

**Рекомендация:**
1. Реализовать `IDisposable` во всех ViewModel
2. Отписываться от старой коллекции перед созданием новой
3. В `App.axaml.cs` явно отписываться от событий в `CloseAllWindows()`

```csharp
// Пример исправления в MainWindowViewModel
private void ApplyFilters()
{
    // Отписка от старых элементов
    if (FilteredTables != null)
    {
        foreach (var table in FilteredTables)
            table.PropertyChanged -= Table_PropertyChanged;
    }

    FilteredTables = new ObservableCollection<TableItemViewModel>(filtered);

    foreach (var table in FilteredTables)
        table.PropertyChanged += Table_PropertyChanged;
}
```

---

### 4. Race condition в настройках

**Файлы:**
- `src/IvkExportTool.Infrastructure/Configuration/SettingsStore.cs:31-50`
- `src/IvkExportTool.Infrastructure/Services/AppSettingsService.cs:19-51`

**Описание:**
Метод `Load()` в `SettingsStore` не использует `lock`, в то время как `Save()` использует. При одновременном чтении и записи возможно получение повреждённого JSON.

В `AppSettingsService` поле `_settings` модифицируется без синхронизации.

**Код проблемы:**
```csharp
// SettingsStore.cs
public static AppSettings Load()  // ⚠️ Без lock!
{
    if (!File.Exists(settingsPath))
        return new AppSettings();

    var json = File.ReadAllText(settingsPath);  // Может читать во время записи
    // ...
}

public static void Save(AppSettings settings)  // ✓ С lock
{
    lock (_lock)
    {
        File.WriteAllText(settingsPath, json);
    }
}
```

**Рекомендация:**
1. Использовать `ReaderWriterLockSlim` для разделения чтения/записи
2. Или переделать на immutable объекты с атомарной заменой

```csharp
private static readonly ReaderWriterLockSlim _rwLock = new();

public static AppSettings Load()
{
    _rwLock.EnterReadLock();
    try
    {
        // ... чтение
    }
    finally
    {
        _rwLock.ExitReadLock();
    }
}

public static void Save(AppSettings settings)
{
    _rwLock.EnterWriteLock();
    try
    {
        // ... запись
    }
    finally
    {
        _rwLock.ExitWriteLock();
    }
}
```

---

### 5. Неопределённые ресурсы в AXAML

**Файл:** `src/IvkExportTool.Desktop/Views/MainWindow.axaml`
**Строки:** 214, 222, 251, 259

**Описание:**
Используются ресурсы, которые не определены в `App.axaml`:
- `SurfaceContainerHighBrush`
- `OnSurfaceVariantBrush`

При отсутствии ресурса используется fallback (прозрачный или чёрный цвет), что нарушает дизайн.

**Рекомендация:**
Добавить в `App.axaml`:
```xml
<!-- Material Design 3 tonal palette -->
<SolidColorBrush x:Key="OnSurfaceVariantBrush" Color="#546e7a"/>
<SolidColorBrush x:Key="SurfaceContainerHighBrush" Color="#e8eef3"/>
```

---

## 🟡 Высокий приоритет (P1)

### 6. Отсутствие валидации в моделях

**Файлы:**
- `src/IvkExportTool.Core/Models/ConnectionConfig.cs`
- `src/IvkExportTool.Core/Models/ExportOptions.cs`

**Описание:**
- `Port` может быть отрицательным или > 65535
- `Host` может быть пустым при создании строки подключения
- `OutputPath` в `ExportOptions` не проверяется на валидность

**Код проблемы:**
```csharp
public class ConnectionConfig
{
    public string Host { get; set; } = "192.168.233.101";  // Может быть null/empty
    public int Port { get; set; } = 3306;  // Может быть -1, 99999
    // ...
}
```

**Рекомендация:**
```csharp
public class ConnectionConfig
{
    private string _host = string.Empty;
    public required string Host
    {
        get => _host;
        set => _host = !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException("Host cannot be empty");
    }

    private int _port = 3306;
    public int Port
    {
        get => _port;
        set => _port = (value >= 1 && value <= 65535)
            ? value
            : throw new ArgumentOutOfRangeException(nameof(Port), "Port must be 1-65535");
    }
}
```

---

### 7. Fire-and-forget в конструкторах

**Файлы:**
- `src/IvkExportTool.Desktop/ViewModels/ConnectionWindowViewModel.cs:71`
- `src/IvkExportTool.Desktop/ViewModels/AutoConnectionWindowViewModel.cs:72`

**Описание:**
Асинхронные методы вызываются в конструкторе без ожидания результата. Исключения теряются, возможны race conditions.

**Код проблемы:**
```csharp
public ConnectionWindowViewModel(IDatabaseService databaseService, IAppSettingsService appSettingsService)
{
    _databaseService = databaseService;
    _appSettingsService = appSettingsService;

    _ = LoadSettingsAsync();  // ⚠️ Исключение потеряется!
}
```

**Рекомендация:**
1. Отложить инициализацию до явного метода `InitializeAsync()`
2. Или обработать исключения:
```csharp
_ = LoadSettingsAsync().ContinueWith(t =>
{
    if (t.IsFaulted)
        Debug.WriteLine($"Failed to load settings: {t.Exception}");
}, TaskScheduler.Default);
```

---

### 8. Неправильный batching при экспорте

**Файл:** `src/IvkExportTool.Infrastructure/Services/SqlExportService.cs:232-235`

**Описание:**
Переменная `currentBatchRow` инкрементируется, но не используется для разделения INSERT-операторов. Для таблицы с 1M строк генерируется один INSERT с 1M VALUES, что может привести к OutOfMemoryException.

**Код проблемы:**
```csharp
if (currentBatchRow >= batchSize)
{
    currentBatchRow = 0;  // Сбрасывается, но INSERT не разделяется!
}
```

**Рекомендация:**
```csharp
if (currentBatchRow >= batchSize)
{
    await writer.WriteLineAsync(";");  // Завершить текущий INSERT
    await writer.WriteAsync(insertHeader);  // Начать новый INSERT
    currentBatchRow = 0;
    isFirstRow = true;
}
```

---

### 9. Пустые catch-блоки

**Файлы:** Множество мест во всех слоях

**Примеры:**
- `MySqlDatabaseService.cs:20-23, 38-40`
- `AppSettingsService.cs:41-44, 60-63, 78-81`
- `ConnectionWindowViewModel.cs` (LoadSettingsAsync)

**Описание:**
Исключения молча игнорируются, что затрудняет диагностику проблем.

**Код проблемы:**
```csharp
catch
{
    // Игнорируем ошибки загрузки
}
```

**Рекомендация:**
Добавить интерфейс `ILogger` и логировать ошибки:
```csharp
catch (Exception ex)
{
    _logger.LogWarning(ex, "Failed to load settings, using defaults");
}
```

---

### 10. Hardcoded IP-адрес

**Файлы:**
- `src/IvkExportTool.Desktop/ViewModels/ConnectionWindowViewModel.cs:19`
- `src/IvkExportTool.Desktop/ViewModels/AutoConnectionWindowViewModel.cs:23`
- `src/IvkExportTool.Core/Models/ConnectionConfig.cs`

**Описание:**
IP-адрес конкретной машины (`192.168.233.101`) захардкожен как значение по умолчанию.

**Код проблемы:**
```csharp
private string _host = "192.168.233.101";  // IP внутренней сети
```

**Рекомендация:**
Использовать пустую строку или `localhost`:
```csharp
private string _host = string.Empty;  // Или "localhost"
```

---

## 🟢 Средний приоритет (P2)

### 11. Нарушение Interface Segregation Principle

**Файл:** `src/IvkExportTool.Core/Interfaces/IDatabaseService.cs`

**Описание:**
Интерфейс содержит 5 методов разной направленности. Можно разделить на более узкие интерфейсы.

**Рекомендация:**
```csharp
public interface IConnectionTester
{
    Task<bool> TestConnectionAsync(ConnectionConfig config);
    Task<bool> TestConnectionAsync(ConnectionConfig config, TimeSpan timeout, CancellationToken ct);
}

public interface IDatabaseProvider
{
    Task<List<string>> GetDatabasesAsync(ConnectionConfig config);
    Task<DatabaseInfo> GetDatabaseInfoAsync(ConnectionConfig config);
    Task<TableInfo> GetTableInfoAsync(ConnectionConfig config, string tableName);
}
```

---

### 12. Дублирование кода

| Код | Файлы | Строки |
|-----|-------|--------|
| `FormatBytes()` | `TableItemViewModel.cs`, `MainWindowViewModel.cs` | 48-63, 234-249 |
| Валидация Port | `ConnectionWindowViewModel.cs`, `AutoConnectionWindowViewModel.cs` | 38, 32 |
| `ShowNotification()` | `ConnectionWindowViewModel.cs`, `AutoConnectionWindowViewModel.cs` | 126-133, 224-231 |
| Try-finally для IsLoading | Все ViewModels | Многократно |

**Рекомендация:**
1. Создать `src/IvkExportTool.Desktop/Helpers/FormatHelper.cs`:
```csharp
public static class FormatHelper
{
    public static string FormatBytes(long bytes) { ... }
    public static string FormatElapsedTime(TimeSpan elapsed) { ... }
}
```

2. Создать `src/IvkExportTool.Desktop/Helpers/ValidationHelper.cs`:
```csharp
public static class ValidationHelper
{
    public static bool IsValidPort(string portString, out int port) { ... }
    public static bool IsValidHost(string host) { ... }
}
```

3. Вынести `ShowNotification()` в базовый класс `ViewModelBase` или создать `INotificationService`.

---

### 13. Отсутствие CancellationToken

**Файл:** `src/IvkExportTool.Core/Interfaces/IDatabaseService.cs`

**Описание:**
Методы `GetDatabasesAsync()` и `GetDatabaseInfoAsync()` не поддерживают `CancellationToken`, что затрудняет отмену долгих операций.

**Рекомендация:**
```csharp
Task<List<string>> GetDatabasesAsync(ConnectionConfig config, CancellationToken ct = default);
Task<DatabaseInfo> GetDatabaseInfoAsync(ConnectionConfig config, CancellationToken ct = default);
```

---

### 14. Отсутствие timeout в ручном подключении

**Файл:** `src/IvkExportTool.Desktop/ViewModels/ConnectionWindowViewModel.cs`

**Описание:**
`AutoConnectionWindowViewModel` имеет timeout 5 секунд на каждую попытку, но `ConnectionWindowViewModel` не имеет timeout вообще. Пользователь может ждать вечно при недоступном хосте.

**Рекомендация:**
Добавить timeout аналогично `AutoConnectionWindowViewModel`:
```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
var success = await _databaseService.TestConnectionAsync(config, TimeSpan.FromSeconds(10), cts.Token);
```

---

### 15. DATETIME без микросекунд

**Файл:** `src/IvkExportTool.Infrastructure/Services/SqlExportService.cs:292`

**Описание:**
При экспорте `DateTime` теряются микросекунды, что критично для столбцов `DATETIME(6)`.

**Код проблемы:**
```csharp
DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'"  // Теряются микросекунды
```

**Рекомендация:**
```csharp
DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss.ffffff}'"
```

---

### 16. Нарушение MVVM: прямая ссылка на Window

**Файл:** `src/IvkExportTool.Desktop/ViewModels/MainWindowViewModel.cs`

**Описание:**
ViewModel хранит прямую ссылку на `Window` для доступа к `StorageProvider`.

**Код проблемы:**
```csharp
private Avalonia.Controls.Window? _window;

public void SetWindow(Avalonia.Controls.Window window)
{
    _window = window;
}
```

**Рекомендация:**
Создать `IFileDialogService`:
```csharp
public interface IFileDialogService
{
    Task<string?> ShowSaveFileDialogAsync(string suggestedFileName, string defaultDirectory);
}
```

---

### 17. Inconsistent window closing behavior

**Файлы:**
- `src/IvkExportTool.Desktop/Views/StartWindow.axaml.cs`
- `src/IvkExportTool.Desktop/Views/ConnectionWindow.axaml.cs`
- `src/IvkExportTool.Desktop/Views/AutoConnectionWindow.axaml.cs`

**Описание:**
- `StartWindow`: кнопка закрытия вызывает `Close()` (закрытие окна)
- `ConnectionWindow`: кнопка закрытия вызывает `desktop.Shutdown()` (закрытие приложения)
- `AutoConnectionWindow`: кнопка закрытия вызывает `Close()` (закрытие окна)

Непоследовательное поведение может запутать пользователя.

**Рекомендация:**
Унифицировать: все окна должны закрываться через `Close()`, а `App.axaml.cs` должен решать — закрыть приложение или показать другое окно.

---

## 🔵 Низкий приоритет (P3)

### 18. Отсутствие обработки Escape

**Описание:**
Ни одно окно не реагирует на нажатие Escape для закрытия. Это стандартное ожидание на Windows/Linux.

**Рекомендация:**
```csharp
protected override void OnKeyDown(KeyEventArgs e)
{
    if (e.Key == Key.Escape)
    {
        Close();
        e.Handled = true;
    }
    base.OnKeyDown(e);
}
```

---

### 19. Использование emoji в watermark

**Файл:** `src/IvkExportTool.Desktop/Views/MainWindow.axaml:120`

**Описание:**
```xml
<TextBox Watermark="🔍 Поиск таблиц..."/>
```
Emoji не гарантирует корректное отображение на всех платформах (особенно Linux).

**Рекомендация:**
Использовать иконку через FontAwesome или PathIcon.

---

### 20. SELECT COUNT(*) перед экспортом

**Файл:** `src/IvkExportTool.Infrastructure/Services/SqlExportService.cs:149`

**Описание:**
Перед экспортом данных выполняется `SELECT COUNT(*)`, что для больших таблиц может быть медленно.

**Рекомендация:**
Использовать информацию из `information_schema.TABLES`, которая уже получена в `MainWindow` при загрузке списка таблиц.

---

### 21. Кэширование ключа шифрования

**Файл:** `src/IvkExportTool.Infrastructure/Services/AppSettingsService.cs:187-191`

**Описание:**
Метод `GetFallbackKey()` вызывается каждый раз при шифровании/дешифровании.

**Код проблемы:**
```csharp
private static byte[] GetFallbackKey()
{
    var source = Encoding.UTF8.GetBytes($"{Environment.UserName}|{Environment.MachineName}|IvkExportTool");
    return SHA256.HashData(source);  // Вычисляется каждый раз!
}
```

**Рекомендация:**
```csharp
private static readonly Lazy<byte[]> _fallbackKey = new(() =>
{
    var source = Encoding.UTF8.GetBytes($"{Environment.UserName}|{Environment.MachineName}|IvkExportTool");
    return SHA256.HashData(source);
});

private static byte[] GetFallbackKey() => _fallbackKey.Value;
```

---

## 📊 Покрытие тестами

### Статистика

| Слой | Тестов | Покрытие | Статус |
|------|--------|----------|--------|
| Core (Models, Security) | 40 | ~90% | ✅ Отлично |
| Infrastructure (Services) | 66 | ~70% | ⚠️ Хорошо |
| Desktop (ViewModels) | 94 | ~60% | ⚠️ Нормально |
| Desktop (App.axaml.cs) | 0 | ~0% | ❌ Критично |
| Integration | 33 | — | ⚠️ Требует Docker |
| E2E | 0 | 0% | ❌ Отсутствуют |

**Всего тестов:** 247

### Хорошо покрыто

- ✅ Валидация полей подключения
- ✅ Криптография (CredentialProtector)
- ✅ Модели данных Core
- ✅ Экспорт SQL (экранирование, конвертация типов)
- ✅ Управление настройками

### Не покрыто

- ❌ Навигация между окнами (`App.axaml.cs`)
- ❌ E2E сценарии (полный цикл работы)
- ❌ Обработка ошибок сети
- ❌ Граничные случаи экспорта (большие таблицы)
- ❌ Тесты производительности

### Рекомендации по тестам

1. **Критично:** Добавить тесты навигации между окнами
2. **Важно:** Добавить E2E тесты основных сценариев
3. **Желательно:** Тесты обработки ошибок сети
4. **Желательно:** Тесты граничных случаев экспорта

---

## 📋 План действий

### Sprint 1 (Немедленно)

- [ ] Исправить SQL-инъекцию в `SqlExportService` — добавить валидацию имён таблиц
- [ ] Добавить пропущенные AXAML ресурсы (`OnSurfaceVariantBrush`, `SurfaceContainerHighBrush`)
- [ ] Реализовать `IDisposable` в ViewModels и исправить утечки памяти
- [ ] Убрать hardcoded IP-адрес из всех файлов
- [ ] Добавить валидацию Port (1-65535) в моделях

### Sprint 2 (Ближайшее время)

- [ ] Исправить batching при экспорте больших таблиц
- [ ] Добавить логирование ошибок (вместо пустых catch-блоков)
- [ ] Исправить race condition в `SettingsStore` — добавить `ReaderWriterLockSlim`
- [ ] Добавить timeout в `ConnectionWindowViewModel`
- [ ] Исправить fire-and-forget в конструкторах ViewModels

### Sprint 3 (Планово)

- [ ] Рефакторинг: создать `INavigationService` для управления окнами
- [ ] Рефакторинг: вынести дублирующийся код в утилиты (`FormatHelper`, `ValidationHelper`)
- [ ] Добавить E2E тесты для основных сценариев
- [ ] Добавить тесты навигации между окнами
- [ ] Пересмотреть систему шифрования credentials

### Backlog (Когда будет время)

- [ ] Разделить `IDatabaseService` на узкие интерфейсы
- [ ] Добавить `CancellationToken` во все асинхронные методы
- [ ] Создать `IFileDialogService` для соблюдения MVVM
- [ ] Унифицировать поведение кнопки закрытия во всех окнах
- [ ] Добавить обработку Escape для закрытия диалогов
- [ ] Заменить emoji на FontAwesome иконку в watermark

---

## 🏆 Итоговая оценка

| Критерий | Оценка | Комментарий |
|----------|--------|-------------|
| Архитектура (Clean Architecture) | ⭐⭐⭐⭐ 4/5 | Хорошее разделение слоёв |
| Качество кода | ⭐⭐⭐ 3.5/5 | Дублирование, отсутствие валидации |
| Безопасность | ⭐⭐ 2/5 | Hardcoded ключ, SQL-инъекция |
| MVVM паттерн | ⭐⭐⭐⭐ 4/5 | Хорошо, но есть нарушения |
| Обработка ошибок | ⭐⭐⭐ 3/5 | Много пустых catch-блоков |
| Thread-safety | ⭐⭐ 2.5/5 | Race conditions в настройках |
| Тестируемость | ⭐⭐⭐⭐ 4/5 | Интерфейсы, DI |
| Покрытие тестами | ⭐⭐⭐ 3.5/5 | Нет E2E и тестов навигации |

### Общая оценка: 3.4/5

**Вывод:** Приложение имеет хорошую архитектурную основу с правильным использованием Clean Architecture и MVVM. Однако есть критические проблемы с безопасностью (hardcoded ключ, SQL-инъекция) и управлением памятью (утечки подписок), которые необходимо исправить перед production-использованием.

---

## Приложение: Список файлов для изменения

### Критические изменения

| Файл | Проблема | Строки |
|------|----------|--------|
| `SqlExportService.cs` | SQL-инъекция | 111, 149, 165, 179 |
| `App.axaml` | Недостающие ресурсы | Добавить |
| `MainWindowViewModel.cs` | Утечка памяти | 188-193 |
| `App.axaml.cs` | Утечка событий | 88-89 |
| `SettingsStore.cs` | Race condition | 31-50 |
| `CredentialProtector.cs` | Hardcoded ключ | Весь файл |

### Высокоприоритетные изменения

| Файл | Проблема | Строки |
|------|----------|--------|
| `ConnectionConfig.cs` | Отсутствие валидации | Весь файл |
| `ExportOptions.cs` | Отсутствие валидации | Весь файл |
| `ConnectionWindowViewModel.cs` | Fire-and-forget, hardcoded IP | 19, 71 |
| `AutoConnectionWindowViewModel.cs` | Fire-and-forget, hardcoded IP | 23, 72 |
| `SqlExportService.cs` | Неправильный batching | 232-235 |
