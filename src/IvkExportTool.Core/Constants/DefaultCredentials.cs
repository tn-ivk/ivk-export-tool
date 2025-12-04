using IvkExportTool.Core.Models;

namespace IvkExportTool.Core.Constants;

/// <summary>
/// Список учётных данных по умолчанию для автоподключения к БД ИВК
/// </summary>
public static class DefaultCredentials
{
    public static readonly IReadOnlyList<Credential> List = new[]
    {
        new Credential("user", "mJKuyb&9!2@m"),
        new Credential("user", "6ViQgjps"),
        new Credential("user", "GFhjkm123@"),
        new Credential("user", "psgrandpass"),
        new Credential("root", "GFhjkm123@"),
        new Credential("root", "psgrandpass"),
        new Credential("root", "mJKuyb&9!2@m"),
    };
}
