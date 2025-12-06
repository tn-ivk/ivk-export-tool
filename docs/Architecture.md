# Архитектура приложения

Детальное описание архитектуры **IvkExportTool**, проектных решений и паттернов проектирования.

## Содержание

- [Обзор архитектуры](#обзор-архитектуры)
- [Clean Architecture](#clean-architecture)
- [Слои приложения](#слои-приложения)
- [Dependency Injection](#dependency-injection)
- [MVVM паттерн](#mvvm-паттерн)
- [Коммуникация между компонентами](#коммуникация-между-компонентами)
- [Управление состоянием](#управление-состоянием)
- [Обработка ошибок](#обработка-ошибок)
- [Производительность](#производительность)
- [Дизайн-система](#дизайн-система)

## Обзор архитектуры

IvkExportTool построен на основе **Clean Architecture** с тремя основными слоями:

```
┌─────────────────────────────────────────────────┐
│          Presentation Layer                     │
│      (IvkExportTool.Desktop)                    │
│                                                 │
│  ┌──────────────┐         ┌──────────────┐     │
│  │  ViewModels  │◄────────┤    Views     │     │
│  │   (MVVM)     │         │   (AXAML)    │     │
│  └──────┬───────┘         └──────────────┘     │
│         │                                       │
└─────────┼───────────────────────────────────────┘
          │ uses
          ▼
┌─────────────────────────────────────────────────┐
│           Core Layer                            │
│      (IvkExportTool.Core)                       │
│                                                 │
│  ┌──────────────┐         ┌──────────────┐     │
│  │  Interfaces  │         │    Models    │     │
│  │   (DI)       │         │  (Entities)  │     │
│  └──────────────┘         └──────────────┘     │
│                                                 │
└─────────────────────────────────────────────────┘
          ▲
          │ implements
          │
┌─────────┴───────────────────────────────────────┐
│       Infrastructure Layer                      │
│   (IvkExportTool.Infrastructure)                │
│                                                 │
│  ┌──────────────┐         ┌──────────────┐     │
│  │   Services   │◄────────┤  External    │     │
│  │ (MySql, SQL) │         │   (MySQL)    │     │
│  └──────────────┘         └──────────────┘     │
│                                                 │
└─────────────────────────────────────────────────┘
```

### Принципы проектирования

1. **Separation of Concerns** — разделение ответственности между слоями
2. **Dependency Inversion** — зависимости направлены к Core слою
3. **Single Responsibility** — каждый класс имеет одну ответственность
4. **Open/Closed Principle** — открыт для расширения, закрыт для изменения
5. **Testability** — все слои независимо тестируются

## Clean Architecture

### Слоистая структура

#### Core Layer (Domain)

**Назначение**: Бизнес-логика и модели предметной области

**Зависимости**: Отсутствуют (чистый .NET)

**Содержит**:
- Модели данных (`ConnectionConfig`, `TableInfo`, `ExportOptions`, `Credential`)
- Интерфейсы сервисов (`IDatabaseService`, `IExportService`)
- Перечисления (`ExportFormat`, `ConnectionStatus`)
- Константы (`DefaultCredentials` — зашифрованные учётные данные)
- Безопасность (`CredentialProtector` — AES-256 шифрование)

**Правила**:
- ❌ Не зависит от внешних библиотек
- ❌ Не знает о UI или базе данных
- ✅ Только POCO классы и интерфейсы

#### Infrastructure Layer (Data Access)

**Назначение**: Реализация работы с внешними системами

**Зависимости**: Core + MySqlConnector

**Содержит**:
- Реализации сервисов (`MySqlDatabaseService`, `SqlExportService`)
- Логика подключения к MySQL
- Экспорт в SQL формат

**Правила**:
- ✅ Реализует интерфейсы из Core
- ✅ Содержит всю логику работы с БД
- ❌ Не знает о UI слое

#### Presentation Layer (UI)

**Назначение**: Пользовательский интерфейс и взаимодействие

**Зависимости**: Core + Infrastructure + Avalonia

**Содержит**:
- ViewModels (MVVM)
- Views (AXAML)
- UI-специфичная логика
- Dependency Injection конфигурация

**Правила**:
- ✅ Использует интерфейсы из Core
- ✅ Не содержит бизнес-логику
- ✅ Использует DI для получения сервисов

### Зависимости между проектами

```
Desktop ──┬──> Core
          └──> Infrastructure ──> Core

Tests ───────> Core + Infrastructure + Desktop
```

## Слои приложения

### Core Layer

#### Модели

**ConnectionConfig** (`Models/ConnectionConfig.cs`):
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

**TableInfo** (`Models/TableInfo.cs`):
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

#### Интерфейсы

**IDatabaseService** (`Interfaces/IDatabaseService.cs`):
```csharp
public interface IDatabaseService
{
    Task<bool> TestConnectionAsync(ConnectionConfig config);
    Task<List<string>> GetDatabasesAsync(ConnectionConfig config);
    Task<List<TableInfo>> GetTablesAsync(string databaseName);
}
```

**IExportService** (`Interfaces/IExportService.cs`):
```csharp
public interface IExportService
{
    Task<ExportResult> ExportAsync(ExportOptions options);
}
```

### Infrastructure Layer

**MySqlDatabaseService** (`Services/MySqlDatabaseService.cs`):

Реализует подключение к MySQL и получение метаданных:

```csharp
public class MySqlDatabaseService : IDatabaseService
{
    public async Task<bool> TestConnectionAsync(ConnectionConfig config)
    {
        using var connection = new MySqlConnection(BuildConnectionString(config));
        await connection.OpenAsync();
        return connection.State == ConnectionState.Open;
    }

    public async Task<List<TableInfo>> GetTablesAsync(string databaseName)
    {
        // Запрос к information_schema для получения метаданных таблиц
        var query = @"
            SELECT
                TABLE_NAME,
                TABLE_ROWS,
                DATA_LENGTH + INDEX_LENGTH as SIZE,
                ENGINE
            FROM information_schema.TABLES
            WHERE TABLE_SCHEMA = @dbName
        ";
        // ...
    }
}
```

**SqlExportService** (`Services/SqlExportService.cs`):

Экспортирует таблицы в SQL формат:

```csharp
public class SqlExportService : IExportService
{
    public async Task<ExportResult> ExportAsync(ExportOptions options)
    {
        using var writer = new StreamWriter(options.FilePath);

        foreach (var table in options.Tables)
        {
            await ExportTableStructureAsync(writer, table);
            await ExportTableDataAsync(writer, table);
        }

        return new ExportResult { Success = true, FilePath = options.FilePath };
    }
}
```

### Presentation Layer

**MVVM Structure**:

```
ViewModels/
├── ViewModelBase.cs           # Базовый класс с INotifyPropertyChanged
├── ConnectionWindowViewModel.cs
├── MainWindowViewModel.cs
└── TableItemViewModel.cs       # Wrapper для TableInfo

Views/
├── ConnectionWindow.axaml      # Окно подключения
└── MainWindow.axaml            # Главное окно
```

## Dependency Injection

### Конфигурация DI

Регистрация сервисов в `App.axaml.cs`:

```csharp
public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();

        // Регистрация сервисов (Singleton)
        services.AddSingleton<IAppSettingsService, AppSettingsService>();
        services.AddSingleton<IDatabaseService, MySqlDatabaseService>();
        services.AddSingleton<IExportService, SqlExportService>();

        // Регистрация ViewModels (Transient)
        services.AddTransient<ConnectionWindowViewModel>();
        services.AddTransient<MainWindowViewModel>();

        _serviceProvider = services.BuildServiceProvider();
    }
}
```

### Жизненный цикл зависимостей

| Тип | Жизненный цикл | Применение |
|-----|----------------|------------|
| **Singleton** | Один экземпляр на всё приложение | Сервисы, Settings |
| **Transient** | Новый экземпляр каждый раз | ViewModels |
| **Scoped** | Один экземпляр на scope | Не используется |

### Инъекция в ViewModels

```csharp
public class MainWindowViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IExportService _exportService;
    private readonly IAppSettingsService _settingsService;

    public MainWindowViewModel(
        IDatabaseService databaseService,
        IExportService exportService,
        IAppSettingsService settingsService)
    {
        _databaseService = databaseService;
        _exportService = exportService;
        _settingsService = settingsService;
    }
}
```

## MVVM паттерн

### Структура MVVM

```
┌──────────┐         ┌──────────────┐         ┌─────────┐
│   View   │◄────────┤  ViewModel   │◄────────┤  Model  │
│ (AXAML)  │ Binding │  (C# Logic)  │  Uses   │ (Data)  │
└──────────┘         └──────────────┘         └─────────┘
```

### ViewModelBase

Базовый класс для всех ViewModels:

```csharp
public class ViewModelBase : ObservableObject
{
    // ObservableObject из CommunityToolkit.Mvvm
    // Реализует INotifyPropertyChanged
}
```

### Пример ViewModel

```csharp
public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<TableItemViewModel> _tables = new();

    [ObservableProperty]
    private bool? _tablesSelectionState;

    [RelayCommand]
    private async Task ExportAsync()
    {
        var selectedTables = Tables
            .Where(t => t.IsSelected)
            .Select(t => t.GetTableInfo())
            .ToList();

        var result = await _exportService.ExportAsync(new ExportOptions
        {
            Tables = selectedTables,
            FilePath = filePath
        });
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var table in FilteredTables)
        {
            table.IsSelected = true;
        }
    }
}
```

### Data Binding

```xml
<DataGrid ItemsSource="{Binding FilteredTables}">
  <DataGrid.Columns>
    <DataGridCheckBoxColumn Binding="{Binding IsSelected}" />
    <DataGridTextColumn Binding="{Binding Name}" Header="Имя" />
    <DataGridTextColumn Binding="{Binding RowCount}" Header="Строки" />
  </DataGrid.Columns>
</DataGrid>

<Button Content="Экспортировать" Command="{Binding ExportCommand}" />
```

### CommunityToolkit.Mvvm

Используются Source Generators для автоматической генерации boilerplate кода:

- `[ObservableProperty]` → генерирует свойство с INotifyPropertyChanged
- `[RelayCommand]` → генерирует ICommand из метода
- `[NotifyPropertyChangedFor]` → уведомляет о связанных свойствах

## Коммуникация между компонентами

### События между ViewModels

**ConnectionWindowViewModel → MainWindowViewModel**:

```csharp
public class ConnectionWindowViewModel
{
    public event Action<ConnectionConfig>? ConnectionSucceeded;

    private async Task ConnectAsync()
    {
        // ... подключение успешно
        ConnectionSucceeded?.Invoke(_config);
    }
}
```

**MainWindowViewModel → ConnectionWindow**:

```csharp
public class MainWindowViewModel
{
    public event Action? ChangeConnectionRequested;

    [RelayCommand]
    private void ChangeConnection()
    {
        ChangeConnectionRequested?.Invoke();
    }
}
```

### Переключение окон

Логика в `App.axaml.cs`:

```csharp
private void OnConnectionSucceeded(ConnectionConfig config)
{
    var position = SaveWindowPosition(_connectionWindow);
    _connectionWindow.Close();

    var mainViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
    mainViewModel.Initialize(config);

    _mainWindow = new MainWindow { DataContext = mainViewModel };
    RestoreWindowPositionOnSameScreen(_mainWindow, position);
    _mainWindow.Show();
}
```

## Управление состоянием

### Сохранение настроек

Настройки управляются через **System.Text.Json с Source Generators** для AOT-совместимости.

**Структура файлов конфигурации** (`Infrastructure/Configuration/`):
- `AppSettings.cs` — POCO-классы настроек
- `AppSettingsJsonContext.cs` — Source Generator контекст
- `SettingsStore.cs` — загрузка/сохранение настроек

**AppSettingsJsonContext** (compile-time сериализация):

```csharp
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AppSettings))]
internal partial class AppSettingsJsonContext : JsonSerializerContext { }
```

**SettingsStore** (потокобезопасные операции):

```csharp
public static class SettingsStore
{
    private static readonly object _lock = new();

    public static AppSettings Load()
    {
        if (!File.Exists(SettingsPath))
            return new AppSettings();

        var json = File.ReadAllText(SettingsPath);
        return JsonSerializer.Deserialize(json, AppSettingsJsonContext.Default.AppSettings)
               ?? new AppSettings();
    }

    public static void Save(AppSettings settings)
    {
        lock (_lock)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            var json = JsonSerializer.Serialize(settings, AppSettingsJsonContext.Default.AppSettings);
            File.WriteAllText(SettingsPath, json);
        }
    }
}
```

### Reactive UI

Используется MVVM с реактивными обновлениями:

```csharp
partial void OnSearchTextChanged(string value)
{
    // Автоматически вызывается при изменении SearchText
    FilterTables();
}

partial void OnIsSelectedChanged(bool value)
{
    // Обновляем исходную модель
    _tableInfo.IsSelected = value;

    // Уведомляем родительский ViewModel
    SelectionChanged?.Invoke();
}
```

## Обработка ошибок

### Try-Catch блоки

```csharp
public async Task<bool> TestConnectionAsync(ConnectionConfig config)
{
    try
    {
        using var connection = new MySqlConnection(BuildConnectionString(config));
        await connection.OpenAsync();
        return true;
    }
    catch (MySqlException ex)
    {
        _logger?.LogError(ex, "Failed to connect to MySQL");
        return false;
    }
}
```

### Валидация пользовательского ввода

```csharp
[RelayCommand(CanExecute = nameof(CanExport))]
private async Task ExportAsync()
{
    // ...
}

private bool CanExport()
{
    return Tables.Any(t => t.IsSelected);
}
```

## Производительность

### Асинхронные операции

Все операции с БД асинхронные:

```csharp
public async Task<List<TableInfo>> GetTablesAsync(string databaseName)
{
    await using var connection = new MySqlConnection(_connectionString);
    await connection.OpenAsync();

    await using var command = new MySqlCommand(query, connection);
    await using var reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        // ...
    }
}
```

### Потоковая обработка экспорта

```csharp
public async Task ExportTableDataAsync(StreamWriter writer, TableInfo table)
{
    const int batchSize = 1000;
    var offset = 0;

    while (true)
    {
        var batch = await GetBatchAsync(table.Name, offset, batchSize);
        if (batch.Count == 0) break;

        await WriteInsertStatementsAsync(writer, batch);
        offset += batchSize;
    }
}
```

### Observable Collections

Используются для автоматического обновления UI:

```csharp
public ObservableCollection<TableItemViewModel> Tables { get; } = new();

private void AddTable(TableInfo info)
{
    // UI автоматически обновится
    Tables.Add(new TableItemViewModel(info));
}
```

## Безопасность

### Шифрование учётных данных

Для автоподключения к БД используются предустановленные учётные данные, которые хранятся в зашифрованном виде:

```
┌─────────────────────────────────────────────────────────────┐
│                    DefaultCredentials                        │
│                                                              │
│  ┌────────────────┐     ┌─────────────────────────────────┐ │
│  │  Encrypted     │────▶│     CredentialProtector         │ │
│  │  byte[][]      │     │                                 │ │
│  │  (IV+cipher)   │     │  ┌───────────────────────────┐  │ │
│  └────────────────┘     │  │  AES-256 CBC + PKCS7      │  │ │
│                          │  │                           │  │ │
│                          │  │  Key: SHA256(parts[0..7]) │  │ │
│                          │  └───────────────────────────┘  │ │
│                          └─────────────────────────────────┘ │
│                                       │                      │
│                                       ▼                      │
│                          ┌─────────────────────────────────┐ │
│                          │  List<Credential> (lazy load)   │ │
│                          │  - Username                     │ │
│                          │  - Password                     │ │
│                          └─────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### Архитектура защиты

**CredentialProtector** (`Core/Security/CredentialProtector.cs`):

```csharp
internal static class CredentialProtector
{
    // Ключ собирается из 8 частей, рассеянных по коду
    private static readonly byte[] KeyPart1 = { 0x49, 0x76, 0x6B, 0x45 };
    // ... KeyPart2-8

    internal static string Unprotect(byte[] encryptedData)
    {
        var key = AssembleKey();  // Собирает и хеширует ключ
        // AES-256 CBC расшифровка
    }

    private static byte[] AssembleKey()
    {
        // Собирает ключ из частей
        // Хеширует SHA256 для получения 256-bit ключа
    }
}
```

**DefaultCredentials** (`Core/Constants/DefaultCredentials.cs`):

```csharp
public static class DefaultCredentials
{
    // Зашифрованные данные (IV + ciphertext)
    private static readonly byte[][] EncryptedCredentials = [...];

    // Lazy-загрузка при первом обращении
    public static IReadOnlyList<Credential> List =>
        _cachedList ??= DecryptCredentials();
}
```

### Уровень защиты

| Защищает от | Не защищает от |
|-------------|----------------|
| ✅ `strings` утилиты | ❌ Отладчик с breakpoint |
| ✅ `grep` по паролям | ❌ Дамп памяти процесса |
| ✅ Hex-редакторов | ❌ Опытный реверс-инженер |
| ✅ Случайного просмотра кода | |
| ✅ Статического анализа (ILSpy) | |

> **Важно**: 100% защита credentials в клиентском приложении невозможна. Шифрование значительно усложняет извлечение, но не является абсолютной защитой.

## Дизайн-система

### Material Design 3

Приложение следует дизайн-системе из `design.md`:

- **Primary Color**: `#546e7a` (Blue Grey)
- **Typography**: Roboto / Inter
- **Spacing**: 8px grid система
- **Border Radius**: 6-8px

### Стили в AXAML

```xml
<Application.Styles>
  <FluentTheme />

  <Style Selector="Button.primary">
    <Setter Property="Background" Value="#546e7a" />
    <Setter Property="Foreground" Value="White" />
    <Setter Property="CornerRadius" Value="6" />
  </Style>
</Application.Styles>
```

## Диаграммы

### Поток подключения к БД

```
User Input ──> ConnectionViewModel ──> IDatabaseService
                                               │
                                               ▼
                                      MySqlDatabaseService
                                               │
                                               ▼
                                          MySQL Server
                                               │
                                               ▼
                                      Success/Failure ──> Event
                                               │
                                               ▼
                                      MainWindow (open)
```

### Поток экспорта

```
User Selection ──> MainViewModel ──> IExportService
                                            │
                                            ▼
                                    SqlExportService
                                            │
                     ┌──────────────────────┼──────────────────────┐
                     ▼                      ▼                      ▼
            Get Table Structure    Get Table Data         Write to File
                     │                      │                      │
                     └──────────────────────┴──────────────────────┘
                                            │
                                            ▼
                                      Export Complete
```

---

**Следующие шаги**: Изучите [API Reference](API-Reference.md) для детального описания классов и методов.
