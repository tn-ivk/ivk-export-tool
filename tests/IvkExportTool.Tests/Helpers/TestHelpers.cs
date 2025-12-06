using IvkExportTool.Core.Models;

namespace IvkExportTool.Tests.Helpers;

/// <summary>
/// Вспомогательные методы для создания тестовых объектов
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Создает тестовый объект TableInfo
    /// </summary>
    public static TableInfo CreateTableInfo(
        string name = "test_table",
        long rowCount = 100,
        long sizeBytes = 1024,
        string engine = "InnoDB",
        bool isSelected = false,
        string? comment = null)
    {
        return new TableInfo
        {
            Name = name,
            RowCount = rowCount,
            SizeBytes = sizeBytes,
            Engine = engine,
            IsSelected = isSelected,
            Comment = comment
        };
    }

    /// <summary>
    /// Создает тестовый объект ConnectionConfig
    /// </summary>
    public static ConnectionConfig CreateConnectionConfig(
        string host = "localhost",
        int port = 3306,
        string username = "root",
        string password = "password",
        string database = "testdb")
    {
        return new ConnectionConfig
        {
            Host = host,
            Port = port,
            Username = username,
            Password = password,
            Database = database
        };
    }

    /// <summary>
    /// Создает временную директорию для тестов
    /// </summary>
    public static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"IvkExportTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>
    /// Безопасно удаляет временную директорию
    /// </summary>
    public static void DeleteTempDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }
}
