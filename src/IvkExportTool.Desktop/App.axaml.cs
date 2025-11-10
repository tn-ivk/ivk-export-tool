using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using IvkExportTool.Core.Interfaces;
using IvkExportTool.Core.Models;
using IvkExportTool.Desktop.ViewModels;
using IvkExportTool.Desktop.Views;
using IvkExportTool.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IvkExportTool.Desktop;

public partial class App : Application
{
    public IServiceProvider? Services { get; private set; }
    private IClassicDesktopStyleApplicationLifetime? _desktop;
    private ConnectionWindow? _connectionWindow;
    private MainWindow? _mainWindow;
    private PixelPoint? _lastWindowPosition;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        ConfigureServices();
    }

    private void ConfigureServices()
    {
        var services = new ServiceCollection();

        // Регистрация сервисов
        services.AddSingleton<IAppSettingsService, AppSettingsService>();
        services.AddSingleton<IDatabaseService, MySqlDatabaseService>();
        services.AddSingleton<IExportService, SqlExportService>();

        // Регистрация ViewModels
        services.AddTransient<ConnectionWindowViewModel>();
        services.AddTransient<MainWindowViewModel>();

        Services = services.BuildServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _desktop = desktop;

            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            DisableAvaloniaDataAnnotationValidation();

            // Запускаем с окна подключения
            ShowConnectionWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ShowConnectionWindow()
    {
        if (_desktop == null || Services == null) return;

        var viewModel = Services.GetRequiredService<ConnectionWindowViewModel>();

        _connectionWindow = new ConnectionWindow
        {
            DataContext = viewModel
        };

        // Подписываемся на событие успешного подключения
        viewModel.ConnectionSucceeded += OnConnectionSucceeded;

        _desktop.MainWindow = _connectionWindow;
        _connectionWindow.Show();
    }

    private async void OnConnectionSucceeded(object? sender, ConnectionConfig connectionConfig)
    {
        if (_desktop == null || Services == null) return;

        // Сохраняем позицию окна подключения
        if (_connectionWindow != null)
        {
            SaveWindowPosition(_connectionWindow);
        }

        // Создаём главное окно
        var mainViewModel = Services.GetRequiredService<MainWindowViewModel>();

        _mainWindow = new MainWindow
        {
            DataContext = mainViewModel,
            WindowStartupLocation = WindowStartupLocation.Manual
        };

        // Передаём ссылку на окно в ViewModel для диалогов
        mainViewModel.SetWindow(_mainWindow);

        // Подписываемся на событие смены подключения
        mainViewModel.ChangeConnectionRequested += OnChangeConnectionRequested;

        // Обработчик для установки позиции после открытия окна
        _mainWindow.Opened += (s, e) =>
        {
            RestoreWindowPositionOnSameScreen(_mainWindow);
        };

        // Инициализируем MainWindow с подключением
        await mainViewModel.InitializeWithConnectionAsync(connectionConfig);

        // Переключаемся на главное окно
        _desktop.MainWindow = _mainWindow;
        _mainWindow.Show();

        // Закрываем окно подключения
        _connectionWindow?.Close();
        _connectionWindow = null;
    }

    private void OnChangeConnectionRequested(object? sender, System.EventArgs e)
    {
        if (_desktop == null || Services == null) return;

        // Сохраняем позицию главного окна
        if (_mainWindow != null)
        {
            SaveWindowPosition(_mainWindow);
        }

        // Сохраняем ссылку на старое окно
        var oldMainWindow = _mainWindow;

        // Создаём окно подключения
        var viewModel = Services.GetRequiredService<ConnectionWindowViewModel>();

        _connectionWindow = new ConnectionWindow
        {
            DataContext = viewModel,
            WindowStartupLocation = WindowStartupLocation.Manual
        };

        // Подписываемся на событие успешного подключения
        viewModel.ConnectionSucceeded += OnConnectionSucceeded;

        // Обработчик для установки позиции после открытия окна
        _connectionWindow.Opened += (s, e) =>
        {
            RestoreWindowPositionOnSameScreen(_connectionWindow);
        };

        // ВАЖНО: Сначала устанавливаем новое главное окно
        _desktop.MainWindow = _connectionWindow;
        _connectionWindow.Show();

        // Затем закрываем старое главное окно
        oldMainWindow?.Close();
        _mainWindow = null;
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    /// <summary>
    /// Сохраняет позицию окна для последующего восстановления на том же мониторе
    /// </summary>
    private void SaveWindowPosition(Window window)
    {
        if (window.WindowState == WindowState.Normal)
        {
            _lastWindowPosition = window.Position;
        }
    }

    /// <summary>
    /// Устанавливает позицию окна на том же мониторе, где было предыдущее окно
    /// </summary>
    private void RestoreWindowPositionOnSameScreen(Window window)
    {
        if (!_lastWindowPosition.HasValue || window.Screens == null)
            return;

        // Находим монитор, который содержит последнюю сохранённую позицию
        var targetScreen = window.Screens.All.FirstOrDefault(screen =>
            screen.WorkingArea.Contains(_lastWindowPosition.Value))
            ?? window.Screens.Primary;

        if (targetScreen != null)
        {
            // Центрируем окно на найденном мониторе
            var screenCenter = targetScreen.WorkingArea.Center;
            var windowWidth = (int)(window.Width > 0 ? window.Width : window.MinWidth);
            var windowHeight = (int)(window.Height > 0 ? window.Height : window.MinHeight);

            var x = screenCenter.X - windowWidth / 2;
            var y = screenCenter.Y - windowHeight / 2;

            // Корректируем позицию, чтобы окно не выходило за границы экрана
            x = Math.Max(targetScreen.WorkingArea.X, Math.Min(x, targetScreen.WorkingArea.Right - windowWidth));
            y = Math.Max(targetScreen.WorkingArea.Y, Math.Min(y, targetScreen.WorkingArea.Bottom - windowHeight));

            window.Position = new PixelPoint(x, y);
        }
    }
}
