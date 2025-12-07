# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Описание проекта

**IvkExportTool** - кроссплатформенное портабельное приложение с GUI на Avalonia для экспорта таблиц MySQL в SQL-файлы. В текущей версии реализован экспорт в SQL формат с полной структурой и данными таблиц.

## Технологический стек

- **.NET 10.0** (SDK 10.0.0)
- **Avalonia UI 11.3.8** - кроссплатформенный GUI фреймворк
- **MySqlConnector 2.4.0** - подключение к MySQL базам данных
- **System.Text.Json с Source Generators** - AOT-совместимая сериализация настроек
- **CommunityToolkit.Mvvm 8.2.1** - MVVM паттерн
- **NUnit 4.2.2** - тестирование
- **FluentAssertions 8.8.0** - assertion библиотека для тестов
- **Moq 4.20.72** - мокирование зависимостей в тестах
- **Testcontainers.MySql 4.9.0** - интеграционные тесты с реальной MySQL в Docker

## Архитектура проекта

Проект использует Clean Architecture с тремя основными слоями:

### Структура решения

```
IvkExportTool/
├── src/
│   ├── IvkExportTool.Core/          # Domain Layer
│   │   ├── Models/                  # Бизнес-модели (ConnectionConfig, DatabaseInfo, TableInfo, Credential)
│   │   ├── Interfaces/              # Интерфейсы сервисов (IDatabaseService, IExportService)
│   │   ├── Constants/               # Константы (DefaultCredentials - зашифрованные учётные данные)
│   │   ├── Security/                # Безопасность (CredentialProtector - AES шифрование)
│   │   └── Enums/                   # Перечисления (ExportFormat, ConnectionStatus)
│   ├── IvkExportTool.Infrastructure/  # Data Access Layer
│   │   ├── Configuration/           # Конфигурация (AppSettings, SettingsStore, JsonContext)
│   │   └── Services/                # Реализация сервисов (MySqlDatabaseService, SqlExportService)
│   └── IvkExportTool.Desktop/       # Presentation Layer
│       ├── ViewModels/              # MVVM ViewModels (CommunityToolkit.Mvvm)
│       ├── Views/                   # Avalonia AXAML представления
│       └── Models/                  # UI модели и обёртки
└── tests/
    └── IvkExportTool.Tests/         # Тесты (NUnit)
        ├── Core/                    # Unit-тесты Core слоя
        ├── Infrastructure/          # Unit-тесты Infrastructure слоя
        ├── Desktop/                 # Unit-тесты Desktop слоя
        └── Integration/             # Интеграционные тесты (Testcontainers)
```

### Зависимости между проектами

- `Infrastructure` → `Core` (реализует интерфейсы из Core)
- `Desktop` → `Core` + `Infrastructure` (использует оба)
- `Tests` → все проекты

### Ключевые интерфейсы и модели

#### Core Layer (src/IvkExportTool.Core/)

**Интерфейсы** (в `Interfaces/`):
- `IDatabaseService` - работа с базами данных (подключение, получение списка БД и таблиц)
- `IExportService` - экспорт таблиц в различные форматы
- `IAppSettingsService` - сохранение/загрузка настроек приложения (подключение, последняя папка экспорта)

**Модели** (в `Models/`):
- `ConnectionConfig` - конфигурация подключения к БД (хост, порт, логин, пароль, БД)
- `DatabaseInfo` - информация о базе данных
- `TableInfo` - информация о таблице (имя, количество строк, размер, тип движка)
- `ExportOptions` - параметры экспорта (список таблиц, формат, путь)
- `ExportResult` - результат экспорта (успех, путь к файлу, ошибки)

### Функциональность экспорта

**Диалог сохранения файла**:
- При экспорте открывается стандартный диалог выбора места сохранения
- Предлагаемое имя файла генерируется автоматически:
  - Одна таблица: `ИмяБазыДанных_ИмяТаблицы_ДатаВремя.sql`
  - Несколько таблиц: `ИмяБазыДанных_ДатаВремя.sql`
- Последняя использованная папка сохраняется в настройках
- По умолчанию (при первом запуске) используется текущая директория приложения
- Настройки сохраняются в `settings.json` в поле `LastExportDirectory`

**Таймер и прогресс экспорта**:
- Общий таймер показывает время экспорта всех выбранных таблиц
- Обновление прогресса происходит каждую секунду (по времени, а не по количеству строк)
- Общий `Stopwatch` передается через всю цепочку методов экспорта в `SqlExportService` (см. `src/IvkExportTool.Infrastructure/Services/SqlExportService.cs:20-60`)
- UI показывает упрощенную строку состояния: детальное сообщение и прошедшее время на одном уровне (без подписи "Время")
- Убрана информация о текущей таблице и количестве обработанных строк для упрощения интерфейса

### Dependency Injection

Приложение использует Microsoft.Extensions.DependencyInjection для управления зависимостями. Конфигурация происходит в `App.axaml.cs` (см. `src/IvkExportTool.Desktop/App.axaml.cs:31-45`):

```csharp
// Регистрация сервисов
services.AddSingleton<IAppSettingsService, AppSettingsService>();
services.AddSingleton<IDatabaseService, MySqlDatabaseService>();
services.AddSingleton<IExportService, SqlExportService>();

// Регистрация ViewModels
services.AddTransient<StartWindowViewModel>();
services.AddTransient<ConnectionWindowViewModel>();
services.AddTransient<AutoConnectionWindowViewModel>();
services.AddTransient<MainWindowViewModel>();
```

ViewModels получают зависимости через конструктор. Сервисы регистрируются как Singleton, ViewModels как Transient.

### Система окон и навигация

Приложение использует систему из четырех окон с интеллектуальной навигацией:

#### Окна приложения

1. **StartWindow** (460x360) - стартовое окно выбора способа подключения
   - Две большие кнопки: "Ручная настройка подключения" и "Автоподключение"
   - События: `ManualConnectionRequested`, `AutoConnectionRequested`
   - ViewModel: `StartWindowViewModel`

2. **ConnectionWindow** (460x360) - окно ручной настройки подключения
   - Поля ввода: хост, порт, логин, пароль
   - Кнопки: "Тест подключения", "Подключиться"
   - Кнопка возврата на стартовое окно (слева от крестика)
   - **Валидация полей**: незаполненные поля подсвечиваются красной рамкой, кнопки заблокированы
   - События: `ConnectionSucceeded`, `BackRequested`
   - ViewModel: `ConnectionWindowViewModel`
   - Заголовок: "Подключение к БД ИВК"

3. **AutoConnectionWindow** (460x360) - окно автоподключения
   - Поля ввода: хост, порт (загружаются из сохранённых настроек)
   - Статус подключения и индикатор загрузки
   - Кнопки: "Отмена" (активна только во время перебора), "Автоподключение"
   - Кнопка возврата на стартовое окно (слева от крестика)
   - **Валидация полей**: незаполненные поля подсвечиваются красной рамкой, кнопка заблокирована
   - Автоматический перебор зашифрованных учётных данных из `DefaultCredentials.List`
   - Таймаут подключения: 5 секунд на каждую попытку
   - Отображение прогресса: "Попытка N из M..."
   - События: `ConnectionSucceeded`, `BackRequested`
   - ViewModel: `AutoConnectionWindowViewModel`
   - Заголовок: "Автоподключение к БД ИВК"

4. **MainWindow** - главное окно работы с базой данных
   - Список баз данных и таблиц
   - Экспорт выбранных таблиц
   - Смена подключения через меню
   - ViewModel: `MainWindowViewModel`

#### Схема навигации

```
StartWindow (выбор способа подключения)
    ├─> ConnectionWindow (ручное подключение) ─┐
    └─> AutoConnectionWindow (автоподключение) ─┤
                                                 ├─> MainWindow (работа с БД)
                                                 │
                                    [Смена подключения] ──> StartWindow
```

#### Логика переключения окон

Реализована в `App.axaml.cs` (см. `src/IvkExportTool.Desktop/App.axaml.cs:67-272`):

**Методы навигации**:
- `ShowStartWindow()` - показывает стартовое окно выбора способа подключения
- `ShowConnectionWindow()` - показывает окно ручной настройки подключения
- `ShowAutoConnectionWindow()` - показывает окно автоподключения
- `SaveCurrentWindowPosition()` - сохраняет позицию любого активного окна
- `CloseAllWindows()` - закрывает все окна подключения (кроме MainWindow)
- `RestoreWindowPositionOnSameScreen()` - восстанавливает позицию окна на том же мониторе

**Обработчики событий**:
- `OnManualConnectionRequested` - переход на окно ручного подключения
- `OnAutoConnectionRequested` - переход на окно автоподключения
- `OnBackFromConnectionRequested` - возврат со страницы ручного подключения на стартовое окно
- `OnBackFromAutoConnectionRequested` - возврат со страницы автоподключения на стартовое окно
- `OnConnectionSucceeded` - успешное подключение (переход на главное окно)
- `OnChangeConnectionRequested` - смена подключения из главного окна (возврат на стартовое окно)

**Особенности реализации**:
- При первом запуске окно центрируется на основном мониторе
- При переключении между окнами новое окно центрируется на том же мониторе, где было предыдущее
- При переключении окон старое закрывается только ПОСЛЕ открытия нового для плавного перехода
- Позиция окна сохраняется в `_lastWindowPosition` и восстанавливается через `RestoreWindowPositionOnSameScreen()`
- Используется `WindowStartupLocation = WindowStartupLocation.Manual` для ручного позиционирования

**Коммуникация между ViewModels**:
- `StartWindowViewModel.ManualConnectionRequested` / `AutoConnectionRequested` - выбор типа подключения
- `ConnectionWindowViewModel.ConnectionSucceeded` / `BackRequested` - результат ручного подключения
- `AutoConnectionWindowViewModel.ConnectionSucceeded` / `BackRequested` - результат автоподключения
- `MainWindowViewModel.ChangeConnectionRequested` - запрос смены подключения

### UI Модели и паттерны

#### TableItemViewModel

`TableItemViewModel` (см. `src/IvkExportTool.Desktop/ViewModels/TableItemViewModel.cs`) - обёртка вокруг `TableInfo` для UI-специфичной логики:

- **Форматирование данных**: `SizeFormatted` преобразует байты в читаемый формат (B, KB, MB, GB)
- **Двусторонняя синхронизация**: изменение `IsSelected` автоматически обновляет исходную модель `TableInfo` через `OnIsSelectedChanged`
- **Доступ к исходной модели**: метод `GetTableInfo()` возвращает оригинальный `TableInfo` для экспорта
- **Свойства только для чтения**: `Name`, `RowCount`, `SizeInBytes`, `Engine` проксируются из `TableInfo`

Паттерн: UI ViewModel обёртывает бизнес-модель для добавления UI-специфичной функциональности без загрязнения Core слоя.

#### Тристейтный чекбокс в заголовке таблицы

Реализован в `MainWindowViewModel.TablesSelectionState` (тип `bool?`):
- `null` - частичный выбор (выбраны не все таблицы)
- `true` - все таблицы выбраны
- `false` - ни одна таблица не выбрана

Команды для управления выбором:
- `ToggleAllTablesSelectionCommand` - переключение состояния чекбокса (из заголовка)
- `SelectAllCommand` - выбор всех отфильтрованных таблиц
- `DeselectAllCommand` - снятие выбора со всех таблиц

Автоматическое обновление: при изменении `IsSelected` у любой `TableItemViewModel` вызывается `UpdateSelectionState()`, который пересчитывает состояние чекбокса в заголовке.

#### Валидация полей подключения

Реализована в `ConnectionWindowViewModel` и `AutoConnectionWindowViewModel`:

**Свойства валидации** (вычисляемые):
- `IsHostValid` - хост не пустой
- `IsPortValid` - порт не пустой, число от 1 до 65535
- `IsUsernameValid` - логин не пустой (только в ConnectionWindow)
- `IsPasswordValid` - пароль не пустой (только в ConnectionWindow)
- `CanConnect` - все поля валидны и не идёт загрузка

**UI реализация**:
- TextBox получает класс `invalid` через `Classes.invalid="{Binding !IsHostValid}"`
- Стиль `TextBox.invalid` задаёт красную рамку (`ErrorBrush`)
- Кнопки привязаны к `IsEnabled="{Binding CanConnect}"`

**Атрибуты CommunityToolkit.Mvvm**:
```csharp
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(IsHostValid))]
[NotifyPropertyChangedFor(nameof(CanConnect))]
private string _host = "";
```

При изменении любого поля автоматически пересчитываются свойства валидации.

#### Версионирование приложения

Версия приложения отображается в нескольких местах UI через свойство `AppVersion` в базовом классе `ViewModelBase` (см. `src/IvkExportTool.Desktop/ViewModels/ViewModelBase.cs`):

**Где отображается версия**:
- **StartWindow** - под кнопками выбора способа подключения (по центру)
- **MainWindow** - в status bar слева (перед статусным сообщением)
- **Заголовок MainWindow** - через свойство `WindowTitle`

**Реализация**:
- **Источник версии**: извлекается из `Assembly.GetExecutingAssembly().GetName().Version`
- **Формат отображения**: `v{Major}.{Minor}.{Build}` (например, `v1.0.0`)
- **Значение по умолчанию**: `v0.0.0` (если версия не установлена)
- **Установка версии в CI/CD**: версия устанавливается при сборке релиза через параметры `/p:Version`, `/p:AssemblyVersion`, `/p:FileVersion`
- **Источник версии для релизов**: извлекается из git-тега (например, тег `v1.0.0` → версия `1.0.0`)
- **Версия для dev-сборок**: `0.0.0-dev` (при сборке без тега)

```csharp
// ViewModelBase.cs - базовый класс для всех ViewModel
public string AppVersion
{
    get
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version != null ? $"v{version.Major}.{version.Minor}.{version.Build}" : "v0.0.0";
    }
}

// MainWindowViewModel.cs - использование в заголовке окна
public string WindowTitle => $"IvkExportTool {AppVersion} - Подключение к БД ИВК";
```

### Система настроек приложения (System.Text.Json + Source Generators)

Приложение использует **System.Text.Json с Source Generators** для AOT-совместимой сериализации настроек.

#### Почему не Config.Net

Ранее использовалась библиотека Config.Net, но она несовместима с IL Trimming из-за использования рефлексии для создания прокси-объектов. При публикации с `PublishTrimmed=true` приложение падало.

#### Расположение файла настроек

Файл `settings.json` хранится в стандартных папках конфигурации ОС:
- **Windows**: `%APPDATA%\IvkExportTool\settings.json`
- **Linux/macOS**: `~/.config/IvkExportTool/settings.json`

#### Структура настроек

```json
{
  "Connection": {
    "Host": "192.168.1.100",
    "Port": 3306,
    "Username": "admin",
    "EncryptedPassword": "base64..."
  },
  "LastExportDirectory": "/path/to/exports"
}
```

**Важно**: По умолчанию все поля подключения пустые. Fallback на дефолтные значения убран для безопасности.

#### Когда сохраняются настройки подключения

Настройки подключения сохраняются **только при успешном подключении** к базе данных:

| Окно | Когда сохраняется | Что сохраняется |
|------|-------------------|-----------------|
| ConnectionWindow (ручное) | После успешного нажатия "Подключиться" | Host, Port, Username, Password (зашифрованный) |
| AutoConnectionWindow | После успешного автоподключения | **Только** Host и Port (без учётных данных) |

**Важно**: При автоподключении логин и пароль **не сохраняются** в settings.json, так как используются зашифрованные credentials из `DefaultCredentials`.

#### Архитектура

**Файлы**:
- `Infrastructure/Configuration/AppSettings.cs` - POCO-классы для настроек (`AppSettings`, `ConnectionSettings`)
- `Infrastructure/Configuration/AppSettingsJsonContext.cs` - JSON Source Generator контекст для AOT-совместимости
- `Infrastructure/Configuration/SettingsStore.cs` - статический класс для загрузки/сохранения настроек
- `Infrastructure/Services/AppSettingsService.cs` - сервис-адаптер, реализующий `IAppSettingsService`

**Использование Source Generators**:
```csharp
// JSON Source Generator контекст (compile-time сериализация)
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AppSettings))]
internal partial class AppSettingsJsonContext : JsonSerializerContext { }

// Загрузка настроек (AOT-совместимо)
var json = File.ReadAllText(settingsPath);
var settings = JsonSerializer.Deserialize(json, AppSettingsJsonContext.Default.AppSettings);

// Сохранение настроек (thread-safe)
lock (_lock)
{
    var json = JsonSerializer.Serialize(settings, AppSettingsJsonContext.Default.AppSettings);
    File.WriteAllText(settingsPath, json);
}
```

**Преимущества**:
- ✅ Полная AOT-совместимость (нет рефлексии)
- ✅ Работает с IL Trimming (`PublishTrimmed=true`)
- ✅ Thread-safe операции записи через `lock`
- ✅ Обратная совместимость с существующими файлами настроек

#### Шифрование паролей

Пароли в настройках хранятся в зашифрованном виде:
- **Windows**: DPAPI (`ProtectedData`, `DataProtectionScope.CurrentUser`)
- **Linux/macOS**: AES-256 в режиме CBC с ключом на основе имени пользователя и машины

**Важно**: Зашифрованный пароль с Windows НЕ совместим с Linux и наоборот.

### Конфигурационные файлы проекта

- **`Directory.Build.props`** - общие настройки для всех проектов (LangVersion, Nullable, метаданные)
- **`global.json`** - версия .NET SDK (10.0.0)
- **`.editorconfig`** - правила форматирования кода (отступы, стиль C#)
- **`IvkExportTool.sln`** - файл решения со всеми проектами

## Команды для разработки

### Базовые команды

```bash
# Восстановление зависимостей
dotnet restore

# Сборка проекта (Debug)
dotnet build

# Сборка проекта (Release)
dotnet build --configuration Release

# Запуск тестов
dotnet test

# Запуск приложения
dotnet run --project src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj
```

### Разработка с hot reload

```bash
# Запуск с автоматической перезагрузкой при изменении файлов
dotnet watch run --project src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj

# Запуск в Debug режиме (с Avalonia DevTools)
dotnet run --project src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj --configuration Debug
# В Debug режиме доступна отладка через F12 (Avalonia.Diagnostics)
```

### Форматирование и качество кода

```bash
# Проверка форматирования (должна проходить перед коммитом)
dotnet format --verify-no-changes

# Автоматическое форматирование
dotnet format

# Проверка с анализаторами
dotnet build --configuration Release /p:RunAnalyzers=true /p:TreatWarningsAsErrors=true
```

### Тестирование

```bash
# Запустить все тесты (unit + integration)
dotnet test

# Запустить только unit-тесты (быстро, без Docker)
dotnet test --filter "Category!=Integration"

# Запустить только интеграционные тесты (требует Docker)
dotnet test --filter "Category=Integration"

# Запустить тесты с подробным выводом
dotnet test --verbosity detailed

# Запустить конкретный тест по имени
dotnet test --filter "FullyQualifiedName~TestName"

# Запустить тесты из конкретного класса
dotnet test --filter "FullyQualifiedName~IvkExportTool.Tests.Services.MySqlDatabaseServiceTests"

# Запустить тесты с конкретным именем метода
dotnet test --filter "Name=TestConnectionAsync_ValidCredentials_ReturnsSuccess"

# Тесты с покрытием кода
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

#### Интеграционные тесты

Интеграционные тесты используют **Testcontainers** для запуска MySQL в Docker-контейнере:

**Структура**:
- `Integration/MySqlIntegrationTestBase.cs` - базовый класс с настройкой MySQL контейнера
- `Integration/MySqlDatabaseServiceIntegrationTests.cs` - тесты для `MySqlDatabaseService`
- `Integration/SqlExportServiceIntegrationTests.cs` - тесты для `SqlExportService`

**Требования**:
- Docker Desktop (Windows/macOS) или Docker Engine (Linux)
- Тесты автоматически определяют Docker endpoint

**Особенности**:
- Все интеграционные тесты помечены `[Category("Integration")]`
- Контейнер MySQL 8.0 создаётся один раз на класс тестов (`[OneTimeSetUp]`)
- Тестовые таблицы создаются/удаляются в каждом тесте

### Сборка релиза

```bash
# Windows x64
dotnet publish src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  /p:PublishSingleFile=true \
  /p:IncludeNativeLibrariesForSelfExtract=true \
  -o ./publish/win-x64

# Linux x64
dotnet publish src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  /p:PublishSingleFile=true \
  -o ./publish/linux-x64

# macOS x64
dotnet publish src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj \
  -c Release \
  -r osx-x64 \
  --self-contained true \
  /p:PublishSingleFile=true \
  -o ./publish/osx-x64
```

### Оптимизации размера исполняемого файла

В проекте включены оптимизации для уменьшения размера финальных релизов (см. `src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj:12-22`):

**IL Trimming** - удаляет неиспользуемый код из сборок:
- `PublishTrimmed=true` - включает trimming
- `TrimMode=link` - агрессивный режим (удаляет неиспользуемые члены типов)

**Компрессия Single-File** - сжимает содержимое исполняемого файла:
- `EnableCompressionInSingleFile=true` - уменьшает размер на 10-20%

**Оптимизация размера**:
- `OptimizationPreference=Size` - приоритет на минимальный размер кода
- `/p:StripSymbols=true` - удаляет отладочные символы (добавлено в CI/CD)

Ожидаемый размер релиза: **~40-70 МБ** (вместо ~100 МБ без оптимизаций).

**Важно**: Эти оптимизации применяются только для Release конфигурации и не влияют на Debug сборки.

## Соглашения о коде

### Стиль кода

- Следовать `.editorconfig` для единообразия
- **PascalCase**: классы, методы, свойства, публичные поля
- **camelCase**: параметры, локальные переменные, приватные поля
- **Interfaces**: начинаются с `I` (например, `IDatabaseService`)
- **Async методы**: всегда заканчиваются на `Async`

### Коммиты

Использовать [Conventional Commits](https://www.conventionalcommits.org/):

- `feat:` - новая функциональность
- `fix:` - исправление бага
- `docs:` - изменения в документации
- `style:` - форматирование кода
- `refactor:` - рефакторинг
- `test:` - тесты
- `chore:` - обновление зависимостей, CI/CD

### Avalonia UI специфика

- AXAML файлы используют отступ 2 пробела (определено в `.editorconfig`)
- ViewModels наследуются от `ObservableObject` (CommunityToolkit.Mvvm) или `ViewModelBase`
- Используется CommunityToolkit.Mvvm для MVVM паттерна
- Команды реализуются через `RelayCommand` и `AsyncRelayCommand`
- Включен `AvaloniaUseCompiledBindingsByDefault` для лучшей производительности (см. `.csproj`)

### Дизайн-система

При создании новых UI элементов обязательно следовать дизайн-системе из `design.md` (Material Design 3, сине-серая палитра с primary `#546e7a`).

## CI/CD

Проект использует GitHub Actions:

- **build-and-test.yml** - автоматическая сборка и тесты на push/PR
- **publish.yml** - создание релизов при создании тега версии (Windows x64, Linux x64)

### Структура build-and-test pipeline

**Job 1: build** (Ubuntu и Windows)
- Сборка проекта
- Запуск unit-тестов (`--filter "Category!=Integration"`)

**Job 2: integration-tests** (только Ubuntu, после build)
- Запуск интеграционных тестов (`--filter "Category=Integration"`)
- Docker доступен в ubuntu-latest runner

### Структура publish pipeline

Релиз создаётся только после успешного прохождения всех тестов:

```
test (ubuntu + windows)     # unit-тесты на обеих платформах
        ↓
integration-tests (ubuntu)  # интеграционные тесты с Docker
        ↓
publish (ubuntu + windows)  # сборка релизов
        ↓
create-release              # создание GitHub Release
```

**Job 1: test** (Ubuntu и Windows параллельно)
- Сборка проекта
- Запуск unit-тестов (`--filter "Category!=Integration"`)

**Job 2: integration-tests** (только Ubuntu, после test)
- Запуск интеграционных тестов (`--filter "Category=Integration"`)
- Требует Docker (доступен в ubuntu-latest runner)

**Job 3: publish** (Ubuntu и Windows параллельно, после integration-tests)
- Публикация self-contained приложения
- Создание архивов (tar.gz для Linux, zip для Windows)

**Job 4: create-release** (после publish, только для тегов)
- Создание GitHub Release с артефактами

### Создание релиза

```bash
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
# GitHub Actions автоматически создаст релиз с артефактами для Windows и Linux
```

### Процесс публикации релиза

**Извлечение версии** (см. `.github/workflows/publish.yml:86-98`):
- Версия извлекается из git-тега автоматически: `VERSION="${GITHUB_REF#refs/tags/}"`
- Если запуск без тега (например, через `workflow_dispatch`), используется версия `0.0.0-dev`
- Версия передается в процесс сборки через параметры `/p:Version`, `/p:AssemblyVersion`, `/p:FileVersion`

**Структура релизных архивов**:
- Архивы содержат исполняемый файл **сразу в корне**
- Нет вложенных директорий типа `runtime/` - пользователь может сразу запустить приложение после распаковки
- Включены файлы:
  - `IvkExportTool.exe` (Windows) или `IvkExportTool` (Linux)
- Настройки приложения хранятся отдельно в `%APPDATA%` (Windows) или `~/.config` (Linux)

**Форматы архивов**:
- Windows: `.zip` архив (создается через `Compress-Archive`)
- Linux: `.tar.gz` архив (создается через `tar -czf`)

**Именование артефактов**:
- Windows: `IvkExportTool-{VERSION}-win-x64.zip` (например, `IvkExportTool-1.0.0-win-x64.zip`)
- Linux: `IvkExportTool-{VERSION}-linux-x64.tar.gz` (например, `IvkExportTool-1.0.0-linux-x64.tar.gz`)
- Dev-сборки: `IvkExportTool-0.0.0-dev-{runtime}.{ext}`

## Важные замечания

### Безопасность

#### Шифрование учётных данных для автоподключения

Учётные данные для автоподключения хранятся в зашифрованном виде (см. `src/IvkExportTool.Core/Constants/DefaultCredentials.cs`):

**Архитектура защиты**:
- **Алгоритм**: AES-256 в режиме CBC с PKCS7 padding
- **Хранение**: credentials хранятся как `byte[][]` (IV + ciphertext для каждой пары)
- **Ключ**: собирается из 8 частей, разбросанных по коду, затем хешируется SHA256
- **Расшифровка**: lazy-загрузка при первом обращении к `DefaultCredentials.List`

**Файлы**:
- `Core/Security/CredentialProtector.cs` - класс шифрования/расшифровки
- `Core/Constants/DefaultCredentials.cs` - зашифрованные credentials
- `Core/Models/Credential.cs` - модель учётных данных

**Уровень защиты**:
- ✅ Защищает от: `strings`, `grep`, hex-редакторов, случайного просмотра
- ✅ Усложняет: статический анализ в ILSpy/dnSpy
- ⚠️ Не защищает от: отладчика с breakpoint на `Unprotect()`, дампа памяти

**Добавление новых credentials**:
1. Запустить утилиту `tools/EncryptHelper` (создать временно)
2. Добавить новую пару в массив credentials
3. Скопировать сгенерированный `byte[]` в `DefaultCredentials.EncryptedCredentials`

#### Общие правила
- Строки подключения к базам данных не должны коммититься в репозиторий
- Настройки хранятся в `%APPDATA%` (Windows) или `~/.config` (Linux), а не в папке приложения
- Не хранить пароли и ключи в коде в открытом виде

### Тестирование

Не пытайся запустить проект для проверки визуальной части. Проект сделан на технологии Avalonia и ты не сможешь получить доступ к фронт-части.