using FluentAssertions;
using IvkExportTool.Core.Models;
using IvkExportTool.Infrastructure.Services;

namespace IvkExportTool.Tests.Infrastructure.Services;

/// <summary>
/// Тесты для MySqlDatabaseService
/// </summary>
[TestFixture]
public class MySqlDatabaseServiceTests
{
    private MySqlDatabaseService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new MySqlDatabaseService();
    }

    #region TestConnectionAsync Tests

    [Test]
    public async Task TestConnectionAsync_InvalidHost_ReturnsFalse()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = "invalid.host.that.does.not.exist.example.com",
            Port = 3306,
            Username = "test",
            Password = "test"
        };

        // Act
        var result = await _service.TestConnectionAsync(config);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task TestConnectionAsync_InvalidPort_ReturnsFalse()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = "localhost",
            Port = 1, // недопустимый порт
            Username = "test",
            Password = "test"
        };

        // Act
        var result = await _service.TestConnectionAsync(config);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task TestConnectionAsync_WithTimeout_InvalidHost_ReturnsFalse()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = "invalid.host.example.com",
            Port = 3306,
            Username = "test",
            Password = "test"
        };

        // Act
        var result = await _service.TestConnectionAsync(config, TimeSpan.FromSeconds(2), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task TestConnectionAsync_WithCancellation_RespectsToken()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = "192.168.255.255", // адрес, который вызовет таймаут
            Port = 3306,
            Username = "test",
            Password = "test"
        };

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // немедленная отмена

        // Act
        var result = await _service.TestConnectionAsync(config, TimeSpan.FromSeconds(30), cts.Token);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetDatabaseInfoAsync Tests

    [Test]
    public void GetDatabaseInfoAsync_EmptyDatabase_ThrowsArgumentException()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = "localhost",
            Port = 3306,
            Username = "root",
            Password = "password",
            Database = "" // пустая БД
        };

        // Act & Assert
        var act = async () => await _service.GetDatabaseInfoAsync(config);

        act.Should().ThrowAsync<ArgumentException>()
           .WithParameterName("config");
    }

    [Test]
    public void GetDatabaseInfoAsync_NullDatabase_ThrowsArgumentException()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = "localhost",
            Port = 3306,
            Username = "root",
            Password = "password",
            Database = null! // null БД
        };

        // Act & Assert
        var act = async () => await _service.GetDatabaseInfoAsync(config);

        act.Should().ThrowAsync<ArgumentException>()
           .WithParameterName("config");
    }

    #endregion

    #region GetTableInfoAsync Tests

    [Test]
    public void GetTableInfoAsync_EmptyDatabase_ThrowsArgumentException()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = "localhost",
            Port = 3306,
            Username = "root",
            Password = "password",
            Database = ""
        };

        // Act & Assert
        var act = async () => await _service.GetTableInfoAsync(config, "test_table");

        act.Should().ThrowAsync<ArgumentException>()
           .WithParameterName("config");
    }

    [Test]
    public void GetTableInfoAsync_NullDatabase_ThrowsArgumentException()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = "localhost",
            Port = 3306,
            Username = "root",
            Password = "password",
            Database = null!
        };

        // Act & Assert
        var act = async () => await _service.GetTableInfoAsync(config, "test_table");

        act.Should().ThrowAsync<ArgumentException>()
           .WithParameterName("config");
    }

    #endregion

    #region Integration Tests (требуют MySQL сервер)

    [Test]
    [Category("Integration")]
    [Ignore("Требуется MySQL сервер")]
    public async Task TestConnectionAsync_ValidCredentials_ReturnsTrue()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = "localhost",
            Port = 3306,
            Username = "root",
            Password = "password" // заменить на реальный пароль
        };

        // Act
        var result = await _service.TestConnectionAsync(config);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    [Category("Integration")]
    [Ignore("Требуется MySQL сервер")]
    public async Task GetDatabasesAsync_RealConnection_ReturnsDatabases()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = "localhost",
            Port = 3306,
            Username = "root",
            Password = "password"
        };

        // Act
        var databases = await _service.GetDatabasesAsync(config);

        // Assert
        databases.Should().NotBeNull();
        databases.Should().NotContain("information_schema");
        databases.Should().NotContain("mysql");
        databases.Should().NotContain("performance_schema");
        databases.Should().NotContain("sys");
    }

    [Test]
    [Category("Integration")]
    [Ignore("Требуется MySQL сервер")]
    public async Task GetDatabaseInfoAsync_RealConnection_ReturnsTableInfo()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = "localhost",
            Port = 3306,
            Username = "root",
            Password = "password",
            Database = "test" // заменить на существующую БД
        };

        // Act
        var dbInfo = await _service.GetDatabaseInfoAsync(config);

        // Assert
        dbInfo.Should().NotBeNull();
        dbInfo.Name.Should().Be("test");
        dbInfo.Tables.Should().NotBeNull();
    }

    [Test]
    [Category("Integration")]
    [Ignore("Требуется MySQL сервер")]
    public async Task GetTableInfoAsync_RealConnection_ReturnsTableInfo()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = "localhost",
            Port = 3306,
            Username = "root",
            Password = "password",
            Database = "test"
        };

        // Act
        var tableInfo = await _service.GetTableInfoAsync(config, "test_table"); // заменить на существующую таблицу

        // Assert
        tableInfo.Should().NotBeNull();
        tableInfo.Name.Should().Be("test_table");
        tableInfo.Engine.Should().NotBeNullOrEmpty();
    }

    #endregion
}
