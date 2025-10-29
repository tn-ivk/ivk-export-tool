namespace IvkExportTool.Core.Models;

/// <summary>
/// Конфигурация подключения к MySQL базе данных
/// </summary>
public class ConnectionConfig
{
    /// <summary>
    /// IP адрес или хост сервера MySQL
    /// </summary>
    public string Host { get; set; } = "192.168.233.101";

    /// <summary>
    /// Порт сервера MySQL (по умолчанию 3306)
    /// </summary>
    public int Port { get; set; } = 3306;

    /// <summary>
    /// Имя пользователя для подключения
    /// </summary>
    public string Username { get; set; } = "user";

    /// <summary>
    /// Пароль для подключения
    /// </summary>
    public string Password { get; set; } = "mJKuyb&9!2@m";

    /// <summary>
    /// Имя базы данных (опционально, можно выбрать после подключения)
    /// </summary>
    public string? Database { get; set; }

    /// <summary>
    /// Строка подключения
    /// </summary>
    public string GetConnectionString()
    {
        var builder = $"Server={Host};Port={Port};User ID={Username};Password={Password};";
        if (!string.IsNullOrEmpty(Database))
        {
            builder += $"Database={Database};";
        }
        builder += "CharSet=utf8mb4;";
        return builder;
    }
}

