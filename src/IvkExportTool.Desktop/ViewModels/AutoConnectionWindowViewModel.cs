using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IvkExportTool.Core.Interfaces;
using IvkExportTool.Core.Models;
using IvkExportTool.Desktop.Enums;
using IvkExportTool.Desktop.Events;

namespace IvkExportTool.Desktop.ViewModels;

public partial class AutoConnectionWindowViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;

    // Состояние
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Нажмите 'Автоподключиться' для поиска и подключения к БД ИВК";

    [ObservableProperty]
    private StatusMessageType _statusType = StatusMessageType.None;

    // События
    public event EventHandler<ConnectionConfig>? ConnectionSucceeded;
    public event EventHandler? BackRequested;
    public event EventHandler<NotificationRequestedEventArgs>? NotificationRequested;

    public AutoConnectionWindowViewModel() : this(null!)
    {
        // Конструктор для дизайнера
    }

    public AutoConnectionWindowViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [RelayCommand]
    private void Back()
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel()
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        IsLoading = true;
        StatusMessage = "Поиск сервера БД ИВК...";

        try
        {
            // TODO: Реализовать логику автоподключения
            // Здесь будет логика поиска сервера в локальной сети
            // и автоматического подключения к БД ИВК

            await Task.Delay(1000); // Симуляция поиска

            // Временная заглушка - используем стандартные параметры
            var config = new ConnectionConfig
            {
                Host = "192.168.233.101",
                Port = 3306,
                Username = "user",
                Password = ""
            };

            // Проверяем подключение
            var databases = await _databaseService.GetDatabasesAsync(config);

            if (databases.Count > 0)
            {
                StatusMessage = $"✓ Успешно! Найдено {databases.Count} баз данных.";
                StatusType = StatusMessageType.Success;

                ShowNotification(
                    $"✓ Автоподключение успешно! Найдено {databases.Count} баз данных.",
                    StatusMessageType.Success);

                // Вызываем событие успешного подключения
                ConnectionSucceeded?.Invoke(this, config);
            }
            else
            {
                StatusMessage = "✗ Не найдено баз данных на сервере.";
                StatusType = StatusMessageType.Warning;

                ShowNotification(
                    "✗ Не найдено баз данных на сервере.",
                    StatusMessageType.Warning);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ Ошибка автоподключения: {ex.Message}";
            StatusType = StatusMessageType.Error;

            ShowNotification(
                $"✗ Ошибка автоподключения: {ex.Message}",
                StatusMessageType.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowNotification(string message, StatusMessageType type)
    {
        StatusMessage = message;
        StatusType = type;

        // Вызываем событие для показа уведомления
        NotificationRequested?.Invoke(this, new NotificationRequestedEventArgs(message, type));
    }
}
