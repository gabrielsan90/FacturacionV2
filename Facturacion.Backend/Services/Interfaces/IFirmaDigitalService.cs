using System.Security.Cryptography.X509Certificates;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para firma digital de documentos XML con XAdES-BES
/// </summary>
public interface IFirmaDigitalService
{
    /// <summary>
    /// Firma un XML con un certificado digital usando XAdES-BES
    /// </summary>
    /// <param name="xmlSinFirmar">XML del documento sin firmar</param>
    /// <param name="certificado">Certificado digital X.509</param>
    /// <returns>XML firmado digitalmente</returns>
    Task<string> FirmarXmlAsync(string xmlSinFirmar, X509Certificate2 certificado, string pinCertificado);

    /// <summary>
    /// Obtiene el certificado digital de una empresa
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>Certificado digital X.509</returns>
    Task<X509Certificate2> ObtenerCertificadoAsync(Guid empresaId);

    /// <summary>
    /// Valida que un certificado sea válido y no esté vencido
    /// </summary>
    /// <param name="certificado">Certificado a validar</param>
    /// <returns>True si el certificado es válido</returns>
    bool ValidarCertificado(X509Certificate2 certificado);

    /// <summary>
    /// Verifica la firma de un XML firmado
    /// </summary>
    /// <param name="xmlFirmado">XML con firma digital</param>
    /// <returns>True si la firma es válida</returns>
    bool VerificarFirma(string xmlFirmado);
}
