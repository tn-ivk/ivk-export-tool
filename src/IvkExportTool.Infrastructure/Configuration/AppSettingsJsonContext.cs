using System.Text.Json.Serialization;

namespace IvkExportTool.Infrastructure.Configuration;

/// <summary>
/// JSON Source Generator контекст для AOT-совместимой сериализации настроек.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AppSettings))]
internal partial class AppSettingsJsonContext : JsonSerializerContext
{
}
