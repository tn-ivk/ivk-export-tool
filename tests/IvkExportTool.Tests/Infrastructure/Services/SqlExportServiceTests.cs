using FluentAssertions;
using IvkExportTool.Core.Models;
using IvkExportTool.Infrastructure.Services;

namespace IvkExportTool.Tests.Infrastructure.Services;

/// <summary>
/// Тесты для SqlExportService
/// </summary>
[TestFixture]
public class SqlExportServiceTests
{
    private SqlExportService _service = null!;
    private string _testOutputPath = null!;

    [SetUp]
    public void Setup()
    {
        _service = new SqlExportService();
        _testOutputPath = Path.Combine(Path.GetTempPath(), $"test_export_{Guid.NewGuid()}.sql");
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_testOutputPath))
        {
            File.Delete(_testOutputPath);
        }
    }

    #region BuildEscapedString Tests

    [Test]
    public void BuildEscapedString_ShouldEscapeSpecialCharacters()
    {
        // Arrange
        var testString = "Test\nwith\ttabs\rand\\backslash'quotes\"and\0null";

        // Act
        var result = _service.BuildEscapedString(testString);

        // Assert
        result.Should().Be("'Test\\nwith\\ttabs\\rand\\\\backslash\\'quotes\\\"and\\0null'");
    }

    [Test]
    public void BuildEscapedString_ShouldHandleEmptyString()
    {
        // Arrange
        var testString = "";

        // Act
        var result = _service.BuildEscapedString(testString);

        // Assert
        result.Should().Be("''");
    }

    [Test]
    public void BuildEscapedString_ShouldHandleNormalString()
    {
        // Arrange
        var testString = "Hello World";

        // Act
        var result = _service.BuildEscapedString(testString);

        // Assert
        result.Should().Be("'Hello World'");
    }

    [Test]
    public void BuildEscapedString_ShouldHandleUnicodeString()
    {
        // Arrange
        var testString = "Привет мир 你好世界 🌍";

        // Act
        var result = _service.BuildEscapedString(testString);

        // Assert
        result.Should().Be("'Привет мир 你好世界 🌍'");
    }

    #endregion

    #region BuildHexString Tests

    [Test]
    public void BuildHexString_ShouldConvertBytesToHex()
    {
        // Arrange
        var bytes = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF };

        // Act
        var result = _service.BuildHexString(bytes);

        // Assert
        result.Should().Be("0x0123456789ABCDEF");
    }

    [Test]
    public void BuildHexString_ShouldHandleEmptyArray()
    {
        // Arrange
        var bytes = Array.Empty<byte>();

        // Act
        var result = _service.BuildHexString(bytes);

        // Assert
        result.Should().Be("X''");
    }

    [Test]
    public void BuildHexString_ShouldHandleSingleByte()
    {
        // Arrange
        var bytes = new byte[] { 0xFF };

        // Act
        var result = _service.BuildHexString(bytes);

        // Assert
        result.Should().Be("0xFF");
    }

    #endregion

    #region ConvertToSqlValue Tests

    [Test]
    public void ConvertToSqlValue_ShouldHandleString()
    {
        // Arrange
        var testString = "Hello World";

        // Act
        var result = _service.ConvertToSqlValue(testString);

        // Assert
        result.Should().Be("'Hello World'");
    }

    [Test]
    public void ConvertToSqlValue_ShouldHandleDateTime()
    {
        // Arrange
        var testDate = new DateTime(2025, 11, 10, 15, 30, 45);

        // Act
        var result = _service.ConvertToSqlValue(testDate);

        // Assert
        result.Should().Be("'2025-11-10 15:30:45'");
    }

    [Test]
    public void ConvertToSqlValue_ShouldHandleBooleanTrue()
    {
        // Act
        var result = _service.ConvertToSqlValue(true);

        // Assert
        result.Should().Be("1");
    }

    [Test]
    public void ConvertToSqlValue_ShouldHandleBooleanFalse()
    {
        // Act
        var result = _service.ConvertToSqlValue(false);

        // Assert
        result.Should().Be("0");
    }

    [Test]
    public void ConvertToSqlValue_ShouldHandleNull()
    {
        // Act
        var result = _service.ConvertToSqlValue(null!);

        // Assert
        result.Should().Be("NULL");
    }

    [Test]
    public void ConvertToSqlValue_ShouldHandleByteArray()
    {
        // Arrange
        var bytes = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };

        // Act
        var result = _service.ConvertToSqlValue(bytes);

        // Assert
        result.Should().Be("0xDEADBEEF");
    }

    [Test]
    public void ConvertToSqlValue_ShouldHandleInteger()
    {
        // Act
        var result = _service.ConvertToSqlValue(42);

        // Assert
        result.Should().Be("42");
    }

    [Test]
    public void ConvertToSqlValue_ShouldHandleDouble()
    {
        // Act
        var result = _service.ConvertToSqlValue(3.14);

        // Assert
        result.Should().Contain("3.14");
    }

    [Test]
    public void ConvertToSqlValue_Decimal_UsesInvariantCulture()
    {
        // Arrange
        var decimalValue = 1234.56m;

        // Act
        var result = _service.ConvertToSqlValue(decimalValue);

        // Assert - должна быть точка, а не запятая
        result.Should().Be("1234.56");
        result.Should().NotContain(",");
    }

    [Test]
    public void ConvertToSqlValue_Guid_ReturnsQuotedString()
    {
        // Arrange
        var guid = new Guid("12345678-1234-1234-1234-123456789012");

        // Act
        var result = _service.ConvertToSqlValue(guid);

        // Assert
        result.Should().Be("12345678-1234-1234-1234-123456789012");
    }

    [Test]
    public void ConvertToSqlValue_Long_ReturnsCorrectValue()
    {
        // Arrange
        var longValue = 9223372036854775807L;

        // Act
        var result = _service.ConvertToSqlValue(longValue);

        // Assert
        result.Should().Be("9223372036854775807");
    }

    [Test]
    public void ConvertToSqlValue_Float_UsesInvariantCulture()
    {
        // Arrange
        var floatValue = 123.456f;

        // Act
        var result = _service.ConvertToSqlValue(floatValue);

        // Assert - должна быть точка
        result.Should().Contain("123.456");
        result.Should().NotContain(",");
    }

    #endregion

    #region CalculatePercent Tests

    [Test]
    public void CalculatePercent_ZeroTables_ReturnsZero()
    {
        // Arrange
        var progress = new ExportProgress
        {
            TotalTables = 0,
            CurrentTableIndex = 0
        };

        // Act
        var result = _service.CalculatePercent(progress);

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public void CalculatePercent_HalfComplete_Returns50()
    {
        // Arrange
        var progress = new ExportProgress
        {
            TotalTables = 4,
            CurrentTableIndex = 2,
            TotalRows = 100,
            RowsProcessed = 0
        };

        // Act
        var result = _service.CalculatePercent(progress);

        // Assert
        result.Should().Be(50);
    }

    [Test]
    public void CalculatePercent_AllComplete_Returns100()
    {
        // Arrange - когда все таблицы завершены, CurrentTableIndex = TotalTables
        // и текущей таблицы уже нет (RowsProcessed/TotalRows не влияет)
        var progress = new ExportProgress
        {
            TotalTables = 4,
            CurrentTableIndex = 4,
            TotalRows = null, // нет текущей таблицы
            RowsProcessed = 0
        };

        // Act
        var result = _service.CalculatePercent(progress);

        // Assert
        result.Should().Be(100);
    }

    [Test]
    public void CalculatePercent_PartialProgress_ReturnsCorrectValue()
    {
        // Arrange - 1 таблица завершена, вторая на 50%
        var progress = new ExportProgress
        {
            TotalTables = 4,
            CurrentTableIndex = 1,
            TotalRows = 100,
            RowsProcessed = 50
        };

        // Act
        var result = _service.CalculatePercent(progress);

        // Assert - (1 + 0.5) / 4 * 100 = 37.5 → 37
        result.Should().Be(37);
    }

    [Test]
    public void CalculatePercent_NoTotalRows_UsesTableIndexOnly()
    {
        // Arrange
        var progress = new ExportProgress
        {
            TotalTables = 4,
            CurrentTableIndex = 2,
            TotalRows = null,
            RowsProcessed = 0
        };

        // Act
        var result = _service.CalculatePercent(progress);

        // Assert - 2 / 4 * 100 = 50
        result.Should().Be(50);
    }

    [Test]
    public void CalculatePercent_ZeroTotalRows_UsesTableIndexOnly()
    {
        // Arrange
        var progress = new ExportProgress
        {
            TotalTables = 4,
            CurrentTableIndex = 3,
            TotalRows = 0,
            RowsProcessed = 0
        };

        // Act
        var result = _service.CalculatePercent(progress);

        // Assert - 3 / 4 * 100 = 75
        result.Should().Be(75);
    }

    #endregion
}
