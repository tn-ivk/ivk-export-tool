using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using IvkExportTool.Desktop.ViewModels;

namespace IvkExportTool.Desktop.Views;

public partial class AutoConnectionWindow : Window
{
    public AutoConnectionWindow()
    {
        InitializeComponent();
        SetupWindowDragging();
        SetupButtons();
    }

    private void SetupWindowDragging()
    {
        var titleBar = this.FindControl<Border>("TitleBar");
        if (titleBar != null)
        {
            titleBar.PointerPressed += (sender, e) =>
            {
                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                {
                    BeginMoveDrag(e);
                }
            };
        }
    }

    private void SetupButtons()
    {
        var closeButton = this.FindControl<Button>("CloseButton");
        if (closeButton != null)
        {
            closeButton.Click += (sender, e) =>
            {
                Close();
            };
        }

        var backButton = this.FindControl<Button>("BackButton");
        if (backButton != null)
        {
            backButton.Click += (sender, e) =>
            {
                if (DataContext is AutoConnectionWindowViewModel vm)
                {
                    vm.BackCommand.Execute(null);
                }
            };
        }
    }
}
