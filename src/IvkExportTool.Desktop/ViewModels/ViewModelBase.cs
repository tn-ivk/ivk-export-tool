using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace IvkExportTool.Desktop.ViewModels;

public class ViewModelBase : ObservableObject
{
    public string AppVersion
    {
        get
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version != null ? $"v{version.Major}.{version.Minor}.{version.Build}" : "v0.0.0";
        }
    }
}
