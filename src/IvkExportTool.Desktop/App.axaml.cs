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
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;

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
        // Регистрация провайдера иконок FontAwesome
        IconProvider.Current.Register<FontAwesomeIconProvider>();

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

        // Устанавливаем новое окно как главное ДО закрытия старого
        _desktop.MainWindow = _startWindow;
        _startWindow.Show();

        // Закрываем старые окна ПОСЛЕ установки нового главного окна (кроме текущего)
        CloseAllWindows(_startWindow);
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

        // Устанавливаем новое окно как главное ДО закрытия старого
        _desktop.MainWindow = _connectionWindow;
        _connectionWindow.Show();

        // Закрываем старые окна ПОСЛЕ установки нового главного окна (кроме текущего)
        CloseAllWindows(_connectionWindow);
    }

    private void ShowAutoConnectionWindow()
    {
        if (_desktop == null || Services == null) return;

        // Сохраняем позицию текущего окна
        SaveCurrentWindowPosition();

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

        // Устанавливаем новое окно как главное ДО закрытия старого
        _desktop.MainWindow = _autoConnectionWindow;
        _autoConnectionWindow.Show();

        // Закрываем старые окна ПОСЛЕ установки нового главного окна (кроме текущего)
        CloseAllWindows(_autoConnectionWindow);
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

    private void CloseAllWindows(Window? except = null)
    {
        if (_startWindow != null && _startWindow != except)
        {
            _startWindow.Close();
            _startWindow = null;
        }

        if (_connectionWindow != null && _connectionWindow != except)
        {
            _connectionWindow.Close();
            _connectionWindow = null;
        }

        if (_autoConnectionWindow != null && _autoConnectionWindow != except)
        {
            _autoConnectionWindow.Close();
            _autoConnectionWindow = null;
        }

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

        // Показываем стартовое окно ПЕРЕД закрытием главного окна
        // (ShowStartWindow устанавливает _desktop.MainWindow, что предотвращает завершение приложения)
        ShowStartWindow();

        // Закрываем главное окно ПОСЛЕ установки нового главного окна
        oldMainWindow?.Close();
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
    /// Устанавливает позицию окна на том же мониторе, где было предыдущее окно,
    /// или центрирует на основном мониторе при первом запуске
    /// </summary>
    private void RestoreWindowPositionOnSameScreen(Window window)
    {
        if (window.Screens == null)
            return;

        // Определяем целевой монитор:
        // - если есть сохранённая позиция, ищем монитор, содержащий эту точку
        // - иначе используем основной монитор (первый запуск)
        var targetScreen = _lastWindowPosition.HasValue
            ? window.Screens.All.FirstOrDefault(screen =>
                screen.WorkingArea.Contains(_lastWindowPosition.Value))
                ?? window.Screens.Primary
            : window.Screens.Primary;

        if (targetScreen is not null)
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
