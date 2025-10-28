namespace IvkExportTool.Core.Models;

/// <summary>
/// Результат операции экспорта
/// </summary>
public class ExportResult
{
    /// <summary>
    /// Успешность операции
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Сообщение об ошибке (если есть)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Путь к созданному файлу
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// Количество экспортированных таблиц
    /// </summary>
    public int TablesExported { get; set; }

    /// <summary>
    /// Количество экспортированных строк
    /// </summary>
    public long RowsExported { get; set; }

    /// <summary>
    /// Время выполнения операции
    /// </summary>
    public TimeSpan Duration { get; set; }
}
