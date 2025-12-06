using FluentAssertions;
using IvkExportTool.Core.Models;

namespace IvkExportTool.Tests.Core.Models;

[TestFixture]
public class ExportResultTests
{
    [Test]
    public void DefaultSuccess_IsFalse()
    {
        // Arrange & Act
        var result = new ExportResult();

        // Assert
        result.Success.Should().BeFalse();
    }

    [Test]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var result = new ExportResult();

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().BeNull();
        result.OutputPath.Should().BeNull();
        result.TablesExported.Should().Be(0);
        result.RowsExported.Should().Be(0);
        result.Duration.Should().Be(TimeSpan.Zero);
    }

    [Test]
    public void SuccessfulResult_CanBeCreated()
    {
        // Arrange & Act
        var result = new ExportResult
        {
            Success = true,
            OutputPath = "/path/to/export.sql",
            TablesExported = 5,
            RowsExported = 10000,
            Duration = TimeSpan.FromSeconds(30)
        };

        // Assert
        result.Success.Should().BeTrue();
        result.OutputPath.Should().Be("/path/to/export.sql");
        result.TablesExported.Should().Be(5);
        result.RowsExported.Should().Be(10000);
        result.Duration.Should().Be(TimeSpan.FromSeconds(30));
        result.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void FailedResult_CanBeCreated()
    {
        // Arrange & Act
        var result = new ExportResult
        {
            Success = false,
            ErrorMessage = "Connection failed"
        };

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Connection failed");
    }

    [Test]
    public void RowsExported_AcceptsLargeValues()
    {
        // Arrange
        var largeCount = long.MaxValue;
        var result = new ExportResult { RowsExported = largeCount };

        // Assert
        result.RowsExported.Should().Be(largeCount);
    }

    [Test]
    public void Duration_AcceptsVariousTimeSpans()
    {
        // Arrange & Act
        var result1 = new ExportResult { Duration = TimeSpan.FromMilliseconds(100) };
        var result2 = new ExportResult { Duration = TimeSpan.FromHours(2) };

        // Assert
        result1.Duration.Should().Be(TimeSpan.FromMilliseconds(100));
        result2.Duration.Should().Be(TimeSpan.FromHours(2));
    }

    [Test]
    public void ErrorMessage_CanBeEmptyString()
    {
        // Arrange & Act
        var result = new ExportResult { ErrorMessage = string.Empty };

        // Assert
        result.ErrorMessage.Should().BeEmpty();
    }

    [Test]
    public void OutputPath_CanContainSpecialCharacters()
    {
        // Arrange
        var pathWithSpaces = "/path/to/my export file.sql";
        var result = new ExportResult { OutputPath = pathWithSpaces };

        // Assert
        result.OutputPath.Should().Be(pathWithSpaces);
    }
}
