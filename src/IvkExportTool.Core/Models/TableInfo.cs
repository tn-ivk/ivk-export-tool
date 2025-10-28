namespace IvkExportTool.Core.Models;

/// <summary>
/// Информация о таблице базы данных
/// </summary>
public class TableInfo
{
    /// <summary>
    /// Имя таблицы
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Количество строк в таблице
    /// </summary>
    public long RowCount { get; set; }

    /// <summary>
    /// Движок таблицы (InnoDB, MyISAM и т.д.)
    /// </summary>
    public string Engine { get; set; } = string.Empty;

    /// <summary>
    /// Размер таблицы (в байтах)
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Комментарий к таблице
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Выбрана ли таблица для экспорта
    /// </summary>
    public bool IsSelected { get; set; }
}
