using System.Security.Cryptography;
using FluentAssertions;
using IvkExportTool.Core.Security;

namespace IvkExportTool.Tests.Core.Security;

[TestFixture]
public class CredentialProtectorTests
{
    [Test]
    public void Protect_ThenUnprotect_ReturnsOriginalString()
    {
        // Arrange
        var originalText = "TestPassword123!@#";

        // Act
        var encrypted = CredentialProtector.Protect(originalText);
        var decrypted = CredentialProtector.Unprotect(encrypted);

        // Assert
        decrypted.Should().Be(originalText);
    }

    [Test]
    public void Protect_SameInput_ProducesDifferentOutput()
    {
        // Arrange
        var text = "SamePassword";

        // Act
        var encrypted1 = CredentialProtector.Protect(text);
        var encrypted2 = CredentialProtector.Protect(text);

        // Assert
        // Разные IV должны давать разный зашифрованный результат
        encrypted1.Should().NotBeEquivalentTo(encrypted2);
    }

    [Test]
    public void Unprotect_ValidData_ReturnsDecryptedString()
    {
        // Arrange
        var originalText = "ValidPassword";
        var encrypted = CredentialProtector.Protect(originalText);

        // Act
        var decrypted = CredentialProtector.Unprotect(encrypted);

        // Assert
        decrypted.Should().Be(originalText);
    }

    [Test]
    public void Unprotect_CorruptedData_ThrowsException()
    {
        // Arrange
        var corruptedData = new byte[32];
        Random.Shared.NextBytes(corruptedData);

        // Act & Assert
        var act = () => CredentialProtector.Unprotect(corruptedData);
        act.Should().Throw<CryptographicException>();
    }

    [Test]
    public void Protect_EmptyString_ReturnsValidEncryption()
    {
        // Arrange
        var emptyString = string.Empty;

        // Act
        var encrypted = CredentialProtector.Protect(emptyString);
        var decrypted = CredentialProtector.Unprotect(encrypted);

        // Assert
        encrypted.Should().NotBeEmpty();
        encrypted.Length.Should().BeGreaterThan(16); // Минимум IV (16 байт) + encrypted data
        decrypted.Should().BeEmpty();
    }

    [Test]
    public void Protect_UnicodeString_PreservesCharacters()
    {
        // Arrange
        var unicodeText = "Пароль123!@#日本語العربية";

        // Act
        var encrypted = CredentialProtector.Protect(unicodeText);
        var decrypted = CredentialProtector.Unprotect(encrypted);

        // Assert
        decrypted.Should().Be(unicodeText);
    }

    [Test]
    public void Protect_LongString_WorksCorrectly()
    {
        // Arrange
        var longText = new string('A', 10000);

        // Act
        var encrypted = CredentialProtector.Protect(longText);
        var decrypted = CredentialProtector.Unprotect(encrypted);

        // Assert
        decrypted.Should().Be(longText);
    }

    [Test]
    public void Unprotect_TooShortData_ThrowsException()
    {
        // Arrange
        // IV требует минимум 16 байт, данные меньше этого размера должны вызывать ошибку
        var tooShortData = new byte[10];

        // Act & Assert
        var act = () => CredentialProtector.Unprotect(tooShortData);
        act.Should().Throw<Exception>();
    }

    [Test]
    public void Protect_SpecialCharacters_PreservesCharacters()
    {
        // Arrange
        var specialChars = "!@#$%^&*()_+-=[]{}|;':\",./<>?`~\\\n\r\t";

        // Act
        var encrypted = CredentialProtector.Protect(specialChars);
        var decrypted = CredentialProtector.Unprotect(encrypted);

        // Assert
        decrypted.Should().Be(specialChars);
    }

    [Test]
    public void Encrypted_DataFormat_HasCorrectStructure()
    {
        // Arrange
        var text = "TestData";

        // Act
        var encrypted = CredentialProtector.Protect(text);

        // Assert
        // Зашифрованные данные должны содержать IV (16 байт) + зашифрованный текст (минимум 16 байт из-за padding)
        encrypted.Length.Should().BeGreaterThanOrEqualTo(32);
    }
}
