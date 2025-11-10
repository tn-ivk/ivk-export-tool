using IvkExportTool.Core.Models;

namespace IvkExportTool.Core.Interfaces;

/// <summary>
/// Сервис для экспорта данных из базы данных
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Экспорт данных в соответствии с заданными опциями
    /// </summary>
    /// <param name="config">Конфигурация подключения</param>
    /// <param name="options">Опции экспорта</param>
    /// <param name="progress">Callback для отслеживания прогресса (процент выполнения)</param>
    /// <param name="detailedProgress">Callback для отслеживания детального прогресса (таблица, строки, время)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат экспорта</returns>
    Task<ExportResult> ExportAsync(
        ConnectionConfig config,
        ExportOptions options,
        IProgress<int>? progress = null,
        IProgress<ExportProgress>? detailedProgress = null,
        CancellationToken cancellationToken = default);
}
