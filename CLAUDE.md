# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Описание проекта

**IvkExportTool** - кроссплатформенное портабельное приложение с GUI на Avalonia для экспорта таблиц MySQL в SQL-файлы. В текущей версии реализован экспорт в SQL формат с полной структурой и данными таблиц.

## Технологический стек

- **.NET 10.0** (SDK 10.0.0)
- **Avalonia UI 11.3.8** - кроссплатформенный GUI фреймворк
- **MySqlConnector 2.4.0** - подключение к MySQL базам данных
- **Config.Net 5.2.1** - управление конфигурацией приложения
- **CommunityToolkit.Mvvm 8.2.1** - MVVM паттерн
- **NUnit 4.2.2** - тестирование
- **FluentAssertions 8.8.0** - assertion библиотека для тестов
- **Moq 4.20.72** - мокирование зависимостей в тестах

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
│   │   ├── Configuration/           # Конфигурация (ISettingsStore, SettingsStoreFactory)
│   │   └── Services/                # Реализация сервисов (MySqlDatabaseService, SqlExportService)
│   └── IvkExportTool.Desktop/       # Presentation Layer
│       ├── ViewModels/              # MVVM ViewModels (CommunityToolkit.Mvvm)
│       ├── Views/                   # Avalonia AXAML представления
│       └── Models/                  # UI модели и обёртки
└── tests/
    └── IvkExportTool.Tests/         # Unit тесты (NUnit)
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
- UI показывает упрощенную строку состояния: детальное сообщение и время в одной строке
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
- Все окна открываются на том же мониторе с сохранением центральной позиции
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

Версия приложения отображается в заголовке главного окна через свойство `WindowTitle` в `MainWindowViewModel` (см. `src/IvkExportTool.Desktop/ViewModels/MainWindowViewModel.cs:89-97`):

- **Источник версии**: извлекается из `Assembly.GetExecutingAssembly().GetName().Version`
- **Формат отображения**: `IvkExportTool v{Major}.{Minor}.{Build} - Подключение к БД ИВК`
- **Значение по умолчанию**: `v0.0.0` (если версия не установлена)
- **Установка версии в CI/CD**: версия устанавливается при сборке релиза через параметры `/p:Version`, `/p:AssemblyVersion`, `/p:FileVersion`
- **Источник версии для релизов**: извлекается из git-тега (например, тег `v1.0.0` → версия `1.0.0`)
- **Версия для dev-сборок**: `0.0.0-dev` (при сборке без тега)

Реализация:
```csharp
public string WindowTitle
{
    get
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        var versionString = version != null ? $"v{version.Major}.{version.Minor}.{version.Build}" : "v0.0.0";
        return $"IvkExportTool {versionString} - Подключение к БД ИВК";
    }
}
```

### Система настроек приложения (Config.Net)

Приложение использует библиотеку [Config.Net](https://github.com/aloneguid/config) для хранения настроек в JSON файле.

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

#### Архитектура

**Файлы**:
- `Infrastructure/Configuration/ISettingsStore.cs` - интерфейс для Config.Net
- `Infrastructure/Configuration/SettingsStoreFactory.cs` - фабрика для создания хранилища настроек
- `Infrastructure/Services/AppSettingsService.cs` - сервис-адаптер, реализующий `IAppSettingsService`

**Использование Config.Net**:
```csharp
// Создание хранилища настроек
var settings = new ConfigurationBuilder<ISettingsStore>()
    .UseJsonFile(settingsPath)
    .Build();

// Чтение/запись настроек (автоматически сохраняется в JSON)
settings.Connection.Host = "localhost";
settings.LastExportDirectory = "/exports";
```

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
# Запустить все тесты
dotnet test

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

- **build-and-test.yml** - автоматическая сборка и тесты на push/PR (Ubuntu и Windows)
- **publish.yml** - создание релизов при создании тега версии (Windows x64, Linux x64)

### Создание релиза

```bash
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
# GitHub Actions автоматически создаст релиз с артефактами для Windows и Linux
```

### Процесс публикации релиза

**Извлечение версии** (см. `.github/workflows/publish.yml:36-48`):
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
- Windows: `IvkExportTool-win-x64.zip`
- Linux: `IvkExportTool-linux-x64.tar.gz`

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