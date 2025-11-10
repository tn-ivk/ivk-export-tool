using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IvkExportTool.Core.Interfaces;
using IvkExportTool.Core.Models;

namespace IvkExportTool.Infrastructure.Services;

public class AppSettingsService : IAppSettingsService
{
    private const string SettingsFileName = "appsettings.json";

    private readonly string _settingsPath;
    private readonly JsonSerializerOptions _jsonOptions;

    public AppSettingsService()
    {
        var baseDirectory = AppContext.BaseDirectory;
        _settingsPath = Path.Combine(baseDirectory, SettingsFileName);
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    public async Task<ConnectionConfig> LoadConnectionAsync()
    {
        var defaultConfig = CreateDefaultConfig();

        if (!File.Exists(_settingsPath))
        {
            await SaveConnectionAsync(defaultConfig);
            return defaultConfig;
        }

        try
        {
            await using var stream = File.OpenRead(_settingsPath);
            var settings = await JsonSerializer.DeserializeAsync<AppSettingsData>(stream, _jsonOptions);

            if (settings?.Connection is null)
            {
                return defaultConfig;
            }

            var connection = settings.Connection;

            var config = new ConnectionConfig
            {
                Host = string.IsNullOrWhiteSpace(connection.Host) ? defaultConfig.Host : connection.Host,
                Port = connection.Port <= 0 ? defaultConfig.Port : connection.Port,
                Username = string.IsNullOrWhiteSpace(connection.Username) ? defaultConfig.Username : connection.Username,
                Database = connection.Database
            };

            config.Password = string.IsNullOrWhiteSpace(connection.EncryptedPassword)
                ? defaultConfig.Password
                : DecryptPassword(connection.EncryptedPassword);

            return config;
        }
        catch
        {
            return defaultConfig;
        }
    }

    public async Task SaveConnectionAsync(ConnectionConfig config)
    {
        var data = new AppSettingsData
        {
            Connection = new ConnectionSettingsData
            {
                Host = config.Host,
                Port = config.Port,
                Username = config.Username,
                Database = config.Database,
                EncryptedPassword = string.IsNullOrEmpty(config.Password)
                    ? string.Empty
                    : EncryptPassword(config.Password)
            }
        };

        try
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var stream = File.Create(_settingsPath);
            await JsonSerializer.SerializeAsync(stream, data, _jsonOptions);
        }
        catch
        {
            // Игнорируем ошибки записи, чтобы не нарушать основной сценарий работы приложения.
        }
    }

    private static ConnectionConfig CreateDefaultConfig() => new();

    private static string EncryptPassword(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);

        if (OperatingSystem.IsWindows())
        {
            var protectedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(protectedBytes);
        }

        return EncryptWithAes(bytes);
    }

    private static string DecryptPassword(string encryptedPassword)
    {
        try
        {
            var protectedBytes = Convert.FromBase64String(encryptedPassword);

            if (OperatingSystem.IsWindows())
            {
                var bytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(bytes);
            }

            return DecryptWithAes(protectedBytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string EncryptWithAes(byte[] plainBytes)
    {
        using var aes = Aes.Create();
        aes.Key = GetFallbackKey();
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    private static string DecryptWithAes(byte[] encryptedBytes)
    {
        using var aes = Aes.Create();
        aes.Key = GetFallbackKey();

        var ivLength = aes.BlockSize / 8;
        if (encryptedBytes.Length < ivLength)
        {
            return string.Empty;
        }

        var iv = new byte[ivLength];
        var cipher = new byte[encryptedBytes.Length - ivLength];

        Buffer.BlockCopy(encryptedBytes, 0, iv, 0, ivLength);
        Buffer.BlockCopy(encryptedBytes, ivLength, cipher, 0, cipher.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }

    private static byte[] GetFallbackKey()
    {
        var source = Encoding.UTF8.GetBytes($"{Environment.UserName}|{Environment.MachineName}|IvkExportTool");
        return SHA256.HashData(source);
    }

    public async Task<string?> LoadLastExportDirectoryAsync()
    {
        if (!File.Exists(_settingsPath))
        {
            return null;
        }

        try
        {
            await using var stream = File.OpenRead(_settingsPath);
            var settings = await JsonSerializer.DeserializeAsync<AppSettingsData>(stream, _jsonOptions);
            return settings?.LastExportDirectory;
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveLastExportDirectoryAsync(string directory)
    {
        try
        {
            AppSettingsData? settings = null;

            if (File.Exists(_settingsPath))
            {
                await using var readStream = File.OpenRead(_settingsPath);
                settings = await JsonSerializer.DeserializeAsync<AppSettingsData>(readStream, _jsonOptions);
            }

            settings ??= new AppSettingsData();
            settings.LastExportDirectory = directory;

            var directoryPath = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            await using var writeStream = File.Create(_settingsPath);
            await JsonSerializer.SerializeAsync(writeStream, settings, _jsonOptions);
        }
        catch
        {
            // Игнорируем ошибки записи
        }
    }

    private class AppSettingsData
    {
        public ConnectionSettingsData? Connection { get; set; }
        public string? LastExportDirectory { get; set; }
    }

    private class ConnectionSettingsData
    {
        public string? Host { get; set; }
        public int Port { get; set; }
        public string? Username { get; set; }
        public string? Database { get; set; }
        public string? EncryptedPassword { get; set; }
    }
}

