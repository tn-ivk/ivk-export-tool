using System.Threading.Tasks;
using IvkExportTool.Core.Models;

namespace IvkExportTool.Core.Interfaces;

/// <summary>
/// Сервис для работы с настройками приложения.
/// </summary>
public interface IAppSettingsService
{
    /// <summary>
    /// Загружает настройки подключения из хранилища.
    /// </summary>
    Task<ConnectionConfig> LoadConnectionAsync();

    /// <summary>
    /// Сохраняет настройки подключения в хранилище.
    /// </summary>
    Task SaveConnectionAsync(ConnectionConfig config);
}

