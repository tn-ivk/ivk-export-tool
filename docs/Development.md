# Руководство разработчика

Это руководство поможет вам настроить окружение для разработки **IvkExportTool** и понять процесс внесения изменений.

## Содержание

- [Настройка окружения](#настройка-окружения)
- [Структура проекта](#структура-проекта)
- [Работа с кодом](#работа-с-кодом)
- [Тестирование](#тестирование)
- [Отладка](#отладка)
- [Соглашения о коде](#соглашения-о-коде)
- [Рабочий процесс](#рабочий-процесс)
- [CI/CD](#cicd)
- [Публикация релизов](#публикация-релизов)

## Настройка окружения

### Требования

- **.NET 10.0 SDK** ([скачать](https://dotnet.microsoft.com/download/dotnet/10.0))
- **Git** ([скачать](https://git-scm.com/downloads))
- **IDE** (на выбор):
  - [Visual Studio 2022](https://visualstudio.microsoft.com/) (Community, Professional, или Enterprise)
  - [JetBrains Rider](https://www.jetbrains.com/rider/)
  - [Visual Studio Code](https://code.visualstudio.com/) + C# Extension
- **MySQL Server** 5.7+ для тестирования подключений

### Клонирование репозитория

```bash
git clone https://github.com/YOUR_USERNAME/ivk-export-tool.git
cd ivk-export-tool
```

### Восстановление зависимостей

```bash
dotnet restore
```

### Проверка сборки

```bash
dotnet build --configuration Debug
```

Ожидаемый результат:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Запуск приложения

```bash
dotnet run --project src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj
```

### Запуск тестов

```bash
dotnet test
```

## Структура проекта

```
IvkExportTool/
├── .github/
│   └── workflows/          # GitHub Actions CI/CD
│       ├── build-and-test.yml
│       └── publish.yml
├── src/
│   ├── IvkExportTool.Core/           # Domain Layer
│   │   ├── Models/                   # Бизнес-модели
│   │   │   ├── ConnectionConfig.cs
│   │   │   ├── DatabaseInfo.cs
│   │   │   ├── TableInfo.cs
│   │   │   ├── ExportOptions.cs
│   │   │   └── ExportResult.cs
│   │   ├── Interfaces/               # Интерфейсы сервисов
│   │   │   ├── IDatabaseService.cs
│   │   │   ├── IExportService.cs
│   │   │   └── IAppSettingsService.cs
│   │   └── Enums/                    # Перечисления
│   │       ├── ExportFormat.cs
│   │       └── ConnectionStatus.cs
│   ├── IvkExportTool.Infrastructure/  # Data Access Layer
│   │   └── Services/                  # Реализация сервисов
│   │       ├── MySqlDatabaseService.cs
│   │       ├── SqlExportService.cs
│   │       └── AppSettingsService.cs
│   └── IvkExportTool.Desktop/         # Presentation Layer
│       ├── ViewModels/                # MVVM ViewModels
│       │   ├── ConnectionWindowViewModel.cs
│       │   ├── MainWindowViewModel.cs
│       │   ├── TableItemViewModel.cs
│       │   └── ViewModelBase.cs
│       ├── Views/                     # Avalonia AXAML
│       │   ├── ConnectionWindow.axaml
│       │   └── MainWindow.axaml
│       ├── Models/                    # UI модели
│       ├── App.axaml                  # Точка входа приложения
│       └── Program.cs                 # Main()
├── tests/
│   └── IvkExportTool.Tests/           # Unit тесты
│       ├── Services/
│       │   ├── MySqlDatabaseServiceTests.cs
│       │   └── SqlExportServiceTests.cs
│       └── ViewModels/
│           └── MainWindowViewModelTests.cs
├── .editorconfig                      # Правила форматирования
├── Directory.Build.props              # Общие настройки MSBuild
├── global.json                        # Версия .NET SDK
├── IvkExportTool.sln                  # Solution файл
└── README.md
```

## Работа с кодом

### Команды для разработки

**Восстановление зависимостей**:

```bash
dotnet restore
```

**Сборка проекта**:

```bash
# Debug (для разработки)
dotnet build --configuration Debug

# Release (для продакшена)
dotnet build --configuration Release
```

**Запуск приложения**:

```bash
# Обычный запуск
dotnet run --project src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj

# С автоматической перезагрузкой (hot reload)
dotnet watch run --project src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj
```

**Запуск в Debug режиме** (с Avalonia DevTools):

```bash
dotnet run --project src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj --configuration Debug
```

В Debug режиме доступна отладка через **F12** (Avalonia.Diagnostics).

### Форматирование кода

Проект использует `.editorconfig` для единообразия кода.

**Проверка форматирования**:

```bash
dotnet format --verify-no-changes
```

**Автоматическое форматирование**:

```bash
dotnet format
```

> **Важно**: Запускайте `dotnet format` перед коммитом!

### Анализ кода

**Проверка с анализаторами**:

```bash
dotnet build --configuration Release /p:RunAnalyzers=true /p:TreatWarningsAsErrors=true
```

### Работа с зависимостями

**Добавление пакета**:

```bash
dotnet add src/IvkExportTool.Core/ package PackageName
```

**Обновление пакета**:

```bash
dotnet add src/IvkExportTool.Core/ package PackageName --version 1.2.3
```

**Список устаревших пакетов**:

```bash
dotnet list package --outdated
```

## Тестирование

### Структура тестов

Проект использует **NUnit** для unit тестов:

```csharp
using NUnit.Framework;
using FluentAssertions;
using Moq;

namespace IvkExportTool.Tests.Services;

[TestFixture]
public class MySqlDatabaseServiceTests
{
    [Test]
    public async Task TestConnectionAsync_ValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var service = new MySqlDatabaseService();
        var config = new ConnectionConfig { ... };

        // Act
        var result = await service.TestConnectionAsync(config);

        // Assert
        result.Should().BeTrue();
    }
}
```

### Запуск тестов

**Все тесты**:

```bash
dotnet test
```

**С подробным выводом**:

```bash
dotnet test --verbosity detailed
```

**Конкретный тест**:

```bash
dotnet test --filter "FullyQualifiedName~MySqlDatabaseServiceTests"
```

**По имени метода**:

```bash
dotnet test --filter "Name=TestConnectionAsync_ValidCredentials_ReturnsSuccess"
```

### Покрытие кода тестами

**Запуск с покрытием**:

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

**Просмотр отчёта** (требует `reportgenerator`):

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:coverage.opencover.xml -targetdir:coverage-report
```

Откройте `coverage-report/index.html` в браузере.

### Мокирование зависимостей

Используйте **Moq** для мокирования:

```csharp
var mockDatabaseService = new Mock<IDatabaseService>();
mockDatabaseService
    .Setup(x => x.GetTablesAsync(It.IsAny<string>()))
    .ReturnsAsync(new List<TableInfo> { ... });
```

## Отладка

### Visual Studio 2022

1. Откройте `IvkExportTool.sln`
2. Установите точки останова (F9)
3. Нажмите **F5** для запуска с отладкой

### JetBrains Rider

1. Откройте `IvkExportTool.sln`
2. Установите точки останова (Ctrl+F8)
3. Нажмите **Shift+F9** для запуска с отладкой

### Visual Studio Code

1. Откройте папку проекта
2. Установите расширение **C# Dev Kit**
3. Используйте **F5** для запуска с отладкой

**launch.json** (если требуется):

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (Desktop)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/IvkExportTool.Desktop/bin/Debug/net10.0/IvkExportTool.Desktop.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/IvkExportTool.Desktop",
      "stopAtEntry": false,
      "console": "internalConsole"
    }
  ]
}
```

### Avalonia DevTools

В Debug режиме доступна отладка UI через **Avalonia DevTools**:

1. Запустите приложение в Debug режиме
2. Нажмите **F12** в окне приложения
3. Откроется окно DevTools с:
   - Visual Tree Explorer
   - Properties Inspector
   - Events Logger

## Соглашения о коде

### Именование

- **PascalCase**: Классы, методы, свойства, публичные поля
- **camelCase**: Параметры, локальные переменные, приватные поля
- **Interfaces**: Начинаются с `I` (например, `IDatabaseService`)
- **Async методы**: Заканчиваются на `Async` (например, `GetTablesAsync`)

### Примеры

```csharp
// ✅ Правильно
public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;

    public async Task<List<TableInfo>> GetTablesAsync(string databaseName)
    {
        var tables = new List<TableInfo>();
        // ...
        return tables;
    }
}

// ❌ Неправильно
public class databaseService  // не PascalCase
{
    private readonly string connectionString;  // нет префикса _

    public async Task<List<TableInfo>> GetTables(string database_name)  // не Async
    {
        // ...
    }
}
```

### Структура файлов

- **Один класс = один файл**
- **Имя файла = имя класса** (например, `TableInfo.cs`)
- **Пространства имён соответствуют структуре папок**

```csharp
// src/IvkExportTool.Core/Models/TableInfo.cs
namespace IvkExportTool.Core.Models;

public class TableInfo
{
    // ...
}
```

### AXAML файлы

- Используйте **2 пробела** для отступов (настроено в `.editorconfig`)
- Следуйте [дизайн-системе](../design.md)

```xml
<Window xmlns="https://github.com/avaloniaui"
        Title="IvkExportTool"
        Width="1200" Height="800">
  <Grid Margin="16">
    <TextBlock Text="Hello World" />
  </Grid>
</Window>
```

## Рабочий процесс

### Workflow разработки

1. **Создайте ветку**:

```bash
git checkout -b feature/my-new-feature
```

2. **Внесите изменения**

3. **Запустите тесты**:

```bash
dotnet test
```

4. **Проверьте форматирование**:

```bash
dotnet format --verify-no-changes
```

5. **Закоммитьте изменения**:

```bash
git add .
git commit -m "feat: add new feature"
```

6. **Отправьте в репозиторий**:

```bash
git push origin feature/my-new-feature
```

7. **Создайте Pull Request**

### Conventional Commits

Используйте [Conventional Commits](https://www.conventionalcommits.org/):

| Тип | Описание | Пример |
|-----|----------|--------|
| `feat:` | Новая функциональность | `feat: add CSV export format` |
| `fix:` | Исправление бага | `fix: resolve connection timeout issue` |
| `docs:` | Документация | `docs: update API reference` |
| `style:` | Форматирование кода | `style: apply editorconfig rules` |
| `refactor:` | Рефакторинг | `refactor: simplify export logic` |
| `test:` | Тесты | `test: add unit tests for ExportService` |
| `chore:` | Обслуживание | `chore: update dependencies` |
| `ci:` | CI/CD | `ci: add GitHub Actions workflow` |

### Branching Strategy

- **`main`** — стабильная ветка (только релизы)
- **`develop`** — ветка разработки (опционально)
- **`feature/*`** — новые функции
- **`bugfix/*`** — исправления багов
- **`hotfix/*`** — срочные исправления

## CI/CD

### GitHub Actions

Проект использует два workflow:

#### 1. Build and Test

Файл: `.github/workflows/build-and-test.yml`

Триггеры:
- Push в `main` или `develop`
- Pull Request

Действия:
- Сборка проекта (Debug и Release)
- Запуск всех тестов
- Проверка форматирования кода

#### 2. Publish Release

Файл: `.github/workflows/publish.yml`

Триггеры:
- Создание тега версии (например, `v1.0.0`)

Действия:
- Сборка релизов для Windows и Linux
- Создание артефактов (ZIP/TAR.GZ)
- Публикация GitHub Release

### Локальная проверка CI/CD

Перед отправкой кода убедитесь, что он пройдёт CI:

```bash
# Сборка
dotnet build --configuration Release

# Тесты
dotnet test

# Форматирование
dotnet format --verify-no-changes

# Анализаторы
dotnet build --configuration Release /p:RunAnalyzers=true /p:TreatWarningsAsErrors=true
```

## Публикация релизов

### Процесс создания релиза

1. **Обновите версию** в `Directory.Build.props`:

```xml
<PropertyGroup>
  <Version>1.2.0</Version>
</PropertyGroup>
```

2. **Обновите CHANGELOG** (если есть)

3. **Закоммитьте изменения**:

```bash
git add .
git commit -m "chore: bump version to 1.2.0"
git push origin main
```

4. **Создайте и отправьте тег**:

```bash
git tag -a v1.2.0 -m "Release version 1.2.0"
git push origin v1.2.0
```

5. **GitHub Actions автоматически**:
   - Соберёт релизы
   - Создаст GitHub Release
   - Приложит артефакты

### Ручная сборка релиза

**Windows x64**:

```bash
dotnet publish src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  /p:PublishSingleFile=true \
  /p:IncludeNativeLibrariesForSelfExtract=true \
  /p:StripSymbols=true \
  -o ./publish/win-x64
```

**Linux x64**:

```bash
dotnet publish src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  /p:PublishSingleFile=true \
  /p:StripSymbols=true \
  -o ./publish/linux-x64
```

### Оптимизации размера

В проекте включены оптимизации:

- **PublishTrimmed** — удаление неиспользуемого кода
- **TrimMode=link** — агрессивный trimming
- **EnableCompressionInSingleFile** — сжатие
- **OptimizationPreference=Size** — приоритет на размер
- **StripSymbols** — удаление отладочных символов

Результат: **~40-70 MB** вместо ~100 MB.

## Полезные ресурсы

- [Avalonia Documentation](https://docs.avaloniaui.net/)
- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [NUnit Documentation](https://docs.nunit.org/)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [Conventional Commits](https://www.conventionalcommits.org/)

---

**Следующие шаги**: Изучите [архитектуру проекта](Architecture.md) для понимания внутреннего устройства приложения.
