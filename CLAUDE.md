# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Описание проекта

**IvkExportTool** - кроссплатформенное портабельное приложение с GUI на Avalonia для экспорта таблиц MySQL в SQL-файлы. В текущей версии реализован экспорт в SQL формат с полной структурой и данными таблиц.

## Технологический стек

- **.NET 10.0** (SDK 10.0.0)
- **Avalonia UI 11.3.8** - кроссплатформенный GUI фреймворк
- **MySqlConnector 2.4.0** - подключение к MySQL базам данных
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
│   │   ├── Models/                  # Бизнес-модели (ConnectionConfig, DatabaseInfo, TableInfo, ExportOptions, ExportResult)
│   │   ├── Interfaces/              # Интерфейсы сервисов (IDatabaseService, IExportService)
│   │   └── Enums/                   # Перечисления (ExportFormat, ConnectionStatus)
│   ├── IvkExportTool.Infrastructure/  # Data Access Layer
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
- Настройки сохраняются в `appsettings.json` в поле `LastExportDirectory`

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
   - События: `ConnectionSucceeded`, `BackRequested`
   - ViewModel: `ConnectionWindowViewModel`
   - Заголовок: "Подключение к БД ИВК"

3. **AutoConnectionWindow** (460x360) - окно автоподключения
   - Статус подключения и индикатор загрузки
   - Кнопки: "Отмена", "Автоподключиться"
   - Кнопка возврата на стартовое окно (слева от крестика)
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

### Конфигурационные файлы

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

Проект использует собственную дизайн-систему на основе Material Design 3 с сине-серой приглушённой палитрой. Полное описание в файле `design.md`:

- **Primary цвет**: `#546e7a` (84, 110, 122) - основные интерактивные элементы
- **Secondary цвет**: `#78909c` (120, 144, 156) - вторичные элементы
- **Тональная палитра**: 20-95 уровней для создания визуальной иерархии
- **Функциональные цвета**: Success `#4caf50`, Error `#f44336`, Warning `#ff9800`, Info `#2196f3`
- **Шрифт**: Roboto / Inter (для Avalonia)
- **Border Radius**: 6-8px для кнопок и полей, 12px для модальных окон
- **Spacing**: 8px grid система (4px, 8px, 16px, 24px, 32px, 48px, 64px)

При создании новых UI элементов обязательно следовать дизайн-системе из `design.md`.

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
  - `appsettings.json` - конфигурация приложения

**Форматы архивов**:
- Windows: `.zip` архив (создается через `Compress-Archive`)
- Linux: `.tar.gz` архив (создается через `tar -czf`)

**Именование артефактов**:
- Windows: `IvkExportTool-win-x64.zip`
- Linux: `IvkExportTool-linux-x64.tar.gz`

## Ключевые зависимости

### Core проект
- Не имеет внешних зависимостей (только .NET 10.0)

### Infrastructure проект
- `MySqlConnector` 2.4.0 - подключение к MySQL

### Desktop проект
- `Avalonia` 11.3.8 - UI фреймворк
- `Avalonia.Desktop` 11.3.8 - поддержка десктопных платформ
- `Avalonia.Themes.Fluent` 11.3.8 - Fluent дизайн тема
- `Avalonia.Controls.DataGrid` 11.3.8 - таблица данных
- `Avalonia.Fonts.Inter` 11.3.8 - шрифт Inter
- `CommunityToolkit.Mvvm` 8.2.1 - MVVM паттерн
- `Microsoft.Extensions.DependencyInjection` 9.0.10 - DI контейнер
- `Microsoft.Extensions.Configuration.Json` 9.0.10 - конфигурация

### Tests проект
- `NUnit` 4.2.2 - фреймворк для тестирования
- `Moq` 4.20.72 - мокирование
- `FluentAssertions` 8.8.0 - assertion библиотека
- `coverlet.collector` 6.0.2 - покрытие кода

## Важные замечания

### Безопасность
- Строки подключения к базам данных не должны коммититься в репозиторий
- Использовать `appsettings.Local.json` для локальных настроек (игнорируется Git)
- Не хранить пароли и ключи в коде

### Производительность
- Экспорт больших таблиц должен выполняться асинхронно
- Использовать потоковую обработку данных для минимизации потребления памяти
- Применять пагинацию для больших выборок

### Тестирование
- Целевое покрытие кода тестами > 70%
- Unit тесты для всей бизнес-логики в Core и Infrastructure
- Mock'ировать все внешние зависимости (база данных, файловая система)
