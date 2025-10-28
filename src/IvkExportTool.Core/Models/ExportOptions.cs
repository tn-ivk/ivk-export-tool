using IvkExportTool.Core.Enums;

namespace IvkExportTool.Core.Models;

/// <summary>
/// Опции экспорта данных
/// </summary>
public class ExportOptions
{
    /// <summary>
    /// Путь к файлу для сохранения
    /// </summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// Формат экспорта
    /// </summary>
    public ExportFormat Format { get; set; } = ExportFormat.Sql;

    /// <summary>
    /// Список таблиц для экспорта
    /// </summary>
    public List<string> Tables { get; set; } = new();

    /// <summary>
    /// Включить структуру таблиц (CREATE TABLE)
    /// </summary>
    public bool IncludeStructure { get; set; } = true;

    /// <summary>
    /// Включить данные (INSERT)
    /// </summary>
    public bool IncludeData { get; set; } = true;

    /// <summary>
    /// Включить DROP TABLE перед CREATE
    /// </summary>
    public bool IncludeDropTable { get; set; } = true;

    /// <summary>
    /// Размер пакета для INSERT (для больших таблиц)
    /// </summary>
    public int BatchSize { get; set; } = 1000;
}
