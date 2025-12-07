using System.Security.Cryptography;
using System.Text;
using IvkExportTool.Core.Interfaces;
using IvkExportTool.Core.Models;
using IvkExportTool.Infrastructure.Configuration;

namespace IvkExportTool.Infrastructure.Services;

/// <summary>
/// Сервис для работы с настройками приложения.
/// Использует System.Text.Json с Source Generators для AOT-совместимости.
/// </summary>
public class AppSettingsService : IAppSettingsService
{
    private AppSettings _settings;

    public AppSettingsService()
    {
        _settings = SettingsStore.Load();
    }

    public Task<ConnectionConfig> LoadConnectionAsync()
    {
        try
        {
            var connection = _settings.Connection;

            var config = new ConnectionConfig
            {
                Host = connection.Host ?? "",
                Port = connection.Port <= 0 ? 3306 : connection.Port,
                Username = connection.Username ?? ""
            };

            config.Password = string.IsNullOrWhiteSpace(connection.EncryptedPassword)
                ? ""
                : DecryptPassword(connection.EncryptedPassword);

            return Task.FromResult(config);
        }
        catch
        {
            return Task.FromResult(new ConnectionConfig());
        }
    }

    public Task SaveConnectionAsync(ConnectionConfig config)
    {
        try
        {
            _settings.Connection.Host = config.Host;
            _settings.Connection.Port = config.Port;
            _settings.Connection.Username = config.Username;
            _settings.Connection.EncryptedPassword = string.IsNullOrEmpty(config.Password)
                ? string.Empty
                : EncryptPassword(config.Password);

            SettingsStore.Save(_settings);
        }
        catch
        {
            // Игнорируем ошибки записи, чтобы не нарушать основной сценарий работы приложения.
        }

        return Task.CompletedTask;
    }

    public Task SaveHostAndPortAsync(string host, int port)
    {
        try
        {
            _settings.Connection.Host = host;
            _settings.Connection.Port = port;
            // Не трогаем Username и EncryptedPassword

            SettingsStore.Save(_settings);
        }
        catch
        {
            // Игнорируем ошибки записи
        }

        return Task.CompletedTask;
    }

    public Task<string?> LoadLastExportDirectoryAsync()
    {
        try
        {
            return Task.FromResult(_settings.LastExportDirectory);
        }
        catch
        {
            return Task.FromResult<string?>(null);
        }
    }

    public Task SaveLastExportDirectoryAsync(string directory)
    {
        try
        {
            _settings.LastExportDirectory = directory;
            SettingsStore.Save(_settings);
        }
        catch
        {
            // Игнорируем ошибки записи
        }

        return Task.CompletedTask;
    }

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
}
