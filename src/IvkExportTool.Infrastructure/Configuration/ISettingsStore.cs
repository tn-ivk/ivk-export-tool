namespace IvkExportTool.Infrastructure.Configuration;

/// <summary>
/// Интерфейс хранилища настроек для Config.Net.
/// Используется для чтения/записи настроек в JSON файл.
/// </summary>
public interface ISettingsStore
{
    /// <summary>
    /// Настройки подключения к базе данных.
    /// </summary>
    IConnectionSettings Connection { get; }

    /// <summary>
    /// Путь к последней папке экспорта.
    /// </summary>
    string? LastExportDirectory { get; set; }
}

/// <summary>
/// Настройки подключения к базе данных.
/// </summary>
public interface IConnectionSettings
{
    /// <summary>
    /// Хост сервера базы данных.
    /// </summary>
    string? Host { get; set; }

    /// <summary>
    /// Порт сервера базы данных.
    /// </summary>
    int Port { get; set; }

    /// <summary>
    /// Имя пользователя для подключения.
    /// </summary>
    string? Username { get; set; }

    /// <summary>
    /// Имя базы данных.
    /// </summary>
    string? Database { get; set; }

    /// <summary>
    /// Зашифрованный пароль (Base64).
    /// </summary>
    string? EncryptedPassword { get; set; }
}
