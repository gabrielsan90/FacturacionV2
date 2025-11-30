using Facturacion.Backend.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Implementación del servicio de encriptación AES-256
/// NUNCA almacenar PINs o claves en texto plano
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public EncryptionService(IConfiguration configuration)
    {
        // Obtener la clave maestra de la configuración
        // En producción, usar Azure Key Vault o AWS Secrets Manager
        var masterKey = configuration["Encryption:MasterKey"]
            ?? throw new InvalidOperationException("La clave de encriptación no está configurada. Configure 'Encryption:MasterKey' en appsettings.json");

        // Generar key de 256 bits (32 bytes) desde la clave maestra
        using var sha256 = SHA256.Create();
        _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(masterKey));

        // IV de 128 bits (16 bytes) - También derivado de forma segura
        var ivSource = configuration["Encryption:IV"] ?? "FacturacionCR44";
        using var md5 = MD5.Create();
        _iv = md5.ComputeHash(Encoding.UTF8.GetBytes(ivSource));
    }

    /// <summary>
    /// Encripta un texto plano usando AES-256-CBC
    /// </summary>
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return Convert.ToBase64String(encryptedBytes);
    }

    /// <summary>
    /// Desencripta un texto usando AES-256-CBC
    /// </summary>
    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var cipherBytes = Convert.FromBase64String(cipherText);
            var decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (FormatException)
        {
            // Si no está encriptado (texto plano), retornar como está
            // Esto es para compatibilidad con datos existentes sin encriptar
            return cipherText;
        }
        catch (CryptographicException)
        {
            // Si hay error de desencriptación, puede ser texto plano
            return cipherText;
        }
    }

    /// <summary>
    /// Encripta un arreglo de bytes
    /// </summary>
    public byte[] EncryptBytes(byte[] data)
    {
        if (data == null || data.Length == 0)
            return data;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(data, 0, data.Length);
    }

    /// <summary>
    /// Desencripta un arreglo de bytes
    /// </summary>
    public byte[] DecryptBytes(byte[] encryptedData)
    {
        if (encryptedData == null || encryptedData.Length == 0)
            return encryptedData;

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
        }
        catch (CryptographicException)
        {
            // Si no está encriptado, retornar como está
            return encryptedData;
        }
    }

    /// <summary>
    /// Genera un hash SHA-256 de un texto
    /// </summary>
    public string ComputeHash(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>
    /// Verifica si un texto coincide con un hash
    /// </summary>
    public bool VerifyHash(string text, string hash)
    {
        var computedHash = ComputeHash(text);
        return string.Equals(computedHash, hash, StringComparison.OrdinalIgnoreCase);
    }
}
