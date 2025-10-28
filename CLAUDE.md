# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Описание проекта

**IvkExportTool** - кроссплатформенное портабельное приложение с GUI на Avalonia для экспорта таблиц MySQL в различные форматы (CSV, Excel, JSON, SQL).

## Технологический стек

- **.NET 8.0**
- **Avalonia UI 11.x** - кроссплатформенный GUI фреймворк
- **MySqlConnector** - подключение к MySQL базам данных
- **ReactiveUI** - MVVM паттерн
- **EPPlus** - экспорт в Excel
- **CsvHelper** - экспорт в CSV
- **NUnit** - тестирование

## Архитектура проекта

Проект использует Clean Architecture с тремя основными слоями:

### Структура решения

```
IvkExportTool/
├── src/
│   ├── IvkExportTool.Core/          # Domain Layer
│   │   ├── Models/                  # Бизнес-модели (ConnectionConfig, TableInfo, etc.)
│   │   ├── Interfaces/              # Интерфейсы сервисов
│   │   └── Enums/                   # Перечисления (ExportFormat, ConnectionStatus)
│   ├── IvkExportTool.Infrastructure/  # Data Access Layer
│   │   ├── Services/                # Реализация сервисов (MySqlService, ExportService)
│   │   └── Exporters/               # Экспортеры для разных форматов
│   └── IvkExportTool.Desktop/       # Presentation Layer
│       ├── ViewModels/              # MVVM ViewModels (ReactiveUI)
│       ├── Views/                   # Avalonia AXAML представления
│       └── Services/                # UI-специфичные сервисы
└── tests/
    └── IvkExportTool.Tests/         # Unit тесты (NUnit)
```

### Зависимости между проектами

- `Infrastructure` → `Core` (реализует интерфейсы из Core)
- `Desktop` → `Core` + `Infrastructure` (использует оба)
- `Tests` → все проекты

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
# Использовать dev.sh скрипт (если создан)
./scripts/dev.sh

# Или напрямую
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
- ViewModels наследуются от `ReactiveObject` или `ViewModelBase`
- Используется ReactiveUI для связывания данных
- Команды реализуются через `ReactiveCommand`

## CI/CD

Проект использует GitHub Actions:

- **build-and-test.yml** - автоматическая сборка и тесты на push/PR
- **publish.yml** - создание релизов при создании тега версии
- **build-deb.yml** - сборка .deb пакета для Debian/Ubuntu
- **coverage.yml** - генерация отчетов о покрытии кода
- **dependency-update.yml** - проверка устаревших пакетов

### Создание релиза

```bash
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
# GitHub Actions автоматически создаст релиз с артефактами для всех платформ
```

## Ключевые зависимости

### Core проект
- Не имеет внешних зависимостей (только .NET 8.0)

### Infrastructure проект
- `MySqlConnector` 2.3.7+ - подключение к MySQL
- `EPPlus` 7.1.2+ - генерация Excel файлов
- `CsvHelper` 33.0.1+ - работа с CSV

### Desktop проект
- `Avalonia` 11.1.3+ - UI фреймворк
- `Avalonia.ReactiveUI` - MVVM с ReactiveUI
- `Microsoft.Extensions.DependencyInjection` 8.0+ - DI контейнер
- `Microsoft.Extensions.Configuration` 8.0+ - конфигурация

### Tests проект
- `NUnit` - фреймворк для тестирования
- `Moq` 4.20.72+ - мокирование
- `FluentAssertions` 6.12.0+ - assertion библиотека
- `coverlet.collector` 6.0.2+ - покрытие кода

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
