using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IvkExportTool.Core.Enums;
using IvkExportTool.Core.Interfaces;
using IvkExportTool.Core.Models;

namespace IvkExportTool.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IExportService _exportService;

    [ObservableProperty]
    private string _host = "localhost";

    [ObservableProperty]
    private string _port = "3306";

    [ObservableProperty]
    private string _username = "root";

    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private string? _selectedDatabase;

    [ObservableProperty]
    private string _statusMessage = "Не подключено";

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _exportProgress;

    [ObservableProperty]
    private bool _isExporting;

    public ObservableCollection<string> Databases { get; } = new();
    public ObservableCollection<TableInfo> Tables { get; } = new();

    public MainWindowViewModel() : this(null!, null!)
    {
        // Конструктор для дизайнера
    }

    public MainWindowViewModel(IDatabaseService databaseService, IExportService exportService)
    {
        _databaseService = databaseService;
        _exportService = exportService;
    }

    private ConnectionConfig GetConnectionConfig()
    {
        return new ConnectionConfig
        {
            Host = Host,
            Port = int.TryParse(Port, out var port) ? port : 3306,
            Username = Username,
            Password = Password,
            Database = SelectedDatabase
        };
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        IsLoading = true;
        StatusMessage = "Тестирование подключения...";

        try
        {
            var config = GetConnectionConfig();
            var result = await _databaseService.TestConnectionAsync(config);

            if (result)
            {
                StatusMessage = "✓ Подключение успешно";
            }
            else
            {
                StatusMessage = "✗ Ошибка подключения";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ Ошибка: {ex.Message}";
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
        StatusMessage = "Подключение...";
        Databases.Clear();

        try
        {
            var config = GetConnectionConfig();
            var databases = await _databaseService.GetDatabasesAsync(config);

            foreach (var db in databases)
            {
                Databases.Add(db);
            }

            if (Databases.Count > 0)
            {
                SelectedDatabase = Databases[0];
                IsConnected = true;
                StatusMessage = $"✓ Подключено. Найдено {Databases.Count} баз данных";

                // Автоматически загружаем таблицы первой БД
                await RefreshTablesAsync();
            }
            else
            {
                StatusMessage = "✗ Не найдено баз данных";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ Ошибка: {ex.Message}";
            IsConnected = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshTablesAsync()
    {
        if (string.IsNullOrEmpty(SelectedDatabase))
            return;

        IsLoading = true;
        Tables.Clear();

        try
        {
            var config = GetConnectionConfig();
            var dbInfo = await _databaseService.GetDatabaseInfoAsync(config);

            foreach (var table in dbInfo.Tables)
            {
                Tables.Add(table);
            }

            StatusMessage = $"✓ Загружено {Tables.Count} таблиц из базы '{SelectedDatabase}'";
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ Ошибка загрузки таблиц: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SelectAllTables()
    {
        foreach (var table in Tables)
        {
            table.IsSelected = true;
        }
    }

    [RelayCommand]
    private void DeselectAllTables()
    {
        foreach (var table in Tables)
        {
            table.IsSelected = false;
        }
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        var selectedTables = Tables.Where(t => t.IsSelected).ToList();

        if (selectedTables.Count == 0)
        {
            StatusMessage = "✗ Выберите хотя бы одну таблицу для экспорта";
            return;
        }

        // Диалог сохранения файла (упрощенный вариант - используем текущую директорию)
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var outputPath = $"{SelectedDatabase}_{timestamp}.sql";

        IsExporting = true;
        ExportProgress = 0;
        StatusMessage = $"Экспорт {selectedTables.Count} таблиц...";

        try
        {
            var config = GetConnectionConfig();
            var options = new ExportOptions
            {
                OutputPath = outputPath,
                Format = ExportFormat.Sql,
                Tables = selectedTables.Select(t => t.Name).ToList(),
                IncludeStructure = true,
                IncludeData = true,
                IncludeDropTable = true,
                BatchSize = 1000
            };

            var progress = new Progress<int>(percent =>
            {
                ExportProgress = percent;
            });

            var result = await _exportService.ExportAsync(config, options, progress);

            if (result.Success)
            {
                StatusMessage = $"✓ Экспорт завершен: {result.TablesExported} таблиц, {result.RowsExported} строк. Файл: {result.OutputPath}";
            }
            else
            {
                StatusMessage = $"✗ Ошибка экспорта: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ Ошибка: {ex.Message}";
        }
        finally
        {
            IsExporting = false;
            ExportProgress = 0;
        }
    }
}
