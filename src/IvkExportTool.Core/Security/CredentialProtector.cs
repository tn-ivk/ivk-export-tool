using System.Security.Cryptography;
using System.Text;

namespace IvkExportTool.Core.Security;

/// <summary>
/// Защита учётных данных с использованием AES-256
/// </summary>
internal static class CredentialProtector
{
    // Части ключа, рассеянные для усложнения анализа
    private static readonly byte[] KeyPart1 = { 0x49, 0x76, 0x6B, 0x45 }; // "IvkE"
    private static readonly byte[] KeyPart2 = { 0x78, 0x70, 0x6F, 0x72 }; // "xpor"
    private static readonly byte[] KeyPart3 = { 0x74, 0x54, 0x6F, 0x6F }; // "tToo"
    private static readonly byte[] KeyPart4 = { 0x6C, 0x5F, 0x32, 0x30 }; // "l_20"
    private static readonly byte[] KeyPart5 = { 0x32, 0x35, 0x5F, 0x4B }; // "25_K"
    private static readonly byte[] KeyPart6 = { 0x65, 0x79, 0x21, 0x40 }; // "ey!@"
    private static readonly byte[] KeyPart7 = { 0x23, 0x24, 0x25, 0x5E }; // "#$%^"
    private static readonly byte[] KeyPart8 = { 0x26, 0x2A, 0x28, 0x29 }; // "&*()"

    /// <summary>
    /// Расшифровывает данные
    /// </summary>
    /// <param name="encryptedData">Зашифрованные данные (IV + ciphertext)</param>
    /// <returns>Расшифрованная строка</returns>
    internal static string Unprotect(byte[] encryptedData)
    {
        var key = AssembleKey();

        try
        {
            // IV - первые 16 байт
            var iv = new byte[16];
            Array.Copy(encryptedData, 0, iv, 0, 16);

            // Зашифрованный текст - остальное
            var ciphertext = new byte[encryptedData.Length - 16];
            Array.Copy(encryptedData, 16, ciphertext, 0, ciphertext.Length);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
        finally
        {
            // Очистка ключа из памяти
            Array.Clear(key, 0, key.Length);
        }
    }

    /// <summary>
    /// Шифрует данные (используется для генерации зашифрованных констант)
    /// </summary>
    internal static byte[] Protect(string plainText)
    {
        var key = AssembleKey();

        try
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var ciphertext = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // IV + ciphertext
            var result = new byte[aes.IV.Length + ciphertext.Length];
            Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
            Array.Copy(ciphertext, 0, result, aes.IV.Length, ciphertext.Length);

            return result;
        }
        finally
        {
            Array.Clear(key, 0, key.Length);
        }
    }

    /// <summary>
    /// Собирает ключ из рассеянных частей
    /// </summary>
    private static byte[] AssembleKey()
    {
        var key = new byte[32]; // AES-256

        // Собираем ключ из частей
        Array.Copy(KeyPart1, 0, key, 0, 4);
        Array.Copy(KeyPart2, 0, key, 4, 4);
        Array.Copy(KeyPart3, 0, key, 8, 4);
        Array.Copy(KeyPart4, 0, key, 12, 4);
        Array.Copy(KeyPart5, 0, key, 16, 4);
        Array.Copy(KeyPart6, 0, key, 20, 4);
        Array.Copy(KeyPart7, 0, key, 24, 4);
        Array.Copy(KeyPart8, 0, key, 28, 4);

        // Дополнительное преобразование для усложнения
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(key);
    }
}
