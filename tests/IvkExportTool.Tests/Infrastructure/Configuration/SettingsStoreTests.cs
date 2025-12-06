using FluentAssertions;
using IvkExportTool.Infrastructure.Configuration;

namespace IvkExportTool.Tests.Infrastructure.Configuration;

/// <summary>
/// Тесты для SettingsStore
/// </summary>
[TestFixture]
public class SettingsStoreTests
{
    private string _tempDirectory = null!;
    private string _tempSettingsPath = null!;

    [SetUp]
    public void Setup()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"IvkExportTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);
        _tempSettingsPath = Path.Combine(_tempDirectory, "settings.json");
        SettingsStore.SetSettingsPathForTesting(_tempSettingsPath);
    }

    [TearDown]
    public void TearDown()
    {
        SettingsStore.ResetSettingsPath();

        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    [Test]
    public void Load_FileNotExists_ReturnsDefaultSettings()
    {
        // Arrange - файл не существует

        // Act
        var settings = SettingsStore.Load();

        // Assert
        settings.Should().NotBeNull();
        settings.Connection.Should().NotBeNull();
        settings.Connection.Host.Should().BeNull();
        settings.Connection.Port.Should().Be(0);
        settings.Connection.Username.Should().BeNull();
        settings.Connection.EncryptedPassword.Should().BeNull();
        settings.LastExportDirectory.Should().BeNull();
    }

    [Test]
    public void Load_ValidJson_ReturnsDeserializedSettings()
    {
        // Arrange
        var json = """
            {
                "Connection": {
                    "Host": "192.168.1.100",
                    "Port": 3307,
                    "Username": "testuser",
                    "EncryptedPassword": "encrypted123"
                },
                "LastExportDirectory": "/path/to/exports"
            }
            """;
        File.WriteAllText(_tempSettingsPath, json);

        // Act
        var settings = SettingsStore.Load();

        // Assert
        settings.Should().NotBeNull();
        settings.Connection.Host.Should().Be("192.168.1.100");
        settings.Connection.Port.Should().Be(3307);
        settings.Connection.Username.Should().Be("testuser");
        settings.Connection.EncryptedPassword.Should().Be("encrypted123");
        settings.LastExportDirectory.Should().Be("/path/to/exports");
    }

    [Test]
    public void Load_InvalidJson_ReturnsDefaultSettings()
    {
        // Arrange
        File.WriteAllText(_tempSettingsPath, "{ invalid json }");

        // Act
        var settings = SettingsStore.Load();

        // Assert
        settings.Should().NotBeNull();
        settings.Connection.Should().NotBeNull();
        settings.Connection.Host.Should().BeNull();
    }

    [Test]
    public void Load_EmptyJson_ReturnsDefaultSettings()
    {
        // Arrange
        File.WriteAllText(_tempSettingsPath, "");

        // Act
        var settings = SettingsStore.Load();

        // Assert
        settings.Should().NotBeNull();
        settings.Connection.Should().NotBeNull();
    }

    [Test]
    public void Save_ThenLoad_ReturnsSameSettings()
    {
        // Arrange
        var originalSettings = new AppSettings
        {
            Connection = new ConnectionSettings
            {
                Host = "localhost",
                Port = 3306,
                Username = "admin",
                EncryptedPassword = "someEncryptedValue"
            },
            LastExportDirectory = "/home/user/exports"
        };

        // Act
        SettingsStore.Save(originalSettings);
        var loadedSettings = SettingsStore.Load();

        // Assert
        loadedSettings.Connection.Host.Should().Be(originalSettings.Connection.Host);
        loadedSettings.Connection.Port.Should().Be(originalSettings.Connection.Port);
        loadedSettings.Connection.Username.Should().Be(originalSettings.Connection.Username);
        loadedSettings.Connection.EncryptedPassword.Should().Be(originalSettings.Connection.EncryptedPassword);
        loadedSettings.LastExportDirectory.Should().Be(originalSettings.LastExportDirectory);
    }

    [Test]
    public void Save_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var nestedPath = Path.Combine(_tempDirectory, "nested", "folder", "settings.json");
        SettingsStore.SetSettingsPathForTesting(nestedPath);

        var settings = new AppSettings
        {
            LastExportDirectory = "/test/path"
        };

        // Act
        SettingsStore.Save(settings);

        // Assert
        File.Exists(nestedPath).Should().BeTrue();
    }

    [Test]
    public void Save_OverwritesExistingFile()
    {
        // Arrange
        var initialSettings = new AppSettings
        {
            LastExportDirectory = "/initial/path"
        };
        SettingsStore.Save(initialSettings);

        var updatedSettings = new AppSettings
        {
            LastExportDirectory = "/updated/path"
        };

        // Act
        SettingsStore.Save(updatedSettings);
        var loadedSettings = SettingsStore.Load();

        // Assert
        loadedSettings.LastExportDirectory.Should().Be("/updated/path");
    }

    [Test]
    public void GetSettingsFilePath_ReturnsNonEmptyPath()
    {
        // Arrange
        SettingsStore.ResetSettingsPath();

        // Act
        var path = SettingsStore.GetSettingsFilePath();

        // Assert
        path.Should().NotBeNullOrEmpty();
        path.Should().EndWith("settings.json");
        path.Should().Contain("IvkExportTool");
    }

    [Test]
    public void GetSettingsFilePath_Windows_ReturnsAppDataPath()
    {
        // Arrange
        SettingsStore.ResetSettingsPath();

        // Act
        var path = SettingsStore.GetSettingsFilePath();

        // Assert
        if (OperatingSystem.IsWindows())
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            path.Should().StartWith(appData);
        }
    }

    [Test]
    public void GetSettingsFilePath_Linux_ReturnsConfigPath()
    {
        // Arrange
        SettingsStore.ResetSettingsPath();

        // Act
        var path = SettingsStore.GetSettingsFilePath();

        // Assert
        if (!OperatingSystem.IsWindows())
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var expectedPath = Path.Combine(home, ".config", "IvkExportTool");
            path.Should().StartWith(expectedPath);
        }
    }

    [Test]
    public void Load_PartialJson_ReturnsPartialSettings()
    {
        // Arrange - только часть полей
        var json = """
            {
                "LastExportDirectory": "/partial/path"
            }
            """;
        File.WriteAllText(_tempSettingsPath, json);

        // Act
        var settings = SettingsStore.Load();

        // Assert
        settings.Should().NotBeNull();
        settings.Connection.Should().NotBeNull();
        settings.LastExportDirectory.Should().Be("/partial/path");
    }
}
