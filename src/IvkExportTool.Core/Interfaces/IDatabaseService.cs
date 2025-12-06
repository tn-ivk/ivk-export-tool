using IvkExportTool.Core.Models;

namespace IvkExportTool.Core.Interfaces;

/// <summary>
/// Сервис для работы с базой данных MySQL
/// </summary>
public interface IDatabaseService
{
    /// <summary>
    /// Тестирование подключения к базе данных
    /// </summary>
    /// <param name="config">Конфигурация подключения</param>
    /// <returns>True если подключение успешно</returns>
    Task<bool> TestConnectionAsync(ConnectionConfig config);

    /// <summary>
    /// Тестирование подключения к базе данных с таймаутом и возможностью отмены
    /// </summary>
    /// <param name="config">Конфигурация подключения</param>
    /// <param name="timeout">Таймаут подключения</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>True если подключение успешно</returns>
    Task<bool> TestConnectionAsync(ConnectionConfig config, TimeSpan timeout, CancellationToken cancellationToken);

    /// <summary>
    /// Получение списка баз данных на сервере
    /// </summary>
    /// <param name="config">Конфигурация подключения</param>
    /// <returns>Список имен баз данных</returns>
    Task<List<string>> GetDatabasesAsync(ConnectionConfig config);

    /// <summary>
    /// Получение информации о базе данных и её таблицах
    /// </summary>
    /// <param name="config">Конфигурация подключения</param>
    /// <returns>Информация о базе данных</returns>
    Task<DatabaseInfo> GetDatabaseInfoAsync(ConnectionConfig config);

    /// <summary>
    /// Получение информации о конкретной таблице
    /// </summary>
    /// <param name="config">Конфигурация подключения</param>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Информация о таблице</returns>
    Task<TableInfo> GetTableInfoAsync(ConnectionConfig config, string tableName);
}
