using FluentAssertions;
using IvkExportTool.Core.Models;
using IvkExportTool.Infrastructure.Services;

namespace IvkExportTool.Tests.Integration;

/// <summary>
/// Интеграционные тесты для MySqlDatabaseService с использованием Testcontainers
/// </summary>
[TestFixture]
[Category("Integration")]
public class MySqlDatabaseServiceIntegrationTests : MySqlIntegrationTestBase
{
    private MySqlDatabaseService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new MySqlDatabaseService();
    }

    #region TestConnectionAsync

    [Test]
    public async Task TestConnectionAsync_ValidCredentials_ReturnsTrue()
    {
        // Act
        var result = await _service.TestConnectionAsync(ConnectionConfig);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task TestConnectionAsync_InvalidPassword_ReturnsFalse()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = ConnectionConfig.Host,
            Port = ConnectionConfig.Port,
            Username = ConnectionConfig.Username,
            Password = "wrongpassword"
        };

        // Act
        var result = await _service.TestConnectionAsync(config);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task TestConnectionAsync_InvalidUsername_ReturnsFalse()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = ConnectionConfig.Host,
            Port = ConnectionConfig.Port,
            Username = "nonexistentuser",
            Password = ConnectionConfig.Password
        };

        // Act
        var result = await _service.TestConnectionAsync(config);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task TestConnectionAsync_WithTimeout_ValidCredentials_ReturnsTrue()
    {
        // Act
        var result = await _service.TestConnectionAsync(
            ConnectionConfig,
            TimeSpan.FromSeconds(30),
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task TestConnectionAsync_RootUser_ReturnsTrue()
    {
        // Arrange
        var rootConfig = GetRootConnectionConfig();

        // Act
        var result = await _service.TestConnectionAsync(rootConfig);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region GetDatabasesAsync

    [Test]
    public async Task GetDatabasesAsync_ReturnsTestDatabase()
    {
        // Arrange
        var configWithoutDb = GetConnectionConfigWithoutDatabase();

        // Act
        var databases = await _service.GetDatabasesAsync(configWithoutDb);

        // Assert
        databases.Should().NotBeNull();
        databases.Should().Contain("testdb");
    }

    [Test]
    public async Task GetDatabasesAsync_ExcludesSystemDatabases()
    {
        // Arrange
        var configWithoutDb = GetConnectionConfigWithoutDatabase();

        // Act
        var databases = await _service.GetDatabasesAsync(configWithoutDb);

        // Assert
        databases.Should().NotContain("information_schema");
        databases.Should().NotContain("mysql");
        databases.Should().NotContain("performance_schema");
        databases.Should().NotContain("sys");
    }

    [Test]
    public async Task GetDatabasesAsync_ReturnsNonEmptyStrings()
    {
        // Arrange
        var configWithoutDb = GetConnectionConfigWithoutDatabase();

        // Act
        var databases = await _service.GetDatabasesAsync(configWithoutDb);

        // Assert
        databases.Should().AllSatisfy(db =>
        {
            db.Should().NotBeNullOrEmpty();
        });
    }

    #endregion

    #region GetDatabaseInfoAsync

    [Test]
    public async Task GetDatabaseInfoAsync_EmptyDatabase_ReturnsEmptyTablesList()
    {
        // Act
        var dbInfo = await _service.GetDatabaseInfoAsync(ConnectionConfig);

        // Assert
        dbInfo.Should().NotBeNull();
        dbInfo.Name.Should().Be("testdb");
        dbInfo.Tables.Should().NotBeNull();
        dbInfo.Tables.Should().BeEmpty(); // пока нет таблиц
    }

    [Test]
    public async Task GetDatabaseInfoAsync_WithTables_ReturnsTableInfo()
    {
        // Arrange
        await CreateTestTableAsync("users", 10);
        await CreateTestTableAsync("orders", 5);

        try
        {
            // Act
            var dbInfo = await _service.GetDatabaseInfoAsync(ConnectionConfig);

            // Assert
            dbInfo.Should().NotBeNull();
            dbInfo.Name.Should().Be("testdb");
            dbInfo.Tables.Should().HaveCount(2);
            dbInfo.Tables.Should().Contain(t => t.Name == "users");
            dbInfo.Tables.Should().Contain(t => t.Name == "orders");
        }
        finally
        {
            // Cleanup
            await DropTestTableAsync("users");
            await DropTestTableAsync("orders");
        }
    }

    [Test]
    public async Task GetDatabaseInfoAsync_TableHasCorrectRowCount()
    {
        // Arrange
        await CreateTestTableAsync("test_count", 15);

        try
        {
            // Act
            var dbInfo = await _service.GetDatabaseInfoAsync(ConnectionConfig);

            // Assert
            var table = dbInfo.Tables.FirstOrDefault(t => t.Name == "test_count");
            table.Should().NotBeNull();
            table!.RowCount.Should().Be(15);
        }
        finally
        {
            await DropTestTableAsync("test_count");
        }
    }

    [Test]
    public async Task GetDatabaseInfoAsync_TableHasEngine()
    {
        // Arrange
        await CreateTestTableAsync("test_engine", 1);

        try
        {
            // Act
            var dbInfo = await _service.GetDatabaseInfoAsync(ConnectionConfig);

            // Assert
            var table = dbInfo.Tables.FirstOrDefault(t => t.Name == "test_engine");
            table.Should().NotBeNull();
            table!.Engine.Should().Be("InnoDB");
        }
        finally
        {
            await DropTestTableAsync("test_engine");
        }
    }

    [Test]
    public async Task GetDatabaseInfoAsync_TableHasSize()
    {
        // Arrange
        await CreateTestTableAsync("test_size", 20);

        try
        {
            // Act
            var dbInfo = await _service.GetDatabaseInfoAsync(ConnectionConfig);

            // Assert
            var table = dbInfo.Tables.FirstOrDefault(t => t.Name == "test_size");
            table.Should().NotBeNull();
            table!.SizeBytes.Should().BeGreaterThan(0);
        }
        finally
        {
            await DropTestTableAsync("test_size");
        }
    }

    #endregion

    #region GetTableInfoAsync

    [Test]
    public async Task GetTableInfoAsync_ExistingTable_ReturnsTableInfo()
    {
        // Arrange
        await CreateTestTableAsync("single_table", 7);

        try
        {
            // Act
            var tableInfo = await _service.GetTableInfoAsync(ConnectionConfig, "single_table");

            // Assert
            tableInfo.Should().NotBeNull();
            tableInfo.Name.Should().Be("single_table");
            tableInfo.RowCount.Should().Be(7);
            tableInfo.Engine.Should().Be("InnoDB");
        }
        finally
        {
            await DropTestTableAsync("single_table");
        }
    }

    [Test]
    public async Task GetTableInfoAsync_NonExistentTable_ReturnsNull()
    {
        // Act
        var tableInfo = await _service.GetTableInfoAsync(ConnectionConfig, "nonexistent_table");

        // Assert
        tableInfo.Should().BeNull();
    }

    [Test]
    public async Task GetTableInfoAsync_TableHasSize()
    {
        // Arrange
        await CreateTestTableAsync("size_test", 50);

        try
        {
            // Act
            var tableInfo = await _service.GetTableInfoAsync(ConnectionConfig, "size_test");

            // Assert
            tableInfo.Should().NotBeNull();
            tableInfo!.SizeBytes.Should().BeGreaterThan(0);
        }
        finally
        {
            await DropTestTableAsync("size_test");
        }
    }

    #endregion
}
