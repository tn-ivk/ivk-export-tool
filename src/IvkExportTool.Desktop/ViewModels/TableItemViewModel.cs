using CommunityToolkit.Mvvm.ComponentModel;
using IvkExportTool.Core.Models;

namespace IvkExportTool.Desktop.ViewModels;

/// <summary>
/// ViewModel для отображения информации о таблице в DataGrid
/// </summary>
public partial class TableItemViewModel : ObservableObject
{
    private readonly TableInfo _tableInfo;

    [ObservableProperty]
    private bool _isSelected;

    public TableItemViewModel(TableInfo tableInfo)
    {
        _tableInfo = tableInfo;
        _isSelected = tableInfo.IsSelected;
    }

    public string Name => _tableInfo.Name;

    public long RowCount => _tableInfo.RowCount;

    public long SizeInBytes => _tableInfo.SizeBytes;

    public string Engine => _tableInfo.Engine;

    /// <summary>
    /// Форматированный размер таблицы (B, KB, MB, GB)
    /// </summary>
    public string SizeFormatted => FormatBytes(SizeInBytes);

    /// <summary>
    /// Синхронизация с исходной моделью TableInfo
    /// </summary>
    partial void OnIsSelectedChanged(bool value)
    {
        _tableInfo.IsSelected = value;
    }

    /// <summary>
    /// Получить исходную модель TableInfo
    /// </summary>
    public TableInfo GetTableInfo() => _tableInfo;

    private static string FormatBytes(long bytes)
    {
        if (bytes == 0) return "0 B";

        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double size = bytes;
        int order = 0;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }
}
