using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using IvkExportTool.Core.Models;
using MySqlConnector;
using Testcontainers.MySql;

namespace IvkExportTool.Tests.Integration;

/// <summary>
/// Базовый класс для интеграционных тестов с MySQL.
/// Использует Testcontainers для запуска MySQL в Docker контейнере.
/// </summary>
[TestFixture]
[Category("Integration")]
public abstract class MySqlIntegrationTestBase
{
    protected MySqlContainer MySqlContainer { get; private set; } = null!;
    protected ConnectionConfig ConnectionConfig { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Настройка Docker endpoint для Windows с Docker Desktop (WSL2)
        var dockerEndpoint = GetDockerEndpoint();

        var builder = new MySqlBuilder()
            .WithImage("mysql:8.0")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .WithEnvironment("MYSQL_ROOT_PASSWORD", "rootpassword");

        // Если есть кастомный Docker endpoint, используем его
        if (!string.IsNullOrEmpty(dockerEndpoint))
        {
            builder = builder.WithDockerEndpoint(dockerEndpoint);
        }

        MySqlContainer = builder.Build();

        await MySqlContainer.StartAsync();

        ConnectionConfig = new ConnectionConfig
        {
            Host = MySqlContainer.Hostname,
            Port = MySqlContainer.GetMappedPublicPort(3306),
            Username = "testuser",
            Password = "testpassword",
            Database = "testdb"
        };
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await MySqlContainer.DisposeAsync();
    }

    /// <summary>
    /// Создает тестовую таблицу с данными
    /// </summary>
    protected async Task CreateTestTableAsync(string tableName, int rowCount = 10)
    {
        await using var connection = new MySqlConnection(MySqlContainer.GetConnectionString());
        await connection.OpenAsync();

        // Создаем таблицу
        var createTableSql = $@"
            CREATE TABLE IF NOT EXISTS `{tableName}` (
                `id` INT AUTO_INCREMENT PRIMARY KEY,
                `name` VARCHAR(100) NOT NULL,
                `email` VARCHAR(255),
                `age` INT,
                `salary` DECIMAL(10,2),
                `is_active` BOOLEAN DEFAULT TRUE,
                `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
                `notes` TEXT,
                `binary_data` BLOB
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

        await using var createCmd = new MySqlCommand(createTableSql, connection);
        await createCmd.ExecuteNonQueryAsync();

        // Вставляем тестовые данные
        for (int i = 1; i <= rowCount; i++)
        {
            var insertSql = $@"
                INSERT INTO `{tableName}` (name, email, age, salary, is_active, notes, binary_data)
                VALUES (
                    'User {i}',
                    'user{i}@example.com',
                    {20 + i},
                    {1000.50m + i * 100},
                    {(i % 2 == 0 ? "TRUE" : "FALSE")},
                    'Notes for user {i} with special chars: ''quotes'' and \\ backslash',
                    X'48454C4C4F'
                );";

            await using var insertCmd = new MySqlCommand(insertSql, connection);
            await insertCmd.ExecuteNonQueryAsync();
        }
    }

    /// <summary>
    /// Создает несколько тестовых таблиц
    /// </summary>
    protected async Task CreateMultipleTestTablesAsync(params string[] tableNames)
    {
        foreach (var tableName in tableNames)
        {
            await CreateTestTableAsync(tableName, 5);
        }
    }

    /// <summary>
    /// Удаляет тестовую таблицу
    /// </summary>
    protected async Task DropTestTableAsync(string tableName)
    {
        await using var connection = new MySqlConnection(MySqlContainer.GetConnectionString());
        await connection.OpenAsync();

        await using var cmd = new MySqlCommand($"DROP TABLE IF EXISTS `{tableName}`", connection);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Получает количество строк в таблице
    /// </summary>
    protected async Task<long> GetRowCountAsync(string tableName)
    {
        await using var connection = new MySqlConnection(MySqlContainer.GetConnectionString());
        await connection.OpenAsync();

        await using var cmd = new MySqlCommand($"SELECT COUNT(*) FROM `{tableName}`", connection);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt64(result);
    }

    /// <summary>
    /// Создает ConnectionConfig для root пользователя
    /// </summary>
    protected ConnectionConfig GetRootConnectionConfig()
    {
        return new ConnectionConfig
        {
            Host = MySqlContainer.Hostname,
            Port = MySqlContainer.GetMappedPublicPort(3306),
            Username = "root",
            Password = "rootpassword",
            Database = "testdb"
        };
    }

    /// <summary>
    /// Создает ConnectionConfig без указания базы данных
    /// </summary>
    protected ConnectionConfig GetConnectionConfigWithoutDatabase()
    {
        return new ConnectionConfig
        {
            Host = MySqlContainer.Hostname,
            Port = MySqlContainer.GetMappedPublicPort(3306),
            Username = "testuser",
            Password = "testpassword"
        };
    }

    /// <summary>
    /// Определяет Docker endpoint для текущей системы.
    /// На Windows с Docker Desktop (WSL2) возвращает специальный pipe.
    /// </summary>
    private static string? GetDockerEndpoint()
    {
        // Проверяем переменную окружения
        var dockerHost = Environment.GetEnvironmentVariable("DOCKER_HOST");
        if (!string.IsNullOrEmpty(dockerHost))
        {
            return dockerHost;
        }

        // На Windows с Docker Desktop WSL2 используем специальный pipe
        if (OperatingSystem.IsWindows())
        {
            // Проверяем наличие Docker Desktop WSL2 pipe
            var wsl2Pipe = @"npipe://./pipe/dockerDesktopLinuxEngine";
            var standardPipe = @"npipe://./pipe/docker_engine";

            // Пробуем оба варианта - WSL2 pipe имеет приоритет
            if (File.Exists(@"\\.\pipe\dockerDesktopLinuxEngine"))
            {
                return wsl2Pipe;
            }
            if (File.Exists(@"\\.\pipe\docker_engine"))
            {
                return standardPipe;
            }

            // Если ни один pipe не найден, возвращаем WSL2 pipe как fallback
            return wsl2Pipe;
        }

        // На Linux/macOS используем стандартный unix socket
        return null;
    }
}
