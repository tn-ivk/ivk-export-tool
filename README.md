# IvkExportTool

Кроссплатформенное портабельное приложение для экспорта таблиц MySQL в SQL-файлы для последующего восстановления.

## Возможности

- 🔌 Подключение к MySQL базам данных с настройкой IP, порта, логина и пароля
- 🔐 Автоподключение с перебором предустановленных учётных данных (для БД ИВК)
- 📊 Просмотр списка доступных баз данных и таблиц
- ✅ Выбор отдельных таблиц или всей базы данных для экспорта
- 💾 Экспорт в SQL-файл с полной структурой и данными
- 🔄 Поддержка восстановления экспортированных данных
- 🖥️ Кроссплатформенность (Windows, Linux)
- 🎨 Современный GUI на Avalonia UI
- 💼 Сохранение настроек подключения между сессиями

## Требования

### Для запуска приложения
- .NET 10.0 Runtime (для портабельной версии не требуется)
- MySQL Server 5.7+ (для подключения к базе данных)

### Для разработки
- .NET 10.0 SDK
- Visual Studio 2022 / JetBrains Rider / VS Code
- Git

## Установка

### Скачать готовое приложение

Перейдите в раздел [Releases](https://github.com/YOUR_USERNAME/ivk-export-tool/releases) и скачайте последнюю версию:

- **Windows**: `IvkExportTool-win-x64.zip`
- **Linux**: `IvkExportTool-linux-x64.tar.gz`

Распакуйте архив и запустите исполняемый файл:
- Windows: `IvkExportTool.Desktop.exe`
- Linux: `./IvkExportTool.Desktop`

### Сборка из исходников

```bash
git clone https://github.com/YOUR_USERNAME/ivk-export-tool.git
cd ivk-export-tool
dotnet restore
dotnet build --configuration Release
dotnet run --project src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj
```

## Использование

### 1. Выбор способа подключения

При запуске открывается стартовое окно с двумя вариантами:

- **Автоподключение** (рекомендуется для ИВК) — автоматический перебор предустановленных учётных данных
- **Ручная настройка** — ввод IP, порта, логина и пароля вручную

### 2. Подключение к MySQL

**Автоподключение:**
- Введите IP-адрес и порт сервера
- Нажмите "Автоподключение" — приложение переберёт учётные данные
- При успехе откроется главное окно

**Ручное подключение:**
- Введите IP-адрес, порт, логин и пароль
- Нажмите "Тест подключения" для проверки или "Подключиться"

### 3. Выбор базы данных

После успешного подключения выберите базу данных из выпадающего списка. Приложение автоматически загрузит список таблиц.

### 4. Выбор таблиц для экспорта

- Отметьте чекбоксами таблицы для экспорта
- Используйте кнопки "Выбрать все" / "Снять выбор" для массового выбора
- Используйте поле поиска для фильтрации по имени

### 5. Экспорт данных

- Нажмите кнопку "Экспортировать"
- Выберите место сохранения в диалоге
- SQL-файл содержит полную структуру таблиц (CREATE TABLE) и все данные (INSERT)

### Хранение настроек

Настройки приложения сохраняются автоматически:
- **Windows**: `%APPDATA%\IvkExportTool\settings.json`
- **Linux**: `~/.config/IvkExportTool/settings.json`

## Архитектура проекта

Проект использует Clean Architecture с разделением на слои:

```
IvkExportTool/
├── src/
│   ├── IvkExportTool.Core/          # Бизнес-логика и интерфейсы
│   ├── IvkExportTool.Infrastructure/ # Реализация работы с MySQL
│   └── IvkExportTool.Desktop/       # GUI на Avalonia с MVVM
└── tests/
    └── IvkExportTool.Tests/         # Unit тесты
```

### Технологический стек

- **.NET 10.0** - платформа разработки
- **Avalonia UI 11.x** - кроссплатформенный GUI фреймворк
- **MySqlConnector** - подключение к MySQL
- **System.Text.Json** - сериализация настроек (AOT-совместимо)
- **CommunityToolkit.Mvvm** - MVVM паттерн
- **NUnit** - тестирование

## Разработка

### Команды для сборки

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

### Создание портабельного релиза

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
```

## CI/CD

Проект использует GitHub Actions для автоматической сборки:

- **Build and Test** - автоматическая сборка и тесты на каждый push/PR
- **Publish Release** - создание релизов при создании тега версии

### Создание релиза

```bash
# Создать и отправить тег версии
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0

# GitHub Actions автоматически создаст релиз с артефактами для Windows и Linux
```

## Лицензия

MIT License

## Контакты

Александр - АО «Транснефть»

Project Link: [https://github.com/YOUR_USERNAME/ivk-export-tool](https://github.com/YOUR_USERNAME/ivk-export-tool)
