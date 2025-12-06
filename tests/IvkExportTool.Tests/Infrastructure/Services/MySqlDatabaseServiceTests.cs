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

    // Интеграционные тесты реализованы в Integration/MySqlDatabaseServiceIntegrationTests.cs
}
