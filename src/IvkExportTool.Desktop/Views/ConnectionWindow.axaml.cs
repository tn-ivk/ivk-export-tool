using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using IvkExportTool.Desktop.Enums;
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
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        // Подписываемся на новую ViewModel
        _viewModel = DataContext as ConnectionWindowViewModel;
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_viewModel is null || _notificationManager is null)
            return;

        // Реагируем только на изменения StatusMessage
        if (e.PropertyName == nameof(ConnectionWindowViewModel.StatusMessage))
        {
            var message = _viewModel.StatusMessage;

            // Игнорируем пустые или дефолтные сообщения
            if (string.IsNullOrWhiteSpace(message) ||
                message == "Введите параметры подключения")
                return;

            // Определяем тип уведомления на основе StatusType
            var notificationType = _viewModel.StatusType switch
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
                    message: message,
                    type: notificationType,
                    expiration: TimeSpan.FromSeconds(5)
                ));

                // Сбрасываем статус после показа уведомления
                _viewModel.StatusType = StatusMessageType.None;
            });
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        // Отписываемся при закрытии окна
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        Opened -= OnWindowOpened;
        DataContextChanged -= OnDataContextChanged;

        base.OnClosed(e);
    }
}
