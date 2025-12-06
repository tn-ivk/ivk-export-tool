using Config.Net;

namespace IvkExportTool.Infrastructure.Configuration;

/// <summary>
/// Фабрика для создания хранилища настроек.
/// </summary>
public static class SettingsStoreFactory
{
    private const string AppName = "IvkExportTool";
    private const string SettingsFileName = "settings.json";

    /// <summary>
    /// Создаёт экземпляр хранилища настроек.
    /// </summary>
    public static ISettingsStore Create()
    {
        var settingsPath = GetSettingsFilePath();
        EnsureDirectoryExists(settingsPath);

        return new ConfigurationBuilder<ISettingsStore>()
            .UseJsonFile(settingsPath)
            .Build();
    }

    /// <summary>
    /// Возвращает путь к файлу настроек в зависимости от ОС.
    /// Windows: %APPDATA%\IvkExportTool\settings.json
    /// Linux/macOS: ~/.config/IvkExportTool/settings.json
    /// </summary>
    public static string GetSettingsFilePath()
    {
        string configDirectory;

        if (OperatingSystem.IsWindows())
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            configDirectory = Path.Combine(appData, AppName);
        }
        else
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            configDirectory = Path.Combine(home, ".config", AppName);
        }

        return Path.Combine(configDirectory, SettingsFileName);
    }

    private static void EnsureDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
