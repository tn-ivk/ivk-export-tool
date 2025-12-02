using CommunityToolkit.Mvvm.Input;

namespace IvkExportTool.Desktop.ViewModels;

public partial class StartWindowViewModel : ViewModelBase
{
    // События для выбора типа подключения
    public event EventHandler? ManualConnectionRequested;
    public event EventHandler? AutoConnectionRequested;

    public StartWindowViewModel()
    {
    }

    [RelayCommand]
    private void ManualConnection()
    {
        ManualConnectionRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void AutoConnection()
    {
        AutoConnectionRequested?.Invoke(this, EventArgs.Empty);
    }
}
