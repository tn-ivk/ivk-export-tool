# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Быстрый старт

```bash
# Сборка и запуск
dotnet build
dotnet run --project src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj

# Тесты
dotnet test                                    # все тесты
dotnet test --filter "Category!=Integration"  # только unit-тесты (быстро)
dotnet test --filter "Category=Integration"   # только интеграционные (требует Docker)

# Форматирование
dotnet format --verify-no-changes             # проверка
dotnet format                                 # автоисправление
```

## Описание проекта

**IvkExportTool** — кроссплатформенное портабельное приложение с GUI на Avalonia для экспорта таблиц MySQL в SQL-файлы. Экспорт включает полную структуру и данные таблиц.

## Технологический стек

| Компонент | Версия | Назначение |
|-----------|--------|-----------|
| .NET | 10.0 | Платформа |
| Avalonia UI | 11.3.8 | Кроссплатформенный GUI |
| MySqlConnector | 2.4.0 | Подключение к MySQL |
| CommunityToolkit.Mvvm | 8.2.1 | MVVM паттерн |
| NUnit + FluentAssertions + Moq | — | Тестирование |
| Testcontainers.MySql | 4.9.0 | Интеграционные тесты (Docker) |

**Особенности**: System.Text.Json с Source Generators для AOT-совместимой сериализации.

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
│       ├── Enums/                   # UI перечисления (StatusMessageType)
│       └── Events/                  # UI события (NotificationRequestedEventArgs)
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
- `ExportProgress` - детальная информация о прогрессе экспорта (текущая таблица, строки, процент)

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
- Общий `Stopwatch` передается через всю цепочку методов экспорта в `SqlExportService` (см. `src/IvkExportTool.Infrastructure/Services/SqlExportService.cs:22-145`)
- UI показывает упрощенную строку состояния: детальное сообщение и прошедшее время на одном уровне (без подписи "Время")
- Убрана информация о текущей таблице и количестве обработанных строк для упрощения интерфейса

### Dependency Injection

Приложение использует Microsoft.Extensions.DependencyInjection для управления зависимостями. Конфигурация происходит в `App.axaml.cs` (см. `src/IvkExportTool.Desktop/App.axaml.cs:43-51`):

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

Реализована в `App.axaml.cs` (см. `src/IvkExportTool.Desktop/App.axaml.cs:72-290`):

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

### Конфигурационные файлы

| Файл | Назначение |
|------|------------|
| `Directory.Build.props` | Общие настройки: LangVersion=latest, Nullable, метаданные |
| `global.json` | .NET SDK 10.0.0 с rollForward=latestMinor |
| `.editorconfig` | Форматирование: 4 пробела C#, 2 пробела AXAML |
| `IvkExportTool.sln` | Файл решения |

## Команды для разработки

### Основные команды

```bash
dotnet build                                    # сборка (Debug)
dotnet build -c Release                         # сборка (Release)
dotnet test                                     # все тесты
dotnet test --filter "Category!=Integration"   # unit-тесты (быстро)
dotnet test --filter "Category=Integration"    # интеграционные (Docker)
dotnet format --verify-no-changes              # проверка форматирования
dotnet format                                  # автоформатирование

# Запуск
dotnet run --project src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj

# Hot reload
dotnet watch run --project src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj

# Тесты с покрытием
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

### Тестирование

**Unit-тесты**: NUnit + FluentAssertions + Moq

**Интеграционные тесты**: Testcontainers (MySQL 8.0 в Docker)
- Помечены `[Category("Integration")]`
- Контейнер создаётся один раз на класс (`[OneTimeSetUp]`)
- Требуют Docker

**Исключения из покрытия**: Views, App, Program, ViewLocator, сгенерированный код

### Сборка релиза

```bash
# Windows x64
dotnet publish src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj \
  -c Release -r win-x64 --self-contained true \
  /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

# Linux x64
dotnet publish src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj \
  -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true
```

### Оптимизации размера (Release)

Настроены в `IvkExportTool.Desktop.csproj`:

| Опция | Эффект |
|-------|--------|
| `PublishTrimmed=true` + `TrimMode=link` | IL Trimming (удаление неиспользуемого кода) |
| `EnableCompressionInSingleFile=true` | Сжатие single-file (-10-20%) |
| `OptimizationPreference=Size` | Оптимизация по размеру |

**Ожидаемый размер**: ~40-70 МБ (вместо ~100 МБ)

## Соглашения о коде

### Именование

- **PascalCase**: классы, методы, свойства, публичные поля
- **camelCase**: параметры, локальные переменные
- **_camelCase**: приватные поля
- **I-prefix**: интерфейсы (`IDatabaseService`)
- **Async-suffix**: асинхронные методы

### Коммиты (Conventional Commits)

`feat:` | `fix:` | `docs:` | `style:` | `refactor:` | `test:` | `chore:`

### Avalonia UI

- AXAML: отступ 2 пробела
- ViewModels наследуются от `ViewModelBase` (→ `ObservableObject`)
- Команды: `[RelayCommand]` и `[AsyncRelayCommand]`
- `AvaloniaUseCompiledBindingsByDefault=true`

### Дизайн-система

Material Design 3, сине-серая палитра. Primary: `#546e7a`. См. `design.md`.

## CI/CD

GitHub Actions workflows:

| Workflow | Триггер | Этапы |
|----------|---------|-------|
| `build-and-test.yml` | push/PR на main, develop | build (ubuntu + windows) → integration-tests |
| `publish.yml` | push тега `*.*.*` | test → integration-tests → publish → create-release |

### Создание релиза

```bash
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
# GitHub Actions автоматически создаст релиз
```

**Артефакты**:
- `IvkExportTool-{VERSION}-win-x64.zip`
- `IvkExportTool-{VERSION}-linux-x64.tar.gz`
- Dev-сборки: версия `0.0.0-dev`

**Версионирование**: извлекается из git-тега, передаётся через `/p:Version`, `/p:AssemblyVersion`, `/p:FileVersion`

## Важные замечания

### Безопасность

**Шифрование credentials для автоподключения** (`Core/Security/CredentialProtector.cs`, `Core/Constants/DefaultCredentials.cs`):
- AES-256 CBC + PKCS7
- Ключ собирается из 8 частей, хешируется SHA256
- Lazy-загрузка при первом обращении

**Защищает от**: `strings`, `grep`, hex-редакторы, ILSpy
**Не защищает от**: отладчик с breakpoint, дамп памяти

**Общие правила**:
- Настройки хранятся в `%APPDATA%` (Windows) / `~/.config` (Linux)
- Не коммитить строки подключения

### Ограничения

**Avalonia UI**: Нет возможности проверить визуальную часть — приложение требует GUI среду.

## Технический долг

Перед внесением изменений ознакомься с документами технического долга:

| Документ | Описание |
|----------|----------|
| `tech_debt/CODE_REVIEW.md` | Детальный обзор кода с приоритизированным списком проблем (P0-P3) |
| `tech-debt/large-table-export-optimization.md` | План оптимизации экспорта больших таблиц (частично выполнен) |

### Известные критические проблемы (P0)

1. **SQL-инъекция** в `SqlExportService.cs` — имена таблиц подставляются без валидации
2. **Утечка памяти** — подписки на события не отписываются при смене фильтра таблиц
3. **Race condition** в `SettingsStore.cs` — `Load()` без блокировки
4. **Недостающие AXAML ресурсы** — `SurfaceContainerHighBrush`, `OnSurfaceVariantBrush`

### Выполненные оптимизации экспорта (2025-11-10)

- ✅ Потоковая запись без накопления `List<string>` в памяти
- ✅ `StringBuilder` для экранирования строк
- ✅ Увеличен буфер `StreamWriter` до 64KB
- ✅ Детальный прогресс экспорта (`ExportProgress`)

Подробности см. в `tech_debt/CODE_REVIEW.md`. Номера строк могут быть неактуальны — используй поиск по ключевым словам.