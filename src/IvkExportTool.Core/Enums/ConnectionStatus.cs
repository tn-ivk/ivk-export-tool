namespace IvkExportTool.Core.Enums;

/// <summary>
/// Статус подключения к базе данных
/// </summary>
public enum ConnectionStatus
{
    /// <summary>
    /// Не подключено
    /// </summary>
    Disconnected = 0,

    /// <summary>
    /// Подключение...
    /// </summary>
    Connecting = 1,

    /// <summary>
    /// Подключено
    /// </summary>
    Connected = 2,

    /// <summary>
    /// Ошибка подключения
    /// </summary>
    Error = 3
}
