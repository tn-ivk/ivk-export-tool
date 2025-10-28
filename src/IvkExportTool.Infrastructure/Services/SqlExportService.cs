using System.Diagnostics;
using System.Text;
using IvkExportTool.Core.Interfaces;
using IvkExportTool.Core.Models;
using MySqlConnector;

namespace IvkExportTool.Infrastructure.Services;

/// <summary>
/// Сервис для экспорта данных в SQL файл
/// </summary>
public class SqlExportService : IExportService
{
    public async Task<ExportResult> ExportAsync(
        ConnectionConfig config,
        ExportOptions options,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new ExportResult();

        try
        {
            await using var connection = new MySqlConnection(config.GetConnectionString());
            await connection.OpenAsync(cancellationToken);

            await using var fileStream = new FileStream(options.OutputPath, FileMode.Create, FileAccess.Write);
            await using var writer = new StreamWriter(fileStream, Encoding.UTF8);

            // Заголовок SQL файла
            await writer.WriteLineAsync("-- MySQL Database Export");
            await writer.WriteLineAsync($"-- Host: {config.Host}:{config.Port}");
            await writer.WriteLineAsync($"-- Database: {config.Database}");
            await writer.WriteLineAsync($"-- Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("SET NAMES utf8mb4;");
            await writer.WriteLineAsync("SET FOREIGN_KEY_CHECKS = 0;");
            await writer.WriteLineAsync();

            var totalTables = options.Tables.Count;
            var currentTable = 0;

            foreach (var tableName in options.Tables)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await ExportTableAsync(connection, writer, tableName, options, cancellationToken);

                result.TablesExported++;
                currentTable++;
                progress?.Report((int)((double)currentTable / totalTables * 100));
            }

            // Футер SQL файла
            await writer.WriteLineAsync("SET FOREIGN_KEY_CHECKS = 1;");

            result.Success = true;
            result.OutputPath = options.OutputPath;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        stopwatch.Stop();
        result.Duration = stopwatch.Elapsed;

        return result;
    }

    private async Task ExportTableAsync(
        MySqlConnection connection,
        StreamWriter writer,
        string tableName,
        ExportOptions options,
        CancellationToken cancellationToken)
    {
        await writer.WriteLineAsync($"-- ----------------------------");
        await writer.WriteLineAsync($"-- Table structure for {tableName}");
        await writer.WriteLineAsync($"-- ----------------------------");

        // DROP TABLE
        if (options.IncludeDropTable)
        {
            await writer.WriteLineAsync($"DROP TABLE IF EXISTS `{tableName}`;");
        }

        // CREATE TABLE
        if (options.IncludeStructure)
        {
            await using var createCommand = new MySqlCommand($"SHOW CREATE TABLE `{tableName}`", connection);
            await using var reader = await createCommand.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                var createTableSql = reader.GetString(1);
                await writer.WriteLineAsync(createTableSql + ";");
                await writer.WriteLineAsync();
            }
            await reader.CloseAsync();
        }

        // INSERT DATA
        if (options.IncludeData)
        {
            await writer.WriteLineAsync($"-- ----------------------------");
            await writer.WriteLineAsync($"-- Records of {tableName}");
            await writer.WriteLineAsync($"-- ----------------------------");

            await ExportTableDataAsync(connection, writer, tableName, options.BatchSize, cancellationToken);
        }

        await writer.WriteLineAsync();
    }

    private async Task ExportTableDataAsync(
        MySqlConnection connection,
        StreamWriter writer,
        string tableName,
        int batchSize,
        CancellationToken cancellationToken)
    {
        await using var selectCommand = new MySqlCommand($"SELECT * FROM `{tableName}`", connection);
        await using var reader = await selectCommand.ExecuteReaderAsync(cancellationToken);

        if (!reader.HasRows)
            return;

        var columnCount = reader.FieldCount;
        var columnNames = new string[columnCount];
        for (int i = 0; i < columnCount; i++)
        {
            columnNames[i] = reader.GetName(i);
        }

        var insertHeader = $"INSERT INTO `{tableName}` ({string.Join(", ", columnNames.Select(c => $"`{c}`"))}) VALUES";
        var rowCount = 0;
        var batchRows = new List<string>();

        while (await reader.ReadAsync(cancellationToken))
        {
            var values = new List<string>();

            for (int i = 0; i < columnCount; i++)
            {
                if (reader.IsDBNull(i))
                {
                    values.Add("NULL");
                }
                else
                {
                    var value = reader.GetValue(i);
                    var sqlValue = ConvertToSqlValue(value);
                    values.Add(sqlValue);
                }
            }

            batchRows.Add($"({string.Join(", ", values)})");
            rowCount++;

            if (rowCount % batchSize == 0)
            {
                await writer.WriteLineAsync($"{insertHeader}");
                await writer.WriteLineAsync(string.Join(",\n", batchRows) + ";");
                batchRows.Clear();
            }
        }

        // Записываем оставшиеся строки
        if (batchRows.Count > 0)
        {
            await writer.WriteLineAsync($"{insertHeader}");
            await writer.WriteLineAsync(string.Join(",\n", batchRows) + ";");
        }
    }

    private string ConvertToSqlValue(object value)
    {
        return value switch
        {
            string str => $"'{EscapeSqlString(str)}'",
            DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'",
            bool b => b ? "1" : "0",
            byte[] bytes => $"0x{BitConverter.ToString(bytes).Replace("-", "")}",
            null => "NULL",
            _ => value.ToString() ?? "NULL"
        };
    }

    private string EscapeSqlString(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("'", "\\'")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t")
            .Replace("\0", "\\0");
    }
}
