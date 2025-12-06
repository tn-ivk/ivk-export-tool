# API Reference

Справочник по всем публичным интерфейсам, классам и методам **IvkExportTool**.

## Содержание

- [Core Layer](#core-layer)
  - [Interfaces](#interfaces)
  - [Models](#models)
  - [Enums](#enums)
  - [Constants](#constants)
  - [Security](#security)
- [Infrastructure Layer](#infrastructure-layer)
  - [Services](#services)
- [Desktop Layer](#desktop-layer)
  - [ViewModels](#viewmodels)

---

## Core Layer

### Interfaces

#### IDatabaseService

**Namespace**: `IvkExportTool.Core.Interfaces`

Интерфейс для работы с базами данных MySQL.

```csharp
public interface IDatabaseService
{
    Task<bool> TestConnectionAsync(ConnectionConfig config);
    Task<List<string>> GetDatabasesAsync(ConnectionConfig config);
    Task<List<TableInfo>> GetTablesAsync(string databaseName);
}
```

##### Methods

**TestConnectionAsync**

Проверяет подключение к MySQL серверу.

```csharp
Task<bool> TestConnectionAsync(ConnectionConfig config)
```

| Параметр | Тип | Описание |
|----------|-----|----------|
| `config` | `ConnectionConfig` | Конфигурация подключения |

**Returns**: `Task<bool>` — `true` если подключение успешно, иначе `false`

**Exceptions**:
- `ArgumentNullException` — если `config` равен `null`
- `MySqlException` — ошибка при подключении к MySQL

**Example**:

```csharp
var service = new MySqlDatabaseService();
var config = new ConnectionConfig
{
    Host = "localhost",
    Port = 3306,
    Username = "root",
    Password = "password"
};

bool isConnected = await service.TestConnectionAsync(config);
```

---

**GetDatabasesAsync**

Получает список всех баз данных на сервере.

```csharp
Task<List<string>> GetDatabasesAsync(ConnectionConfig config)
```

| Параметр | Тип | Описание |
|----------|-----|----------|
| `config` | `ConnectionConfig` | Конфигурация подключения |

**Returns**: `Task<List<string>>` — список имён баз данных

**Exceptions**:
- `ArgumentNullException` — если `config` равен `null`
- `MySqlException` — ошибка при выполнении запроса

**Example**:

```csharp
var databases = await service.GetDatabasesAsync(config);
foreach (var db in databases)
{
    Console.WriteLine(db);
}
```

---

**GetTablesAsync**

Получает список таблиц из указанной базы данных.

```csharp
Task<List<TableInfo>> GetTablesAsync(string databaseName)
```

| Параметр | Тип | Описание |
|----------|-----|----------|
| `databaseName` | `string` | Имя базы данных |

**Returns**: `Task<List<TableInfo>>` — список объектов `TableInfo`

**Exceptions**:
- `ArgumentException` — если `databaseName` пустой или `null`
- `MySqlException` — ошибка при выполнении запроса

**Example**:

```csharp
var tables = await service.GetTablesAsync("mydb");
foreach (var table in tables)
{
    Console.WriteLine($"{table.Name}: {table.RowCount} rows");
}
```

---

#### IExportService

**Namespace**: `IvkExportTool.Core.Interfaces`

Интерфейс для экспорта таблиц в различные форматы.

```csharp
public interface IExportService
{
    Task<ExportResult> ExportAsync(ExportOptions options);
}
```

##### Methods

**ExportAsync**

Экспортирует таблицы согласно указанным опциям.

```csharp
Task<ExportResult> ExportAsync(ExportOptions options)
```

| Параметр | Тип | Описание |
|----------|-----|----------|
| `options` | `ExportOptions` | Параметры экспорта |

**Returns**: `Task<ExportResult>` — результат экспорта

**Exceptions**:
- `ArgumentNullException` — если `options` равен `null`
- `IOException` — ошибка при записи файла
- `MySqlException` — ошибка при чтении данных из БД

**Example**:

```csharp
var exportService = new SqlExportService();
var options = new ExportOptions
{
    Tables = tables,
    FilePath = "C:\\exports\\mydb.sql",
    Format = ExportFormat.SQL
};

var result = await exportService.ExportAsync(options);
if (result.Success)
{
    Console.WriteLine($"Export saved to: {result.FilePath}");
}
```

---

#### IAppSettingsService

**Namespace**: `IvkExportTool.Core.Interfaces`

Интерфейс для сохранения и загрузки настроек приложения.

```csharp
public interface IAppSettingsService
{
    void SaveSettings(AppSettings settings);
    AppSettings LoadSettings();
}
```

##### Methods

**SaveSettings**

Сохраняет настройки приложения в файл.

```csharp
void SaveSettings(AppSettings settings)
```

| Параметр | Тип | Описание |
|----------|-----|----------|
| `settings` | `AppSettings` | Объект с настройками |

**Exceptions**:
- `ArgumentNullException` — если `settings` равен `null`
- `IOException` — ошибка при записи файла

---

**LoadSettings**

Загружает настройки приложения из файла.

```csharp
AppSettings LoadSettings()
```

**Returns**: `AppSettings` — объект с настройками (или настройки по умолчанию)

**Exceptions**:
- `IOException` — ошибка при чтении файла

---

### Models

#### ConnectionConfig

**Namespace**: `IvkExportTool.Core.Models`

Конфигурация подключения к MySQL серверу.

```csharp
public class ConnectionConfig
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 3306;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
}
```

##### Properties

| Свойство | Тип | Описание | По умолчанию |
|----------|-----|----------|--------------|
| `Host` | `string` | IP-адрес или имя хоста MySQL сервера | `"localhost"` |
| `Port` | `int` | Порт MySQL сервера | `3306` |
| `Username` | `string` | Имя пользователя | `string.Empty` |
| `Password` | `string` | Пароль пользователя | `string.Empty` |
| `Database` | `string` | Имя базы данных (опционально) | `string.Empty` |

**Example**:

```csharp
var config = new ConnectionConfig
{
    Host = "192.168.1.100",
    Port = 3306,
    Username = "myuser",
    Password = "mypassword",
    Database = "mydb"
};
```

---

#### TableInfo

**Namespace**: `IvkExportTool.Core.Models`

Информация о таблице базы данных.

```csharp
public class TableInfo
{
    public string Name { get; set; } = string.Empty;
    public long RowCount { get; set; }
    public long SizeInBytes { get; set; }
    public string Engine { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}
```

##### Properties

| Свойство | Тип | Описание |
|----------|-----|----------|
| `Name` | `string` | Имя таблицы |
| `RowCount` | `long` | Количество строк в таблице |
| `SizeInBytes` | `long` | Размер таблицы в байтах (данные + индексы) |
| `Engine` | `string` | Тип движка (InnoDB, MyISAM, и т.д.) |
| `IsSelected` | `bool` | Флаг выбора таблицы для экспорта |

**Example**:

```csharp
var table = new TableInfo
{
    Name = "users",
    RowCount = 10000,
    SizeInBytes = 2097152, // 2 MB
    Engine = "InnoDB",
    IsSelected = true
};
```

---

#### ExportOptions

**Namespace**: `IvkExportTool.Core.Models`

Параметры экспорта таблиц.

```csharp
public class ExportOptions
{
    public List<TableInfo> Tables { get; set; } = new();
    public string FilePath { get; set; } = string.Empty;
    public ExportFormat Format { get; set; } = ExportFormat.SQL;
}
```

##### Properties

| Свойство | Тип | Описание |
|----------|-----|----------|
| `Tables` | `List<TableInfo>` | Список таблиц для экспорта |
| `FilePath` | `string` | Путь к файлу для сохранения |
| `Format` | `ExportFormat` | Формат экспорта |

**Example**:

```csharp
var options = new ExportOptions
{
    Tables = new List<TableInfo> { table1, table2 },
    FilePath = "C:\\exports\\backup.sql",
    Format = ExportFormat.SQL
};
```

---

#### Credential

**Namespace**: `IvkExportTool.Core.Models`

Учётные данные для подключения к базе данных.

```csharp
public record Credential(string Username, string Password);
```

##### Properties

| Свойство | Тип | Описание |
|----------|-----|----------|
| `Username` | `string` | Имя пользователя MySQL |
| `Password` | `string` | Пароль пользователя |

**Example**:

```csharp
var credential = new Credential("root", "password123");
Console.WriteLine(credential.Username); // "root"
```

> **Примечание**: Этот record используется для хранения расшифрованных учётных данных в `DefaultCredentials.List`.

---

#### ExportResult

**Namespace**: `IvkExportTool.Core.Models`

Результат операции экспорта.

```csharp
public class ExportResult
{
    public bool Success { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
```

##### Properties

| Свойство | Тип | Описание |
|----------|-----|----------|
| `Success` | `bool` | `true` если экспорт успешен, иначе `false` |
| `FilePath` | `string` | Путь к созданному файлу |
| `ErrorMessage` | `string` | Сообщение об ошибке (если `Success = false`) |

**Example**:

```csharp
var result = await exportService.ExportAsync(options);
if (result.Success)
{
    Console.WriteLine($"Exported to: {result.FilePath}");
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
}
```

---

### Enums

#### ExportFormat

**Namespace**: `IvkExportTool.Core.Enums`

Формат экспорта данных.

```csharp
public enum ExportFormat
{
    SQL,
    CSV,
    JSON
}
```

| Значение | Описание |
|----------|----------|
| `SQL` | SQL файл с CREATE TABLE и INSERT запросами |
| `CSV` | CSV файл (в разработке) |
| `JSON` | JSON файл (в разработке) |

**Example**:

```csharp
var options = new ExportOptions
{
    Format = ExportFormat.SQL
};
```

---

#### ConnectionStatus

**Namespace**: `IvkExportTool.Core.Enums`

Статус подключения к базе данных.

```csharp
public enum ConnectionStatus
{
    Disconnected,
    Connecting,
    Connected,
    Error
}
```

| Значение | Описание |
|----------|----------|
| `Disconnected` | Подключение отсутствует |
| `Connecting` | Процесс подключения |
| `Connected` | Подключение установлено |
| `Error` | Ошибка подключения |

---

### Constants

#### DefaultCredentials

**Namespace**: `IvkExportTool.Core.Constants`

Предустановленные учётные данные для автоподключения к БД ИВК.

```csharp
public static class DefaultCredentials
{
    public static IReadOnlyList<Credential> List { get; }
}
```

##### Properties

| Свойство | Тип | Описание |
|----------|-----|----------|
| `List` | `IReadOnlyList<Credential>` | Список учётных данных (расшифровывается при первом обращении) |

##### Implementation Details

- Credentials хранятся в зашифрованном виде как `byte[][]`
- Расшифровка происходит lazy при первом обращении к `List`
- Используется AES-256 шифрование через `CredentialProtector`
- Результат кешируется для повторного использования

**Example**:

```csharp
// Получение списка credentials для автоподключения
foreach (var credential in DefaultCredentials.List)
{
    var config = new ConnectionConfig
    {
        Host = host,
        Port = port,
        Username = credential.Username,
        Password = credential.Password
    };

    if (await databaseService.TestConnectionAsync(config))
    {
        // Успешное подключение
        break;
    }
}
```

---

### Security

#### CredentialProtector

**Namespace**: `IvkExportTool.Core.Security`

**Access**: `internal` (не доступен извне сборки)

Защита учётных данных с использованием AES-256 шифрования.

```csharp
internal static class CredentialProtector
{
    internal static string Unprotect(byte[] encryptedData);
    internal static byte[] Protect(string plainText);
}
```

##### Methods

**Unprotect**

Расшифровывает данные.

```csharp
internal static string Unprotect(byte[] encryptedData)
```

| Параметр | Тип | Описание |
|----------|-----|----------|
| `encryptedData` | `byte[]` | Зашифрованные данные (IV + ciphertext) |

**Returns**: `string` — расшифрованная строка

---

**Protect**

Шифрует данные (используется для генерации зашифрованных констант).

```csharp
internal static byte[] Protect(string plainText)
```

| Параметр | Тип | Описание |
|----------|-----|----------|
| `plainText` | `string` | Исходная строка для шифрования |

**Returns**: `byte[]` — зашифрованные данные (IV + ciphertext)

##### Implementation Details

- **Алгоритм**: AES-256 в режиме CBC с PKCS7 padding
- **Ключ**: Собирается из 8 частей (по 4 байта), затем хешируется SHA256
- **IV**: Генерируется случайно и хранится в начале зашифрованных данных
- **Очистка памяти**: Ключ очищается после использования через `Array.Clear()`

**Структура зашифрованных данных**:

```
[IV: 16 bytes][Ciphertext: N bytes]
```

---

## Infrastructure Layer

### Services

#### MySqlDatabaseService

**Namespace**: `IvkExportTool.Infrastructure.Services`

Реализация `IDatabaseService` для работы с MySQL.

```csharp
public class MySqlDatabaseService : IDatabaseService
{
    public async Task<bool> TestConnectionAsync(ConnectionConfig config);
    public async Task<List<string>> GetDatabasesAsync(ConnectionConfig config);
    public async Task<List<TableInfo>> GetTablesAsync(string databaseName);
}
```

##### Implementation Details

- Использует `MySqlConnector` для подключения
- Выполняет запросы к `information_schema` для получения метаданных
- Обрабатывает `MySqlException` и возвращает `false` при ошибках

**Internal Query (GetTablesAsync)**:

```sql
SELECT
    TABLE_NAME,
    TABLE_ROWS,
    DATA_LENGTH + INDEX_LENGTH as SIZE,
    ENGINE
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = @databaseName
ORDER BY TABLE_NAME
```

---

#### SqlExportService

**Namespace**: `IvkExportTool.Infrastructure.Services`

Реализация `IExportService` для экспорта в SQL формат.

```csharp
public class SqlExportService : IExportService
{
    public async Task<ExportResult> ExportAsync(ExportOptions options);
}
```

##### Implementation Details

- Экспортирует структуру таблиц (`CREATE TABLE`)
- Экспортирует данные таблиц (`INSERT INTO`)
- Использует потоковую запись (`StreamWriter`)
- Обрабатывает специальные символы и кодировку UTF-8

**Generated SQL Format**:

```sql
-- IvkExportTool Export
SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS `table_name`;
CREATE TABLE `table_name` (...);

BEGIN;
INSERT INTO `table_name` VALUES (...);
COMMIT;

SET FOREIGN_KEY_CHECKS = 1;
```

---

#### AppSettingsService

**Namespace**: `IvkExportTool.Infrastructure.Services`

Реализация `IAppSettingsService` для работы с настройками.

```csharp
public class AppSettingsService : IAppSettingsService
{
    public void SaveSettings(AppSettings settings);
    public AppSettings LoadSettings();
}
```

##### Implementation Details

- Использует `System.Text.Json` для сериализации
- Сохраняет настройки в `appsettings.json` в папке приложения
- При отсутствии файла возвращает настройки по умолчанию

---

## Desktop Layer

### ViewModels

#### ConnectionWindowViewModel

**Namespace**: `IvkExportTool.Desktop.ViewModels`

ViewModel для окна подключения к базе данных.

```csharp
public partial class ConnectionWindowViewModel : ViewModelBase
{
    // Observable Properties
    [ObservableProperty]
    private string _host = "localhost";

    [ObservableProperty]
    private int _port = 3306;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    // Commands
    [RelayCommand]
    private async Task TestConnectionAsync();

    [RelayCommand]
    private async Task ConnectAsync();

    // Events
    public event Action<ConnectionConfig>? ConnectionSucceeded;
}
```

##### Properties

| Свойство | Тип | Описание |
|----------|-----|----------|
| `Host` | `string` | IP-адрес или имя хоста |
| `Port` | `int` | Порт MySQL |
| `Username` | `string` | Имя пользователя |
| `Password` | `string` | Пароль |

##### Commands

**TestConnectionCommand**

Проверяет подключение к серверу без перехода к главному окну.

**ConnectCommand**

Подключается к серверу и вызывает событие `ConnectionSucceeded`.

##### Events

**ConnectionSucceeded**

Вызывается при успешном подключении.

```csharp
viewModel.ConnectionSucceeded += (config) =>
{
    // Открыть главное окно
};
```

---

#### MainWindowViewModel

**Namespace**: `IvkExportTool.Desktop.ViewModels`

ViewModel для главного окна приложения.

```csharp
public partial class MainWindowViewModel : ViewModelBase
{
    // Observable Properties
    [ObservableProperty]
    private ObservableCollection<string> _databases = new();

    [ObservableProperty]
    private string? _selectedDatabase;

    [ObservableProperty]
    private ObservableCollection<TableItemViewModel> _tables = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool? _tablesSelectionState;

    // Commands
    [RelayCommand]
    private async Task LoadDatabasesAsync();

    [RelayCommand]
    private async Task LoadTablesAsync();

    [RelayCommand]
    private async Task ExportAsync();

    [RelayCommand]
    private void SelectAll();

    [RelayCommand]
    private void DeselectAll();

    [RelayCommand]
    private void ToggleAllTablesSelection();

    [RelayCommand]
    private void ChangeConnection();

    // Events
    public event Action? ChangeConnectionRequested;
}
```

##### Properties

| Свойство | Тип | Описание |
|----------|-----|----------|
| `Databases` | `ObservableCollection<string>` | Список доступных баз данных |
| `SelectedDatabase` | `string?` | Выбранная база данных |
| `Tables` | `ObservableCollection<TableItemViewModel>` | Список таблиц |
| `SearchText` | `string` | Текст поиска таблиц |
| `TablesSelectionState` | `bool?` | Состояние тристейтного чекбокса |

##### Commands

| Команда | Описание |
|---------|----------|
| `LoadDatabasesCommand` | Загружает список баз данных |
| `LoadTablesCommand` | Загружает список таблиц из выбранной БД |
| `ExportCommand` | Экспортирует выбранные таблицы |
| `SelectAllCommand` | Выбирает все отфильтрованные таблицы |
| `DeselectAllCommand` | Снимает выбор со всех таблиц |
| `ToggleAllTablesSelectionCommand` | Переключает состояние всех таблиц |
| `ChangeConnectionCommand` | Запрашивает смену подключения |

##### Computed Properties

**FilteredTables**

Возвращает отфильтрованный список таблиц согласно `SearchText`.

```csharp
public IEnumerable<TableItemViewModel> FilteredTables =>
    Tables.Where(t => t.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
```

**SelectedTablesCount**

Возвращает количество выбранных таблиц.

```csharp
public int SelectedTablesCount =>
    Tables.Count(t => t.IsSelected);
```

---

#### TableItemViewModel

**Namespace**: `IvkExportTool.Desktop.ViewModels`

ViewModel-обёртка для `TableInfo` с UI-специфичной логикой.

```csharp
public partial class TableItemViewModel : ViewModelBase
{
    private readonly TableInfo _tableInfo;

    [ObservableProperty]
    private bool _isSelected;

    public string Name => _tableInfo.Name;
    public long RowCount => _tableInfo.RowCount;
    public long SizeInBytes => _tableInfo.SizeInBytes;
    public string Engine => _tableInfo.Engine;
    public string SizeFormatted => FormatBytes(_tableInfo.SizeInBytes);

    public event Action? SelectionChanged;

    public TableInfo GetTableInfo() => _tableInfo;
}
```

##### Properties

| Свойство | Тип | Описание |
|----------|-----|----------|
| `Name` | `string` | Имя таблицы (read-only) |
| `RowCount` | `long` | Количество строк (read-only) |
| `SizeInBytes` | `long` | Размер в байтах (read-only) |
| `Engine` | `string` | Тип движка (read-only) |
| `SizeFormatted` | `string` | Размер в читаемом формате (1.5 MB, 234 KB) |
| `IsSelected` | `bool` | Флаг выбора для экспорта |

##### Methods

**GetTableInfo**

Возвращает оригинальный объект `TableInfo`.

```csharp
public TableInfo GetTableInfo() => _tableInfo;
```

##### Events

**SelectionChanged**

Вызывается при изменении `IsSelected`.

---

## Usage Examples

### Подключение к БД

```csharp
var dbService = new MySqlDatabaseService();
var config = new ConnectionConfig
{
    Host = "localhost",
    Port = 3306,
    Username = "root",
    Password = "password"
};

// Тест подключения
bool connected = await dbService.TestConnectionAsync(config);

// Получение баз данных
var databases = await dbService.GetDatabasesAsync(config);

// Получение таблиц
var tables = await dbService.GetTablesAsync("mydb");
```

### Экспорт таблиц

```csharp
var exportService = new SqlExportService();
var options = new ExportOptions
{
    Tables = tables.Where(t => t.IsSelected).ToList(),
    FilePath = "C:\\exports\\mydb_20250120.sql",
    Format = ExportFormat.SQL
};

var result = await exportService.ExportAsync(options);
if (result.Success)
{
    Console.WriteLine($"Exported to: {result.FilePath}");
}
```

### Использование в ViewModel

```csharp
public class MainWindowViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IExportService _exportService;

    public MainWindowViewModel(
        IDatabaseService databaseService,
        IExportService exportService)
    {
        _databaseService = databaseService;
        _exportService = exportService;
    }

    [RelayCommand]
    private async Task LoadTablesAsync()
    {
        var tables = await _databaseService.GetTablesAsync(SelectedDatabase!);
        Tables.Clear();
        foreach (var table in tables)
        {
            Tables.Add(new TableItemViewModel(table));
        }
    }
}
```

---

**Вернуться к**: [Содержание Wiki](Home.md) | [Архитектура](Architecture.md) | [Разработка](Development.md)
