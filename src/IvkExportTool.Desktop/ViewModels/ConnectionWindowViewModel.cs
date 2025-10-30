using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IvkExportTool.Core.Interfaces;
using IvkExportTool.Core.Models;
using IvkExportTool.Desktop.Enums;

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
    private string _host = "192.168.233.101";

    [ObservableProperty]
    private string _port = "3306";

    [ObservableProperty]
    private string _username = "user";

    [ObservableProperty]
    private string _password = "mJKuyb&9!2@m";

    // Состояние
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Введите параметры подключения";

    [ObservableProperty]
    private StatusMessageType _statusType = StatusMessageType.None;

    // Событие успешного подключения
    public event EventHandler<ConnectionConfig>? ConnectionSucceeded;

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

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        IsLoading = true;
        StatusMessage = "Тестирование подключения...";
        StatusType = StatusMessageType.None;

        try
        {
            var config = GetConnectionConfig();
            var result = await _databaseService.TestConnectionAsync(config);

            if (result)
            {
                StatusMessage = "✓ Подключение успешно! Нажмите 'Подключиться' для продолжения.";
                StatusType = StatusMessageType.Success;
            }
            else
            {
                StatusMessage = "✗ Ошибка подключения. Проверьте параметры.";
                StatusType = StatusMessageType.Error;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ Ошибка: {ex.Message}";
            StatusType = StatusMessageType.Error;
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
        StatusMessage = "Подключение к базе данных...";
        StatusType = StatusMessageType.None;

        try
        {
            var config = GetConnectionConfig();

            // Проверяем подключение
            var databases = await _databaseService.GetDatabasesAsync(config);

            if (databases.Count > 0)
            {
                StatusMessage = $"✓ Успешно! Найдено {databases.Count} баз данных.";
                StatusType = StatusMessageType.Success;

                // Сохраняем настройки
                await SaveSettingsInternalAsync();

                // Вызываем событие успешного подключения
                ConnectionSucceeded?.Invoke(this, config);
            }
            else
            {
                StatusMessage = "✗ Не найдено баз данных на сервере.";
                StatusType = StatusMessageType.Warning;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ Ошибка подключения: {ex.Message}";
            StatusType = StatusMessageType.Error;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
