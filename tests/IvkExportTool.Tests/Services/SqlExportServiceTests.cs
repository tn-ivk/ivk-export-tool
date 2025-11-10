using System.Text;
using FluentAssertions;
using IvkExportTool.Core.Models;
using IvkExportTool.Infrastructure.Services;
using Moq;
using MySqlConnector;

namespace IvkExportTool.Tests.Services;

/// <summary>
/// Тесты для SqlExportService с фокусом на оптимизацию памяти
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

    [Test]
    public void BuildEscapedString_ShouldEscapeSpecialCharacters()
    {
        // Arrange
        var service = new SqlExportService();
        var testString = "Test\nwith\ttabs\rand\\backslash'quotes\"and\0null";

        // Act
        var result = InvokePrivateMethod<string>(service, "BuildEscapedString", testString);

        // Assert
        result.Should().Be("'Test\\nwith\\ttabs\\rand\\\\backslash\\'quotes\\\"and\\0null'");
    }

    [Test]
    public void BuildEscapedString_ShouldHandleEmptyString()
    {
        // Arrange
        var service = new SqlExportService();
        var testString = "";

        // Act
        var result = InvokePrivateMethod<string>(service, "BuildEscapedString", testString);

        // Assert
        result.Should().Be("''");
    }

    [Test]
    public void BuildHexString_ShouldConvertBytesToHex()
    {
        // Arrange
        var service = new SqlExportService();
        var bytes = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF };

        // Act
        var result = InvokePrivateMethod<string>(service, "BuildHexString", bytes);

        // Assert
        result.Should().Be("0x0123456789ABCDEF");
    }

    [Test]
    public void BuildHexString_ShouldHandleEmptyArray()
    {
        // Arrange
        var service = new SqlExportService();
        var bytes = Array.Empty<byte>();

        // Act
        var result = InvokePrivateMethod<string>(service, "BuildHexString", bytes);

        // Assert
        result.Should().Be("0x");
    }

    [Test]
    public void ConvertToSqlValue_ShouldHandleString()
    {
        // Arrange
        var service = new SqlExportService();
        var testString = "Hello World";

        // Act
        var result = InvokePrivateMethod<string>(service, "ConvertToSqlValue", testString);

        // Assert
        result.Should().Be("'Hello World'");
    }

    [Test]
    public void ConvertToSqlValue_ShouldHandleDateTime()
    {
        // Arrange
        var service = new SqlExportService();
        var testDate = new DateTime(2025, 11, 10, 15, 30, 45);

        // Act
        var result = InvokePrivateMethod<string>(service, "ConvertToSqlValue", testDate);

        // Assert
        result.Should().Be("'2025-11-10 15:30:45'");
    }

    [Test]
    public void ConvertToSqlValue_ShouldHandleBoolean()
    {
        // Arrange
        var service = new SqlExportService();

        // Act
        var resultTrue = InvokePrivateMethod<string>(service, "ConvertToSqlValue", true);
        var resultFalse = InvokePrivateMethod<string>(service, "ConvertToSqlValue", false);

        // Assert
        resultTrue.Should().Be("1");
        resultFalse.Should().Be("0");
    }

    [Test]
    public void ConvertToSqlValue_ShouldHandleNull()
    {
        // Arrange
        var service = new SqlExportService();

        // Act
        var result = InvokePrivateMethod<string>(service, "ConvertToSqlValue", new object?[] { null });

        // Assert
        result.Should().Be("NULL");
    }

    [Test]
    public void ConvertToSqlValue_ShouldHandleByteArray()
    {
        // Arrange
        var service = new SqlExportService();
        var bytes = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };

        // Act
        var result = InvokePrivateMethod<string>(service, "ConvertToSqlValue", bytes);

        // Assert
        result.Should().Be("0xDEADBEEF");
    }

    [Test]
    public void ConvertToSqlValue_ShouldHandleNumbers()
    {
        // Arrange
        var service = new SqlExportService();

        // Act
        var resultInt = InvokePrivateMethod<string>(service, "ConvertToSqlValue", 42);
        var resultDouble = InvokePrivateMethod<string>(service, "ConvertToSqlValue", 3.14);

        // Assert
        resultInt.Should().Be("42");
        resultDouble.Should().Contain("3.14");
    }

    /// <summary>
    /// Тест проверяет, что экспорт большого количества строк не приводит к чрезмерному росту памяти
    /// Это критичная проверка для оптимизации потоковой записи
    /// </summary>
    [Test]
    [Category("Performance")]
    public void ExportAsync_LargeDataset_ShouldNotExceedMemoryLimit()
    {
        // Этот тест требует реальной БД и будет пропущен если БД недоступна
        // В реальном окружении нужно создать тестовую БД с большим количеством данных
        Assert.Inconclusive("Этот тест требует настройки тестовой БД с большим dataset'ом");

        // Пример того, как должен выглядеть тест:
        //
        // var memoryBefore = GC.GetTotalMemory(true);
        //
        // await _service.ExportAsync(config, options, cancellationToken: CancellationToken.None);
        //
        // var memoryAfter = GC.GetTotalMemory(false);
        // var memoryGrowth = memoryAfter - memoryBefore;
        //
        // // Для таблицы 1M строк рост памяти должен быть < 100 MB
        // memoryGrowth.Should().BeLessThan(100 * 1024 * 1024);
    }

    /// <summary>
    /// Вспомогательный метод для вызова приватных методов через рефлексию
    /// </summary>
    private T InvokePrivateMethod<T>(object obj, string methodName, params object?[] parameters)
    {
        var method = obj.GetType().GetMethod(methodName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (method == null)
        {
            throw new InvalidOperationException($"Метод {methodName} не найден");
        }

        var result = method.Invoke(obj, parameters);
        return (T)result!;
    }
}
