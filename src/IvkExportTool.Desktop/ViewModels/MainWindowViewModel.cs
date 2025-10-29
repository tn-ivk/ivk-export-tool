using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IvkExportTool.Core.Enums;
using IvkExportTool.Core.Interfaces;
using IvkExportTool.Core.Models;
using System.Threading;

namespace IvkExportTool.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IExportService _exportService;
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

    [ObservableProperty]
    private string? _selectedDatabase;

    // Состояние подключения
    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Не подключено";

    // Экспорт
    [ObservableProperty]
    private int _exportProgress;

    [ObservableProperty]
    private bool _isExporting;

    // Поиск и фильтрация
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private long? _minSizeFilter;

    [ObservableProperty]
    private long? _minRowsFilter;

    [ObservableProperty]
    private bool _showEmptyTablesOnly;

    // Статистика
    [ObservableProperty]
    private int _selectedCount;

    [ObservableProperty]
    private long _totalRows;

    [ObservableProperty]
    private string _totalSize = "0 B";

    [ObservableProperty]
    private int _activeFiltersCount;

    // Коллекции
    public ObservableCollection<string> Databases { get; } = new();

    [ObservableProperty]
    private ObservableCollection<TableItemViewModel> _allTables = new();

    [ObservableProperty]
    private ObservableCollection<TableItemViewModel> _filteredTables = new();

    // Свойства для ConnectionStatusBar
    public string ConnectionString => IsConnected
        ? $"{Username}@{Host}:{Port}"
        : "Не подключено";

    public SolidColorBrush ConnectionStatusColor => IsConnected
        ? new SolidColorBrush(Color.Parse("#4caf50"))  // Success green
        : new SolidColorBrush(Color.Parse("#f44336")); // Error red

    public bool HasSelectedTables => SelectedCount > 0;

    public bool HasActiveFilters => ActiveFiltersCount > 0;

    public MainWindowViewModel() : this(null!, null!, null!)
    {
        // Конструктор для дизайнера
    }

    public MainWindowViewModel(
        IDatabaseService databaseService,
        IExportService exportService,
        IAppSettingsService appSettingsService)
    {
        _databaseService = databaseService;
        _exportService = exportService;
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
            Password = Password,
            Database = SelectedDatabase
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
            SelectedDatabase = config.Database;
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

    partial void OnHostChanged(string value)
    {
        ScheduleSaveSettings();
        OnPropertyChanged(nameof(ConnectionString));
    }

    partial void OnPortChanged(string value)
    {
        ScheduleSaveSettings();
        OnPropertyChanged(nameof(ConnectionString));
    }

    partial void OnUsernameChanged(string value)
    {
        ScheduleSaveSettings();
        OnPropertyChanged(nameof(ConnectionString));
    }

    partial void OnPasswordChanged(string value) => ScheduleSaveSettings();

    partial void OnSelectedDatabaseChanged(string? value)
    {
        ScheduleSaveSettings();
        if (!string.IsNullOrEmpty(value) && IsConnected)
        {
            _ = RefreshTablesAsync();
        }
    }

    partial void OnIsConnectedChanged(bool value)
    {
        OnPropertyChanged(nameof(ConnectionString));
        OnPropertyChanged(nameof(ConnectionStatusColor));
    }

    #endregion

    #region Search and Filtering

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnMinSizeFilterChanged(long? value)
    {
        UpdateActiveFiltersCount();
        ApplyFilters();
    }

    partial void OnMinRowsFilterChanged(long? value)
    {
        UpdateActiveFiltersCount();
        ApplyFilters();
    }

    partial void OnShowEmptyTablesOnlyChanged(bool value)
    {
        UpdateActiveFiltersCount();
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = AllTables.AsEnumerable();

        // Поиск по имени
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(t =>
                t.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        // Фильтр по размеру
        if (MinSizeFilter.HasValue)
        {
            filtered = filtered.Where(t => t.SizeInBytes >= MinSizeFilter.Value);
        }

        // Фильтр по количеству строк
        if (MinRowsFilter.HasValue)
        {
            filtered = filtered.Where(t => t.RowCount >= MinRowsFilter.Value);
        }

        // Показывать только пустые таблицы
        if (ShowEmptyTablesOnly)
        {
            filtered = filtered.Where(t => t.RowCount == 0);
        }

        FilteredTables = new ObservableCollection<TableItemViewModel>(filtered);

        // Подписываемся на изменения IsSelected для обновления статистики
        foreach (var table in FilteredTables)
        {
            table.PropertyChanged -= Table_PropertyChanged;
            table.PropertyChanged += Table_PropertyChanged;
        }

        UpdateStatistics();
    }

    private void Table_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TableItemViewModel.IsSelected))
        {
            UpdateStatistics();
        }
    }

    private void UpdateStatistics()
    {
        var selected = FilteredTables.Where(t => t.IsSelected).ToList();
        SelectedCount = selected.Count;
        TotalRows = selected.Sum(t => t.RowCount);
        TotalSize = FormatBytes(selected.Sum(t => t.SizeInBytes));

        OnPropertyChanged(nameof(HasSelectedTables));
    }

    private void UpdateActiveFiltersCount()
    {
        int count = 0;
        if (MinSizeFilter.HasValue) count++;
        if (MinRowsFilter.HasValue) count++;
        if (ShowEmptyTablesOnly) count++;

        ActiveFiltersCount = count;
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes == 0) return "0 B";

        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double size = bytes;
        int order = 0;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }

    [RelayCommand]
    private void ClearFilters()
    {
        MinSizeFilter = null;
        MinRowsFilter = null;
        ShowEmptyTablesOnly = false;
        SearchText = string.Empty;
    }

    #endregion

    #region Connection Commands

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
                if (string.IsNullOrEmpty(SelectedDatabase) || !Databases.Contains(SelectedDatabase))
                {
                    SelectedDatabase = Databases[0];
                }

                IsConnected = true;
                StatusMessage = $"✓ Подключено. Найдено {Databases.Count} баз данных";

                // Автоматически загружаем таблицы
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
    private void Disconnect()
    {
        IsConnected = false;
        Databases.Clear();
        AllTables.Clear();
        FilteredTables.Clear();
        SelectedDatabase = null;
        StatusMessage = "Отключено";
        ClearFilters();
    }

    [RelayCommand]
    private async Task RefreshDatabasesAsync()
    {
        if (!IsConnected) return;

        await ConnectAsync();
    }

    #endregion

    #region Table Commands

    [RelayCommand]
    private async Task RefreshTablesAsync()
    {
        if (string.IsNullOrEmpty(SelectedDatabase))
            return;

        IsLoading = true;
        AllTables.Clear();
        FilteredTables.Clear();

        try
        {
            var config = GetConnectionConfig();
            var dbInfo = await _databaseService.GetDatabaseInfoAsync(config);

            foreach (var table in dbInfo.Tables)
            {
                var tableVm = new TableItemViewModel(table);
                AllTables.Add(tableVm);
            }

            ApplyFilters();
            StatusMessage = $"✓ Загружено {AllTables.Count} таблиц из базы '{SelectedDatabase}'";
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
        foreach (var table in FilteredTables)
        {
            table.IsSelected = true;
        }
    }

    [RelayCommand]
    private void DeselectAllTables()
    {
        foreach (var table in FilteredTables)
        {
            table.IsSelected = false;
        }
    }

    [RelayCommand]
    private void FilterBySize(long minSizeMB)
    {
        MinSizeFilter = minSizeMB * 1024 * 1024; // Convert MB to bytes
    }

    [RelayCommand]
    private void FilterByRows(long minRows)
    {
        MinRowsFilter = minRows;
    }

    [RelayCommand]
    private void ShowEmptyTables()
    {
        ShowEmptyTablesOnly = !ShowEmptyTablesOnly;
    }

    #endregion

    #region Export Commands

    [RelayCommand]
    private async Task ExportAsync()
    {
        var selectedTables = FilteredTables
            .Where(t => t.IsSelected)
            .Select(t => t.GetTableInfo())
            .ToList();

        if (selectedTables.Count == 0)
        {
            StatusMessage = "✗ Выберите хотя бы одну таблицу для экспорта";
            return;
        }

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
                StatusMessage = $"✓ Экспорт завершен: {result.TablesExported} таблиц, {result.RowsExported:N0} строк. Файл: {result.OutputPath}";
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

    #endregion
}
