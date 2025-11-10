using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
    private readonly IAppSettingsService _appSettingsService;
    private ConnectionConfig? _connectionConfig;

    // Поиск и фильтрация
    [ObservableProperty]
    private string _searchText = string.Empty;

    // Статистика
    [ObservableProperty]
    private int _selectedCount;

    [ObservableProperty]
    private long _totalRows;

    [ObservableProperty]
    private string _totalSize = "0 B";

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

    // События
    public event EventHandler? ChangeConnectionRequested;

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

    private void ApplyFilters()
    {
        var filtered = AllTables.AsEnumerable();

        // Поиск по имени
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(t =>
                t.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
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
    private void ToggleAllTablesSelection()
    {
        // Если все таблицы выбраны - снять выбор со всех
        if (TablesSelectionState == true)
        {
            foreach (var table in FilteredTables)
            {
                table.IsSelected = false;
            }
        }
        // Если выбраны не все или не выбрано ни одной - выбрать все
        else
        {
            foreach (var table in FilteredTables)
            {
                table.IsSelected = true;
            }
        }
    }

    #endregion

    #region Export Commands

    /// <summary>
    /// Генерирует имя файла для экспорта на основе выбранных таблиц
    /// </summary>
    private string GenerateExportFileName(List<TableInfo> selectedTables)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        if (selectedTables.Count == 1)
        {
            // Если выбрана только одна таблица: ИмяБазыДанных_ИмяТаблицы_ДатаВремя.sql
            return $"{SelectedDatabase}_{selectedTables[0].Name}_{timestamp}.sql";
        }
        else
        {
            // Если таблиц несколько: ИмяБазыДанных_ДатаВремя.sql
            return $"{SelectedDatabase}_{timestamp}.sql";
        }
    }

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

        // Получаем последнюю использованную папку или текущую директорию
        var lastDirectory = await _appSettingsService.LoadLastExportDirectoryAsync();
        var defaultDirectory = string.IsNullOrEmpty(lastDirectory)
            ? AppContext.BaseDirectory
            : lastDirectory;

        // Генерируем предложенное имя файла
        var suggestedFileName = GenerateExportFileName(selectedTables);

        // Открываем диалог сохранения файла
        var topLevel = Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;

        if (topLevel == null)
        {
            StatusMessage = "✗ Не удалось открыть диалог сохранения файла";
            return;
        }

        var saveDialog = new Avalonia.Platform.Storage.FilePickerSaveOptions
        {
            Title = "Сохранить экспорт",
            SuggestedFileName = suggestedFileName,
            DefaultExtension = "sql",
            FileTypeChoices = new[]
            {
                new Avalonia.Platform.Storage.FilePickerFileType("SQL файлы")
                {
                    Patterns = new[] { "*.sql" }
                },
                new Avalonia.Platform.Storage.FilePickerFileType("Все файлы")
                {
                    Patterns = new[] { "*" }
                }
            }
        };

        // Устанавливаем начальную директорию
        if (Directory.Exists(defaultDirectory))
        {
            saveDialog.SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(new Uri(defaultDirectory));
        }

        var result = await topLevel.StorageProvider.SaveFilePickerAsync(saveDialog);

        if (result == null)
        {
            // Пользователь отменил диалог
            StatusMessage = "Экспорт отменен";
            return;
        }

        var outputPath = result.Path.LocalPath;

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

            var exportResult = await _exportService.ExportAsync(_connectionConfig, options, progress);

            if (exportResult.Success)
            {
                // Сохраняем папку для следующего экспорта
                var exportDirectory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(exportDirectory))
                {
                    await _appSettingsService.SaveLastExportDirectoryAsync(exportDirectory);
                }

                StatusMessage = $"✓ Экспорт завершен: {exportResult.TablesExported} таблиц, {exportResult.RowsExported:N0} строк. Файл: {exportResult.OutputPath}";
            }
            else
            {
                StatusMessage = $"✗ Ошибка экспорта: {exportResult.ErrorMessage}";
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
