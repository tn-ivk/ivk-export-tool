# Установка

Подробное руководство по установке **IvkExportTool** для различных платформ и сценариев использования.

## Содержание

- [Системные требования](#системные-требования)
- [Готовые бинарные файлы](#готовые-бинарные-файлы)
- [Установка на Windows](#установка-на-windows)
- [Установка на Linux](#установка-на-linux)
- [Установка на macOS](#установка-на-macos)
- [Сборка из исходников](#сборка-из-исходников)
- [Проверка установки](#проверка-установки)
- [Обновление](#обновление)
- [Удаление](#удаление)

## Системные требования

### Для конечных пользователей (готовые релизы)

- **Windows**: Windows 10 (версия 1809) или выше
- **Linux**: Ubuntu 20.04+, Debian 10+, Fedora 33+, или совместимые дистрибутивы
- **macOS**: macOS 10.15 (Catalina) или выше
- **RAM**: минимум 512 MB (рекомендуется 2 GB+)
- **Дисковое пространство**: ~100 MB для приложения
- **MySQL Server**: версия 5.7 или выше (для подключения)

### Для разработчиков (сборка из исходников)

- **.NET 10.0 SDK** или выше
- **Git** для клонирования репозитория
- **IDE** (опционально): Visual Studio 2022, JetBrains Rider, или VS Code

## Готовые бинарные файлы

Самый простой способ установки — скачать готовые релизы.

### Скачивание

1. Откройте страницу [Releases](https://github.com/YOUR_USERNAME/ivk-export-tool/releases)
2. Выберите последнюю версию
3. Скачайте архив для вашей платформы:

| Платформа | Файл | Размер (~) |
|-----------|------|-----------|
| Windows x64 | `IvkExportTool-win-x64.zip` | ~50 MB |
| Linux x64 | `IvkExportTool-linux-x64.tar.gz` | ~60 MB |
| macOS x64 | `IvkExportTool-osx-x64.tar.gz` | ~60 MB |

> **Примечание**: Приложение является **портабельным** — установка .NET Runtime не требуется.

## Установка на Windows

### Вариант 1: Портабельная версия (рекомендуется)

1. **Скачайте** `IvkExportTool-win-x64.zip` из [Releases](https://github.com/YOUR_USERNAME/ivk-export-tool/releases)

2. **Распакуйте архив**:
   - Щелкните правой кнопкой мыши на файле
   - Выберите "Извлечь всё..." или используйте 7-Zip/WinRAR
   - Укажите папку назначения (например, `C:\Tools\IvkExportTool`)

3. **Запустите приложение**:
   - Перейдите в папку с приложением
   - Дважды щелкните на `IvkExportTool.Desktop.exe`

### Вариант 2: Запуск через .NET Runtime

Если у вас уже установлен .NET 10.0 Runtime:

1. Скачайте framework-dependent версию (если доступна)
2. Распакуйте архив
3. Запустите через командную строку:

```cmd
dotnet IvkExportTool.Desktop.dll
```

### Создание ярлыка на рабочем столе

1. Щелкните правой кнопкой на `IvkExportTool.Desktop.exe`
2. Выберите "Создать ярлык"
3. Перетащите ярлык на рабочий стол

### Добавление в PATH (опционально)

Для запуска из командной строки из любой папки:

1. Откройте **"Параметры системы" → "Дополнительные параметры системы"**
2. Нажмите **"Переменные среды"**
3. В разделе **"Системные переменные"** найдите `Path`
4. Нажмите **"Изменить"** → **"Создать"**
5. Добавьте путь к папке с приложением (например, `C:\Tools\IvkExportTool`)
6. Нажмите **"ОК"** для сохранения

Теперь можно запускать:

```cmd
IvkExportTool.Desktop
```

## Установка на Linux

### Ubuntu/Debian

1. **Скачайте архив**:

```bash
wget https://github.com/YOUR_USERNAME/ivk-export-tool/releases/latest/download/IvkExportTool-linux-x64.tar.gz
```

2. **Распакуйте**:

```bash
mkdir -p ~/Applications/IvkExportTool
tar -xzf IvkExportTool-linux-x64.tar.gz -C ~/Applications/IvkExportTool
```

3. **Добавьте права на выполнение**:

```bash
chmod +x ~/Applications/IvkExportTool/IvkExportTool.Desktop
```

4. **Запустите приложение**:

```bash
~/Applications/IvkExportTool/IvkExportTool.Desktop
```

### Установка системных зависимостей

Если приложение не запускается, установите необходимые библиотеки:

```bash
# Ubuntu/Debian
sudo apt-get update
sudo apt-get install -y libicu-dev libssl-dev libx11-dev

# Fedora/CentOS/RHEL
sudo dnf install -y icu openssl-libs libX11

# Arch Linux
sudo pacman -S icu openssl libx11
```

### Создание desktop-файла (опционально)

Создайте файл `~/.local/share/applications/ivkexporttool.desktop`:

```ini
[Desktop Entry]
Version=1.0
Type=Application
Name=IvkExportTool
Comment=MySQL Export Tool
Exec=/home/USERNAME/Applications/IvkExportTool/IvkExportTool.Desktop
Icon=/home/USERNAME/Applications/IvkExportTool/icon.png
Terminal=false
Categories=Development;Database;
```

Замените `USERNAME` на ваше имя пользователя.

### Добавление в PATH (опционально)

Добавьте в `~/.bashrc` или `~/.zshrc`:

```bash
export PATH="$HOME/Applications/IvkExportTool:$PATH"
```

Примените изменения:

```bash
source ~/.bashrc
```

## Установка на macOS

1. **Скачайте архив**:

```bash
curl -L -o IvkExportTool-osx-x64.tar.gz \
  https://github.com/YOUR_USERNAME/ivk-export-tool/releases/latest/download/IvkExportTool-osx-x64.tar.gz
```

2. **Распакуйте**:

```bash
mkdir -p ~/Applications/IvkExportTool
tar -xzf IvkExportTool-osx-x64.tar.gz -C ~/Applications/IvkExportTool
```

3. **Добавьте права на выполнение**:

```bash
chmod +x ~/Applications/IvkExportTool/IvkExportTool.Desktop
```

4. **Разрешите запуск** (требуется для неподписанных приложений):

```bash
xattr -d com.apple.quarantine ~/Applications/IvkExportTool/IvkExportTool.Desktop
```

5. **Запустите приложение**:

```bash
~/Applications/IvkExportTool/IvkExportTool.Desktop
```

### Установка через Homebrew (в будущем)

```bash
# Пока недоступно
brew install ivk-export-tool
```

## Сборка из исходников

### Предварительные требования

- **.NET 10.0 SDK**: [скачать](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Git**: [скачать](https://git-scm.com/downloads)

### Клонирование репозитория

```bash
git clone https://github.com/YOUR_USERNAME/ivk-export-tool.git
cd ivk-export-tool
```

### Восстановление зависимостей

```bash
dotnet restore
```

### Сборка проекта

**Debug сборка** (для разработки):

```bash
dotnet build --configuration Debug
```

**Release сборка**:

```bash
dotnet build --configuration Release
```

### Запуск из исходников

```bash
dotnet run --project src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj
```

### Создание портабельного релиза

**Windows x64**:

```bash
dotnet publish src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  /p:PublishSingleFile=true \
  /p:IncludeNativeLibrariesForSelfExtract=true \
  -o ./publish/win-x64
```

**Linux x64**:

```bash
dotnet publish src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  /p:PublishSingleFile=true \
  -o ./publish/linux-x64
```

**macOS x64**:

```bash
dotnet publish src/IvkExportTool.Desktop/IvkExportTool.Desktop.csproj \
  -c Release \
  -r osx-x64 \
  --self-contained true \
  /p:PublishSingleFile=true \
  -o ./publish/osx-x64
```

Результат будет в папке `./publish/{platform}/`.

## Проверка установки

После установки проверьте работоспособность:

1. **Запустите приложение**
2. **Проверьте версию**: В окне приложения → "О программе" (или в логах)
3. **Тестовое подключение**: Попробуйте подключиться к тестовой базе данных

Ожидаемое поведение:
- Приложение запускается без ошибок
- Окно подключения отображается корректно
- Подключение к MySQL работает

## Обновление

### Для портабельной версии

1. Скачайте новую версию из [Releases](https://github.com/YOUR_USERNAME/ivk-export-tool/releases)
2. **Сохраните настройки** (опционально):
   - Скопируйте файл `appsettings.json` из старой версии
3. Удалите старую версию
4. Распакуйте новую версию
5. **Восстановите настройки** (если сохраняли):
   - Скопируйте `appsettings.json` в новую папку

### Для сборки из исходников

```bash
cd ivk-export-tool
git pull origin main
dotnet restore
dotnet build --configuration Release
```

## Удаление

### Windows

1. Закройте приложение
2. Удалите папку с приложением
3. Удалите ярлык с рабочего стола (если создавали)
4. Удалите запись из PATH (если добавляли)

### Linux/macOS

```bash
rm -rf ~/Applications/IvkExportTool
rm ~/.local/share/applications/ivkexporttool.desktop  # если создавали
```

Также удалите запись из `~/.bashrc` или `~/.zshrc` (если добавляли в PATH).

## Решение проблем при установке

### Windows: "Windows protected your PC"

**Проблема**: SmartScreen блокирует запуск

**Решение**:
1. Нажмите **"More info"**
2. Нажмите **"Run anyway"**

Или отключите SmartScreen (не рекомендуется):
```
Settings → Update & Security → Windows Security → App & browser control → SmartScreen for apps
```

### Linux: "error while loading shared libraries"

**Проблема**: Отсутствуют системные библиотеки

**Решение**: Установите зависимости (см. раздел ["Установка системных зависимостей"](#установка-системных-зависимостей))

### macOS: "cannot be opened because the developer cannot be verified"

**Проблема**: Приложение не подписано

**Решение**: Снимите карантин с приложения:

```bash
xattr -d com.apple.quarantine ~/Applications/IvkExportTool/IvkExportTool.Desktop
```

### Размер загружаемого файла слишком большой

**Объяснение**: Приложение включает полный .NET Runtime (~50-60 MB)

**Альтернатива**: Если у вас установлен .NET 10.0 Runtime, можно собрать framework-dependent версию (~5 MB):

```bash
dotnet publish -c Release -r win-x64 --self-contained false -o ./publish/win-x64-fd
```

## Получить помощь

Если установка не удалась:

- Проверьте [раздел FAQ](User-Guide.md#faq)
- Откройте [Issue на GitHub](https://github.com/YOUR_USERNAME/ivk-export-tool/issues)
- Укажите: операционную систему, версию, текст ошибки

---

**Следующие шаги**: Перейдите к [Быстрому старту](Getting-Started.md) для первого использования приложения.
