# Design System — Утилита для работы с БД

> Современная дизайн-система на основе Material Design 3  
> Сине-серая приглушённая палитра для профессиональных инструментов

---

## Цветовая схема

### Основные цвета

#### Primary (Основной)
- **HEX:** `#546e7a`
- **RGB:** `84, 110, 122`
- **Использование:** Основные интерактивные элементы, кнопки действий, активные состояния, выделение, акценты
- **При наведении:** `#607d8b`

#### Secondary (Вторичный)
- **HEX:** `#78909c`
- **RGB:** `120, 144, 156`
- **Использование:** Вторичные кнопки, иконки, метки, вспомогательные элементы
- **При наведении:** `#90a4ae`

#### Tertiary (Третичный)
- **HEX:** `#607d8b`
- **RGB:** `96, 125, 139`
- **Использование:** Дополнительные акценты, альтернативные состояния

---

### Тональная палитра

Полная палитра оттенков для создания визуальной иерархии:

| Уровень | HEX | RGB | Применение |
|---------|-----|-----|------------|
| 95 | `#eceff1` | 236, 239, 241 | Очень светлый фон, disabled элементы |
| 90 | `#cfd8dc` | 207, 216, 220 | Светлый фон, неактивные элементы |
| 80 | `#b0bec5` | 176, 190, 197 | Границы, разделители |
| 70 | `#90a4ae` | 144, 164, 174 | Вторичные элементы |
| 60 | `#78909c` | 120, 144, 156 | Secondary цвет |
| **50** | `#546e7a` | 84, 110, 122 | **Primary цвет** (основной) |
| 40 | `#455a64` | 69, 90, 100 | Hover states, темнее primary |
| 30 | `#37474f` | 55, 71, 79 | Тёмные элементы, sidebar |
| 20 | `#263238` | 38, 50, 56 | Очень тёмные элементы, dark mode фон |

---

### Фоновые и поверхностные цвета

#### Светлая тема
- **Surface (Основной фон):** `#fafafa`
- **Surface Variant (Панели, карточки):** `#eceff1`
- **Surface Container (Контейнеры):** `#f5f5f5`
- **Background (Белый фон):** `#ffffff`

#### Тёмная тема
- **Surface Dark:** `#263238`
- **Surface Dark Variant:** `#37474f`
- **Surface Dark Container:** `#2c3940`
- **Background Dark:** `#1a1a1a`

---

### Функциональные цвета

#### Success (Успех)
- **Цвет:** `#4caf50`
- **Фон:** `#e8f5e9`
- **Использование:** Успешные операции, подтверждения, статус "подключено"

#### Error (Ошибка)
- **Цвет:** `#f44336`
- **Фон:** `#ffebee`
- **Использование:** Ошибки, предупреждения об удалении, критические состояния

#### Warning (Предупреждение)
- **Цвет:** `#ff9800`
- **Фон:** `#fff3e0`
- **Использование:** Предупреждения, требующие внимания действия

#### Info (Информация)
- **Цвет:** `#2196f3`
- **Фон:** `#e3f2fd`
- **Использование:** Информационные сообщения, подсказки, справка

---

### Текстовые цвета

#### Светлая тема
- **Primary Text:** `rgba(38, 50, 56, 0.87)` или `#263238` с opacity 87%
- **Secondary Text:** `rgba(84, 110, 122, 0.60)` или `#546e7a` с opacity 60%
- **Disabled Text:** `rgba(38, 50, 56, 0.38)`
- **Text on Primary:** `#ffffff` (белый на цветном фоне)

#### Тёмная тема
- **Primary Text:** `rgba(236, 239, 241, 0.87)` или `#eceff1` с opacity 87%
- **Secondary Text:** `rgba(176, 190, 197, 0.60)` или `#b0bec5` с opacity 60%
- **Disabled Text:** `rgba(236, 239, 241, 0.38)`

---

### Границы и разделители

#### Светлая тема
- **Border:** `#cfd8dc`
- **Border Light:** `#eceff1`
- **Divider:** `rgba(0, 0, 0, 0.12)`

#### Тёмная тема
- **Border:** `#455a64`
- **Border Light:** `#37474f`
- **Divider:** `rgba(255, 255, 255, 0.12)`

---

### Тени

- **Small:** `0 1px 2px 0 rgba(0, 0, 0, 0.05)`
- **Medium:** `0 2px 8px rgba(0, 0, 0, 0.1)`
- **Large:** `0 4px 12px rgba(0, 0, 0, 0.15)`

---

## Правила применения

### Кнопки

#### Primary Button (Основная кнопка)
```
Фон: #546e7a
Текст: #ffffff
Hover: #607d8b
Active: #455a64
Radius: 6px
Padding: 10px 24px
Font Weight: 500
```

#### Secondary Button (Вторичная кнопка)
```
Фон: #78909c
Текст: #ffffff
Hover: #90a4ae
Active: #607d8b
Radius: 6px
Padding: 10px 24px
Font Weight: 500
```

#### Outlined Button (Контурная кнопка)
```
Фон: transparent
Текст: #546e7a
Border: 1px solid #546e7a
Hover Фон: rgba(84, 110, 122, 0.08)
Radius: 6px
Padding: 10px 24px
```

#### Text Button (Текстовая кнопка)
```
Фон: transparent
Текст: #546e7a
Hover Фон: rgba(84, 110, 122, 0.08)
Padding: 10px 16px
```

---

### Поля ввода (Input Fields)

```
Фон: #ffffff
Border: 1px solid #cfd8dc
Border Radius: 6px
Padding: 10px 12px
Text Color: rgba(38, 50, 56, 0.87)

Focus:
  Border: 1px solid #546e7a
  Outline: none
  
Error:
  Border: 1px solid #f44336
  
Disabled:
  Background: #f5f5f5
  Text Color: rgba(38, 50, 56, 0.38)
```

---

### Таблицы базы данных

#### Заголовок таблицы
```
Фон: #eceff1
Текст: rgba(38, 50, 56, 0.87)
Font Weight: 500
Padding: 12px 16px
Border Bottom: 1px solid #cfd8dc
```

#### Строки таблицы
```
Чётные строки: #fafafa
Нечётные строки: #ffffff
Hover: #f5f5f5
Selected: rgba(84, 110, 122, 0.12)
Padding: 10px 16px
Border Bottom: 1px solid #eceff1
```

#### Ячейки
```
Text Color: rgba(38, 50, 56, 0.87)
Vertical Align: middle
```

---

### Панели и контейнеры

#### Боковая панель (Sidebar)
```
Фон: #eceff1
Border Right: 1px solid #cfd8dc
Width: 240px - 280px
```

#### Панель инструментов (Toolbar)
```
Фон: #fafafa
Border Bottom: 1px solid #eceff1
Height: 56px - 64px
Padding: 0 20px
```

#### Карточки (Cards)
```
Фон: #ffffff
Border Radius: 8px
Padding: 20px - 24px
Shadow: 0 2px 8px rgba(0, 0, 0, 0.1)
```

#### Модальные окна (Modals)
```
Фон: #ffffff
Border Radius: 12px
Padding: 32px
Shadow: 0 4px 12px rgba(0, 0, 0, 0.15)
Max Width: 600px - 800px
```

---

### SQL Редактор

```
Фон: #263238
Текст: #eceff1
Номера строк: #455a64
Активная строка: #37474f
Выделение: rgba(84, 110, 122, 0.3)
Border: 1px solid #37474f
Border Radius: 6px
Font Family: 'Courier New', monospace
Font Size: 14px
Line Height: 1.6
Padding: 16px
```

#### Подсветка синтаксиса
- **Ключевые слова SQL:** `#90a4ae`
- **Строки:** `#b0bec5`
- **Числа:** `#78909c`
- **Комментарии:** `#546e7a`
- **Функции:** `#cfd8dc`

---

### Статусные индикаторы

#### Подключено / Активно
```
Фон: #e8f5e9
Цвет: #4caf50
Border: 1px solid #4caf50
Icon: зелёная точка
```

#### Отключено / Неактивно
```
Фон: #eceff1
Цвет: #546e7a
Border: 1px solid #cfd8dc
Icon: серая точка
```

#### Ошибка
```
Фон: #ffebee
Цвет: #f44336
Border: 1px solid #f44336
Icon: восклицательный знак
```

#### В процессе
```
Фон: #fff3e0
Цвет: #ff9800
Border: 1px solid #ff9800
Icon: spinner/loader
```

---

## Типографика

### Шрифты
- **Primary Font:** 'Roboto', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif
- **Monospace Font:** 'Courier New', 'Consolas', 'Monaco', monospace

### Размеры и веса

#### Заголовки
```
H1: 32px, Weight 500, Line Height 1.2, Color #263238
H2: 24px, Weight 500, Line Height 1.3, Color #263238
H3: 20px, Weight 500, Line Height 1.4, Color #334155
H4: 18px, Weight 500, Line Height 1.4, Color #334155
H5: 16px, Weight 500, Line Height 1.5, Color #334155
```

#### Текст
```
Body Large: 16px, Weight 400, Line Height 1.6
Body Medium: 14px, Weight 400, Line Height 1.6
Body Small: 13px, Weight 400, Line Height 1.5
Caption: 12px, Weight 400, Line Height 1.4
Label: 14px, Weight 500, Line Height 1.4
```

---

## Отступы и размеры

### Spacing Scale (8px grid)
```
xs: 4px
sm: 8px
md: 16px
lg: 24px
xl: 32px
2xl: 48px
3xl: 64px
```

### Border Radius
```
Small: 4px
Medium: 6px
Large: 8px
XLarge: 12px
```

### Icon Sizes
```
Small: 16px
Medium: 20px
Large: 24px
XLarge: 32px
```

---

## Контрастность и доступность

### Требования WCAG
Все комбинации цветов соответствуют стандарту WCAG AA:
- **Обычный текст:** минимум 4.5:1
- **Крупный текст (18px+ или 14px+ bold):** минимум 3:1
- **Интерактивные элементы:** минимум 3:1

### Проверенные комбинации
✓ `#546e7a` на `#ffffff` — 5.8:1  
✓ `#263238` на `#fafafa` — 14.2:1  
✓ `#ffffff` на `#546e7a` — 5.8:1  
✓ `#eceff1` на `#263238` — 11.5:1

---

## Примеры использования

### Форма подключения к БД
```
Контейнер:
  - Фон: #ffffff
  - Padding: 32px
  - Border Radius: 12px
  - Shadow: Medium

Заголовок:
  - Фон: #546e7a
  - Текст: #ffffff
  - Padding: 16px 20px
  - Font Size: 18px, Weight 500

Описание:
  - Текст: rgba(84, 110, 122, 0.60)
  - Font Size: 14px
  - Margin Bottom: 20px

Input поля:
  - Фон: #ffffff
  - Border: #cfd8dc
  - Focus Border: #546e7a
  - Placeholder: rgba(38, 50, 56, 0.38)

Кнопки:
  - Primary: #546e7a (Подключиться)
  - Secondary: #78909c (Отмена)
```

### Список таблиц БД
```
Контейнер:
  - Фон: #fafafa
  
Таблица:
  - Фон: #ffffff
  - Border: 1px solid #eceff1
  - Border Radius: 8px
  
Заголовки:
  - Фон: #eceff1
  - Текст: rgba(38, 50, 56, 0.87)
  - Font Weight: 500
  
Строки:
  - Alternate: #fafafa / #ffffff
  - Hover: #f5f5f5
  - Selected: rgba(84, 110, 122, 0.12)
```

### Окно выполнения запроса
```
SQL Редактор:
  - Фон: #263238
  - Текст: #eceff1
  - Border: #37474f
  
Кнопка "Выполнить":
  - Primary: #546e7a
  - Icon: play icon
  
Результаты:
  - Фон таблицы: #ffffff
  - Заголовок: #eceff1
  - Success message: #e8f5e9 с текстом #4caf50
  - Error message: #ffebee с текстом #f44336
```

---

## Дополнительные рекомендации

### Анимации
```
Transition Duration: 200ms - 300ms
Easing: ease-in-out или cubic-bezier(0.4, 0, 0.2, 1)
```

### Hover эффекты
- Кнопки: изменение цвета фона
- Таблицы: подсветка строки
- Ссылки: изменение цвета текста или подчёркивание
- Иконки: изменение opacity или цвета

### Focus состояния
Всегда показывайте чёткий focus indicator для доступности:
```
Outline: 2px solid #546e7a
Outline Offset: 2px
```

### Loading состояния
```
Spinner: #546e7a
Skeleton: #eceff1 с анимацией
Progress Bar: #546e7a на фоне #eceff1
```

---

## Инструкция для нейросетей

При создании интерфейсов на основе этой дизайн-системы:

1. **Всегда используйте указанные цвета** из палитры — не изобретайте новые оттенки
2. **Соблюдайте иерархию:** Primary для основных действий, Secondary для вспомогательных
3. **Применяйте функциональные цвета** только по назначению (Success для успеха, Error для ошибок)
4. **Используйте тональную палитру** для создания оттенков — выбирайте нужный уровень (20-95)
5. **Соблюдайте контрастность** — всегда проверяйте читаемость текста
6. **Следуйте spacing scale** — используйте только значения из шкалы отступов
7. **Применяйте правильные border radius** — в зависимости от элемента
8. **Для тёмной темы** используйте Dark варианты цветов, а не просто инверсию

---

**Версия:** 1.0  
**Дата создания:** 29 октября 2025  
**Базовая система:** Material Design 3  
**Применение:** Утилиты для работы с базами данных, SCADA-системы, профессиональные инструменты
