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

        // Создаём главное окно
        var mainViewModel = Services.GetRequiredService<MainWindowViewModel>();

        _mainWindow = new MainWindow
        {
            DataContext = mainViewModel
        };

        // Подписываемся на событие смены подключения
        mainViewModel.ChangeConnectionRequested += OnChangeConnectionRequested;

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
        if (_desktop == null) return;

        // Закрываем главное окно
        _mainWindow?.Close();
        _mainWindow = null;

        // Показываем окно подключения
        ShowConnectionWindow();
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
}
