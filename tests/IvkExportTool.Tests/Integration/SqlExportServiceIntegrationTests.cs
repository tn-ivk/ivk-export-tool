using System.Text.RegularExpressions;
using FluentAssertions;
using IvkExportTool.Core.Enums;
using IvkExportTool.Core.Models;
using IvkExportTool.Infrastructure.Services;

namespace IvkExportTool.Tests.Integration;

/// <summary>
/// Интеграционные тесты для SqlExportService с использованием Testcontainers
/// </summary>
[TestFixture]
[Category("Integration")]
public class SqlExportServiceIntegrationTests : MySqlIntegrationTestBase
{
    private SqlExportService _service = null!;
    private string _tempDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new SqlExportService();
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"SqlExportTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    #region Basic Export Tests

    [Test]
    public async Task ExportAsync_SingleTable_CreatesFile()
    {
        // Arrange
        await CreateTestTableAsync("export_test", 5);
        var outputPath = Path.Combine(_tempDirectory, "export.sql");

        var options = new ExportOptions
        {
            OutputPath = outputPath,
            Tables = new List<string> { "export_test" },
            Format = ExportFormat.Sql
        };

        try
        {
            // Act
            var result = await _service.ExportAsync(ConnectionConfig, options);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.OutputPath.Should().Be(outputPath);
            result.TablesExported.Should().Be(1);
            result.RowsExported.Should().Be(5);
            File.Exists(outputPath).Should().BeTrue();
        }
        finally
        {
            await DropTestTableAsync("export_test");
        }
    }

    [Test]
    public async Task ExportAsync_MultipleTables_ExportsAll()
    {
        // Arrange
        await CreateMultipleTestTablesAsync("users", "orders", "products");
        var outputPath = Path.Combine(_tempDirectory, "multi_export.sql");

        var options = new ExportOptions
        {
            OutputPath = outputPath,
            Tables = new List<string> { "users", "orders", "products" },
            Format = ExportFormat.Sql
        };

        try
        {
            // Act
            var result = await _service.ExportAsync(ConnectionConfig, options);

            // Assert
            result.Success.Should().BeTrue();
            result.TablesExported.Should().Be(3);
            result.RowsExported.Should().Be(15); // 5 rows per table
        }
        finally
        {
            await DropTestTableAsync("users");
            await DropTestTableAsync("orders");
            await DropTestTableAsync("products");
        }
    }

    [Test]
    public async Task ExportAsync_EmptyTable_ExportsStructureOnly()
    {
        // Arrange
        await CreateTestTableAsync("empty_table", 0);
        var outputPath = Path.Combine(_tempDirectory, "empty.sql");

        var options = new ExportOptions
        {
            OutputPath = outputPath,
            Tables = new List<string> { "empty_table" },
            IncludeStructure = true,
            IncludeData = true
        };

        try
        {
            // Act
            var result = await _service.ExportAsync(ConnectionConfig, options);

            // Assert
            result.Success.Should().BeTrue();
            result.TablesExported.Should().Be(1);
            result.RowsExported.Should().Be(0);

            var content = await File.ReadAllTextAsync(outputPath);
            content.Should().Contain("CREATE TABLE");
            content.Should().NotContain("INSERT INTO");
        }
        finally
        {
            await DropTestTableAsync("empty_table");
        }
    }

    #endregion

    #region SQL Content Tests

    [Test]
    public async Task ExportAsync_IncludesDropTable_WhenEnabled()
    {
        // Arrange
        await CreateTestTableAsync("drop_test", 1);
        var outputPath = Path.Combine(_tempDirectory, "drop.sql");

        var options = new ExportOptions
        {
            OutputPath = outputPath,
            Tables = new List<string> { "drop_test" },
            IncludeDropTable = true
        };

        try
        {
            // Act
            var result = await _service.ExportAsync(ConnectionConfig, options);

            // Assert
            result.Success.Should().BeTrue();
            var content = await File.ReadAllTextAsync(outputPath);
            content.Should().Contain("DROP TABLE IF EXISTS");
        }
        finally
        {
            await DropTestTableAsync("drop_test");
        }
    }

    [Test]
    public async Task ExportAsync_ExcludesDropTable_WhenDisabled()
    {
        // Arrange
        await CreateTestTableAsync("no_drop_test", 1);
        var outputPath = Path.Combine(_tempDirectory, "no_drop.sql");

        var options = new ExportOptions
        {
            OutputPath = outputPath,
            Tables = new List<string> { "no_drop_test" },
            IncludeDropTable = false
        };

        try
        {
            // Act
            var result = await _service.ExportAsync(ConnectionConfig, options);

            // Assert
            result.Success.Should().BeTrue();
            var content = await File.ReadAllTextAsync(outputPath);
            content.Should().NotContain("DROP TABLE");
        }
        finally
        {
            await DropTestTableAsync("no_drop_test");
        }
    }

    [Test]
    public async Task ExportAsync_StructureOnly_NoInsertStatements()
    {
        // Arrange
        await CreateTestTableAsync("struct_only", 10);
        var outputPath = Path.Combine(_tempDirectory, "struct.sql");

        var options = new ExportOptions
        {
            OutputPath = outputPath,
            Tables = new List<string> { "struct_only" },
            IncludeStructure = true,
            IncludeData = false
        };

        try
        {
            // Act
            var result = await _service.ExportAsync(ConnectionConfig, options);

            // Assert
            result.Success.Should().BeTrue();
            result.RowsExported.Should().Be(0);

            var content = await File.ReadAllTextAsync(outputPath);
            content.Should().Contain("CREATE TABLE");
            content.Should().NotContain("INSERT INTO");
        }
        finally
        {
            await DropTestTableAsync("struct_only");
        }
    }

    [Test]
    public async Task ExportAsync_DataOnly_NoCreateStatements()
    {
        // Arrange
        await CreateTestTableAsync("data_only", 3);
        var outputPath = Path.Combine(_tempDirectory, "data.sql");

        var options = new ExportOptions
        {
            OutputPath = outputPath,
            Tables = new List<string> { "data_only" },
            IncludeStructure = false,
            IncludeData = true,
            IncludeDropTable = false
        };

        try
        {
            // Act
            var result = await _service.ExportAsync(ConnectionConfig, options);

            // Assert
            result.Success.Should().BeTrue();
            result.RowsExported.Should().Be(3);

            var content = await File.ReadAllTextAsync(outputPath);
            content.Should().NotContain("CREATE TABLE");
            content.Should().Contain("INSERT INTO");
        }
        finally
        {
            await DropTestTableAsync("data_only");
        }
    }

    [Test]
    public async Task ExportAsync_ContainsValidInsertStatements()
    {
        // Arrange
        await CreateTestTableAsync("insert_test", 3);
        var outputPath = Path.Combine(_tempDirectory, "inserts.sql");

        var options = new ExportOptions
        {
            OutputPath = outputPath,
            Tables = new List<string> { "insert_test" },
            IncludeData = true
        };

        try
        {
            // Act
            var result = await _service.ExportAsync(ConnectionConfig, options);

            // Assert
            result.Success.Should().BeTrue();

            var content = await File.ReadAllTextAsync(outputPath);
            var insertCount = Regex.Matches(content, @"INSERT INTO `insert_test`").Count;
            insertCount.Should().BeGreaterThan(0);
        }
        finally
        {
            await DropTestTableAsync("insert_test");
        }
    }

    #endregion

    #region Progress Tests

    [Test]
    public async Task ExportAsync_ReportsProgress()
    {
        // Arrange
        await CreateTestTableAsync("progress_test", 10);
        var outputPath = Path.Combine(_tempDirectory, "progress.sql");
        var progressValues = new List<int>();

        var options = new ExportOptions
        {
            OutputPath = outputPath,
            Tables = new List<string> { "progress_test" }
        };

        var progress = new Progress<int>(value => progressValues.Add(value));

        try
        {
            // Act
            var result = await _service.ExportAsync(ConnectionConfig, options, progress);

            // Assert
            result.Success.Should().BeTrue();
            // Дождёмся обработки прогресса
            await Task.Delay(100);
            progressValues.Should().Contain(100); // должен быть 100% в конце
        }
        finally
        {
            await DropTestTableAsync("progress_test");
        }
    }

    [Test]
    public async Task ExportAsync_ReportsDetailedProgress()
    {
        // Arrange
        await CreateTestTableAsync("detailed_progress", 5);
        var outputPath = Path.Combine(_tempDirectory, "detailed.sql");
        var detailedProgressList = new List<ExportProgress>();

        var options = new ExportOptions
        {
            OutputPath = outputPath,
            Tables = new List<string> { "detailed_progress" }
        };

        var detailedProgress = new Progress<ExportProgress>(p => detailedProgressList.Add(p));

        try
        {
            // Act
            var result = await _service.ExportAsync(ConnectionConfig, options, null, detailedProgress);

            // Assert
            result.Success.Should().BeTrue();
            await Task.Delay(100);
            detailedProgressList.Should().NotBeEmpty();
        }
        finally
        {
            await DropTestTableAsync("detailed_progress");
        }
    }

    #endregion

    #region Cancellation Tests

    [Test]
    public async Task ExportAsync_WithCancellation_StopsExport()
    {
        // Arrange
        await CreateTestTableAsync("cancel_test", 100);
        var outputPath = Path.Combine(_tempDirectory, "cancel.sql");

        var options = new ExportOptions
        {
            OutputPath = outputPath,
            Tables = new List<string> { "cancel_test" }
        };

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // немедленная отмена

        try
        {
            // Act
            var result = await _service.ExportAsync(
                ConnectionConfig, options, null, null, cts.Token);

            // Assert
            result.Success.Should().BeFalse();
        }
        finally
        {
            await DropTestTableAsync("cancel_test");
        }
    }

    #endregion

    #region Special Characters Tests

    [Test]
    public async Task ExportAsync_EscapesSpecialCharacters()
    {
        // Arrange
        await CreateTestTableAsync("escape_test", 5);
        var outputPath = Path.Combine(_tempDirectory, "escape.sql");

        var options = new ExportOptions
        {
            OutputPath = outputPath,
            Tables = new List<string> { "escape_test" },
            IncludeData = true
        };

        try
        {
            // Act
            var result = await _service.ExportAsync(ConnectionConfig, options);

            // Assert
            result.Success.Should().BeTrue();

            var content = await File.ReadAllTextAsync(outputPath);
            // Проверяем, что кавычки экранированы
            content.Should().Contain("\\'");
            // Проверяем, что бэкслеши экранированы
            content.Should().Contain("\\\\");
        }
        finally
        {
            await DropTestTableAsync("escape_test");
        }
    }

    #endregion

    #region Duration Tests

    [Test]
    public async Task ExportAsync_RecordsDuration()
    {
        // Arrange
        await CreateTestTableAsync("duration_test", 5);
        var outputPath = Path.Combine(_tempDirectory, "duration.sql");

        var options = new ExportOptions
        {
            OutputPath = outputPath,
            Tables = new List<string> { "duration_test" }
        };

        try
        {
            // Act
            var result = await _service.ExportAsync(ConnectionConfig, options);

            // Assert
            result.Success.Should().BeTrue();
            result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
        }
        finally
        {
            await DropTestTableAsync("duration_test");
        }
    }

    #endregion

    #region Error Handling Tests

    [Test]
    public async Task ExportAsync_NonExistentTable_ReturnsFailure()
    {
        // Arrange
        var outputPath = Path.Combine(_tempDirectory, "nonexistent.sql");

        var options = new ExportOptions
        {
            OutputPath = outputPath,
            Tables = new List<string> { "table_that_does_not_exist" }
        };

        // Act
        var result = await _service.ExportAsync(ConnectionConfig, options);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task ExportAsync_InvalidOutputPath_ReturnsFailure()
    {
        // Arrange
        await CreateTestTableAsync("path_test", 1);
        var invalidPath = Path.Combine("Z:\\NonExistent\\Path", "export.sql");

        var options = new ExportOptions
        {
            OutputPath = invalidPath,
            Tables = new List<string> { "path_test" }
        };

        try
        {
            // Act
            var result = await _service.ExportAsync(ConnectionConfig, options);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrEmpty();
        }
        finally
        {
            await DropTestTableAsync("path_test");
        }
    }

    #endregion
}
