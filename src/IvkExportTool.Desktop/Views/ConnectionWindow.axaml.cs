using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Threading;
using IvkExportTool.Desktop.Enums;
using IvkExportTool.Desktop.Events;
using IvkExportTool.Desktop.ViewModels;

namespace IvkExportTool.Desktop.Views;

public partial class ConnectionWindow : Window
{
    private WindowNotificationManager? _notificationManager;
    private ConnectionWindowViewModel? _viewModel;

    public ConnectionWindow()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
        Opened += OnWindowOpened;

        // Настройка перетаскивания окна за заголовок
        var titleBar = this.FindControl<Border>("TitleBar");
        if (titleBar != null)
        {
            titleBar.PointerPressed += OnTitleBarPointerPressed;
        }

        // Настройка кнопки закрытия
        var closeButton = this.FindControl<Button>("CloseButton");
        if (closeButton != null)
        {
            closeButton.Click += OnCloseButtonClick;
        }
    }

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void OnCloseButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Закрываем приложение
        if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    private void OnWindowOpened(object? sender, EventArgs e)
    {
        // Инициализируем WindowNotificationManager после полной загрузки окна
        // чтобы избежать проблем с layout
        if (_notificationManager == null)
        {
            _notificationManager = new WindowNotificationManager(this)
            {
                Position = NotificationPosition.TopRight,
                MaxItems = 1
            };
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        // Отписываемся от старой ViewModel
        if (_viewModel is not null)
        {
            _viewModel.NotificationRequested -= OnNotificationRequested;
        }

        // Подписываемся на новую ViewModel
        _viewModel = DataContext as ConnectionWindowViewModel;
        if (_viewModel is not null)
        {
            _viewModel.NotificationRequested += OnNotificationRequested;
        }
    }

    private void OnNotificationRequested(object? sender, NotificationRequestedEventArgs e)
    {
        if (_notificationManager is null)
            return;

        // Игнорируем пустые или дефолтные сообщения
        if (string.IsNullOrWhiteSpace(e.Message) ||
            e.Message == "Введите параметры подключения")
            return;

        // Определяем тип уведомления на основе StatusType
        var notificationType = e.Type switch
        {
            StatusMessageType.Success => NotificationType.Success,
            StatusMessageType.Warning => NotificationType.Warning,
            StatusMessageType.Error => NotificationType.Error,
            _ => NotificationType.Information
        };

        // Показываем уведомление в UI потоке
        Dispatcher.UIThread.Post(() =>
        {
            _notificationManager.Show(new Notification(
                title: "IvkExportTool",
                message: e.Message,
                type: notificationType,
                expiration: TimeSpan.FromSeconds(5)
            ));
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        // Отписываемся от событий TitleBar
        var titleBar = this.FindControl<Border>("TitleBar");
        if (titleBar != null)
        {
            titleBar.PointerPressed -= OnTitleBarPointerPressed;
        }

        // Отписываемся от кнопки закрытия
        var closeButton = this.FindControl<Button>("CloseButton");
        if (closeButton != null)
        {
            closeButton.Click -= OnCloseButtonClick;
        }

        // Отписываемся при закрытии окна
        if (_viewModel is not null)
        {
            _viewModel.NotificationRequested -= OnNotificationRequested;
        }

        Opened -= OnWindowOpened;
        DataContextChanged -= OnDataContextChanged;

        base.OnClosed(e);
    }
}
