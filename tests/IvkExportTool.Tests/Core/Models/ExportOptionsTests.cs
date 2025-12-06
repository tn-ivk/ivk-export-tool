using FluentAssertions;
using IvkExportTool.Core.Enums;
using IvkExportTool.Core.Models;

namespace IvkExportTool.Tests.Core.Models;

[TestFixture]
public class ExportOptionsTests
{
    [Test]
    public void DefaultBatchSize_Is1000()
    {
        // Arrange & Act
        var options = new ExportOptions();

        // Assert
        options.BatchSize.Should().Be(1000);
    }

    [Test]
    public void DefaultFlags_AreTrue()
    {
        // Arrange & Act
        var options = new ExportOptions();

        // Assert
        options.IncludeStructure.Should().BeTrue();
        options.IncludeData.Should().BeTrue();
        options.IncludeDropTable.Should().BeTrue();
    }

    [Test]
    public void DefaultFormat_IsSql()
    {
        // Arrange & Act
        var options = new ExportOptions();

        // Assert
        options.Format.Should().Be(ExportFormat.Sql);
    }

    [Test]
    public void DefaultOutputPath_IsEmpty()
    {
        // Arrange & Act
        var options = new ExportOptions();

        // Assert
        options.OutputPath.Should().BeEmpty();
    }

    [Test]
    public void DefaultTables_IsEmptyList()
    {
        // Arrange & Act
        var options = new ExportOptions();

        // Assert
        options.Tables.Should().NotBeNull();
        options.Tables.Should().BeEmpty();
    }

    [Test]
    public void Tables_CanBeAddedAndRetrieved()
    {
        // Arrange
        var options = new ExportOptions();

        // Act
        options.Tables.Add("users");
        options.Tables.Add("orders");
        options.Tables.Add("products");

        // Assert
        options.Tables.Should().HaveCount(3);
        options.Tables.Should().Contain("users");
        options.Tables.Should().Contain("orders");
        options.Tables.Should().Contain("products");
    }

    [Test]
    public void AllProperties_CanBeSetAndRetrieved()
    {
        // Arrange & Act
        var options = new ExportOptions
        {
            OutputPath = "/path/to/export.sql",
            Format = ExportFormat.Sql,
            Tables = new List<string> { "table1", "table2" },
            IncludeStructure = false,
            IncludeData = true,
            IncludeDropTable = false,
            BatchSize = 500
        };

        // Assert
        options.OutputPath.Should().Be("/path/to/export.sql");
        options.Format.Should().Be(ExportFormat.Sql);
        options.Tables.Should().HaveCount(2);
        options.IncludeStructure.Should().BeFalse();
        options.IncludeData.Should().BeTrue();
        options.IncludeDropTable.Should().BeFalse();
        options.BatchSize.Should().Be(500);
    }

    [Test]
    public void BatchSize_CanBeSetToCustomValue()
    {
        // Arrange
        var options = new ExportOptions();

        // Act
        options.BatchSize = 5000;

        // Assert
        options.BatchSize.Should().Be(5000);
    }
}
