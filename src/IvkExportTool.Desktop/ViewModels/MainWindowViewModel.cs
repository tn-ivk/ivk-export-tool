using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
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
    private ConnectionConfig? _connectionConfig;

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

    // Состояние чекбокса в заголовке (тристейтный)
    [ObservableProperty]
    private bool? _tablesSelectionState = false;

    // Экспорт
    [ObservableProperty]
    private int _exportProgress;

    [ObservableProperty]
    private bool _isExporting;

    [ObservableProperty]
    private string _statusMessage = "Загрузка...";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _selectedDatabase;

    // Коллекции
    public ObservableCollection<string> Databases { get; } = new();

    [ObservableProperty]
    private ObservableCollection<TableItemViewModel> _allTables = new();

    [ObservableProperty]
    private ObservableCollection<TableItemViewModel> _filteredTables = new();

    // Свойства для ConnectionStatusBar
    public string ConnectionString => _connectionConfig != null
        ? $"{_connectionConfig.Username}@{_connectionConfig.Host}:{_connectionConfig.Port}"
        : "Не подключено";

    public SolidColorBrush ConnectionStatusColor => _connectionConfig != null
        ? new SolidColorBrush(Color.Parse("#4caf50"))  // Success green
        : new SolidColorBrush(Color.Parse("#f44336")); // Error red

    public bool HasSelectedTables => SelectedCount > 0;

    public bool HasActiveFilters => ActiveFiltersCount > 0;

    // События
    public event EventHandler? ChangeConnectionRequested;

    public MainWindowViewModel() : this(null!, null!)
    {
        // Конструктор для дизайнера
    }

    public MainWindowViewModel(
        IDatabaseService databaseService,
        IExportService exportService)
    {
        _databaseService = databaseService;
        _exportService = exportService;
    }

    /// <summary>
    /// Инициализация с существующим подключением
    /// </summary>
    public async Task InitializeWithConnectionAsync(ConnectionConfig connectionConfig)
    {
        _connectionConfig = connectionConfig;
        OnPropertyChanged(nameof(ConnectionString));
        OnPropertyChanged(nameof(ConnectionStatusColor));

        IsLoading = true;
        StatusMessage = "Загрузка списка баз данных...";

        try
        {
            var databases = await _databaseService.GetDatabasesAsync(connectionConfig);

            Databases.Clear();
            foreach (var db in databases)
            {
                Databases.Add(db);
            }

            if (Databases.Count > 0)
            {
                // Устанавливаем первую БД, что вызовет OnSelectedDatabaseChanged
                // и загрузит таблицы через RefreshTablesAsync
                SelectedDatabase = Databases[0];
                // RefreshTablesAsync управляет IsLoading самостоятельно
            }
            else
            {
                StatusMessage = "Не найдено баз данных";
                IsLoading = false;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
            IsLoading = false;
        }
    }

    partial void OnSelectedDatabaseChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value) && _connectionConfig != null)
        {
            _connectionConfig.Database = value;
            _ = RefreshTablesAsync();
        }
    }

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

    // Реакция на клик по чекбоксу в заголовке первой колонки
    partial void OnTablesSelectionStateChanged(bool? value)
    {
        if (value == true)
        {
            SelectAllTables();
        }
        else if (value == false)
        {
            DeselectAllTables();
        }
        // null — частичный выбор, ничего не делаем
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

        // Обновляем состояние чекбокса в заголовке
        if (FilteredTables.Count == 0)
        {
            TablesSelectionState = false;
        }
        else if (SelectedCount == 0)
        {
            TablesSelectionState = false;
        }
        else if (SelectedCount == FilteredTables.Count)
        {
            TablesSelectionState = true;
        }
        else
        {
            TablesSelectionState = null; // Частичный выбор
        }

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
    private void ChangeConnection()
    {
        ChangeConnectionRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task RefreshDatabasesAsync()
    {
        if (_connectionConfig == null) return;

        IsLoading = true;
        StatusMessage = "Обновление списка баз данных...";

        try
        {
            var databases = await _databaseService.GetDatabasesAsync(_connectionConfig);

            Databases.Clear();
            foreach (var db in databases)
            {
                Databases.Add(db);
            }

            StatusMessage = $"Загружено {Databases.Count} баз данных";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region Table Commands

    [RelayCommand]
    private async Task RefreshTablesAsync()
    {
        if (string.IsNullOrEmpty(SelectedDatabase) || _connectionConfig == null)
            return;

        IsLoading = true;
        AllTables.Clear();
        FilteredTables.Clear();
        StatusMessage = "Загрузка таблиц...";

        try
        {
            _connectionConfig.Database = SelectedDatabase;
            var dbInfo = await _databaseService.GetDatabaseInfoAsync(_connectionConfig);

            foreach (var table in dbInfo.Tables)
            {
                var tableVm = new TableItemViewModel(table);
                AllTables.Add(tableVm);
            }

            ApplyFilters();
            StatusMessage = $"Загружено {AllTables.Count} таблиц из базы '{SelectedDatabase}'";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки таблиц: {ex.Message}";
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

    // Команда ToggleAllTablesSelection больше не используется (заменена биндингом на TablesSelectionState)

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
        if (_connectionConfig == null) return;

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

            var result = await _exportService.ExportAsync(_connectionConfig, options, progress);

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
