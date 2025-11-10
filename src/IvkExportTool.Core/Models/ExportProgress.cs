namespace IvkExportTool.Core.Models;

/// <summary>
/// Детальная информация о прогрессе экспорта
/// </summary>
public class ExportProgress
{
    /// <summary>
    /// Имя текущей экспортируемой таблицы
    /// </summary>
    public string CurrentTable { get; set; } = string.Empty;

    /// <summary>
    /// Количество обработанных строк текущей таблицы
    /// </summary>
    public long RowsProcessed { get; set; }

    /// <summary>
    /// Общее количество строк в текущей таблице (если известно)
    /// </summary>
    public long? TotalRows { get; set; }

    /// <summary>
    /// Индекс текущей таблицы (начиная с 0)
    /// </summary>
    public int CurrentTableIndex { get; set; }

    /// <summary>
    /// Общее количество таблиц для экспорта
    /// </summary>
    public int TotalTables { get; set; }

    /// <summary>
    /// Процент выполнения (0-100)
    /// </summary>
    public int PercentComplete { get; set; }

    /// <summary>
    /// Прошедшее время с начала экспорта
    /// </summary>
    public TimeSpan Elapsed { get; set; }

    /// <summary>
    /// Текстовое описание текущего состояния
    /// </summary>
    public string StatusMessage { get; set; } = string.Empty;
}
