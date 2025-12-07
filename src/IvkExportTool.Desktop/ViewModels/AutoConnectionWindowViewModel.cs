using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IvkExportTool.Core.Constants;
using IvkExportTool.Core.Interfaces;
using IvkExportTool.Core.Models;
using IvkExportTool.Desktop.Enums;
using IvkExportTool.Desktop.Events;

namespace IvkExportTool.Desktop.ViewModels;

public partial class AutoConnectionWindowViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IAppSettingsService? _appSettingsService;
    private CancellationTokenSource? _cts;

    private static readonly TimeSpan ConnectionTimeout = TimeSpan.FromSeconds(5);

    // Параметры подключения
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHostValid))]
    [NotifyPropertyChangedFor(nameof(CanConnect))]
    private string _host = "192.168.233.101";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPortValid))]
    [NotifyPropertyChangedFor(nameof(CanConnect))]
    private string _port = "3306";

    // Валидация полей
    public bool IsHostValid => !string.IsNullOrWhiteSpace(Host);
    public bool IsPortValid => !string.IsNullOrWhiteSpace(Port) && int.TryParse(Port, out var p) && p > 0 && p <= 65535;
    public bool CanConnect => IsHostValid && IsPortValid && !IsLoading;

    // Состояние
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConnect))]
    private bool _isLoading;

    [ObservableProperty]
    private bool _canCancel;

    [ObservableProperty]
    private int _currentAttempt;

    [ObservableProperty]
    private int _totalAttempts;

    [ObservableProperty]
    private string _statusMessage = "Нажмите 'Автоподключение' для поиска и подключения к БД ИВК";

    [ObservableProperty]
    private StatusMessageType _statusType = StatusMessageType.None;

    // События
    public event EventHandler<ConnectionConfig>? ConnectionSucceeded;
    public event EventHandler? BackRequested;
    public event EventHandler<NotificationRequestedEventArgs>? NotificationRequested;

    public AutoConnectionWindowViewModel() : this(null!, null!)
    {
        // Конструктор для дизайнера
    }

    public AutoConnectionWindowViewModel(
        IDatabaseService databaseService,
        IAppSettingsService appSettingsService)
    {
        _databaseService = databaseService;
        _appSettingsService = appSettingsService;

        _ = LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        if (_appSettingsService is null)
            return;

        try
        {
            var config = await _appSettingsService.LoadConnectionAsync();
            Host = config.Host;
            Port = config.Port.ToString();
        }
        catch
        {
            // Игнорируем ошибки загрузки
        }
    }

    private async Task SaveHostAndPortAsync()
    {
        if (_appSettingsService is null)
            return;

        try
        {
            var port = int.TryParse(Port, out var p) ? p : 3306;
            await _appSettingsService.SaveHostAndPortAsync(Host, port);
        }
        catch
        {
            // Игнорируем ошибки сохранения
        }
    }

    [RelayCommand]
    private void Back()
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel()
    {
        _cts?.Cancel();
        StatusMessage = "Отменено пользователем";
        StatusType = StatusMessageType.Warning;
        CanCancel = false;
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        _cts = new CancellationTokenSource();
        IsLoading = true;
        CanCancel = true;
        TotalAttempts = DefaultCredentials.List.Count;
        CurrentAttempt = 0;

        try
        {
            var port = int.TryParse(Port, out var p) ? p : 3306;

            for (var i = 0; i < DefaultCredentials.List.Count; i++)
            {
                if (_cts.Token.IsCancellationRequested)
                    break;

                CurrentAttempt = i + 1;
                var credential = DefaultCredentials.List[i];

                StatusMessage = $"Попытка {CurrentAttempt} из {TotalAttempts}...";
                StatusType = StatusMessageType.None;

                var config = new ConnectionConfig
                {
                    Host = Host,
                    Port = port,
                    Username = credential.Username,
                    Password = credential.Password
                };

                var success = await _databaseService.TestConnectionAsync(config, ConnectionTimeout, _cts.Token);

                if (_cts.Token.IsCancellationRequested)
                    break;

                if (success)
                {
                    // Проверяем, есть ли базы данных
                    try
                    {
                        var databases = await _databaseService.GetDatabasesAsync(config);

                        if (databases.Count > 0)
                        {
                            StatusMessage = $"✓ Успешно! Найдено {databases.Count} баз данных.";
                            StatusType = StatusMessageType.Success;

                            ShowNotification(
                                $"✓ Автоподключение успешно! Найдено {databases.Count} баз данных.",
                                StatusMessageType.Success);

                            // Сохраняем только Host и Port (без логина/пароля)
                            await SaveHostAndPortAsync();

                            // Вызываем событие успешного подключения → переход на MainWindow
                            ConnectionSucceeded?.Invoke(this, config);
                            return;
                        }
                    }
                    catch
                    {
                        // Продолжаем перебор при ошибке получения списка БД
                    }
                }
            }

            // Все попытки исчерпаны или отменено
            if (!_cts.Token.IsCancellationRequested)
            {
                StatusMessage = "✗ Не удалось подключиться. Проверьте адрес сервера.";
                StatusType = StatusMessageType.Error;

                ShowNotification(
                    "✗ Не удалось подключиться к устройству.",
                    StatusMessageType.Error);
            }
        }
        catch (OperationCanceledException)
        {
            // Отмена — уже обработана в Cancel()
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ Ошибка: {ex.Message}";
            StatusType = StatusMessageType.Error;

            ShowNotification(
                $"✗ Ошибка автоподключения: {ex.Message}",
                StatusMessageType.Error);
        }
        finally
        {
            IsLoading = false;
            CanCancel = false;
            _cts?.Dispose();
            _cts = null;
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
