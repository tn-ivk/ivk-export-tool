using FluentAssertions;
using IvkExportTool.Core.Models;
using IvkExportTool.Infrastructure.Configuration;
using IvkExportTool.Infrastructure.Services;

namespace IvkExportTool.Tests.Infrastructure.Services;

/// <summary>
/// Тесты для AppSettingsService
/// </summary>
[TestFixture]
public class AppSettingsServiceTests
{
    private string _tempDirectory = null!;
    private string _tempSettingsPath = null!;
    private AppSettingsService _service = null!;

    [SetUp]
    public void Setup()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"IvkExportTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);
        _tempSettingsPath = Path.Combine(_tempDirectory, "settings.json");
        SettingsStore.SetSettingsPathForTesting(_tempSettingsPath);

        _service = new AppSettingsService();
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

    #region LoadConnectionAsync Tests

    [Test]
    public async Task LoadConnectionAsync_NoSettings_ReturnsDefaultConfig()
    {
        // Act
        var config = await _service.LoadConnectionAsync();

        // Assert
        config.Should().NotBeNull();
        config.Host.Should().BeEmpty();
        config.Port.Should().Be(3306); // дефолтный порт
        config.Username.Should().BeEmpty();
        config.Password.Should().BeEmpty();
    }

    [Test]
    public async Task LoadConnectionAsync_WithSavedSettings_ReturnsCorrectConfig()
    {
        // Arrange
        var originalConfig = new ConnectionConfig
        {
            Host = "192.168.1.100",
            Port = 3307,
            Username = "admin",
            Password = "secret123"
        };

        await _service.SaveConnectionAsync(originalConfig);

        // Создаём новый сервис для загрузки (эмуляция перезапуска)
        var newService = new AppSettingsService();

        // Act
        var loadedConfig = await newService.LoadConnectionAsync();

        // Assert
        loadedConfig.Host.Should().Be("192.168.1.100");
        loadedConfig.Port.Should().Be(3307);
        loadedConfig.Username.Should().Be("admin");
        loadedConfig.Password.Should().Be("secret123");
    }

    #endregion

    #region SaveConnectionAsync Tests

    [Test]
    public async Task SaveConnectionAsync_ThenLoad_ReturnsSameConfig()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = "localhost",
            Port = 3306,
            Username = "root",
            Password = "password123"
        };

        // Act
        await _service.SaveConnectionAsync(config);
        var loadedConfig = await _service.LoadConnectionAsync();

        // Assert
        loadedConfig.Host.Should().Be(config.Host);
        loadedConfig.Port.Should().Be(config.Port);
        loadedConfig.Username.Should().Be(config.Username);
        loadedConfig.Password.Should().Be(config.Password);
    }

    [Test]
    public async Task SaveConnectionAsync_EmptyPassword_SavesCorrectly()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = "localhost",
            Port = 3306,
            Username = "user",
            Password = ""
        };

        // Act
        await _service.SaveConnectionAsync(config);
        var loadedConfig = await _service.LoadConnectionAsync();

        // Assert
        loadedConfig.Password.Should().BeEmpty();
    }

    [Test]
    public async Task SaveConnectionAsync_NullPassword_SavesCorrectly()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = "localhost",
            Port = 3306,
            Username = "user",
            Password = null!
        };

        // Act
        await _service.SaveConnectionAsync(config);
        var loadedConfig = await _service.LoadConnectionAsync();

        // Assert
        loadedConfig.Password.Should().BeEmpty();
    }

    #endregion

    #region Password Encryption Tests

    [Test]
    public async Task EncryptPassword_DecryptPassword_RoundTrip()
    {
        // Arrange
        var originalPassword = "MySecretPassword!@#$%^&*()123";
        var config = new ConnectionConfig
        {
            Host = "localhost",
            Port = 3306,
            Username = "user",
            Password = originalPassword
        };

        // Act
        await _service.SaveConnectionAsync(config);

        // Проверяем, что пароль зашифрован в файле
        var json = await File.ReadAllTextAsync(_tempSettingsPath);
        json.Should().NotContain(originalPassword);

        // Загружаем и проверяем расшифровку
        var loadedConfig = await _service.LoadConnectionAsync();

        // Assert
        loadedConfig.Password.Should().Be(originalPassword);
    }

    [Test]
    public async Task EncryptPassword_UnicodeCharacters_PreservesContent()
    {
        // Arrange
        var unicodePassword = "Пароль123!中文🔐";
        var config = new ConnectionConfig
        {
            Host = "localhost",
            Port = 3306,
            Username = "user",
            Password = unicodePassword
        };

        // Act
        await _service.SaveConnectionAsync(config);
        var loadedConfig = await _service.LoadConnectionAsync();

        // Assert
        loadedConfig.Password.Should().Be(unicodePassword);
    }

    #endregion

    #region LoadLastExportDirectoryAsync Tests

    [Test]
    public async Task LoadLastExportDirectoryAsync_NoSettings_ReturnsNull()
    {
        // Act
        var directory = await _service.LoadLastExportDirectoryAsync();

        // Assert
        directory.Should().BeNull();
    }

    [Test]
    public async Task LoadLastExportDirectoryAsync_WithSavedDirectory_ReturnsCorrectPath()
    {
        // Arrange
        var expectedPath = "/home/user/exports";
        await _service.SaveLastExportDirectoryAsync(expectedPath);

        // Act
        var directory = await _service.LoadLastExportDirectoryAsync();

        // Assert
        directory.Should().Be(expectedPath);
    }

    #endregion

    #region SaveLastExportDirectoryAsync Tests

    [Test]
    public async Task SaveLastExportDirectoryAsync_ThenLoad_ReturnsSamePath()
    {
        // Arrange
        var path = "/path/to/exports";

        // Act
        await _service.SaveLastExportDirectoryAsync(path);
        var loadedPath = await _service.LoadLastExportDirectoryAsync();

        // Assert
        loadedPath.Should().Be(path);
    }

    [Test]
    public async Task SaveLastExportDirectoryAsync_WindowsPath_SavesCorrectly()
    {
        // Arrange
        var windowsPath = "C:\\Users\\Admin\\Documents\\Exports";

        // Act
        await _service.SaveLastExportDirectoryAsync(windowsPath);
        var loadedPath = await _service.LoadLastExportDirectoryAsync();

        // Assert
        loadedPath.Should().Be(windowsPath);
    }

    [Test]
    public async Task SaveLastExportDirectoryAsync_OverwritesExisting()
    {
        // Arrange
        await _service.SaveLastExportDirectoryAsync("/initial/path");
        await _service.SaveLastExportDirectoryAsync("/updated/path");

        // Act
        var loadedPath = await _service.LoadLastExportDirectoryAsync();

        // Assert
        loadedPath.Should().Be("/updated/path");
    }

    #endregion

    #region Combined Settings Tests

    [Test]
    public async Task SaveConnection_DoesNotAffectExportDirectory()
    {
        // Arrange
        await _service.SaveLastExportDirectoryAsync("/exports");

        var config = new ConnectionConfig
        {
            Host = "localhost",
            Port = 3306,
            Username = "user",
            Password = "pass"
        };

        // Act
        await _service.SaveConnectionAsync(config);
        var directory = await _service.LoadLastExportDirectoryAsync();

        // Assert
        directory.Should().Be("/exports");
    }

    [Test]
    public async Task SaveExportDirectory_DoesNotAffectConnection()
    {
        // Arrange
        var config = new ConnectionConfig
        {
            Host = "192.168.1.1",
            Port = 3307,
            Username = "admin",
            Password = "secret"
        };
        await _service.SaveConnectionAsync(config);

        // Act
        await _service.SaveLastExportDirectoryAsync("/new/path");
        var loadedConfig = await _service.LoadConnectionAsync();

        // Assert
        loadedConfig.Host.Should().Be("192.168.1.1");
        loadedConfig.Port.Should().Be(3307);
        loadedConfig.Username.Should().Be("admin");
    }

    #endregion
}
