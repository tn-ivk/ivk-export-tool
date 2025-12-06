using FluentAssertions;
using IvkExportTool.Core.Models;

namespace IvkExportTool.Tests.Core.Models;

[TestFixture]
public class TableInfoTests
{
    [Test]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var tableInfo = new TableInfo();

        // Assert
        tableInfo.Name.Should().BeEmpty();
        tableInfo.RowCount.Should().Be(0);
        tableInfo.Engine.Should().BeEmpty();
        tableInfo.SizeBytes.Should().Be(0);
        tableInfo.Comment.Should().BeNull();
        tableInfo.IsSelected.Should().BeFalse();
    }

    [Test]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var tableInfo = new TableInfo
        {
            Name = "users",
            RowCount = 1000,
            Engine = "InnoDB",
            SizeBytes = 1024 * 1024,
            Comment = "User accounts table",
            IsSelected = true
        };

        // Assert
        tableInfo.Name.Should().Be("users");
        tableInfo.RowCount.Should().Be(1000);
        tableInfo.Engine.Should().Be("InnoDB");
        tableInfo.SizeBytes.Should().Be(1024 * 1024);
        tableInfo.Comment.Should().Be("User accounts table");
        tableInfo.IsSelected.Should().BeTrue();
    }

    [Test]
    public void IsSelected_DefaultIsFalse()
    {
        // Arrange & Act
        var tableInfo = new TableInfo { Name = "test" };

        // Assert
        tableInfo.IsSelected.Should().BeFalse();
    }

    [Test]
    public void Comment_CanBeNull()
    {
        // Arrange
        var tableInfo = new TableInfo
        {
            Name = "test",
            Comment = null
        };

        // Assert
        tableInfo.Comment.Should().BeNull();
    }

    [Test]
    public void SizeBytes_AcceptsLargeValues()
    {
        // Arrange
        var largeSize = long.MaxValue;
        var tableInfo = new TableInfo { SizeBytes = largeSize };

        // Assert
        tableInfo.SizeBytes.Should().Be(largeSize);
    }

    [Test]
    public void RowCount_AcceptsLargeValues()
    {
        // Arrange
        var largeCount = long.MaxValue;
        var tableInfo = new TableInfo { RowCount = largeCount };

        // Assert
        tableInfo.RowCount.Should().Be(largeCount);
    }
}
