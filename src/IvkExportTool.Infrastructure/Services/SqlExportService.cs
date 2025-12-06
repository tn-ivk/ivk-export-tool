using System.Diagnostics;
using System.Globalization;
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
        IProgress<ExportProgress>? detailedProgress = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new ExportResult();

        try
        {
            await using var connection = new MySqlConnection(config.GetConnectionString());
            await connection.OpenAsync(cancellationToken);

            await using var fileStream = new FileStream(options.OutputPath, FileMode.Create, FileAccess.Write);
            await using var writer = new StreamWriter(
                fileStream,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                bufferSize: 64 * 1024); // 64KB буфер для оптимизации I/O

            // Заголовок SQL файла
            await writer.WriteLineAsync("-- MySQL Database Export");
            await writer.WriteLineAsync($"-- Host: {config.Host}:{config.Port}");
            await writer.WriteLineAsync($"-- Database: {config.Database}");
            await writer.WriteLineAsync($"-- Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("SET NAMES utf8;");
            await writer.WriteLineAsync("SET FOREIGN_KEY_CHECKS = 0;");
            await writer.WriteLineAsync();

            var totalTables = options.Tables.Count;
            var currentTableIndex = 0;

            foreach (var tableName in options.Tables)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var tableProgress = new ExportProgress
                {
                    CurrentTable = tableName,
                    CurrentTableIndex = currentTableIndex,
                    TotalTables = totalTables,
                    Elapsed = stopwatch.Elapsed
                };

                await ExportTableAsync(connection, writer, tableName, options, tableProgress, detailedProgress, stopwatch, cancellationToken);

                result.TablesExported++;
                currentTableIndex++;
                progress?.Report((int)((double)currentTableIndex / totalTables * 100));
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
        ExportProgress tableProgress,
        IProgress<ExportProgress>? detailedProgress,
        Stopwatch stopwatch,
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

            await ExportTableDataAsync(connection, writer, tableName, options.BatchSize, tableProgress, detailedProgress, stopwatch, cancellationToken);
        }

        await writer.WriteLineAsync();
    }

    private async Task ExportTableDataAsync(
        MySqlConnection connection,
        StreamWriter writer,
        string tableName,
        int batchSize,
        ExportProgress tableProgress,
        IProgress<ExportProgress>? detailedProgress,
        Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        // 1) Посчитаем общее количество строк для корректного процента
        await using (var countCommand = new MySqlCommand($"SELECT COUNT(*) FROM `{tableName}`", connection))
        {
            var totalRowsObj = await countCommand.ExecuteScalarAsync(cancellationToken);
            if (totalRowsObj != null && totalRowsObj != DBNull.Value)
            {
                tableProgress.TotalRows = Convert.ToInt64(totalRowsObj);
            }
        }

        // 2) Начальный отчёт (0 строк), чтобы UI сразу показал активную таблицу
        tableProgress.RowsProcessed = 0;
        tableProgress.PercentComplete = CalculatePercent(tableProgress);
        tableProgress.StatusMessage = $"Экспорт таблицы {tableName}: 0 строк";
        tableProgress.Elapsed = stopwatch.Elapsed;
        detailedProgress?.Report(tableProgress);

        await using var selectCommand = new MySqlCommand($"SELECT * FROM `{tableName}`", connection);
        selectCommand.CommandTimeout = 3600; // 1 час для больших таблиц
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
        var rowCount = 0L;
        var currentBatchRow = 0;
        var lastProgressUpdate = stopwatch.Elapsed;
        const int progressUpdateIntervalMs = 1000; // обновление прогресса каждую секунду

        while (await reader.ReadAsync(cancellationToken))
        {
            // Начало нового INSERT батча
            if (currentBatchRow == 0)
            {
                if (rowCount > 0)
                {
                    await writer.WriteLineAsync(";");  // завершаем предыдущий INSERT
                    await writer.WriteLineAsync();
                }

                await writer.WriteAsync(insertHeader);
                await writer.WriteLineAsync();
            }

            // Запись строки напрямую в writer без промежуточного List
            if (currentBatchRow > 0)
            {
                await writer.WriteLineAsync(",");
            }

            await writer.WriteAsync("(");

            for (int i = 0; i < columnCount; i++)
            {
                if (i > 0)
                {
                    await writer.WriteAsync(", ");
                }

                if (reader.IsDBNull(i))
                {
                    await writer.WriteAsync("NULL");
                }
                else
                {
                    var value = reader.GetValue(i);
                    var sqlValue = ConvertToSqlValue(value);
                    await writer.WriteAsync(sqlValue);
                }
            }

            await writer.WriteAsync(")");

            rowCount++;
            currentBatchRow++;

            if (currentBatchRow >= batchSize)
            {
                currentBatchRow = 0;
            }

            // Отчет о прогрессе каждую секунду
            var elapsed = stopwatch.Elapsed;
            if ((elapsed - lastProgressUpdate).TotalMilliseconds >= progressUpdateIntervalMs)
            {
                tableProgress.RowsProcessed = rowCount;
                tableProgress.Elapsed = elapsed;
                tableProgress.PercentComplete = CalculatePercent(tableProgress);
                tableProgress.StatusMessage = $"Экспорт таблицы {tableName}: {rowCount:N0} строк";
                detailedProgress?.Report(tableProgress);
                lastProgressUpdate = elapsed;
            }
        }

        // Финальный отчет о прогрессе
        if (rowCount > 0)
        {
            tableProgress.RowsProcessed = rowCount;
            tableProgress.Elapsed = stopwatch.Elapsed;
            tableProgress.PercentComplete = CalculatePercent(tableProgress);
            tableProgress.StatusMessage = $"Завершен экспорт таблицы {tableName}: {rowCount:N0} строк";
            detailedProgress?.Report(tableProgress);
        }

        // Завершаем последний INSERT если были строки
        if (rowCount > 0)
        {
            await writer.WriteLineAsync(";");
        }
    }

    private int CalculatePercent(ExportProgress progress)
    {
        if (progress.TotalTables == 0)
            return 0;

        // Процент = (завершенные таблицы + прогресс текущей таблицы) / общее количество таблиц * 100
        var completedTables = progress.CurrentTableIndex;
        var currentTableProgress = 0.0;

        if (progress.TotalRows.HasValue && progress.TotalRows.Value > 0)
        {
            currentTableProgress = (double)progress.RowsProcessed / progress.TotalRows.Value;
        }

        var totalProgress = (completedTables + currentTableProgress) / progress.TotalTables;
        return (int)(totalProgress * 100);
    }

    private string ConvertToSqlValue(object value)
    {
        return value switch
        {
            string str => BuildEscapedString(str),
            DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'",
            bool b => b ? "1" : "0",
            byte[] bytes => BuildHexString(bytes),
            null => "NULL",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? "NULL",
            _ => value.ToString() ?? "NULL"
        };
    }

    private string BuildEscapedString(string value)
    {
        var sb = new StringBuilder(value.Length + 20);
        sb.Append('\'');

        foreach (var ch in value)
        {
            switch (ch)
            {
                case '\\':
                    sb.Append("\\\\");
                    break;
                case '\'':
                    sb.Append("\\'");
                    break;
                case '"':
                    sb.Append("\\\"");
                    break;
                case '\n':
                    sb.Append("\\n");
                    break;
                case '\r':
                    sb.Append("\\r");
                    break;
                case '\t':
                    sb.Append("\\t");
                    break;
                case '\0':
                    sb.Append("\\0");
                    break;
                default:
                    sb.Append(ch);
                    break;
            }
        }

        sb.Append('\'');
        return sb.ToString();
    }

    private string BuildHexString(byte[] bytes)
    {
        if (bytes.Length == 0)
            return "X''";

        var sb = new StringBuilder(bytes.Length * 2 + 2);
        sb.Append("0x");

        foreach (var b in bytes)
        {
            sb.Append(b.ToString("X2"));
        }

        return sb.ToString();
    }
}
