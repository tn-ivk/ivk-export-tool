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
    private StartWindow? _startWindow;
    private ConnectionWindow? _connectionWindow;
    private AutoConnectionWindow? _autoConnectionWindow;
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
        services.AddTransient<StartWindowViewModel>();
        services.AddTransient<ConnectionWindowViewModel>();
        services.AddTransient<AutoConnectionWindowViewModel>();
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

            // Запускаем со стартового окна выбора способа подключения
            ShowStartWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ShowStartWindow()
    {
        if (_desktop == null || Services == null) return;

        // Сохраняем позицию текущего окна
        SaveCurrentWindowPosition();

        // Закрываем старые окна перед созданием нового
        CloseAllWindows();

        var viewModel = Services.GetRequiredService<StartWindowViewModel>();

        _startWindow = new StartWindow
        {
            DataContext = viewModel,
            WindowStartupLocation = WindowStartupLocation.Manual
        };

        // Подписываемся на события выбора типа подключения
        viewModel.ManualConnectionRequested += OnManualConnectionRequested;
        viewModel.AutoConnectionRequested += OnAutoConnectionRequested;

        // Обработчик для установки позиции после открытия окна
        _startWindow.Opened += (s, e) =>
        {
            RestoreWindowPositionOnSameScreen(_startWindow);
        };

        _desktop.MainWindow = _startWindow;
        _startWindow.Show();
    }

    private void OnManualConnectionRequested(object? sender, EventArgs e)
    {
        ShowConnectionWindow();
    }

    private void OnAutoConnectionRequested(object? sender, EventArgs e)
    {
        ShowAutoConnectionWindow();
    }

    private void ShowConnectionWindow()
    {
        if (_desktop == null || Services == null) return;

        // Сохраняем позицию текущего окна
        SaveCurrentWindowPosition();

        // Закрываем старые окна перед созданием нового
        CloseAllWindows();

        var viewModel = Services.GetRequiredService<ConnectionWindowViewModel>();

        _connectionWindow = new ConnectionWindow
        {
            DataContext = viewModel,
            WindowStartupLocation = WindowStartupLocation.Manual
        };

        // Подписываемся на события
        viewModel.ConnectionSucceeded += OnConnectionSucceeded;
        viewModel.BackRequested += OnBackFromConnectionRequested;

        // Обработчик для установки позиции после открытия окна
        _connectionWindow.Opened += (s, e) =>
        {
            RestoreWindowPositionOnSameScreen(_connectionWindow);
        };

        _desktop.MainWindow = _connectionWindow;
        _connectionWindow.Show();
    }

    private void ShowAutoConnectionWindow()
    {
        if (_desktop == null || Services == null) return;

        // Сохраняем позицию текущего окна
        SaveCurrentWindowPosition();

        // Закрываем старые окна перед созданием нового
        CloseAllWindows();

        var viewModel = Services.GetRequiredService<AutoConnectionWindowViewModel>();

        _autoConnectionWindow = new AutoConnectionWindow
        {
            DataContext = viewModel,
            WindowStartupLocation = WindowStartupLocation.Manual
        };

        // Подписываемся на события
        viewModel.ConnectionSucceeded += OnConnectionSucceeded;
        viewModel.BackRequested += OnBackFromAutoConnectionRequested;

        // Обработчик для установки позиции после открытия окна
        _autoConnectionWindow.Opened += (s, e) =>
        {
            RestoreWindowPositionOnSameScreen(_autoConnectionWindow);
        };

        _desktop.MainWindow = _autoConnectionWindow;
        _autoConnectionWindow.Show();
    }

    private void OnBackFromConnectionRequested(object? sender, EventArgs e)
    {
        ShowStartWindow();
    }

    private void OnBackFromAutoConnectionRequested(object? sender, EventArgs e)
    {
        ShowStartWindow();
    }

    private void SaveCurrentWindowPosition()
    {
        if (_startWindow != null && _startWindow.WindowState == WindowState.Normal)
        {
            _lastWindowPosition = _startWindow.Position;
        }
        else if (_connectionWindow != null && _connectionWindow.WindowState == WindowState.Normal)
        {
            _lastWindowPosition = _connectionWindow.Position;
        }
        else if (_autoConnectionWindow != null && _autoConnectionWindow.WindowState == WindowState.Normal)
        {
            _lastWindowPosition = _autoConnectionWindow.Position;
        }
        else if (_mainWindow != null && _mainWindow.WindowState == WindowState.Normal)
        {
            _lastWindowPosition = _mainWindow.Position;
        }
    }

    private void CloseAllWindows()
    {
        _startWindow?.Close();
        _startWindow = null;

        _connectionWindow?.Close();
        _connectionWindow = null;

        _autoConnectionWindow?.Close();
        _autoConnectionWindow = null;

        // Главное окно не закрываем здесь
    }

    private async void OnConnectionSucceeded(object? sender, ConnectionConfig connectionConfig)
    {
        if (_desktop == null || Services == null) return;

        // Сохраняем позицию текущего окна подключения
        SaveCurrentWindowPosition();

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

        // Закрываем окна подключения
        CloseAllWindows();
    }

    private void OnChangeConnectionRequested(object? sender, System.EventArgs e)
    {
        if (_desktop == null || Services == null) return;

        // Сохраняем позицию главного окна
        SaveCurrentWindowPosition();

        // Сохраняем ссылку на старое окно
        var oldMainWindow = _mainWindow;
        _mainWindow = null;

        // Закрываем главное окно и показываем стартовое окно
        oldMainWindow?.Close();

        // Показываем стартовое окно выбора способа подключения
        ShowStartWindow();
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
