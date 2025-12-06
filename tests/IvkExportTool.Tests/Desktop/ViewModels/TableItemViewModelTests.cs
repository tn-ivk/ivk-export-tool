using FluentAssertions;
using IvkExportTool.Core.Models;
using IvkExportTool.Desktop.ViewModels;
using IvkExportTool.Tests.Helpers;

namespace IvkExportTool.Tests.Desktop.ViewModels;

[TestFixture]
public class TableItemViewModelTests
{
    #region Constructor Tests

    [Test]
    public void Constructor_SetsPropertiesFromTableInfo()
    {
        // Arrange
        var tableInfo = TestHelpers.CreateTableInfo(
            name: "users",
            rowCount: 1000,
            sizeBytes: 1024 * 1024,
            engine: "InnoDB",
            isSelected: true);

        // Act
        var viewModel = new TableItemViewModel(tableInfo);

        // Assert
        viewModel.Name.Should().Be("users");
        viewModel.RowCount.Should().Be(1000);
        viewModel.SizeInBytes.Should().Be(1024 * 1024);
        viewModel.Engine.Should().Be("InnoDB");
        viewModel.IsSelected.Should().BeTrue();
    }

    [Test]
    public void Constructor_DefaultTableInfo_HasDefaultValues()
    {
        // Arrange
        var tableInfo = new TableInfo();

        // Act
        var viewModel = new TableItemViewModel(tableInfo);

        // Assert
        viewModel.Name.Should().BeEmpty();
        viewModel.RowCount.Should().Be(0);
        viewModel.SizeInBytes.Should().Be(0);
        viewModel.Engine.Should().BeEmpty();
        viewModel.IsSelected.Should().BeFalse();
    }

    #endregion

    #region IsSelected Synchronization Tests

    [Test]
    public void IsSelected_Changed_UpdatesTableInfo()
    {
        // Arrange
        var tableInfo = TestHelpers.CreateTableInfo(isSelected: false);
        var viewModel = new TableItemViewModel(tableInfo);

        // Act
        viewModel.IsSelected = true;

        // Assert
        tableInfo.IsSelected.Should().BeTrue();
    }

    [Test]
    public void IsSelected_ChangedToFalse_UpdatesTableInfo()
    {
        // Arrange
        var tableInfo = TestHelpers.CreateTableInfo(isSelected: true);
        var viewModel = new TableItemViewModel(tableInfo);

        // Act
        viewModel.IsSelected = false;

        // Assert
        tableInfo.IsSelected.Should().BeFalse();
    }

    [Test]
    public void IsSelected_RaisesPropertyChangedEvent()
    {
        // Arrange
        var tableInfo = TestHelpers.CreateTableInfo();
        var viewModel = new TableItemViewModel(tableInfo);
        var eventRaised = false;

        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TableItemViewModel.IsSelected))
                eventRaised = true;
        };

        // Act
        viewModel.IsSelected = true;

        // Assert
        eventRaised.Should().BeTrue();
    }

    #endregion

    #region SizeFormatted Tests

    [TestCase(0, "0 B")]
    [TestCase(500, "500 B")]
    [TestCase(1023, "1023 B")]
    [TestCase(1024, "1 KB")]
    [TestCase(2048, "2 KB")]
    [TestCase(1048576, "1 MB")]
    [TestCase(1073741824, "1 GB")]
    [TestCase(1099511627776, "1 TB")]
    public void SizeFormatted_VariousSizes_ReturnsCorrectFormat(long bytes, string expected)
    {
        // Arrange
        var tableInfo = TestHelpers.CreateTableInfo(sizeBytes: bytes);
        var viewModel = new TableItemViewModel(tableInfo);

        // Act
        var result = viewModel.SizeFormatted;

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void SizeFormatted_LargeSize_UsesCorrectUnit()
    {
        // Arrange
        var tableInfo = TestHelpers.CreateTableInfo(sizeBytes: 5_368_709_120); // 5 GB
        var viewModel = new TableItemViewModel(tableInfo);

        // Act
        var result = viewModel.SizeFormatted;

        // Assert
        result.Should().Be("5 GB");
    }

    [Test]
    public void SizeFormatted_FractionalKilobytes_ShowsDecimals()
    {
        // Arrange
        var tableInfo = TestHelpers.CreateTableInfo(sizeBytes: 1126); // ~1.1 KB
        var viewModel = new TableItemViewModel(tableInfo);

        // Act
        var result = viewModel.SizeFormatted;

        // Assert - проверяем формат независимо от локали (точка или запятая)
        result.Should().StartWith("1");
        result.Should().EndWith("KB");
        result.Should().MatchRegex(@"^1[,.]1 KB$");
    }

    #endregion

    #region GetTableInfo Tests

    [Test]
    public void GetTableInfo_ReturnsOriginalTableInfo()
    {
        // Arrange
        var tableInfo = TestHelpers.CreateTableInfo(name: "original_table");
        var viewModel = new TableItemViewModel(tableInfo);

        // Act
        var result = viewModel.GetTableInfo();

        // Assert
        result.Should().BeSameAs(tableInfo);
    }

    [Test]
    public void GetTableInfo_ReturnsTableInfoWithUpdatedIsSelected()
    {
        // Arrange
        var tableInfo = TestHelpers.CreateTableInfo(isSelected: false);
        var viewModel = new TableItemViewModel(tableInfo);

        // Act
        viewModel.IsSelected = true;
        var result = viewModel.GetTableInfo();

        // Assert
        result.IsSelected.Should().BeTrue();
    }

    #endregion

    #region Property Proxy Tests

    [Test]
    public void Name_ReturnsTableInfoName()
    {
        // Arrange
        var tableInfo = TestHelpers.CreateTableInfo(name: "test_table");
        var viewModel = new TableItemViewModel(tableInfo);

        // Act & Assert
        viewModel.Name.Should().Be("test_table");
    }

    [Test]
    public void RowCount_ReturnsTableInfoRowCount()
    {
        // Arrange
        var tableInfo = TestHelpers.CreateTableInfo(rowCount: 42);
        var viewModel = new TableItemViewModel(tableInfo);

        // Act & Assert
        viewModel.RowCount.Should().Be(42);
    }

    [Test]
    public void SizeInBytes_ReturnsTableInfoSizeBytes()
    {
        // Arrange
        var tableInfo = TestHelpers.CreateTableInfo(sizeBytes: 12345);
        var viewModel = new TableItemViewModel(tableInfo);

        // Act & Assert
        viewModel.SizeInBytes.Should().Be(12345);
    }

    [Test]
    public void Engine_ReturnsTableInfoEngine()
    {
        // Arrange
        var tableInfo = TestHelpers.CreateTableInfo(engine: "MyISAM");
        var viewModel = new TableItemViewModel(tableInfo);

        // Act & Assert
        viewModel.Engine.Should().Be("MyISAM");
    }

    #endregion
}
