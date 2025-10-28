using IvkExportTool.Core.Interfaces;
using IvkExportTool.Core.Models;
using MySqlConnector;

namespace IvkExportTool.Infrastructure.Services;

/// <summary>
/// Сервис для работы с базой данных MySQL
/// </summary>
public class MySqlDatabaseService : IDatabaseService
{
    public async Task<bool> TestConnectionAsync(ConnectionConfig config)
    {
        try
        {
            await using var connection = new MySqlConnection(config.GetConnectionString());
            await connection.OpenAsync();
            return connection.State == System.Data.ConnectionState.Open;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<string>> GetDatabasesAsync(ConnectionConfig config)
    {
        var databases = new List<string>();

        await using var connection = new MySqlConnection(config.GetConnectionString());
        await connection.OpenAsync();

        await using var command = new MySqlCommand("SHOW DATABASES", connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var dbName = reader.GetString(0);
            // Исключаем системные базы данных
            if (dbName != "information_schema" && dbName != "mysql" &&
                dbName != "performance_schema" && dbName != "sys")
            {
                databases.Add(dbName);
            }
        }

        return databases;
    }

    public async Task<DatabaseInfo> GetDatabaseInfoAsync(ConnectionConfig config)
    {
        if (string.IsNullOrEmpty(config.Database))
        {
            throw new ArgumentException("Database name must be specified", nameof(config));
        }

        var dbInfo = new DatabaseInfo
        {
            Name = config.Database
        };

        await using var connection = new MySqlConnection(config.GetConnectionString());
        await connection.OpenAsync();

        // Получаем список таблиц с информацией
        var query = @"
            SELECT
                TABLE_NAME,
                ENGINE,
                TABLE_ROWS,
                DATA_LENGTH + INDEX_LENGTH as SIZE_BYTES,
                TABLE_COMMENT
            FROM information_schema.TABLES
            WHERE TABLE_SCHEMA = @database
            ORDER BY TABLE_NAME";

        await using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@database", config.Database);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var tableInfo = new TableInfo
            {
                Name = reader.GetString("TABLE_NAME"),
                Engine = reader.IsDBNull(reader.GetOrdinal("ENGINE")) ? string.Empty : reader.GetString("ENGINE"),
                RowCount = reader.IsDBNull(reader.GetOrdinal("TABLE_ROWS")) ? 0 : reader.GetInt64("TABLE_ROWS"),
                SizeBytes = reader.IsDBNull(reader.GetOrdinal("SIZE_BYTES")) ? 0 : reader.GetInt64("SIZE_BYTES"),
                Comment = reader.IsDBNull(reader.GetOrdinal("TABLE_COMMENT")) ? null : reader.GetString("TABLE_COMMENT")
            };

            dbInfo.Tables.Add(tableInfo);
        }

        return dbInfo;
    }

    public async Task<TableInfo> GetTableInfoAsync(ConnectionConfig config, string tableName)
    {
        if (string.IsNullOrEmpty(config.Database))
        {
            throw new ArgumentException("Database name must be specified", nameof(config));
        }

        await using var connection = new MySqlConnection(config.GetConnectionString());
        await connection.OpenAsync();

        var query = @"
            SELECT
                TABLE_NAME,
                ENGINE,
                TABLE_ROWS,
                DATA_LENGTH + INDEX_LENGTH as SIZE_BYTES,
                TABLE_COMMENT
            FROM information_schema.TABLES
            WHERE TABLE_SCHEMA = @database AND TABLE_NAME = @tableName";

        await using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@database", config.Database);
        command.Parameters.AddWithValue("@tableName", tableName);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new TableInfo
            {
                Name = reader.GetString("TABLE_NAME"),
                Engine = reader.IsDBNull(reader.GetOrdinal("ENGINE")) ? string.Empty : reader.GetString("ENGINE"),
                RowCount = reader.IsDBNull(reader.GetOrdinal("TABLE_ROWS")) ? 0 : reader.GetInt64("TABLE_ROWS"),
                SizeBytes = reader.IsDBNull(reader.GetOrdinal("SIZE_BYTES")) ? 0 : reader.GetInt64("SIZE_BYTES"),
                Comment = reader.IsDBNull(reader.GetOrdinal("TABLE_COMMENT")) ? null : reader.GetString("TABLE_COMMENT")
            };
        }

        throw new ArgumentException($"Table '{tableName}' not found", nameof(tableName));
    }
}
