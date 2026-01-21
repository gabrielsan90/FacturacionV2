using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Implementación del servicio de firma digital con XAdES-BES
/// Basado en código de Francisco de la Peña (https://fran.cr/)
/// Compatible con Hacienda Costa Rica v4.4
/// </summary>
public class FirmaDigitalService : IFirmaDigitalService
{
    private readonly DataContext _context;
    private readonly ILogger<FirmaDigitalService> _logger;
    private readonly IConfiguration _configuration;

    // PolicyId y PolicyDigest para Hacienda Costa Rica v4.4
    private const string PolicyId = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/Resoluci%C3%B3n_General_sobre_disposiciones_t%C3%A9cnicas_comprobantes_electr%C3%B3nicos_para_efectos_tributarios.pdf";
    private const string PolicyDigest = "DWxin1xWOeI8OuWQXazh4VjLWAaCLAA954em7DMh0h8=";

    public FirmaDigitalService(DataContext context, ILogger<FirmaDigitalService> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<string> FirmarXmlAsync(string xmlSinFirmar, X509Certificate2 certificado, string pinCertificado)
    {
        try
        {
            if (!ValidarCertificado(certificado))
            {
                throw new InvalidOperationException("El certificado digital no es válido o está vencido");
            }

            var xmlFirmado = FirmarDocumentoXades(xmlSinFirmar, certificado);

            _logger.LogInformation("XML firmado exitosamente con XAdES-BES");

            return await Task.FromResult(xmlFirmado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al firmar XML con XAdES");
            throw new InvalidOperationException($"Error al firmar el documento: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Firma el documento XML usando XAdES-BES según especificación de Hacienda CR v4.4
    /// Código adaptado de Francisco de la Peña (https://fran.cr/)
    /// IMPORTANTE: SignedInfo debe canonicalizarse antes de firmarse (exc-c14n)
    /// </summary>
    private string FirmarDocumentoXades(string xmlSinFirmar, X509Certificate2 certificate)
    {
        // 1. Load and prepare XML
        var xml = new XmlDocument { PreserveWhitespace = true };
        xml.LoadXml(xmlSinFirmar);

        // 2. Canonicalize document using exc-c14n
        var canonicalXml = CanonicalizarXml(xml);

        // 3. Calculate document digest
        var documentDigest = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalXml)));

        // 4. Signing time in UTC
        var signingTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

        // 5. Certificate data
        var certDigest = Convert.ToBase64String(SHA256.HashData(certificate.GetRawCertData()));
        var issuer = certificate.Issuer;
        var serial = long.Parse(certificate.SerialNumber, NumberStyles.HexNumber);

        // 6. Create SignedProperties XML (with namespace for hashing)
        var signedPropertiesXml = $@"<xades:SignedProperties xmlns:xades=""http://uri.etsi.org/01903/v1.3.2#"" xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"" Id=""p1""><xades:SignedSignatureProperties><xades:SigningTime>{signingTime}</xades:SigningTime><xades:SigningCertificate><xades:Cert><xades:CertDigest><ds:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""></ds:DigestMethod><ds:DigestValue>{certDigest}</ds:DigestValue></xades:CertDigest><xades:IssuerSerial><ds:X509IssuerName>{issuer}</ds:X509IssuerName><ds:X509SerialNumber>{serial}</ds:X509SerialNumber></xades:IssuerSerial></xades:Cert></xades:SigningCertificate><xades:SignaturePolicyIdentifier><xades:SignaturePolicyId><xades:SigPolicyId><xades:Identifier>{PolicyId}</xades:Identifier></xades:SigPolicyId><xades:SigPolicyHash><ds:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""></ds:DigestMethod><ds:DigestValue>{PolicyDigest}</ds:DigestValue></xades:SigPolicyHash></xades:SignaturePolicyId></xades:SignaturePolicyIdentifier></xades:SignedSignatureProperties><xades:SignedDataObjectProperties><xades:DataObjectFormat ObjectReference=""#r1""><xades:MimeType>text/xml</xades:MimeType></xades:DataObjectFormat></xades:SignedDataObjectProperties></xades:SignedProperties>";

        // 7. Canonicalize and hash SignedProperties
        var canonicalSignedProperties = CanonicalizarXmlString(signedPropertiesXml);
        var propertiesDigest = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalSignedProperties)));

        // 8. Create SignedInfo XML
        var signedInfoXml = $@"<ds:SignedInfo xmlns:ds=""http://www.w3.org/2000/09/xmldsig#""><ds:CanonicalizationMethod Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#""></ds:CanonicalizationMethod><ds:SignatureMethod Algorithm=""http://www.w3.org/2001/04/xmldsig-more#rsa-sha256""></ds:SignatureMethod><ds:Reference Id=""r1"" URI=""""><ds:Transforms><ds:Transform Algorithm=""http://www.w3.org/2002/06/xmldsig-filter2""><dsig-filter2:XPath xmlns:dsig-filter2=""http://www.w3.org/2002/06/xmldsig-filter2"" Filter=""subtract"">/descendant::ds:Signature</dsig-filter2:XPath></ds:Transform><ds:Transform Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#""></ds:Transform></ds:Transforms><ds:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""></ds:DigestMethod><ds:DigestValue>{documentDigest}</ds:DigestValue></ds:Reference><ds:Reference Type=""http://uri.etsi.org/01903#SignedProperties"" URI=""#p1""><ds:Transforms><ds:Transform Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#""></ds:Transform></ds:Transforms><ds:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""></ds:DigestMethod><ds:DigestValue>{propertiesDigest}</ds:DigestValue></ds:Reference></ds:SignedInfo>";

        // 9. Canonicalize SignedInfo before signing (CRITICAL!)
        var canonicalSignedInfo = CanonicalizarXmlString(signedInfoXml);

        // 10. Sign the canonical SignedInfo
        var signatureValue = certificate.GetRSAPrivateKey()?.SignData(
            Encoding.UTF8.GetBytes(canonicalSignedInfo),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        if (signatureValue == null)
        {
            throw new InvalidOperationException("Error al firmar: el certificado no tiene clave privada RSA");
        }

        var signatureValueBase64 = Convert.ToBase64String(signatureValue);
        var certBase64 = Convert.ToBase64String(certificate.GetRawCertData());

        // 11. Build SignedInfo for final XML (without redundant xmlns since parent has it)
        var signedInfoForXml = signedInfoXml.Replace(" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\"", "");

        // 12. Build SignedProperties for final XML (without redundant xmlns)
        var signedPropertiesForXml = signedPropertiesXml
            .Replace(" xmlns:xades=\"http://uri.etsi.org/01903/v1.3.2#\"", "")
            .Replace(" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\"", "");

        // 13. Build complete signature block
        var signature = $@"<ds:Signature xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"" Id=""s1"">{signedInfoForXml}<ds:SignatureValue Id=""v1"">{signatureValueBase64}</ds:SignatureValue><ds:KeyInfo><ds:X509Data><ds:X509Certificate>{certBase64}</ds:X509Certificate></ds:X509Data></ds:KeyInfo><ds:Object><xades:QualifyingProperties xmlns:xades=""http://uri.etsi.org/01903/v1.3.2#"" Target=""#s1"">{signedPropertiesForXml}</xades:QualifyingProperties></ds:Object></ds:Signature>";

        // 14. Insertar firma de forma robusta antes del cierre del elemento raiz
        string signedDocument = InsertarFirmaEnDocumento(canonicalXml, signature, xml);

        return signedDocument;
    }

    /// <summary>
    /// Inserta la firma antes del cierre del elemento raiz de forma segura
    /// Usa XmlDocument para determinar el nombre correcto del elemento raiz
    /// en lugar de asumir que el ultimo cierre es el correcto
    /// </summary>
    private string InsertarFirmaEnDocumento(string canonicalXml, string signature, XmlDocument xmlDoc)
    {
        // Obtener el nombre del elemento raiz
        var rootName = xmlDoc.DocumentElement?.LocalName ?? "FacturaElectronica";
        var prefix = xmlDoc.DocumentElement?.Prefix ?? "";

        // Construir el tag de cierre esperado
        string closingTag;
        if (!string.IsNullOrEmpty(prefix))
        {
            // Con prefijo: </ns:FacturaElectronica>
            closingTag = $"</{prefix}:{rootName}>";
        }
        else
        {
            // Sin prefijo: </FacturaElectronica>
            closingTag = $"</{rootName}>";
        }

        // Buscar la posicion del cierre del elemento raiz
        int insertPosition = canonicalXml.LastIndexOf(closingTag);

        if (insertPosition < 0)
        {
            // Intentar sin prefijo como fallback
            closingTag = $"</{rootName}>";
            insertPosition = canonicalXml.LastIndexOf(closingTag);
        }

        if (insertPosition < 0)
        {
            // Como ultimo recurso, usar el metodo original
            _logger.LogWarning("No se encontro el cierre del elemento raiz {RootName}, usando metodo alternativo", rootName);
            insertPosition = canonicalXml.LastIndexOf("</");
        }

        if (insertPosition < 0)
        {
            throw new InvalidOperationException($"No se pudo encontrar el cierre del elemento raiz: {closingTag}");
        }

        return canonicalXml.Insert(insertPosition, signature);
    }

    /// <summary>
    /// Canonicaliza un documento XML usando exc-c14n
    /// </summary>
    private string CanonicalizarXml(XmlDocument xmlDoc)
    {
        var transform = new XmlDsigExcC14NTransform();
        transform.LoadInput(xmlDoc);
        using var stream = (Stream)transform.GetOutput(typeof(Stream));
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Canonicaliza un string XML usando exc-c14n
    /// </summary>
    private string CanonicalizarXmlString(string xml)
    {
        var doc = new XmlDocument { PreserveWhitespace = true };
        doc.LoadXml(xml);
        return CanonicalizarXml(doc);
    }

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
            var password = empresa.PinCertificado ?? string.Empty;

            // Usar el nuevo X509CertificateLoader de .NET 9
            var certificado = X509CertificateLoader.LoadPkcs12(
                empresa.CertificadoDigital,
                password,
                X509KeyStorageFlags.Exportable);

            return certificado;
        }
        catch (CryptographicException ex)
        {
            _logger.LogError(ex, "Error criptográfico al cargar el certificado de la empresa {EmpresaId}", empresaId);
            throw new InvalidOperationException(
                "Error al cargar el certificado digital. Verifique que el PIN sea correcto.", ex);
        }
    }

    public bool ValidarCertificado(X509Certificate2 certificado)
    {
        if (certificado == null)
        {
            _logger.LogWarning("Certificado es null");
            return false;
        }

        if (!certificado.HasPrivateKey)
        {
            _logger.LogWarning("El certificado no tiene clave privada");
            return false;
        }

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

    public bool VerificarFirma(string xmlFirmado)
    {
        try
        {
            var xmlDoc = new XmlDocument { PreserveWhitespace = true };
            xmlDoc.LoadXml(xmlFirmado);

            var signatureNode = xmlDoc.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#");

            if (signatureNode.Count == 0)
            {
                _logger.LogWarning("No se encontró la firma en el XML");
                return false;
            }

            var signedXml = new SignedXml(xmlDoc);
            signedXml.LoadXml((XmlElement)signatureNode[0]!);

            return signedXml.CheckSignature();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar la firma del XML");
            return false;
        }
    }
}
