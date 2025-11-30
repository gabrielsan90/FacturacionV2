using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.Entities;
using FirmaXadesNet;
using FirmaXadesNet.Crypto;
using FirmaXadesNet.Signature;
using FirmaXadesNet.Signature.Parameters;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Implementación del servicio de firma digital con XAdES-BES
/// Usa la librería FirmaXadesNet para cumplir con los requisitos de Hacienda Costa Rica
/// </summary>
public class FirmaDigitalService : IFirmaDigitalService
{
    private readonly DataContext _context;
    private readonly ILogger<FirmaDigitalService> _logger;
    private readonly IConfiguration _configuration;

    // Política de firma de Hacienda Costa Rica
    private const string PolicyIdentifier = "https://www.hacienda.go.cr/ATV/ComprobanteElectronico/docs/esquemas/2016/v4.3/ResolucionComprobantesElectronicosDGT-R-48-2016_4.3.pdf";
    private const string PolicyHash = "V8lVVNGDCPen6VELRD1Ja8HARFk=";

    public FirmaDigitalService(DataContext context, ILogger<FirmaDigitalService> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Firma un XML con certificado digital usando XAdES-BES
    /// </summary>
    public async Task<string> FirmarXmlAsync(string xmlSinFirmar, X509Certificate2 certificado, string pinCertificado)
    {
        try
        {
            // Validar certificado
            if (!ValidarCertificado(certificado))
            {
                throw new InvalidOperationException("El certificado digital no es válido o está vencido");
            }

            // Cargar el XML
            var xmlDoc = new XmlDocument { PreserveWhitespace = true };
            xmlDoc.LoadXml(xmlSinFirmar);

            // Configurar el servicio XAdES
            var xadesService = new XadesService();

            // Configurar parámetros de firma según especificación Hacienda
            var parametros = new SignatureParameters
            {
                SignatureMethod = SignatureMethod.RSAwithSHA256,
                SignaturePolicyInfo = new SignaturePolicyInfo
                {
                    PolicyIdentifier = _configuration["Hacienda:PolicyIdentifier"] ?? PolicyIdentifier,
                    PolicyHash = _configuration["Hacienda:PolicyHash"] ?? PolicyHash,
                    PolicyDigestAlgorithm = FirmaXadesNet.Crypto.DigestMethod.SHA1
                },
                SignaturePackaging = SignaturePackaging.ENVELOPED,
                Signer = new Signer(certificado)
            };

            // Configurar XML sin BOM
            var settings = new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(false) // UTF-8 sin BOM
            };

            using var msSinFirma = new MemoryStream();
            using (var writer = XmlWriter.Create(msSinFirma, settings))
            {
                xmlDoc.Save(writer);
            }
            msSinFirma.Flush();
            msSinFirma.Position = 0;

            // Firmar el documento
            SignatureDocument docFirmado = xadesService.Sign(msSinFirma, parametros);

            // Guardar el documento firmado
            using var msFirmado = new MemoryStream();
            docFirmado.Save(msFirmado);

            // Convertir a string XML
            var xmlFirmado = Encoding.UTF8.GetString(msFirmado.ToArray());

            _logger.LogInformation("XML firmado exitosamente con XAdES-BES");

            return xmlFirmado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al firmar XML con XAdES");
            throw new InvalidOperationException($"Error al firmar el documento: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Obtiene el certificado digital de una empresa
    /// </summary>
    public async Task<X509Certificate2> ObtenerCertificadoAsync(Guid empresaId)
    {
        var empresa = await _context.Set<Empresa>()
            .FirstOrDefaultAsync(e => e.Id == empresaId)
            ?? throw new InvalidOperationException("No se encontró la empresa");

        if (empresa.CertificadoDigital == null || empresa.CertificadoDigital.Length == 0)
        {
            throw new InvalidOperationException("La empresa no tiene un certificado digital configurado");
        }

        try
        {
            // El certificado se almacena en formato PKCS#12 (.p12 o .pfx)
            // Requiere el PIN/contraseña para cargar la clave privada
            var password = empresa.PinCertificado ?? string.Empty;

            // IMPORTANTE: Usar estos flags para evitar el error "Keyset does not exist"
            // - MachineKeySet: Almacena la clave en el almacén de la máquina
            // - PersistKeySet: Persiste la clave para poder usarla
            // - Exportable: Permite exportar la clave privada (necesario para firmar)
            var certificado = new X509Certificate2(
                empresa.CertificadoDigital,
                password,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable
            );

            return certificado;
        }
        catch (CryptographicException ex)
        {
            _logger.LogError(ex, "Error criptográfico al cargar el certificado de la empresa {EmpresaId}", empresaId);
            throw new InvalidOperationException("Error al cargar el certificado digital. Verifique que el PIN sea correcto y que el certificado esté en formato .p12 o .pfx.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar el certificado digital de la empresa {EmpresaId}", empresaId);
            throw new InvalidOperationException("Error al cargar el certificado digital.", ex);
        }
    }

    /// <summary>
    /// Valida que un certificado sea válido y no esté vencido
    /// </summary>
    public bool ValidarCertificado(X509Certificate2 certificado)
    {
        if (certificado == null)
        {
            _logger.LogWarning("Certificado es null");
            return false;
        }

        // Verificar que tenga clave privada
        if (!certificado.HasPrivateKey)
        {
            _logger.LogWarning("El certificado no tiene clave privada");
            return false;
        }

        // Verificar fechas de validez
        var ahora = DateTime.Now;

        if (ahora < certificado.NotBefore)
        {
            _logger.LogWarning("El certificado aún no es válido. Válido desde: {NotBefore}", certificado.NotBefore);
            return false;
        }

        if (ahora > certificado.NotAfter)
        {
            _logger.LogWarning("El certificado está vencido. Válido hasta: {NotAfter}", certificado.NotAfter);
            return false;
        }

        // Verificar que el certificado esté diseñado para firma digital
        foreach (var extension in certificado.Extensions)
        {
            if (extension is X509KeyUsageExtension keyUsage)
            {
                if ((keyUsage.KeyUsages & X509KeyUsageFlags.DigitalSignature) == 0)
                {
                    _logger.LogWarning("El certificado no está habilitado para firma digital");
                    return false;
                }
            }
        }

        _logger.LogInformation("Certificado válido. Subject: {Subject}, Válido hasta: {NotAfter}",
            certificado.Subject, certificado.NotAfter);

        return true;
    }

    /// <summary>
    /// Verifica la firma de un XML firmado
    /// </summary>
    public bool VerificarFirma(string xmlFirmado)
    {
        try
        {
            var xmlDoc = new XmlDocument { PreserveWhitespace = true };
            xmlDoc.LoadXml(xmlFirmado);

            // Buscar el nodo de firma
            var signatureNode = xmlDoc.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl);

            if (signatureNode.Count == 0)
            {
                _logger.LogWarning("No se encontró la firma en el XML");
                return false;
            }

            var signedXml = new SignedXml(xmlDoc);
            signedXml.LoadXml((XmlElement)signatureNode[0]!);

            // Verificar la firma
            return signedXml.CheckSignature();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar la firma del XML");
            return false;
        }
    }
}
