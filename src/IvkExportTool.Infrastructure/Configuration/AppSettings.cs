namespace IvkExportTool.Infrastructure.Configuration;

/// <summary>
/// Настройки приложения для сериализации в JSON.
/// </summary>
public sealed class AppSettings
{
    /// <summary>
    /// Настройки подключения к базе данных.
    /// </summary>
    public ConnectionSettings Connection { get; set; } = new();

    /// <summary>
    /// Путь к последней папке экспорта.
    /// </summary>
    public string? LastExportDirectory { get; set; }
}

/// <summary>
/// Настройки подключения к базе данных.
/// </summary>
public sealed class ConnectionSettings
{
    /// <summary>
    /// Хост сервера базы данных.
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// Порт сервера базы данных.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Имя пользователя для подключения.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Зашифрованный пароль (Base64).
    /// </summary>
    public string? EncryptedPassword { get; set; }
}
