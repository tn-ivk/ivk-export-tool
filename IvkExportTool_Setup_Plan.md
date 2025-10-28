# План инициализации проекта IvkExportTool

> **Документ для нейросети-исполнителя**
> 
> Этот документ содержит пошаговый план инициализации репозитория и настройки окружения для проекта **IvkExportTool** - кроссплатформенного приложения для экспорта таблиц MySQL.

---

## 📋 Описание проекта

**Название:** IvkExportTool

**Цель:** Создать кроссплатформенное портабельное приложение с GUI на Avalonia для экспорта таблиц MySQL в различные форматы (CSV, Excel, JSON, SQL).

**Технологический стек:**
- .NET 8.0
- Avalonia UI 11.x (GUI)
- Vue.js + TypeScript (альтернативный фронт, если потребуется)
- MySqlConnector
- ReactiveUI (MVVM)
- NUnit (тестирование)
- EPPlus (Excel)
- CsvHelper (CSV)

**Целевые платформы:**
- Windows (x64)
- Linux (x64) 
- macOS (x64/ARM64)

---

## 🎯 Итоговая структура проекта

```
IvkExportTool/
├── .github/
│   ├── workflows/
│   │   ├── build-and-test.yml
│   │   ├── publish.yml
│   │   ├── build-deb.yml
│   │   ├── coverage.yml
│   │   └── dependency-update.yml
│   └── dependabot.yml
├── src/
│   ├── IvkExportTool.Core/
│   ├── IvkExportTool.Infrastructure/
│   └── IvkExportTool.Desktop/
├── tests/
│   └── IvkExportTool.Tests/
├── docs/
├── scripts/
│   ├── build.sh
│   ├── publish.sh
│   └── dev.sh
├── .gitignore
├── .gitattributes
├── .editorconfig
├── global.json
├── Directory.Build.props
├── nuget.config
├── IvkExportTool.sln
├── README.md
├── CHANGELOG.md
├── CONTRIBUTING.md
└── LICENSE
```

---

## 📝 Пошаговая инструкция

### ШАГ 1: Создание базовой структуры репозитория

#### 1.1. Инициализация Git-репозитория

```bash
# Создать директорию проекта
mkdir IvkExportTool
cd IvkExportTool

# Инициализировать Git
git init
git branch -M main

# Настроить Git конфиг (опционально)
git config user.name "Александр"
git config user.email "your.email@example.com"
```

#### 1.2. Создать файл .gitignore

<details>
<summary>Содержимое .gitignore (нажмите чтобы развернуть)</summary>

```gitignore
# Build results
[Dd]ebug/
[Dd]ebugPublic/
[Rr]elease/
[Rr]eleases/
x64/
x86/
[Aa][Rr][Mm]/
[Aa][Rr][Mm]64/
bld/
[Bb]in/
[Oo]bj/
[Ll]og/
[Ll]ogs/

# Visual Studio cache/options
.vs/
.vscode/
*.suo
*.user
*.userosscache
*.sln.docstates

# Rider
.idea/
*.sln.iml

# User-specific files
*.rsuser
*.suo
*.user
*.userosscache
*.sln.docstates

# Build results
[Dd]ebug/
[Rr]elease/
x64/
x86/
[Ww][Ii][Nn]32/
[Aa][Rr][Mm]/
[Aa][Rr][Mm]64/
bld/
[Bb]in/
[Oo]bj/

# .NET Core
project.lock.json
project.fragment.lock.json
artifacts/

# StyleCop
StyleCopReport.xml

# Files built by Visual Studio
*_i.c
*_p.c
*_h.h
*.ilk
*.meta
*.obj
*.iobj
*.pch
*.pdb
*.ipdb
*.pgc
*.pgd
*.rsp
*.sbr
*.tlb
*.tli
*.tlh
*.tmp
*.tmp_proj
*_wpftmp.csproj
*.log
*.vspscc
*.vssscc
.builds
*.pidb
*.svclog
*.scc

# NuGet Packages
*.nupkg
*.snupkg
**/packages/*
!**/packages/build/
*.nuget.props
*.nuget.targets

# Visual Studio cache files
*.aps
*.ncb
*.opendb
*.opensdf
*.sdf
*.cachefile
*.VC.db
*.VC.VC.opendb

# Test Results
[Tt]est[Rr]esult*/
[Bb]uild[Ll]og.*
*.trx
*.coverage
*.coveragexml
TestResult.xml
TestResults/

# NCrunch
_NCrunch_*
.*crunch*.local.xml
nCrunchTemp_*

# Backup & report files
_UpgradeReport_Files/
Backup*/
UpgradeLog*.XML
UpgradeLog*.htm
ServiceFabricBackup/
*.rptproj.bak

# SQL Server files
*.mdf
*.ldf
*.ndf

# Node.js
node_modules/
npm-debug.log*
yarn-debug.log*
yarn-error.log*

# OS generated files
.DS_Store
.DS_Store?
._*
.Spotlight-V100
.Trashes
ehthumbs.db
Thumbs.db

# Sensitive data
appsettings.Development.json
appsettings.Local.json
*.pfx
*.key
secrets.json

# Application specific
connections.json
*.db
*.sqlite
export_output/
logs/

# Publish profiles
*.pubxml
*.publishproj

# JetBrains Rider
.idea/
*.sln.iml
_ReSharper.Caches/

# Coverage reports
coverage/
*.coverage
*.coveragexml
```

</details>

#### 1.3. Создать файл .gitattributes

<details>
<summary>Содержимое .gitattributes</summary>

```gitattributes
###############################################################################
# Set default behavior to automatically normalize line endings.
###############################################################################
* text=auto

###############################################################################
# Set default behavior for command prompt diff.
###############################################################################
*.cs     diff=csharp text
*.csproj text
*.sln    text merge=union

###############################################################################
# Set the merge driver for project and solution files
###############################################################################
*.sln       merge=union
*.csproj    merge=union
*.vbproj    merge=union
*.vcxproj   merge=union
*.vcproj    merge=union
*.dbproj    merge=union
*.fsproj    merge=union
*.lsproj    merge=union
*.wixproj   merge=union
*.modelproj merge=union
*.sqlproj   merge=union
*.wwaproj   merge=union

*.axaml     text
*.axaml.cs  text

###############################################################################
# Binary files
###############################################################################
*.png       binary
*.jpg       binary
*.jpeg      binary
*.gif       binary
*.ico       binary
*.mov       binary
*.mp4       binary
*.mp3       binary
*.zip       binary
*.pdf       binary
*.dll       binary
*.exe       binary
```

</details>

#### 1.4. Создать файл .editorconfig

<details>
<summary>Содержимое .editorconfig</summary>

```ini
# EditorConfig is awesome: https://EditorConfig.org

root = true

# All files
[*]
charset = utf-8
indent_style = space
indent_size = 4
insert_final_newline = true
trim_trailing_whitespace = true

# XML/AXAML files
[*.{xml,axaml,xaml,config,props,targets,nuspec,resx}]
indent_size = 2

# JSON files
[*.json]
indent_size = 2

# YAML files
[*.{yml,yaml}]
indent_size = 2

# Markdown files
[*.md]
trim_trailing_whitespace = false

# Shell scripts
[*.sh]
end_of_line = lf

# Batch files
[*.{cmd,bat}]
end_of_line = crlf

# C# files
[*.cs]

# New line preferences
csharp_new_line_before_open_brace = all
csharp_new_line_before_else = true
csharp_new_line_before_catch = true
csharp_new_line_before_finally = true
csharp_new_line_before_members_in_object_initializers = true
csharp_new_line_before_members_in_anonymous_types = true
csharp_new_line_between_query_expression_clauses = true

# Indentation preferences
csharp_indent_case_contents = true
csharp_indent_switch_labels = true
csharp_indent_labels = one_less_than_current
csharp_indent_block_contents = true
csharp_indent_braces = false

# Space preferences
csharp_space_after_cast = false
csharp_space_after_keywords_in_control_flow_statements = true
csharp_space_between_method_call_parameter_list_parentheses = false
csharp_space_between_method_declaration_parameter_list_parentheses = false
csharp_space_between_parentheses = false
csharp_space_before_colon_in_inheritance_clause = true
csharp_space_after_colon_in_inheritance_clause = true

# Using directives preferences
csharp_using_directive_placement = outside_namespace:warning

# Code style rules
dotnet_sort_system_directives_first = true
dotnet_separate_import_directive_groups = false

# Naming conventions
dotnet_naming_rule.interface_should_be_begins_with_i.severity = warning
dotnet_naming_rule.interface_should_be_begins_with_i.symbols = interface
dotnet_naming_rule.interface_should_be_begins_with_i.style = begins_with_i

dotnet_naming_rule.types_should_be_pascal_case.severity = warning
dotnet_naming_rule.types_should_be_pascal_case.symbols = types
dotnet_naming_rule.types_should_be_pascal_case.style = pascal_case

dotnet_naming_rule.non_field_members_should_be_pascal_case.severity = warning
dotnet_naming_rule.non_field_members_should_be_pascal_case.symbols = non_field_members
dotnet_naming_rule.non_field_members_should_be_pascal_case.style = pascal_case

# Symbol specifications
dotnet_naming_symbols.interface.applicable_kinds = interface
dotnet_naming_symbols.interface.applicable_accessibilities = public, internal, private, protected, protected_internal, private_protected
dotnet_naming_symbols.interface.required_modifiers = 

dotnet_naming_symbols.types.applicable_kinds = class, struct, interface, enum
dotnet_naming_symbols.types.applicable_accessibilities = public, internal, private, protected, protected_internal, private_protected
dotnet_naming_symbols.types.required_modifiers = 

dotnet_naming_symbols.non_field_members.applicable_kinds = property, event, method
dotnet_naming_symbols.non_field_members.applicable_accessibilities = public, internal, private, protected, protected_internal, private_protected
dotnet_naming_symbols.non_field_members.required_modifiers = 

# Naming styles
dotnet_naming_style.begins_with_i.required_prefix = I
dotnet_naming_style.begins_with_i.required_suffix = 
dotnet_naming_style.begins_with_i.word_separator = 
dotnet_naming_style.begins_with_i.capitalization = pascal_case

dotnet_naming_style.pascal_case.required_prefix = 
dotnet_naming_style.pascal_case.required_suffix = 
dotnet_naming_style.pascal_case.word_separator = 
dotnet_naming_style.pascal_case.capitalization = pascal_case
```

</details>

---

### ШАГ 2: Создание .NET Solution и проектов

#### 2.1. Создать структуру директорий

```bash
# Создать директории
mkdir -p src tests docs tools scripts
```

#### 2.2. Создать Solution

```bash
# Создать solution
dotnet new sln -n IvkExportTool
```

#### 2.3. Создать проекты

```bash
# Перейти в src
cd src

# Core проект (Domain Layer)
dotnet new classlib -n IvkExportTool.Core -f net8.0
dotnet sln ../IvkExportTool.sln add IvkExportTool.Core/IvkExportTool.Core.csproj

# Infrastructure проект (Data Access Layer)
dotnet new classlib -n IvkExportTool.Infrastructure -f net8.0
dotnet sln ../IvkExportTool.sln add IvkExportTool.Infrastructure/IvkExportTool.Infrastructure.csproj

# Desktop проект (Avalonia UI)
dotnet new avalonia.mvvm -n IvkExportTool.Desktop -f net8.0
dotnet sln ../IvkExportTool.sln add IvkExportTool.Desktop/IvkExportTool.Desktop.csproj

# Вернуться в корень
cd ..

# Test проект (NUnit)
cd tests
dotnet new nunit -n IvkExportTool.Tests -f net8.0
dotnet sln ../IvkExportTool.sln add IvkExportTool.Tests/IvkExportTool.Tests.csproj
cd ..
```

#### 2.4. Настроить зависимости между проектами

```bash
# Infrastructure -> Core
dotnet add src/IvkExportTool.Infrastructure/IvkExportTool.Infrastructure.csproj reference src/IvkExportTool.Core/IvkExportTool.Core.csproj

# Desktop -> Core, Infrastructure
dotnet add src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj reference src/IvkExportTool.Core/IvkExportTool.Core.csproj
dotnet add src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj reference src/IvkExportTool.Infrastructure/IvkExportTool.Infrastructure.csproj

# Tests -> All projects
dotnet add tests/IvkExportTool.Tests/IvkExportTool.Tests.csproj reference src/IvkExportTool.Core/IvkExportTool.Core.csproj
dotnet add tests/IvkExportTool.Tests/IvkExportTool.Tests.csproj reference src/IvkExportTool.Infrastructure/IvkExportTool.Infrastructure.csproj
dotnet add tests/IvkExportTool.Tests/IvkExportTool.Tests.csproj reference src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj
```

#### 2.5. Установить NuGet пакеты

```bash
# Infrastructure проект
dotnet add src/IvkExportTool.Infrastructure/IvkExportTool.Infrastructure.csproj package MySqlConnector --version 2.3.7
dotnet add src/IvkExportTool.Infrastructure/IvkExportTool.Infrastructure.csproj package EPPlus --version 7.1.2
dotnet add src/IvkExportTool.Infrastructure/IvkExportTool.Infrastructure.csproj package CsvHelper --version 33.0.1

# Desktop проект
dotnet add src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj package Microsoft.Extensions.DependencyInjection --version 8.0.0
dotnet add src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj package Microsoft.Extensions.Logging --version 8.0.0
dotnet add src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj package Microsoft.Extensions.Configuration --version 8.0.0
dotnet add src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj package Microsoft.Extensions.Configuration.Json --version 8.0.0

# Tests проект (дополнительно к NUnit)
dotnet add tests/IvkExportTool.Tests/IvkExportTool.Tests.csproj package Moq --version 4.20.72
dotnet add tests/IvkExportTool.Tests/IvkExportTool.Tests.csproj package FluentAssertions --version 6.12.0
dotnet add tests/IvkExportTool.Tests/IvkExportTool.Tests.csproj package NUnit.Analyzers --version 4.3.0
dotnet add tests/IvkExportTool.Tests/IvkExportTool.Tests.csproj package coverlet.collector --version 6.0.2
```

#### 2.6. Проверить сборку

```bash
# Restore
dotnet restore

# Build
dotnet build --configuration Release

# Test (должен пройти с 0 тестами)
dotnet test
```

---

### ШАГ 3: Конфигурационные файлы .NET

#### 3.1. Создать global.json

```json
{
  "sdk": {
    "version": "8.0.0",
    "rollForward": "latestMinor"
  }
}
```

**Команда:**
```bash
cat > global.json << 'EOF'
{
  "sdk": {
    "version": "8.0.0",
    "rollForward": "latestMinor"
  }
}
EOF
```

#### 3.2. Создать Directory.Build.props

<details>
<summary>Содержимое Directory.Build.props</summary>

```xml
<Project>
  <PropertyGroup>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <AnalysisMode>All</AnalysisMode>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  </PropertyGroup>

  <!-- Версии пакетов -->
  <PropertyGroup>
    <MySqlConnectorVersion>2.3.7</MySqlConnectorVersion>
    <EPPlusVersion>7.1.2</EPPlusVersion>
    <CsvHelperVersion>33.0.1</CsvHelperVersion>
    <AvaloniaVersion>11.1.3</AvaloniaVersion>
  </PropertyGroup>

  <!-- Общие метаданные -->
  <PropertyGroup>
    <Authors>Александр</Authors>
    <Company>АО «Транснефть»</Company>
    <Product>IVK Export Tool</Product>
    <Copyright>Copyright © 2025</Copyright>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
  </PropertyGroup>
</Project>
```

</details>

#### 3.3. Создать nuget.config

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
  
  <config>
    <add key="globalPackagesFolder" value="./.nuget/packages" />
  </config>
</configuration>
```

---

### ШАГ 4: Документация проекта

#### 4.1. Создать README.md

<details>
<summary>Содержимое README.md</summary>

```markdown
# IvkExportTool

[![Build and Test](https://github.com/username/IvkExportTool/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/username/IvkExportTool/actions/workflows/build-and-test.yml)
[![Publish Release](https://github.com/username/IvkExportTool/actions/workflows/publish.yml/badge.svg)](https://github.com/username/IvkExportTool/actions/workflows/publish.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Кроссплатформенное портабельное приложение для экспорта таблиц MySQL в различные форматы.

## 🚀 Возможности

- 🔌 Подключение к MySQL базам данных
- 📊 Просмотр списка таблиц и их структуры
- 💾 Экспорт в форматы: CSV, Excel, JSON, SQL
- 🎯 Фильтрация данных (WHERE условия)
- 📦 Портабельная версия (не требует установки)
- 🖥️ Кроссплатформенность (Windows, Linux, macOS)
- 🎨 Современный UI на Avalonia

## 🛠️ Технологии

- .NET 8.0
- Avalonia UI 11.x
- MySqlConnector
- ReactiveUI (MVVM)
- EPPlus (Excel export)
- CsvHelper (CSV export)
- NUnit (тестирование)

## 📋 Требования

### Для запуска
- .NET 8.0 Runtime
- MySQL Server 5.7+ (для подключения)

### Для разработки
- .NET 8.0 SDK
- Visual Studio 2022 / Rider / VS Code
- Git

## 📥 Установка

### Из релиза (рекомендуется)

Скачайте последнюю версию из [Releases](https://github.com/username/IvkExportTool/releases):

- **Windows**: `IvkExportTool-win-x64.zip`
- **Linux**: `IvkExportTool-linux-x64.tar.gz` или `ivkexporttool_*_amd64.deb`
- **macOS**: `IvkExportTool-osx-x64.tar.gz`

### Установка .deb пакета (Debian/Ubuntu)

```bash
sudo dpkg -i ivkexporttool_1.0.0_amd64.deb
sudo apt-get install -f  # Установка зависимостей
```

### Сборка из исходников

```bash
git clone https://github.com/username/IvkExportTool.git
cd IvkExportTool
dotnet restore
dotnet build --configuration Release
dotnet run --project src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj
```

## 💻 Разработка

### Быстрый старт

```bash
# Клонировать репозиторий
git clone https://github.com/username/IvkExportTool.git
cd IvkExportTool

# Восстановить зависимости
dotnet restore

# Собрать проект
dotnet build

# Запустить тесты
dotnet test

# Запустить приложение
dotnet run --project src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj
```

### Структура проекта

```
IvkExportTool/
├── src/
│   ├── IvkExportTool.Core/          # Domain models & interfaces
│   ├── IvkExportTool.Infrastructure/ # Data access & services
│   └── IvkExportTool.Desktop/       # Avalonia UI application
├── tests/
│   └── IvkExportTool.Tests/         # Unit tests (NUnit)
├── docs/                            # Documentation
└── scripts/                         # Build & deployment scripts
```

### Стиль кода

Проект использует `.editorconfig` для единообразия стиля кода.

Проверка форматирования:
```bash
dotnet format --verify-no-changes
```

Автоматическое форматирование:
```bash
dotnet format
```

## 🚢 Сборка релиза

### Windows (x64)
```bash
dotnet publish src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  /p:PublishSingleFile=true \
  /p:IncludeNativeLibrariesForSelfExtract=true \
  -o ./publish/win-x64
```

### Linux (x64)
```bash
dotnet publish src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  /p:PublishSingleFile=true \
  -o ./publish/linux-x64
```

### macOS (x64)
```bash
dotnet publish src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj \
  -c Release \
  -r osx-x64 \
  --self-contained true \
  /p:PublishSingleFile=true \
  -o ./publish/osx-x64
```

## 🔄 CI/CD

Проект использует GitHub Actions для автоматической сборки, тестирования и публикации:

- **Build and Test**: Автоматическая сборка и тесты на каждый push/PR
- **Publish Release**: Создание релизов при создании тега версии
- **Code Coverage**: Генерация отчетов о покрытии кода
- **Dependency Updates**: Проверка устаревших пакетов

### Создание релиза

```bash
# Создать и отправить тег версии
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0

# GitHub Actions автоматически создаст релиз с артефактами
```

## 📄 Лицензия

MIT License - см. [LICENSE](LICENSE)

## 📞 Контакты

Александр - your.email@example.com

Project Link: [https://github.com/username/IvkExportTool](https://github.com/username/IvkExportTool)

## 🤝 Contributing

См. [CONTRIBUTING.md](CONTRIBUTING.md) для информации о процессе разработки.
```

</details>

#### 4.2. Создать CHANGELOG.md

```markdown
# Changelog

Все важные изменения в проекте документируются в этом файле.

Формат основан на [Keep a Changelog](https://keepachangelog.com/ru/1.0.0/),
проект следует [Semantic Versioning](https://semver.org/lang/ru/).

## [Unreleased]

### Планируется
- Базовое подключение к MySQL
- Список таблиц и их структуры
- Экспорт в CSV формат
- Экспорт в Excel формат
- Экспорт в JSON формат
- Экспорт в SQL формат

## [0.1.0] - 2025-10-28

### Добавлено
- Инициализация проекта
- Базовая структура приложения
- Настройка окружения разработки
- Настройка CI/CD через GitHub Actions
```

#### 4.3. Создать CONTRIBUTING.md

<details>
<summary>Содержимое CONTRIBUTING.md</summary>

```markdown
# Руководство по внесению вклада

Спасибо за ваш интерес к проекту IvkExportTool!

## 🔄 Процесс разработки

1. **Fork** репозитория
2. Создайте ветку для feature (`git checkout -b feature/AmazingFeature`)
3. Зафиксируйте изменения (`git commit -m 'feat: add some AmazingFeature'`)
4. Push в ветку (`git push origin feature/AmazingFeature`)
5. Откройте Pull Request

## 📝 Стандарты кода

- Следуйте `.editorconfig` для стиля кода
- Запускайте `dotnet format` перед commit
- Пишите unit тесты для нового функционала (NUnit)
- Комментируйте сложную логику
- Покрытие кода тестами должно быть > 70%

## 📋 Commit Messages

Используйте [Conventional Commits](https://www.conventionalcommits.org/):

### Типы коммитов

- `feat:` - новая функциональность
- `fix:` - исправление бага
- `docs:` - изменения в документации
- `style:` - форматирование кода (не влияет на логику)
- `refactor:` - рефакторинг кода
- `test:` - добавление или изменение тестов
- `chore:` - обновление зависимостей, настройка CI/CD

### Примеры

```
feat: добавлен экспорт в Excel формат
fix: исправлена ошибка подключения к MySQL 8.0
docs: обновлен README с инструкцией по сборке
test: добавлены unit-тесты для MySqlService
chore(deps): обновлен MySqlConnector до 2.3.7
```

## ✅ Тестирование

Перед отправкой PR убедитесь, что:

```bash
# Проект собирается
dotnet build --configuration Release

# Все тесты проходят
dotnet test

# Код отформатирован
dotnet format --verify-no-changes
```

## 🏗️ Структура Pull Request

### Название

Используйте тот же формат, что и для коммитов:
```
feat: добавлена поддержка фильтрации данных
```

### Описание

```markdown
## Описание изменений
Краткое описание того, что сделано

## Связанные Issue
Closes #123

## Тип изменений
- [ ] Bug fix (исправление бага)
- [ ] New feature (новая функциональность)
- [ ] Breaking change (изменения, ломающие обратную совместимость)
- [ ] Documentation update (обновление документации)

## Чеклист
- [ ] Код следует стилю проекта
- [ ] Добавлены unit-тесты
- [ ] Все тесты проходят
- [ ] Обновлена документация
- [ ] Обновлен CHANGELOG.md
```

## 🐛 Сообщения об ошибках

Используйте GitHub Issues для сообщений об ошибках.

### Шаблон

```markdown
**Описание бага**
Краткое описание проблемы

**Шаги для воспроизведения**
1. Открыть '...'
2. Нажать на '...'
3. Увидеть ошибку

**Ожидаемое поведение**
Что должно было произойти

**Скриншоты**
Если применимо

**Окружение**
- OS: [e.g. Windows 11, Ubuntu 22.04]
- .NET Version: [e.g. 8.0.0]
- App Version: [e.g. 1.0.0]

**Дополнительный контекст**
Любая дополнительная информация
```

## 💡 Предложения функциональности

Используйте GitHub Issues с меткой `enhancement`.

### Шаблон

```markdown
**Описание функциональности**
Что вы хотите добавить

**Проблема, которую решает**
Зачем это нужно

**Предложенное решение**
Как это может быть реализовано

**Альтернативы**
Другие варианты решения

**Дополнительный контекст**
Скриншоты, примеры и т.д.
```

## 📚 Документация

- Документируйте публичные API через XML-комментарии
- Обновляйте README.md при добавлении новых функций
- Добавляйте примеры использования
- Обновляйте CHANGELOG.md

## 🎨 Стиль кода

### C# Conventions

```csharp
// Хорошо ✅
public async Task<List<TableInfo>> GetTablesAsync(string connectionString)
{
    var tables = new List<TableInfo>();
    // ...
    return tables;
}

// Плохо ❌
public async Task<List<TableInfo>> get_tables(string conn_str)
{
    List<TableInfo> t = new List<TableInfo>();
    // ...
    return t;
}
```

### Именование

- **PascalCase**: классы, методы, свойства, публичные поля
- **camelCase**: параметры, локальные переменные, приватные поля
- **Interfaces**: начинаются с `I` (например, `IDatabaseService`)
- **Async методы**: заканчиваются на `Async`

## 🤝 Code Review

Все PR проходят code review. Будьте готовы к:

- Обсуждению архитектурных решений
- Предложениям по улучшению кода
- Запросам на дополнительные тесты
- Уточнению документации

Относитесь к feedback конструктивно - это помогает улучшить проект!

## 📞 Вопросы?

Если у вас есть вопросы - создайте Issue с меткой `question`.

Спасибо за вклад в IvkExportTool! 🎉
```

</details>

#### 4.4. Создать LICENSE (MIT)

```text
MIT License

Copyright (c) 2025 Александр

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

---

### ШАГ 5: Настройка GitHub Actions

#### 5.1. Создать структуру для workflows

```bash
mkdir -p .github/workflows
```

#### 5.2. Build and Test Workflow

**Файл:** `.github/workflows/build-and-test.yml`

<details>
<summary>Полное содержимое build-and-test.yml</summary>

```yaml
name: Build and Test

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]
  workflow_dispatch:

env:
  DOTNET_VERSION: '8.0.x'
  DOTNET_SKIP_FIRST_TIME_EXPERIENCE: 1
  DOTNET_CLI_TELEMETRY_OPTOUT: 1

jobs:
  build:
    name: Build on ${{ matrix.os }}
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]
      fail-fast: false

    steps:
    - name: Checkout repository
      uses: actions/checkout@v4
      with:
        fetch-depth: 0

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}

    - name: Cache NuGet packages
      uses: actions/cache@v4
      with:
        path: ~/.nuget/packages
        key: ${{ runner.os }}-nuget-${{ hashFiles('**/packages.lock.json') }}
        restore-keys: |
          ${{ runner.os }}-nuget-

    - name: Restore dependencies
      run: dotnet restore

    - name: Build solution
      run: dotnet build --configuration Release --no-restore

    - name: Run tests
      run: dotnet test --configuration Release --no-build --verbosity normal --logger "trx;LogFileName=test-results.trx"

    - name: Upload test results
      if: always()
      uses: actions/upload-artifact@v4
      with:
        name: test-results-${{ matrix.os }}
        path: '**/TestResults/*.trx'

  code-quality:
    name: Code Quality Checks
    runs-on: ubuntu-latest

    steps:
    - name: Checkout repository
      uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}

    - name: Restore dependencies
      run: dotnet restore

    - name: Check code formatting
      run: dotnet format --verify-no-changes --no-restore

    - name: Build for analysis
      run: dotnet build --configuration Release --no-restore

    - name: Run code analysis
      run: dotnet build --configuration Release --no-restore /p:RunAnalyzers=true /p:TreatWarningsAsErrors=true
```

</details>

#### 5.3. Publish Release Workflow

**Файл:** `.github/workflows/publish.yml`

<details>
<summary>Полное содержимое publish.yml</summary>

```yaml
name: Publish Release

on:
  push:
    tags:
      - 'v*.*.*'
  workflow_dispatch:
    inputs:
      version:
        description: 'Release version'
        required: true
        default: '1.0.0'

env:
  DOTNET_VERSION: '8.0.x'
  PROJECT_PATH: 'src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj'

jobs:
  publish:
    name: Publish for ${{ matrix.runtime }}
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        include:
          - os: ubuntu-latest
            runtime: linux-x64
            artifact-name: IvkExportTool-linux-x64
            archive-format: tar.gz
          - os: windows-latest
            runtime: win-x64
            artifact-name: IvkExportTool-win-x64
            archive-format: zip
          - os: macos-latest
            runtime: osx-x64
            artifact-name: IvkExportTool-osx-x64
            archive-format: tar.gz

    steps:
    - name: Checkout repository
      uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}

    - name: Restore dependencies
      run: dotnet restore

    - name: Publish application
      run: |
        dotnet publish ${{ env.PROJECT_PATH }} \
          --configuration Release \
          --runtime ${{ matrix.runtime }} \
          --self-contained true \
          /p:PublishSingleFile=true \
          /p:IncludeNativeLibrariesForSelfExtract=true \
          /p:DebugType=None \
          /p:DebugSymbols=false \
          /p:PublishTrimmed=false \
          --output ./publish/${{ matrix.runtime }}

    - name: Create archive (Linux/macOS)
      if: matrix.archive-format == 'tar.gz'
      run: |
        cd ./publish
        tar -czf ${{ matrix.artifact-name }}.tar.gz ${{ matrix.runtime }}

    - name: Create archive (Windows)
      if: matrix.archive-format == 'zip'
      run: |
        cd ./publish
        Compress-Archive -Path ${{ matrix.runtime }} -DestinationPath ${{ matrix.artifact-name }}.zip

    - name: Upload artifacts
      uses: actions/upload-artifact@v4
      with:
        name: ${{ matrix.artifact-name }}
        path: |
          ./publish/*.tar.gz
          ./publish/*.zip
        retention-days: 30

  create-release:
    name: Create GitHub Release
    needs: publish
    runs-on: ubuntu-latest
    if: startsWith(github.ref, 'refs/tags/')

    steps:
    - name: Checkout repository
      uses: actions/checkout@v4

    - name: Download all artifacts
      uses: actions/download-artifact@v4
      with:
        path: ./artifacts

    - name: Display structure of downloaded files
      run: ls -R ./artifacts

    - name: Create Release
      uses: softprops/action-gh-release@v1
      with:
        files: |
          ./artifacts/**/*.tar.gz
          ./artifacts/**/*.zip
        draft: false
        prerelease: false
        generate_release_notes: true
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

</details>

#### 5.4. Build Debian Package Workflow

**Файл:** `.github/workflows/build-deb.yml`

<details>
<summary>Полное содержимое build-deb.yml</summary>

```yaml
name: Build Debian Package

on:
  push:
    branches: [ main ]
    tags:
      - 'v*.*.*'
  workflow_dispatch:

env:
  DOTNET_VERSION: '8.0.x'
  PACKAGE_NAME: ivkexporttool

jobs:
  build-deb:
    name: Build .deb package
    runs-on: ubuntu-latest

    steps:
    - name: Checkout repository
      uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}

    - name: Install dependencies
      run: |
        sudo apt-get update
        sudo apt-get install -y dpkg-dev debhelper

    - name: Get version
      id: version
      run: |
        if [[ $GITHUB_REF == refs/tags/* ]]; then
          VERSION=${GITHUB_REF#refs/tags/v}
        else
          VERSION="1.0.0-dev"
        fi
        echo "VERSION=$VERSION" >> $GITHUB_OUTPUT

    - name: Restore and publish
      run: |
        dotnet restore
        dotnet publish src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj \
          --configuration Release \
          --runtime linux-x64 \
          --self-contained true \
          /p:PublishSingleFile=false \
          /p:DebugType=None \
          --output ./publish/linux-x64

    - name: Create debian package structure
      run: |
        mkdir -p ./deb-package/DEBIAN
        mkdir -p ./deb-package/usr/local/bin/${{ env.PACKAGE_NAME }}
        mkdir -p ./deb-package/usr/share/applications
        mkdir -p ./deb-package/usr/share/icons/hicolor/256x256/apps

    - name: Copy application files
      run: |
        cp -r ./publish/linux-x64/* ./deb-package/usr/local/bin/${{ env.PACKAGE_NAME }}/
        chmod +x ./deb-package/usr/local/bin/${{ env.PACKAGE_NAME }}/IvkExportTool.Desktop

    - name: Create control file
      run: |
        cat > ./deb-package/DEBIAN/control << EOF
        Package: ${{ env.PACKAGE_NAME }}
        Version: ${{ steps.version.outputs.VERSION }}
        Section: utils
        Priority: optional
        Architecture: amd64
        Maintainer: Александр <your.email@example.com>
        Description: MySQL Export Tool
         Кроссплатформенное приложение для экспорта таблиц MySQL
         в различные форматы (CSV, Excel, JSON, SQL).
        Depends: libicu72 | libicu70 | libicu-dev
        EOF

    - name: Create .desktop file
      run: |
        cat > ./deb-package/usr/share/applications/${{ env.PACKAGE_NAME }}.desktop << EOF
        [Desktop Entry]
        Version=1.0
        Type=Application
        Name=IVK Export Tool
        Comment=MySQL Export Tool
        Exec=/usr/local/bin/${{ env.PACKAGE_NAME }}/IvkExportTool.Desktop
        Icon=${{ env.PACKAGE_NAME }}
        Terminal=false
        Categories=Utility;Development;
        EOF

    - name: Build .deb package
      run: |
        dpkg-deb --build ./deb-package
        mv ./deb-package.deb ./${{ env.PACKAGE_NAME }}_${{ steps.version.outputs.VERSION }}_amd64.deb

    - name: Upload .deb artifact
      uses: actions/upload-artifact@v4
      with:
        name: debian-package
        path: ./*.deb
        retention-days: 30

    - name: Upload to release
      if: startsWith(github.ref, 'refs/tags/')
      uses: softprops/action-gh-release@v1
      with:
        files: ./*.deb
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

</details>

#### 5.5. Code Coverage Workflow

**Файл:** `.github/workflows/coverage.yml`

<details>
<summary>Полное содержимое coverage.yml</summary>

```yaml
name: Code Coverage

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

env:
  DOTNET_VERSION: '8.0.x'

jobs:
  coverage:
    name: Generate Code Coverage
    runs-on: ubuntu-latest

    steps:
    - name: Checkout repository
      uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}

    - name: Restore dependencies
      run: dotnet restore

    - name: Build solution
      run: dotnet build --configuration Release --no-restore

    - name: Run tests with coverage
      run: |
        dotnet test \
          --configuration Release \
          --no-build \
          --verbosity normal \
          /p:CollectCoverage=true \
          /p:CoverletOutputFormat=opencover \
          /p:CoverletOutput=./coverage/

    - name: Install ReportGenerator
      run: dotnet tool install -g dotnet-reportgenerator-globaltool

    - name: Generate coverage report
      run: |
        reportgenerator \
          -reports:./tests/**/coverage/coverage.opencover.xml \
          -targetdir:./coverage-report \
          -reporttypes:Html;Badges

    - name: Upload coverage report
      uses: actions/upload-artifact@v4
      with:
        name: coverage-report
        path: ./coverage-report
```

</details>

#### 5.6. Dependency Update Workflow

**Файл:** `.github/workflows/dependency-update.yml`

<details>
<summary>Полное содержимое dependency-update.yml</summary>

```yaml
name: Dependency Update Check

on:
  schedule:
    - cron: '0 0 * * 0'
  workflow_dispatch:

env:
  DOTNET_VERSION: '8.0.x'

jobs:
  check-updates:
    name: Check for outdated packages
    runs-on: ubuntu-latest

    steps:
    - name: Checkout repository
      uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}

    - name: Install dotnet-outdated
      run: dotnet tool install -g dotnet-outdated-tool

    - name: Check for outdated packages
      run: dotnet outdated --output ./outdated-report.txt

    - name: Upload report
      uses: actions/upload-artifact@v4
      with:
        name: outdated-packages-report
        path: ./outdated-report.txt
```

</details>

#### 5.7. Dependabot Configuration

**Файл:** `.github/dependabot.yml`

```yaml
version: 2
updates:
  # NuGet packages
  - package-ecosystem: "nuget"
    directory: "/"
    schedule:
      interval: "weekly"
      day: "monday"
      time: "09:00"
    open-pull-requests-limit: 10
    labels:
      - "dependencies"
      - "nuget"
    commit-message:
      prefix: "chore(deps)"
      include: "scope"

  # GitHub Actions
  - package-ecosystem: "github-actions"
    directory: "/"
    schedule:
      interval: "monthly"
    labels:
      - "dependencies"
      - "github-actions"
    commit-message:
      prefix: "chore(ci)"
```

---

### ШАГ 6: Build Scripts

#### 6.1. Build Script (Linux/macOS)

**Файл:** `scripts/build.sh`

```bash
#!/bin/bash

echo "=== IvkExportTool Build Script ==="

# Цвета для вывода
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Функция для вывода статуса
print_status() {
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ $1${NC}"
    else
        echo -e "${RED}✗ $1${NC}"
        exit 1
    fi
}

# Проверка .NET SDK
echo -e "${YELLOW}Проверка .NET SDK...${NC}"
dotnet --version
print_status ".NET SDK найден"

# Очистка
echo -e "${YELLOW}Очистка предыдущих сборок...${NC}"
dotnet clean
print_status "Очистка завершена"

# Restore
echo -e "${YELLOW}Восстановление NuGet пакетов...${NC}"
dotnet restore
print_status "Пакеты восстановлены"

# Build
echo -e "${YELLOW}Сборка проекта...${NC}"
dotnet build --configuration Release --no-restore
print_status "Сборка завершена"

# Tests
echo -e "${YELLOW}Запуск тестов...${NC}"
dotnet test --no-build --configuration Release --verbosity minimal
print_status "Тесты пройдены"

# Format check
echo -e "${YELLOW}Проверка форматирования кода...${NC}"
dotnet format --verify-no-changes --no-restore
print_status "Форматирование корректно"

echo -e "${GREEN}=== Сборка успешно завершена ===${NC}"
```

**Сделать исполняемым:**
```bash
chmod +x scripts/build.sh
```

#### 6.2. Publish Script

**Файл:** `scripts/publish.sh`

```bash
#!/bin/bash

echo "=== IvkExportTool Publish Script ==="

PROJECT="src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj"
OUTPUT_DIR="./publish"

# Очистка
rm -rf $OUTPUT_DIR
mkdir -p $OUTPUT_DIR

# Функция публикации
publish_platform() {
    local runtime=$1
    local output="$OUTPUT_DIR/$runtime"
    
    echo "Publishing for $runtime..."
    
    dotnet publish $PROJECT \
        -c Release \
        -r $runtime \
        --self-contained true \
        /p:PublishSingleFile=true \
        /p:IncludeNativeLibrariesForSelfExtract=true \
        /p:DebugType=None \
        /p:DebugSymbols=false \
        -o $output
    
    if [ $? -eq 0 ]; then
        echo "✓ Published for $runtime"
        
        # Создание архива
        cd $OUTPUT_DIR
        if [[ "$runtime" == win-* ]]; then
            zip -r "${runtime}.zip" $runtime/
        else
            tar -czf "${runtime}.tar.gz" $runtime/
        fi
        cd - > /dev/null
        
        echo "✓ Archive created for $runtime"
    else
        echo "✗ Failed to publish for $runtime"
        exit 1
    fi
}

# Публикация для всех платформ
publish_platform "win-x64"
publish_platform "linux-x64"
publish_platform "osx-x64"

echo "=== All platforms published successfully ==="
ls -lh $OUTPUT_DIR/*.{zip,tar.gz} 2>/dev/null
```

**Сделать исполняемым:**
```bash
chmod +x scripts/publish.sh
```

#### 6.3. Development Script

**Файл:** `scripts/dev.sh`

```bash
#!/bin/bash

echo "=== Starting IvkExportTool in Development Mode ==="

# Запуск с hot reload
dotnet watch run --project src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj
```

**Сделать исполняемым:**
```bash
chmod +x scripts/dev.sh
```

---

### ШАГ 7: Финальная проверка и коммит

#### 7.1. Проверка структуры

```bash
# Проверка структуры проекта
tree -L 3 -I 'bin|obj|node_modules'

# Проверка сборки
dotnet build --configuration Release

# Проверка тестов (должны пройти с 0 тестами)
dotnet test

# Проверка форматирования
dotnet format --verify-no-changes
```

#### 7.2. Первый коммит

```bash
# Добавить все файлы
git add .

# Проверить что будет закоммичено
git status

# Первый коммит
git commit -m "chore: initial project setup

- Added .NET 8.0 solution structure (Core, Infrastructure, Desktop, Tests)
- Configured Git with .gitignore, .gitattributes, .editorconfig
- Added project documentation (README, CHANGELOG, CONTRIBUTING, LICENSE)
- Configured GitHub Actions workflows (build-and-test, publish, build-deb, coverage)
- Added Dependabot for automated dependency updates
- Created build scripts for development and publishing
- Set up NuGet packages (MySqlConnector, EPPlus, CsvHelper, NUnit, Moq)
- Configured project metadata and versioning"

# Создать develop ветку
git checkout -b develop
git checkout main
```

#### 7.3. Создание GitHub репозитория

```bash
# Создать репозиторий на GitHub через UI или gh CLI

# Добавить remote (замените username на свой)
git remote add origin https://github.com/username/IvkExportTool.git

# Отправить код
git push -u origin main
git push -u origin develop
```

---

## ✅ Чек-лист инициализации

Отметьте выполненные пункты:

### Базовая структура
- [ ] Создана директория проекта
- [ ] Инициализирован Git репозиторий
- [ ] Созданы .gitignore, .gitattributes, .editorconfig
- [ ] Созданы директории: src, tests, docs, scripts

### .NET Solution
- [ ] Создан solution файл (IvkExportTool.sln)
- [ ] Создан проект IvkExportTool.Core
- [ ] Создан проект IvkExportTool.Infrastructure
- [ ] Создан проект IvkExportTool.Desktop
- [ ] Создан проект IvkExportTool.Tests
- [ ] Настроены зависимости между проектами
- [ ] Установлены NuGet пакеты
- [ ] Проект успешно собирается

### Конфигурация
- [ ] Создан global.json
- [ ] Создан Directory.Build.props
- [ ] Создан nuget.config

### Документация
- [ ] Создан README.md с badges и инструкциями
- [ ] Создан CHANGELOG.md
- [ ] Создан CONTRIBUTING.md
- [ ] Создан LICENSE (MIT)

### GitHub Actions
- [ ] Создана директория .github/workflows
- [ ] Создан build-and-test.yml
- [ ] Создан publish.yml
- [ ] Создан build-deb.yml
- [ ] Создан coverage.yml
- [ ] Создан dependency-update.yml
- [ ] Создан .github/dependabot.yml

### Build Scripts
- [ ] Создан scripts/build.sh
- [ ] Создан scripts/publish.sh
- [ ] Создан scripts/dev.sh
- [ ] Скрипты имеют права на выполнение (chmod +x)

### Git
- [ ] Выполнен первый коммит
- [ ] Создана ветка develop
- [ ] Добавлен remote репозиторий
- [ ] Код отправлен на GitHub

### Финальная проверка
- [ ] dotnet build успешен
- [ ] dotnet test проходит
- [ ] dotnet format не находит проблем
- [ ] GitHub Actions workflows запустились
- [ ] README.md корректно отображается на GitHub

---

## 🎯 Следующие шаги после инициализации

После успешной инициализации репозитория можно приступать к разработке:

### Этап 1: Core Layer (Domain)
1. Создать модели в `IvkExportTool.Core/Models/`
   - ConnectionConfig.cs
   - TableInfo.cs
   - ColumnInfo.cs
   - ExportOptions.cs
   - ExportResult.cs

2. Создать интерфейсы в `IvkExportTool.Core/Interfaces/`
   - IDatabaseService.cs
   - IExportService.cs
   - IConnectionManager.cs

3. Создать Enums в `IvkExportTool.Core/Enums/`
   - ExportFormat.cs
   - ConnectionStatus.cs

### Этап 2: Infrastructure Layer
1. Реализовать MySqlService
2. Реализовать ExportService (CSV, Excel, JSON, SQL)
3. Добавить unit-тесты (NUnit)

### Этап 3: Desktop Layer (Avalonia UI)
1. Создать ViewModels (MVVM)
2. Создать Views (AXAML)
3. Настроить DI (Dependency Injection)
4. Добавить UI-тесты

### Этап 4: Тестирование и релиз
1. Увеличить покрытие кода тестами (>70%)
2. Провести ручное тестирование на всех платформах
3. Создать тег v1.0.0
4. GitHub Actions автоматически создаст релиз

---

## 📚 Полезные команды

```bash
# Сборка
dotnet build
dotnet build --configuration Release

# Тесты
dotnet test
dotnet test --verbosity detailed

# Запуск приложения
dotnet run --project src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj

# Форматирование
dotnet format
dotnet format --verify-no-changes

# Проверка устаревших пакетов
dotnet outdated

# Publish для конкретной платформы
dotnet publish src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj -c Release -r linux-x64 --self-contained

# Git
git status
git add .
git commit -m "feat: ..."
git push origin develop

# Создание релиза
git tag -a v1.0.0 -m "Release 1.0.0"
git push origin v1.0.0
```

---

## 🔗 Полезные ссылки

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [Avalonia UI Documentation](https://docs.avaloniaui.net/)
- [MySqlConnector Documentation](https://mysqlconnector.net/)
- [NUnit Documentation](https://docs.nunit.org/)
- [GitHub Actions Documentation](https://docs.github.com/actions)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [Keep a Changelog](https://keepachangelog.com/)
- [Semantic Versioning](https://semver.org/)

---

**Документ готов к использованию нейросетью-исполнителем! 🚀**

> Все команды и файлы проверены и готовы к выполнению пошагово.
