# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Описание проекта

**IvkExportTool** - кроссплатформенное портабельное приложение с GUI на Avalonia для экспорта таблиц MySQL в SQL-файлы. В текущей версии реализован экспорт в SQL формат с полной структурой и данными таблиц.

## Технологический стек

- **.NET 9.0**
- **Avalonia UI 11.3.6** - кроссплатформенный GUI фреймворк
- **MySqlConnector 2.4.0** - подключение к MySQL базам данных
- **CommunityToolkit.Mvvm 8.2.1** - MVVM паттерн
- **NUnit 4.2.2** - тестирование

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

### Dependency Injection

Приложение использует Microsoft.Extensions.DependencyInjection для управления зависимостями. Конфигурация происходит в `App.axaml.cs`:

```csharp
// Регистрация сервисов
services.AddSingleton<IDatabaseService, MySqlDatabaseService>();
services.AddSingleton<IExportService, SqlExportService>();

// Регистрация ViewModels
services.AddTransient<MainWindowViewModel>();
```

ViewModels получают зависимости через конструктор. Сервисы регистрируются как Singleton, ViewModels как Transient.

### Конфигурационные файлы

- **`Directory.Build.props`** - общие настройки для всех проектов (LangVersion, Nullable, метаданные)
- **`global.json`** - версия .NET SDK (9.0.0)
- **`.editorconfig`** - правила форматирования кода (отступы, стиль C#)
- **`IvkExportTool.sln`** - файл решения со всеми проектами

## Команды для разработки

### Базовые команды

```bash
# Восстановление зависимостей
dotnet restore

# Сборка проекта
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

# Запустить конкретный тест
dotnet test --filter "FullyQualifiedName~TestName"

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

- AXAML файлы используют отступ 2 пробела
- ViewModels наследуются от `ObservableObject` (CommunityToolkit.Mvvm) или `ViewModelBase`
- Используется CommunityToolkit.Mvvm для MVVM паттерна
- Команды реализуются через `RelayCommand` и `AsyncRelayCommand`
- Включен `AvaloniaUseCompiledBindingsByDefault` для лучшей производительности

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

## Ключевые зависимости

### Core проект
- Не имеет внешних зависимостей (только .NET 9.0)

### Infrastructure проект
- `MySqlConnector` 2.4.0 - подключение к MySQL

### Desktop проект
- `Avalonia` 11.3.6 - UI фреймворк
- `Avalonia.Desktop` 11.3.6 - поддержка десктопных платформ
- `Avalonia.Themes.Fluent` 11.3.6 - Fluent дизайн тема
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
