namespace IvkExportTool.Desktop.Enums;

/// <summary>
/// Тип статусного сообщения, определяющий цвет фона
/// </summary>
public enum StatusMessageType
{
    /// <summary>
    /// Нейтральное состояние (без особого цвета)
    /// </summary>
    None,

    /// <summary>
    /// Успешное выполнение (зеленый фон)
    /// </summary>
    Success,

    /// <summary>
    /// Предупреждение (желтый фон)
    /// </summary>
    Warning,

    /// <summary>
    /// Ошибка (красный фон)
    /// </summary>
    Error
}
