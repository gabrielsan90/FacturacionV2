using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Implementación del servicio de firma digital con XAdES-BES
/// Compatible con .NET 6+ (no depende de FirmaXadesNet)
/// Cumple con los requisitos de Hacienda Costa Rica
/// </summary>
public class FirmaDigitalService : IFirmaDigitalService
{
    private readonly DataContext _context;
    private readonly ILogger<FirmaDigitalService> _logger;
    private readonly IConfiguration _configuration;

    private const string PolicyIdentifier = "https://www.hacienda.go.cr/ATV/ComprobanteElectronico/docs/esquemas/2016/v4.3/ResolucionComprobantesElectronicosDGT-R-48-2016_4.3.pdf";
    private const string PolicyHash = "V8lVVNGDCPen6VELRD1Ja8HARFk=";

    private const string XadesNamespaceUri = "http://uri.etsi.org/01903/v1.3.2#";
    private const string DsNamespaceUri = "http://www.w3.org/2000/09/xmldsig#";

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

            var xmlDoc = new XmlDocument { PreserveWhitespace = true };
            xmlDoc.LoadXml(xmlSinFirmar);

            var xmlFirmado = FirmarDocumentoXades(xmlDoc, certificado);

            _logger.LogInformation("XML firmado exitosamente con XAdES-BES");

            return xmlFirmado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al firmar XML con XAdES");
            throw new InvalidOperationException($"Error al firmar el documento: {ex.Message}", ex);
        }
    }

    private string FirmarDocumentoXades(XmlDocument xmlDoc, X509Certificate2 certificado)
    {
        using var rsaKey = certificado.GetRSAPrivateKey()
            ?? throw new InvalidOperationException("El certificado no contiene una clave privada RSA");

        // Generar IDs
        var uuid = Guid.NewGuid().ToString("N");
        var signatureId = $"Signature-{uuid}";
        var signedPropertiesId = $"SignedProperties-{uuid}";
        var keyInfoId = $"KeyInfo-{uuid}";
        var referenceId = $"Reference-{uuid}";
        var signatureValueId = $"SignatureValue-{uuid}";

        // Crear el elemento Signature manualmente
        var signatureElement = CrearElementoSignature(
            xmlDoc, certificado, rsaKey,
            signatureId, signedPropertiesId, keyInfoId, referenceId, signatureValueId);

        // Insertar la firma en el documento
        xmlDoc.DocumentElement!.AppendChild(xmlDoc.ImportNode(signatureElement, true));

        return SerializarXml(xmlDoc);
    }

    private XmlElement CrearElementoSignature(
        XmlDocument xmlDoc, X509Certificate2 certificado, RSA rsaKey,
        string signatureId, string signedPropertiesId, string keyInfoId,
        string referenceId, string signatureValueId)
    {
        var policyId = _configuration["Hacienda:PolicyIdentifier"] ?? PolicyIdentifier;
        var policyHash = _configuration["Hacienda:PolicyHash"] ?? PolicyHash;

        // Crear documento temporal para la firma
        var sigDoc = new XmlDocument();
        sigDoc.PreserveWhitespace = true;

        var nsmgr = new XmlNamespaceManager(sigDoc.NameTable);
        nsmgr.AddNamespace("ds", DsNamespaceUri);
        nsmgr.AddNamespace("xades", XadesNamespaceUri);

        // Crear elemento Signature
        var signature = sigDoc.CreateElement("ds", "Signature", DsNamespaceUri);
        signature.SetAttribute("Id", signatureId);
        signature.SetAttribute("xmlns:xades", XadesNamespaceUri);
        sigDoc.AppendChild(signature);

        // === SignedInfo ===
        var signedInfo = sigDoc.CreateElement("ds", "SignedInfo", DsNamespaceUri);
        signature.AppendChild(signedInfo);

        // CanonicalizationMethod
        var canonMethod = sigDoc.CreateElement("ds", "CanonicalizationMethod", DsNamespaceUri);
        canonMethod.SetAttribute("Algorithm", "http://www.w3.org/2001/10/xml-exc-c14n#");
        signedInfo.AppendChild(canonMethod);

        // SignatureMethod
        var sigMethod = sigDoc.CreateElement("ds", "SignatureMethod", DsNamespaceUri);
        sigMethod.SetAttribute("Algorithm", "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256");
        signedInfo.AppendChild(sigMethod);

        // Reference al documento
        var refDoc = sigDoc.CreateElement("ds", "Reference", DsNamespaceUri);
        refDoc.SetAttribute("Id", referenceId);
        refDoc.SetAttribute("URI", "");
        signedInfo.AppendChild(refDoc);

        var transforms1 = sigDoc.CreateElement("ds", "Transforms", DsNamespaceUri);
        refDoc.AppendChild(transforms1);

        var transform1 = sigDoc.CreateElement("ds", "Transform", DsNamespaceUri);
        transform1.SetAttribute("Algorithm", "http://www.w3.org/2000/09/xmldsig#enveloped-signature");
        transforms1.AppendChild(transform1);

        var transform2 = sigDoc.CreateElement("ds", "Transform", DsNamespaceUri);
        transform2.SetAttribute("Algorithm", "http://www.w3.org/2001/10/xml-exc-c14n#");
        transforms1.AppendChild(transform2);

        var digestMethod1 = sigDoc.CreateElement("ds", "DigestMethod", DsNamespaceUri);
        digestMethod1.SetAttribute("Algorithm", "http://www.w3.org/2001/04/xmlenc#sha256");
        refDoc.AppendChild(digestMethod1);

        // Calcular digest del documento
        var docDigest = CalcularDigestDocumento(xmlDoc);
        var digestValue1 = sigDoc.CreateElement("ds", "DigestValue", DsNamespaceUri);
        digestValue1.InnerText = docDigest;
        refDoc.AppendChild(digestValue1);

        // === Crear SignedProperties para calcular su digest ===
        var signedPropertiesXml = CrearSignedPropertiesXml(
            certificado, signatureId, signedPropertiesId, referenceId, policyId, policyHash);

        // Reference a SignedProperties
        var refProps = sigDoc.CreateElement("ds", "Reference", DsNamespaceUri);
        refProps.SetAttribute("Type", $"{XadesNamespaceUri}SignedProperties");
        refProps.SetAttribute("URI", $"#{signedPropertiesId}");
        signedInfo.AppendChild(refProps);

        var transforms2 = sigDoc.CreateElement("ds", "Transforms", DsNamespaceUri);
        refProps.AppendChild(transforms2);

        var transform3 = sigDoc.CreateElement("ds", "Transform", DsNamespaceUri);
        transform3.SetAttribute("Algorithm", "http://www.w3.org/2001/10/xml-exc-c14n#");
        transforms2.AppendChild(transform3);

        var digestMethod2 = sigDoc.CreateElement("ds", "DigestMethod", DsNamespaceUri);
        digestMethod2.SetAttribute("Algorithm", "http://www.w3.org/2001/04/xmlenc#sha256");
        refProps.AppendChild(digestMethod2);

        // Calcular digest de SignedProperties
        var propsDigest = CalcularDigestSignedProperties(signedPropertiesXml);
        var digestValue2 = sigDoc.CreateElement("ds", "DigestValue", DsNamespaceUri);
        digestValue2.InnerText = propsDigest;
        refProps.AppendChild(digestValue2);

        // === SignatureValue ===
        var signatureValue = sigDoc.CreateElement("ds", "SignatureValue", DsNamespaceUri);
        signatureValue.SetAttribute("Id", signatureValueId);
        signature.AppendChild(signatureValue);

        // Calcular la firma de SignedInfo
        var signedInfoCanonical = CanonicalizarElemento(signedInfo);
        var signatureBytes = rsaKey.SignData(
            Encoding.UTF8.GetBytes(signedInfoCanonical),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        signatureValue.InnerText = Convert.ToBase64String(signatureBytes);

        // === KeyInfo ===
        var keyInfo = sigDoc.CreateElement("ds", "KeyInfo", DsNamespaceUri);
        keyInfo.SetAttribute("Id", keyInfoId);
        signature.AppendChild(keyInfo);

        var x509Data = sigDoc.CreateElement("ds", "X509Data", DsNamespaceUri);
        keyInfo.AppendChild(x509Data);

        var x509Certificate = sigDoc.CreateElement("ds", "X509Certificate", DsNamespaceUri);
        x509Certificate.InnerText = Convert.ToBase64String(certificado.RawData);
        x509Data.AppendChild(x509Certificate);

        // === Object con QualifyingProperties ===
        var objectElement = sigDoc.CreateElement("ds", "Object", DsNamespaceUri);
        signature.AppendChild(objectElement);

        // Importar SignedProperties al Object
        var importedProps = sigDoc.ImportNode(signedPropertiesXml.DocumentElement!, true);
        objectElement.AppendChild(importedProps);

        return signature;
    }

    private XmlDocument CrearSignedPropertiesXml(
        X509Certificate2 certificado, string signatureId, string signedPropertiesId,
        string referenceId, string policyId, string policyHash)
    {
        var certHash = Convert.ToBase64String(SHA256.HashData(certificado.RawData));
        var certSerial = ConvertHexToDecimal(certificado.SerialNumber);
        var certIssuer = certificado.IssuerName.Name;
        var signingTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

        var doc = new XmlDocument();
        doc.PreserveWhitespace = true;

        var qualifyingProps = doc.CreateElement("xades", "QualifyingProperties", XadesNamespaceUri);
        qualifyingProps.SetAttribute("xmlns:ds", DsNamespaceUri);
        qualifyingProps.SetAttribute("Target", $"#{signatureId}");
        doc.AppendChild(qualifyingProps);

        var signedProps = doc.CreateElement("xades", "SignedProperties", XadesNamespaceUri);
        signedProps.SetAttribute("Id", signedPropertiesId);
        qualifyingProps.AppendChild(signedProps);

        // SignedSignatureProperties
        var signedSigProps = doc.CreateElement("xades", "SignedSignatureProperties", XadesNamespaceUri);
        signedProps.AppendChild(signedSigProps);

        // SigningTime
        var signingTimeEl = doc.CreateElement("xades", "SigningTime", XadesNamespaceUri);
        signingTimeEl.InnerText = signingTime;
        signedSigProps.AppendChild(signingTimeEl);

        // SigningCertificate
        var signingCert = doc.CreateElement("xades", "SigningCertificate", XadesNamespaceUri);
        signedSigProps.AppendChild(signingCert);

        var cert = doc.CreateElement("xades", "Cert", XadesNamespaceUri);
        signingCert.AppendChild(cert);

        var certDigest = doc.CreateElement("xades", "CertDigest", XadesNamespaceUri);
        cert.AppendChild(certDigest);

        var digestMethod = doc.CreateElement("ds", "DigestMethod", DsNamespaceUri);
        digestMethod.SetAttribute("Algorithm", "http://www.w3.org/2001/04/xmlenc#sha256");
        certDigest.AppendChild(digestMethod);

        var digestValue = doc.CreateElement("ds", "DigestValue", DsNamespaceUri);
        digestValue.InnerText = certHash;
        certDigest.AppendChild(digestValue);

        var issuerSerial = doc.CreateElement("xades", "IssuerSerial", XadesNamespaceUri);
        cert.AppendChild(issuerSerial);

        var x509IssuerName = doc.CreateElement("ds", "X509IssuerName", DsNamespaceUri);
        x509IssuerName.InnerText = certIssuer;
        issuerSerial.AppendChild(x509IssuerName);

        var x509SerialNumber = doc.CreateElement("ds", "X509SerialNumber", DsNamespaceUri);
        x509SerialNumber.InnerText = certSerial;
        issuerSerial.AppendChild(x509SerialNumber);

        // SignaturePolicyIdentifier
        var sigPolicyIdentifier = doc.CreateElement("xades", "SignaturePolicyIdentifier", XadesNamespaceUri);
        signedSigProps.AppendChild(sigPolicyIdentifier);

        var sigPolicyId = doc.CreateElement("xades", "SignaturePolicyId", XadesNamespaceUri);
        sigPolicyIdentifier.AppendChild(sigPolicyId);

        var sigPolicyIdInner = doc.CreateElement("xades", "SigPolicyId", XadesNamespaceUri);
        sigPolicyId.AppendChild(sigPolicyIdInner);

        var identifier = doc.CreateElement("xades", "Identifier", XadesNamespaceUri);
        identifier.InnerText = policyId;
        sigPolicyIdInner.AppendChild(identifier);

        var sigPolicyHash = doc.CreateElement("xades", "SigPolicyHash", XadesNamespaceUri);
        sigPolicyId.AppendChild(sigPolicyHash);

        var policyDigestMethod = doc.CreateElement("ds", "DigestMethod", DsNamespaceUri);
        policyDigestMethod.SetAttribute("Algorithm", "http://www.w3.org/2000/09/xmldsig#sha1");
        sigPolicyHash.AppendChild(policyDigestMethod);

        var policyDigestValue = doc.CreateElement("ds", "DigestValue", DsNamespaceUri);
        policyDigestValue.InnerText = policyHash;
        sigPolicyHash.AppendChild(policyDigestValue);

        // SignedDataObjectProperties
        var signedDataObjProps = doc.CreateElement("xades", "SignedDataObjectProperties", XadesNamespaceUri);
        signedProps.AppendChild(signedDataObjProps);

        var dataObjFormat = doc.CreateElement("xades", "DataObjectFormat", XadesNamespaceUri);
        dataObjFormat.SetAttribute("ObjectReference", $"#{referenceId}");
        signedDataObjProps.AppendChild(dataObjFormat);

        var mimeType = doc.CreateElement("xades", "MimeType", XadesNamespaceUri);
        mimeType.InnerText = "text/xml";
        dataObjFormat.AppendChild(mimeType);

        var encoding = doc.CreateElement("xades", "Encoding", XadesNamespaceUri);
        encoding.InnerText = "UTF-8";
        dataObjFormat.AppendChild(encoding);

        return doc;
    }

    private string CalcularDigestDocumento(XmlDocument xmlDoc)
    {
        // Canonicalizar el documento
        var transform = new XmlDsigExcC14NTransform();
        transform.LoadInput(xmlDoc);
        using var stream = (MemoryStream)transform.GetOutput(typeof(Stream));
        var hash = SHA256.HashData(stream.ToArray());
        return Convert.ToBase64String(hash);
    }

    private string CalcularDigestSignedProperties(XmlDocument signedPropsDoc)
    {
        // Encontrar el elemento SignedProperties
        var nsmgr = new XmlNamespaceManager(signedPropsDoc.NameTable);
        nsmgr.AddNamespace("xades", XadesNamespaceUri);
        var signedProps = signedPropsDoc.SelectSingleNode("//xades:SignedProperties", nsmgr) as XmlElement;

        if (signedProps == null)
            throw new InvalidOperationException("No se encontró el elemento SignedProperties");

        // Canonicalizar SignedProperties
        var canonicalXml = CanonicalizarElemento(signedProps);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonicalXml));
        return Convert.ToBase64String(hash);
    }

    private string CanonicalizarElemento(XmlElement element)
    {
        var doc = new XmlDocument();
        doc.PreserveWhitespace = true;
        doc.LoadXml(element.OuterXml);

        var transform = new XmlDsigExcC14NTransform();
        transform.LoadInput(doc);
        using var stream = (MemoryStream)transform.GetOutput(typeof(Stream));
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static string SerializarXml(XmlDocument xmlDoc)
    {
        using var ms = new MemoryStream();
        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = false,
            OmitXmlDeclaration = false
        };
        using (var writer = XmlWriter.Create(ms, settings))
        {
            xmlDoc.Save(writer);
        }
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static string ConvertHexToDecimal(string hex)
    {
        hex = hex.Replace(" ", "").Replace("-", "");
        if (string.IsNullOrEmpty(hex))
            return "0";

        var bytes = Convert.FromHexString(hex);
        var bigInt = new System.Numerics.BigInteger(bytes, isUnsigned: true, isBigEndian: true);
        return bigInt.ToString();
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

            var certificado = new X509Certificate2(
                empresa.CertificadoDigital,
                password,
                X509KeyStorageFlags.Exportable
            );

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

            var signatureNode = xmlDoc.GetElementsByTagName("Signature", DsNamespaceUri);

            if (signatureNode.Count == 0)
            {
                _logger.LogWarning("No se encontró la firma en el XML");
                return false;
            }

            var signedXml = new SignedXml(xmlDoc);
            signedXml.LoadXml((XmlElement)signatureNode[0]!);

            // Para verificar, necesitamos registrar el namespace xades
            return signedXml.CheckSignature();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar la firma del XML");
            return false;
        }
    }
}