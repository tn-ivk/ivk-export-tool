using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using IvkExportTool.Desktop.ViewModels;

namespace IvkExportTool.Desktop.Views;

public partial class StartWindow : Window
{
    public StartWindow()
    {
        InitializeComponent();
        SetupWindowDragging();
        SetupCloseButton();
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

    private void SetupCloseButton()
    {
        var closeButton = this.FindControl<Button>("CloseButton");
        if (closeButton != null)
        {
            closeButton.Click += (sender, e) =>
            {
                Close();
            };
        }
    }
}
