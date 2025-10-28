namespace IvkExportTool.Core.Models;

/// <summary>
/// Информация о базе данных
/// </summary>
public class DatabaseInfo
{
    /// <summary>
    /// Имя базы данных
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Список таблиц в базе данных
    /// </summary>
    public List<TableInfo> Tables { get; set; } = new();
}
