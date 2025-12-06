using System.Text.Json;

namespace IvkExportTool.Infrastructure.Configuration;

/// <summary>
/// Статический класс для загрузки и сохранения настроек приложения.
/// Использует System.Text.Json с Source Generators для AOT-совместимости.
/// </summary>
public static class SettingsStore
{
    private const string AppName = "IvkExportTool";
    private const string SettingsFileName = "settings.json";

    private static readonly object _lock = new();
    private static string? _cachedSettingsPath;

    /// <summary>
    /// Загружает настройки из JSON файла.
    /// </summary>
    /// <returns>Настройки приложения. Возвращает новый экземпляр, если файл не существует.</returns>
    public static AppSettings Load()
    {
        var settingsPath = GetSettingsFilePath();

        if (!File.Exists(settingsPath))
        {
            return new AppSettings();
        }

        try
        {
            var json = File.ReadAllText(settingsPath);
            var settings = JsonSerializer.Deserialize(json, AppSettingsJsonContext.Default.AppSettings);
            return settings ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    /// <summary>
    /// Сохраняет настройки в JSON файл.
    /// Thread-safe операция.
    /// </summary>
    /// <param name="settings">Настройки для сохранения.</param>
    public static void Save(AppSettings settings)
    {
        var settingsPath = GetSettingsFilePath();
        EnsureDirectoryExists(settingsPath);

        lock (_lock)
        {
            var json = JsonSerializer.Serialize(settings, AppSettingsJsonContext.Default.AppSettings);
            File.WriteAllText(settingsPath, json);
        }
    }

    /// <summary>
    /// Возвращает путь к файлу настроек в зависимости от ОС.
    /// Windows: %APPDATA%\IvkExportTool\settings.json
    /// Linux/macOS: ~/.config/IvkExportTool/settings.json
    /// </summary>
    public static string GetSettingsFilePath()
    {
        if (_cachedSettingsPath != null)
        {
            return _cachedSettingsPath;
        }

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

        _cachedSettingsPath = Path.Combine(configDirectory, SettingsFileName);
        return _cachedSettingsPath;
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
