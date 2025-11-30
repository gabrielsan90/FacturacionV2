namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio de encriptación para datos sensibles
/// Recomendación 14 de Hacienda v4.4: Seguridad del certificado digital
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encripta un texto plano usando AES-256
    /// </summary>
    /// <param name="plainText">Texto a encriptar</param>
    /// <returns>Texto encriptado en Base64</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Desencripta un texto usando AES-256
    /// </summary>
    /// <param name="cipherText">Texto encriptado en Base64</param>
    /// <returns>Texto plano desencriptado</returns>
    string Decrypt(string cipherText);

    /// <summary>
    /// Encripta un arreglo de bytes
    /// </summary>
    /// <param name="data">Datos a encriptar</param>
    /// <returns>Datos encriptados</returns>
    byte[] EncryptBytes(byte[] data);

    /// <summary>
    /// Desencripta un arreglo de bytes
    /// </summary>
    /// <param name="encryptedData">Datos encriptados</param>
    /// <returns>Datos desencriptados</returns>
    byte[] DecryptBytes(byte[] encryptedData);

    /// <summary>
    /// Genera un hash SHA-256 de un texto
    /// </summary>
    /// <param name="text">Texto a hashear</param>
    /// <returns>Hash en hexadecimal</returns>
    string ComputeHash(string text);

    /// <summary>
    /// Verifica si un texto coincide con un hash
    /// </summary>
    /// <param name="text">Texto a verificar</param>
    /// <param name="hash">Hash esperado</param>
    /// <returns>True si coinciden</returns>
    bool VerifyHash(string text, string hash);
}
