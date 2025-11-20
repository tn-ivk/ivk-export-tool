# Участие в проекте

Спасибо за интерес к **IvkExportTool**! Мы приветствуем любые формы участия: от сообщений об ошибках до Pull Request'ов.

## Содержание

- [Кодекс поведения](#кодекс-поведения)
- [Как вы можете помочь](#как-вы-можете-помочь)
- [Сообщение об ошибках](#сообщение-об-ошибках)
- [Предложение улучшений](#предложение-улучшений)
- [Процесс разработки](#процесс-разработки)
- [Pull Request Guidelines](#pull-request-guidelines)
- [Стиль кода](#стиль-кода)
- [Тестирование](#тестирование)
- [Документация](#документация)

## Кодекс поведения

### Наши стандарты

Примеры поведения, которое создаёт позитивную среду:

- ✅ Использование доброжелательного и инклюзивного языка
- ✅ Уважение к различным точкам зрения и опыту
- ✅ Конструктивное принятие критики
- ✅ Фокус на том, что лучше для сообщества
- ✅ Проявление эмпатии к другим участникам

Примеры неприемлемого поведения:

- ❌ Использование сексуализированного языка или образов
- ❌ Троллинг, оскорбительные комментарии и личные атаки
- ❌ Публичное или приватное преследование
- ❌ Публикация личной информации других без разрешения

### Наши обязательства

Мы обязуемся сделать участие в этом проекте безопасным и комфортным для всех, независимо от:

- Возраста, размера тела, инвалидности
- Этнической принадлежности, гендерной идентичности и выражения
- Уровня опыта, образования, социально-экономического статуса
- Национальности, внешности, расы, религии
- Сексуальной идентичности и ориентации

## Как вы можете помочь

### Для всех

- 🐛 **Сообщайте об ошибках** — помогите нам найти баги
- 💡 **Предлагайте улучшения** — делитесь идеями новых функций
- 📖 **Улучшайте документацию** — исправляйте опечатки и дополняйте руководства
- 🌍 **Переводите** — помогите локализовать интерфейс и документацию
- 💬 **Помогайте другим** — отвечайте на вопросы в Issues

### Для разработчиков

- 🔧 **Исправляйте баги** — выбирайте Issues с меткой `good first issue`
- ✨ **Реализуйте функции** — добавляйте новые возможности
- 🧪 **Пишите тесты** — увеличивайте покрытие кода тестами
- ⚡ **Оптимизируйте производительность** — улучшайте скорость и память
- 🎨 **Улучшайте UI/UX** — делайте интерфейс удобнее

## Сообщение об ошибках

### Перед созданием Issue

1. **Проверьте существующие Issues** — возможно, проблема уже известна
2. **Обновите до последней версии** — баг может быть уже исправлен
3. **Убедитесь, что это баг** — проверьте документацию

### Как создать хороший Bug Report

Откройте [новый Issue](https://github.com/YOUR_USERNAME/ivk-export-tool/issues/new) и укажите:

**Обязательная информация**:

- **Версия приложения**: Укажите точную версию (например, v1.2.0)
- **Операционная система**: Windows 10, Ubuntu 22.04, macOS 13, и т.д.
- **Описание проблемы**: Что произошло? Что вы ожидали?
- **Шаги воспроизведения**: Как воспроизвести ошибку?

**Дополнительная информация** (желательно):

- Скриншоты или видео
- Логи ошибок
- Версия MySQL Server
- Конфигурация подключения (без паролей!)

### Пример Bug Report

```markdown
**Описание**
При экспорте таблицы размером > 1 GB приложение зависает и не отвечает.

**Шаги воспроизведения**
1. Подключиться к БД с таблицей > 1 GB
2. Выбрать таблицу для экспорта
3. Нажать "Экспортировать"
4. Приложение зависает на 5 минут и крашится

**Ожидаемое поведение**
Экспорт должен выполняться с прогрессом или разбиваться на батчи.

**Скриншоты**
[прикрепить скриншот]

**Окружение**
- Версия: v1.1.0
- ОС: Windows 11
- MySQL: 8.0.30
- Размер таблицы: 2.5 GB
```

## Предложение улучшений

### Feature Requests

Откройте [новый Issue](https://github.com/YOUR_USERNAME/ivk-export-tool/issues/new) с меткой `enhancement`:

**Укажите**:

- **Описание функции**: Что вы хотите добавить?
- **Причина**: Зачем это нужно? Какую проблему решает?
- **Альтернативы**: Рассматривали ли другие варианты?
- **Дополнительный контекст**: Примеры использования, макеты UI

### Пример Feature Request

```markdown
**Описание**
Добавить возможность экспорта в CSV формат.

**Мотивация**
Многие пользователи хотят импортировать данные в Excel или Google Sheets,
где CSV формат проще в работе, чем SQL.

**Предложенное решение**
- Добавить выбор формата при экспорте (SQL / CSV)
- Поддержка настроек: разделитель, кодировка, заголовки

**Альтернативы**
Использовать внешние инструменты для конвертации SQL → CSV

**Дополнительный контекст**
Подобная функциональность есть в MySQL Workbench и phpMyAdmin.
```

## Процесс разработки

### Настройка окружения

1. **Fork репозиторий** на GitHub

2. **Клонируйте свой fork**:

```bash
git clone https://github.com/YOUR_USERNAME/ivk-export-tool.git
cd ivk-export-tool
```

3. **Добавьте upstream**:

```bash
git remote add upstream https://github.com/ORIGINAL_OWNER/ivk-export-tool.git
```

4. **Восстановите зависимости**:

```bash
dotnet restore
```

5. **Проверьте сборку**:

```bash
dotnet build
dotnet test
```

### Workflow разработки

1. **Создайте ветку** от `main`:

```bash
git checkout -b feature/my-awesome-feature
```

Используйте префиксы:
- `feature/` — новая функциональность
- `bugfix/` — исправление бага
- `docs/` — изменения в документации
- `refactor/` — рефакторинг кода
- `test/` — добавление/изменение тестов

2. **Внесите изменения** и commit'ы:

```bash
git add .
git commit -m "feat: add CSV export format"
```

Используйте [Conventional Commits](https://www.conventionalcommits.org/).

3. **Синхронизируйтесь с upstream**:

```bash
git fetch upstream
git rebase upstream/main
```

4. **Запустите тесты и форматирование**:

```bash
dotnet test
dotnet format --verify-no-changes
```

5. **Push в свой fork**:

```bash
git push origin feature/my-awesome-feature
```

6. **Создайте Pull Request**

## Pull Request Guidelines

### Перед созданием PR

- ✅ Код проходит все тесты (`dotnet test`)
- ✅ Код отформатирован (`dotnet format`)
- ✅ Добавлены тесты для новой функциональности
- ✅ Документация обновлена (если требуется)
- ✅ Commits следуют Conventional Commits
- ✅ PR description заполнен корректно

### Шаблон PR Description

```markdown
## Описание изменений
Краткое описание того, что делает PR.

## Тип изменений
- [ ] Исправление бага (bug fix)
- [ ] Новая функциональность (feature)
- [ ] Критическое изменение (breaking change)
- [ ] Изменения в документации (documentation)

## Связанные Issues
Fixes #123
Closes #456

## Как протестировано
Опишите, как вы тестировали изменения:
- [ ] Unit тесты
- [ ] Ручное тестирование
- [ ] Тестирование на разных ОС

## Checklist
- [ ] Код следует стилю проекта
- [ ] Добавлены/обновлены тесты
- [ ] Все тесты проходят
- [ ] Документация обновлена
- [ ] Коммиты следуют Conventional Commits
```

### Code Review процесс

После создания PR:

1. **Автоматическая проверка** (CI/CD):
   - Сборка проекта
   - Запуск тестов
   - Проверка форматирования

2. **Review от maintainer'ов**:
   - Проверка кода
   - Комментарии и предложения
   - Запрос изменений (если требуется)

3. **Внесение правок** (если требуется):

```bash
# Внесите изменения
git add .
git commit -m "fix: address review comments"
git push origin feature/my-awesome-feature
```

4. **Merge**: После одобрения PR будет смержен в `main`

### Что делать, если PR не принят?

Не расстраивайтесь! Возможные причины:

- Функциональность не соответствует целям проекта
- Требуются дополнительные изменения
- Есть более подходящее решение

Maintainer объяснит причину и предложит альтернативы.

## Стиль кода

### Следуйте .editorconfig

Проект использует `.editorconfig` для единообразия:

```bash
dotnet format
```

### Naming Conventions

```csharp
// ✅ Правильно
public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;
    private const int DefaultTimeout = 30;

    public async Task<List<TableInfo>> GetTablesAsync(string databaseName)
    {
        var tables = new List<TableInfo>();
        foreach (var table in tables)
        {
            // ...
        }
        return tables;
    }
}

// ❌ Неправильно
public class databaseservice  // PascalCase для классов
{
    private readonly string connectionString;  // _ префикс для приватных полей
    private const int default_timeout = 30;  // PascalCase для констант

    public async Task<List<TableInfo>> GetTables(string database_name)  // Async суффикс
    {
        List<TableInfo> Tables = new List<TableInfo>();  // camelCase для локальных переменных
        return Tables;
    }
}
```

### XML Documentation

Добавляйте XML комментарии для публичных API:

```csharp
/// <summary>
/// Получает список таблиц из указанной базы данных.
/// </summary>
/// <param name="databaseName">Имя базы данных</param>
/// <returns>Список объектов TableInfo</returns>
/// <exception cref="MySqlException">Ошибка подключения к БД</exception>
public async Task<List<TableInfo>> GetTablesAsync(string databaseName)
{
    // ...
}
```

## Тестирование

### Покрытие тестами

- **Обязательно**: Unit тесты для новой функциональности
- **Желательно**: Integration тесты для критичных компонентов
- **Цель**: > 70% покрытия кода

### Структура тестов

```csharp
using NUnit.Framework;
using FluentAssertions;

namespace IvkExportTool.Tests.Services;

[TestFixture]
public class ExportServiceTests
{
    [Test]
    public async Task ExportAsync_ValidOptions_ReturnsSuccess()
    {
        // Arrange
        var service = new SqlExportService();
        var options = new ExportOptions { /* ... */ };

        // Act
        var result = await service.ExportAsync(options);

        // Assert
        result.Success.Should().BeTrue();
        result.FilePath.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task ExportAsync_NullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new SqlExportService();

        // Act & Assert
        await service.Invoking(s => s.ExportAsync(null!))
            .Should().ThrowAsync<ArgumentNullException>();
    }
}
```

### Мокирование

```csharp
var mockDbService = new Mock<IDatabaseService>();
mockDbService
    .Setup(x => x.GetTablesAsync(It.IsAny<string>()))
    .ReturnsAsync(new List<TableInfo> { /* ... */ });

var viewModel = new MainWindowViewModel(mockDbService.Object, ...);
```

## Документация

### Обновление Wiki

При добавлении новых функций обновите:

- **User Guide** — если функция видна пользователю
- **API Reference** — если изменились публичные API
- **Architecture** — если изменилась архитектура

### README.md

Обновите `README.md`, если:

- Изменились системные требования
- Добавлены новые зависимости
- Изменился процесс установки

### Комментарии в коде

- Избегайте очевидных комментариев
- Комментируйте **почему**, а не **что**
- Используйте XML комментарии для публичных API

```csharp
// ❌ Плохо
// Проверяем, что tables не null
if (tables != null)

// ✅ Хорошо
// Используем пагинацию для избежания OutOfMemory на больших таблицах
const int batchSize = 1000;
```

## Вопросы?

Если у вас есть вопросы:

- Откройте [Discussion](https://github.com/YOUR_USERNAME/ivk-export-tool/discussions)
- Задайте вопрос в [Issues](https://github.com/YOUR_USERNAME/ivk-export-tool/issues)
- Напишите на email: support@example.com

---

**Спасибо за ваш вклад в IvkExportTool!** 🎉
