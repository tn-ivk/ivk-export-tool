using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IvkExportTool.Core.Interfaces;
using IvkExportTool.Core.Models;
using IvkExportTool.Desktop.Enums;
using IvkExportTool.Desktop.Events;

namespace IvkExportTool.Desktop.ViewModels;

public partial class ConnectionWindowViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IAppSettingsService? _appSettingsService;
    private readonly SemaphoreSlim _saveSemaphore = new(1, 1);
    private CancellationTokenSource? _saveCts;
    private bool _isInitializingSettings;

    // Параметры подключения
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHostValid))]
    [NotifyPropertyChangedFor(nameof(CanConnect))]
    private string _host = "192.168.233.101";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPortValid))]
    [NotifyPropertyChangedFor(nameof(CanConnect))]
    private string _port = "3306";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUsernameValid))]
    [NotifyPropertyChangedFor(nameof(CanConnect))]
    private string _username = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPasswordValid))]
    [NotifyPropertyChangedFor(nameof(CanConnect))]
    private string _password = "";

    // Валидация полей
    public bool IsHostValid => !string.IsNullOrWhiteSpace(Host);
    public bool IsPortValid => !string.IsNullOrWhiteSpace(Port) && int.TryParse(Port, out var p) && p > 0 && p <= 65535;
    public bool IsUsernameValid => !string.IsNullOrWhiteSpace(Username);
    public bool IsPasswordValid => !string.IsNullOrWhiteSpace(Password);
    public bool CanConnect => IsHostValid && IsPortValid && IsUsernameValid && IsPasswordValid && !IsLoading;

    // Состояние
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConnect))]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Введите параметры подключения";

    [ObservableProperty]
    private StatusMessageType _statusType = StatusMessageType.None;

    // События
    public event EventHandler<ConnectionConfig>? ConnectionSucceeded;
    public event EventHandler<NotificationRequestedEventArgs>? NotificationRequested;
    public event EventHandler? BackRequested;

    public ConnectionWindowViewModel() : this(null!, null!)
    {
        // Конструктор для дизайнера
    }

    public ConnectionWindowViewModel(
        IDatabaseService databaseService,
        IAppSettingsService appSettingsService)
    {
        _databaseService = databaseService;
        _appSettingsService = appSettingsService;

        _ = LoadSettingsAsync();
    }

    private ConnectionConfig GetConnectionConfig()
    {
        return new ConnectionConfig
        {
            Host = Host,
            Port = int.TryParse(Port, out var port) ? port : 3306,
            Username = Username,
            Password = Password
        };
    }

    #region Settings Management

    private async Task LoadSettingsAsync()
    {
        if (_appSettingsService is null)
            return;

        _isInitializingSettings = true;

        try
        {
            var config = await _appSettingsService.LoadConnectionAsync();
            Host = config.Host;
            Port = config.Port.ToString();
            Username = config.Username;
            Password = config.Password;
        }
        catch
        {
            // Игнорируем ошибки загрузки
        }
        finally
        {
            _isInitializingSettings = false;
        }
    }

    private void ScheduleSaveSettings()
    {
        if (_appSettingsService is null || _isInitializingSettings)
            return;

        _saveCts?.Cancel();
        var cts = new CancellationTokenSource();
        _saveCts = cts;

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(500, cts.Token);
                await SaveSettingsInternalAsync();
            }
            catch (TaskCanceledException) { }
        });
    }

    private async Task SaveSettingsInternalAsync()
    {
        if (_appSettingsService is null)
            return;

        try
        {
            await _saveSemaphore.WaitAsync();
            try
            {
                var config = GetConnectionConfig();
                await _appSettingsService.SaveConnectionAsync(config);
            }
            finally
            {
                _saveSemaphore.Release();
            }
        }
        catch { }
    }

    partial void OnHostChanged(string value) => ScheduleSaveSettings();
    partial void OnPortChanged(string value) => ScheduleSaveSettings();
    partial void OnUsernameChanged(string value) => ScheduleSaveSettings();
    partial void OnPasswordChanged(string value) => ScheduleSaveSettings();

    #endregion

    #region Notification

    private void ShowNotification(string message, StatusMessageType type)
    {
        StatusMessage = message;
        StatusType = type;

        // Вызываем событие для показа уведомления
        NotificationRequested?.Invoke(this, new NotificationRequestedEventArgs(message, type));
    }

    #endregion

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        IsLoading = true;

        try
        {
            var config = GetConnectionConfig();
            var result = await _databaseService.TestConnectionAsync(config);

            if (result)
            {
                ShowNotification(
                    "✓ Подключение успешно! Нажмите 'Подключиться' для продолжения.",
                    StatusMessageType.Success);
            }
            else
            {
                ShowNotification(
                    "✗ Ошибка подключения. Проверьте параметры.",
                    StatusMessageType.Error);
            }
        }
        catch (Exception ex)
        {
            ShowNotification(
                $"✗ Ошибка: {ex.Message}",
                StatusMessageType.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        IsLoading = true;

        try
        {
            var config = GetConnectionConfig();

            // Проверяем подключение
            var databases = await _databaseService.GetDatabasesAsync(config);

            if (databases.Count > 0)
            {
                ShowNotification(
                    $"✓ Успешно! Найдено {databases.Count} баз данных.",
                    StatusMessageType.Success);

                // Сохраняем настройки
                await SaveSettingsInternalAsync();

                // Вызываем событие успешного подключения
                ConnectionSucceeded?.Invoke(this, config);
            }
            else
            {
                ShowNotification(
                    "✗ Не найдено баз данных на сервере.",
                    StatusMessageType.Warning);
            }
        }
        catch (Exception ex)
        {
            ShowNotification(
                $"✗ Ошибка подключения: {ex.Message}",
                StatusMessageType.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Back()
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }
}
