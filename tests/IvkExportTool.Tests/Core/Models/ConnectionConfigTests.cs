using FluentAssertions;
using IvkExportTool.Core.Models;

namespace IvkExportTool.Tests.Core.Models;

[TestFixture]
public class ConnectionConfigTests
{
    [Test]
    public void GetConnectionString_WithAllParameters_ReturnsCorrectString()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = "localhost",
            Port = 3307,
            Username = "admin",
            Password = "secret",
            Database = "testdb"
        };

        // Act
        var result = config.GetConnectionString();

        // Assert
        result.Should().Contain("Server=localhost");
        result.Should().Contain("Port=3307");
        result.Should().Contain("User ID=admin");
        result.Should().Contain("Password=secret");
        result.Should().Contain("Database=testdb");
        result.Should().Contain("CharSet=utf8mb4");
    }

    [Test]
    public void GetConnectionString_WithoutDatabase_ExcludesDatabaseFromString()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = "localhost",
            Port = 3306,
            Username = "root",
            Password = "password",
            Database = string.Empty
        };

        // Act
        var result = config.GetConnectionString();

        // Assert
        result.Should().NotContain("Database=");
        result.Should().Contain("Server=localhost");
        result.Should().Contain("Port=3306");
        result.Should().Contain("User ID=root");
        result.Should().Contain("Password=password");
        result.Should().Contain("CharSet=utf8mb4");
    }

    [Test]
    public void GetConnectionString_WithEmptyPassword_IncludesEmptyPassword()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = "localhost",
            Port = 3306,
            Username = "admin",
            Password = string.Empty,
            Database = "testdb"
        };

        // Act
        var result = config.GetConnectionString();

        // Assert
        result.Should().Contain("Password=;");
    }

    [Test]
    public void GetConnectionString_AlwaysIncludesCharset()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = "127.0.0.1",
            Port = 3306,
            Username = "user",
            Password = "pass"
        };

        // Act
        var result = config.GetConnectionString();

        // Assert
        result.Should().EndWith("CharSet=utf8mb4;");
    }

    [Test]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var config = new ConnectionConfig();

        // Assert
        config.Host.Should().Be("192.168.233.101");
        config.Port.Should().Be(3306);
        config.Username.Should().BeEmpty();
        config.Password.Should().BeEmpty();
        config.Database.Should().BeEmpty();
    }

    [Test]
    public void GetConnectionString_WithSpecialCharactersInPassword_IncludesPasswordAsIs()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = "localhost",
            Port = 3306,
            Username = "admin",
            Password = "p@ss=word;test",
            Database = "testdb"
        };

        // Act
        var result = config.GetConnectionString();

        // Assert
        result.Should().Contain("Password=p@ss=word;test");
    }
}
