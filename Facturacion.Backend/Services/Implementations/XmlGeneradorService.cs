using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Generador de XML para documentos electrónicos según Hacienda v4.4
/// Actualizado a namespaces v4.4 según Resolución MH-DGT-RES-0027-2024
/// </summary>
public class XmlGeneradorService : IXmlGeneradorService
{
    private readonly DataContext _context;

    // ========================================
    // NAMESPACES v4.4 (ACTUALIZADOS)
    // ========================================
    private const string NamespaceFactura = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronica";
    private const string NamespaceTiquete = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/tiqueteElectronico";
    private const string NamespaceNotaCredito = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/notaCreditoElectronica";
    private const string NamespaceNotaDebito = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/notaDebitoElectronica";
    private const string NamespaceFacturaExportacion = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronicaExportacion";
    private const string NamespaceFacturaCompra = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronicaCompra";
    private const string NamespaceREP = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/reciboElectronicoPago";
    private const string NamespaceMensajeReceptor = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/mensajeReceptor";

    public XmlGeneradorService(DataContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Genera el XML del documento según su tipo
    /// </summary>
    public async Task<string> GenerarXmlAsync(Documento documento)
    {
        // Cargar todas las relaciones necesarias
        await CargarRelacionesAsync(documento);

        // Generar XML según tipo de documento
        XDocument xmlDoc = documento.TipoDocumento switch
        {
            DocumentoTipo.FacturaElectronica => GenerarFacturaElectronica(documento),
            DocumentoTipo.TiqueteElectronico => GenerarTiqueteElectronico(documento),
            DocumentoTipo.NotaCreditoElectronica => GenerarNotaCredito(documento),
            DocumentoTipo.NotaDebitoElectronica => GenerarNotaDebito(documento),
            DocumentoTipo.FacturaElectronicaExportacion => GenerarFacturaExportacion(documento),
            _ => throw new NotImplementedException($"Tipo de documento {documento.TipoDocumento} no implementado")
        };

        // Convertir a string con formato
        return xmlDoc.Declaration?.ToString() + Environment.NewLine + xmlDoc.ToString();
    }

    /// <summary>
    /// Valida estructura básica del XML
    /// </summary>
    public bool ValidarXml(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            return doc.Root != null;
        }
        catch
        {
            return false;
        }
    }

    #region Cargar Relaciones

    private async Task CargarRelacionesAsync(Documento documento)
    {
        // Cargar Empresa
        if (documento.Empresa == null)
        {
            documento.Empresa = await _context.Set<Empresa>()
                .Include(e => e.Telefonos)
                .Include(e => e.Emails)
                .Include(e => e.ActividadesEconomicas)
                .FirstOrDefaultAsync(e => e.Id == documento.EmpresaId)
                ?? throw new InvalidOperationException("No se encontró la empresa");
        }

        // Cargar Sucursal
        if (documento.Sucursal == null)
        {
            documento.Sucursal = await _context.Set<Sucursal>()
                .FirstOrDefaultAsync(s => s.Id == documento.SucursalId)
                ?? throw new InvalidOperationException("No se encontró la sucursal");
        }

        // Cargar Terminal
        if (documento.Terminal == null)
        {
            documento.Terminal = await _context.Set<Terminal>()
                .FirstOrDefaultAsync(t => t.Id == documento.TerminalId)
                ?? throw new InvalidOperationException("No se encontró la terminal");
        }

        // Cargar Cliente o Proveedor
        if (documento.ClienteId.HasValue && documento.Cliente == null)
        {
            documento.Cliente = await _context.Set<Cliente>()
                .Include(c => c.Telefonos)
                .Include(c => c.Emails)
                .FirstOrDefaultAsync(c => c.Id == documento.ClienteId.Value);
        }

        if (documento.ProveedorId.HasValue && documento.Proveedor == null)
        {
            documento.Proveedor = await _context.Set<Proveedor>()
                .FirstOrDefaultAsync(p => p.Id == documento.ProveedorId.Value);
        }

        // Cargar Detalles con sus impuestos y descuentos
        if (documento.Detalles == null || !documento.Detalles.Any())
        {
            var detalles = await _context.Set<DocumentoDetalle>()
                .Include(d => d.Impuestos)
                .Include(d => d.Descuentos)
                .Include(d => d.NumerosVIN) // NUEVO v4.4 - M6: Múltiples VINs
                .Include(d => d.FormaFarmaceuticaNavigation) // NUEVO v4.4 - M7: Forma Farmacéutica
                .Include(d => d.Producto)
                .Where(d => d.DocumentoId == documento.Id)
                .OrderBy(d => d.NumeroLinea)
                .ToListAsync();

            documento.Detalles = detalles;
        }

        // Cargar Referencias
        if (documento.Referencias == null || !documento.Referencias.Any())
        {
            documento.Referencias = await _context.Set<DocumentoReferencia>()
                .Where(r => r.DocumentoId == documento.Id)
                .ToListAsync();
        }

        // Cargar Medios de Pago
        if (documento.MediosPago == null || !documento.MediosPago.Any())
        {
            documento.MediosPago = await _context.Set<DocumentoMedioPago>()
                .Where(m => m.DocumentoId == documento.Id)
                .ToListAsync();
        }

        // Cargar Otra Información
        if (documento.OtraInformacion == null || !documento.OtraInformacion.Any())
        {
            documento.OtraInformacion = await _context.Set<DocumentoOtraInformacion>()
                .Where(o => o.DocumentoId == documento.Id)
                .ToListAsync();
        }

        // Cargar Exportación si aplica
        if (documento.TipoDocumento == DocumentoTipo.FacturaElectronicaExportacion && documento.Exportacion == null)
        {
            documento.Exportacion = await _context.Set<DocumentoExportacion>()
                .FirstOrDefaultAsync(e => e.DocumentoId == documento.Id);
        }
    }

    #endregion

    #region Factura Electrónica (FE)

    private XDocument GenerarFacturaElectronica(Documento doc)
    {
        XNamespace ns = NamespaceFactura;
        XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

        var documento = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(ns + "FacturaElectronica",
                new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                GenerarClave(doc, ns),
                GenerarProveedorSistemas(doc, ns), // v4.4 - OBLIGATORIO (posición 2)
                GenerarCodigoActividadEmisor(doc, ns), // v4.4 - CodigoActividadEmisor (posición 3)
                GenerarCodigoActividadReceptor(doc, ns), // v4.4 - CodigoActividadReceptor (posición 4, opcional)
                GenerarNumeroConsecutivo(doc, ns),
                GenerarFechaEmision(doc, ns),
                GenerarEmisor(doc, ns),
                GenerarReceptor(doc, ns),
                GenerarCondicionVenta(doc, ns),
                GenerarPlazoCredito(doc, ns),
                // v4.4: MedioPago removido del nivel documento
                GenerarDetalleServicio(doc, ns),
                GenerarOtrosCargos(doc, ns), // v4.4
                GenerarResumenFactura(doc, ns),
                GenerarInformacionReferencia(doc, ns),
                GenerarOtros(doc, ns)
            )
        );

        return documento;
    }

    #endregion

    #region Tiquete Electrónico (TE)

    private XDocument GenerarTiqueteElectronico(Documento doc)
    {
        XNamespace ns = NamespaceTiquete;
        XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

        var documento = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(ns + "TiqueteElectronico",
                new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                GenerarClave(doc, ns),
                GenerarProveedorSistemas(doc, ns), // v4.4 - OBLIGATORIO (posición 2)
                GenerarCodigoActividadEmisor(doc, ns), // v4.4 - CodigoActividadEmisor (posición 3)
                // v4.4: Tiquete NO tiene CodigoActividadReceptor
                GenerarNumeroConsecutivo(doc, ns),
                GenerarFechaEmision(doc, ns),
                GenerarEmisor(doc, ns),
                GenerarReceptorTiquete(doc, ns), // v4.4: Receptor es opcional en Tiquete
                GenerarCondicionVenta(doc, ns),
                GenerarPlazoCredito(doc, ns),
                // v4.4: MedioPago removido del nivel documento
                GenerarDetalleServicio(doc, ns, esParaTiquete: true), // Tiquete NO lleva TipoTransaccion
                GenerarOtrosCargos(doc, ns), // v4.4
                GenerarResumenFactura(doc, ns),
                GenerarInformacionReferencia(doc, ns),
                GenerarOtros(doc, ns)
            )
        );

        return documento;
    }

    #endregion

    #region Nota de Crédito (NC)

    private XDocument GenerarNotaCredito(Documento doc)
    {
        XNamespace ns = NamespaceNotaCredito;
        XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

        var documento = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(ns + "NotaCreditoElectronica",
                new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                GenerarClave(doc, ns),
                GenerarProveedorSistemas(doc, ns), // v4.4 - OBLIGATORIO (posición 2)
                GenerarCodigoActividadEmisorOpcional(doc, ns), // v4.4 - CodigoActividadEmisor (OPCIONAL en NC)
                GenerarCodigoActividadReceptor(doc, ns), // v4.4 - CodigoActividadReceptor (opcional)
                GenerarNumeroConsecutivo(doc, ns),
                GenerarFechaEmision(doc, ns),
                GenerarEmisor(doc, ns),
                GenerarReceptor(doc, ns),
                GenerarCondicionVenta(doc, ns),
                GenerarPlazoCredito(doc, ns),
                // v4.4: MedioPago removido del nivel documento
                GenerarDetalleServicio(doc, ns),
                GenerarOtrosCargos(doc, ns), // v4.4
                GenerarResumenFactura(doc, ns),
                GenerarInformacionReferencia(doc, ns), // OBLIGATORIO en NC
                GenerarOtros(doc, ns)
            )
        );

        return documento;
    }

    #endregion

    #region Nota de Débito (ND)

    private XDocument GenerarNotaDebito(Documento doc)
    {
        XNamespace ns = NamespaceNotaDebito;
        XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

        var documento = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(ns + "NotaDebitoElectronica",
                new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                GenerarClave(doc, ns),
                GenerarProveedorSistemas(doc, ns), // v4.4 - OBLIGATORIO (posición 2)
                GenerarCodigoActividadEmisorOpcional(doc, ns), // v4.4 - CodigoActividadEmisor (OPCIONAL en ND)
                GenerarCodigoActividadReceptor(doc, ns), // v4.4 - CodigoActividadReceptor (opcional)
                GenerarNumeroConsecutivo(doc, ns),
                GenerarFechaEmision(doc, ns),
                GenerarEmisor(doc, ns),
                GenerarReceptor(doc, ns),
                GenerarCondicionVenta(doc, ns),
                GenerarPlazoCredito(doc, ns),
                // v4.4: MedioPago removido del nivel documento
                GenerarDetalleServicio(doc, ns),
                GenerarOtrosCargos(doc, ns), // v4.4
                GenerarResumenFactura(doc, ns),
                GenerarInformacionReferencia(doc, ns), // OBLIGATORIO en ND
                GenerarOtros(doc, ns)
            )
        );

        return documento;
    }

    #endregion

    #region Factura de Exportación (FEE)

    private XDocument GenerarFacturaExportacion(Documento doc)
    {
        XNamespace ns = NamespaceFacturaExportacion;
        XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

        var documento = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(ns + "FacturaElectronicaExportacion",
                new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                GenerarClave(doc, ns),
                GenerarProveedorSistemas(doc, ns), // v4.4 - OBLIGATORIO (posición 2)
                GenerarCodigoActividadEmisor(doc, ns), // v4.4 - CodigoActividadEmisor (posición 3, requerido)
                // v4.4: FEE NO tiene CodigoActividadReceptor
                GenerarNumeroConsecutivo(doc, ns),
                GenerarFechaEmision(doc, ns),
                GenerarEmisor(doc, ns),
                GenerarReceptor(doc, ns),
                GenerarCondicionVenta(doc, ns),
                GenerarPlazoCredito(doc, ns),
                // v4.4: MedioPago removido del nivel documento
                GenerarDetalleServicio(doc, ns),
                GenerarOtrosCargos(doc, ns), // v4.4
                GenerarResumenFactura(doc, ns),
                GenerarInformacionReferencia(doc, ns),
                GenerarOtros(doc, ns)
            )
        );

        return documento;
    }

    #endregion

    #region Elementos Comunes

    private XElement GenerarClave(Documento doc, XNamespace ns)
    {
        return new XElement(ns + "Clave", doc.Clave);
    }

    private XElement GenerarCodigoActividadEmisor(Documento doc, XNamespace ns)
    {
        return new XElement(ns + "CodigoActividadEmisor", doc.ActividadEconomica);
    }

    /// <summary>
    /// v4.4 - OPCIONAL: Genera el elemento CodigoActividadEmisor para NC/ND donde es opcional
    /// </summary>
    private XElement? GenerarCodigoActividadEmisorOpcional(Documento doc, XNamespace ns)
    {
        // En NC/ND el código de actividad es opcional
        if (string.IsNullOrWhiteSpace(doc.ActividadEconomica))
            return null;

        return new XElement(ns + "CodigoActividadEmisor", doc.ActividadEconomica);
    }

    /// <summary>
    /// v4.4 - OPCIONAL: Genera el elemento CodigoActividadReceptor
    /// Solo para FE, NC, ND cuando el receptor necesita indicar actividad para crédito/gasto deducible
    /// </summary>
    private XElement? GenerarCodigoActividadReceptor(Documento doc, XNamespace ns)
    {
        // Solo incluir si el documento tiene actividad económica del receptor
        if (string.IsNullOrWhiteSpace(doc.ReceptorActividadEconomica))
            return null;

        // Validar que sea exactamente 6 caracteres
        if (doc.ReceptorActividadEconomica.Length != 6)
            return null;

        return new XElement(ns + "CodigoActividadReceptor", doc.ReceptorActividadEconomica);
    }

    /// <summary>
    /// v4.4 - OPCIONAL: Genera los elementos OtrosCargos (puede haber múltiples)
    /// Para cargos adicionales como flete, seguros, timbres, etc.
    /// </summary>
    private XElement? GenerarOtrosCargos(Documento doc, XNamespace ns)
    {
        // Si hay OtrosCargos individuales en la colección, usarlos
        if (doc.OtrosCargos != null && doc.OtrosCargos.Any(c => !c.IsDeleted && c.Monto > 0))
        {
            var cargosActivos = doc.OtrosCargos.Where(c => !c.IsDeleted && c.Monto > 0).ToList();

            // Retornar el primer cargo (Hacienda acepta múltiples pero como elementos separados)
            // Para múltiples cargos, se deben agregar como hermanos en el XML
            var primerCargo = cargosActivos.First();
            var tipoDoc = primerCargo.TipoDocumento ?? "99";

            var elementos = new List<object>
            {
                new XElement(ns + "TipoDocumentoOC", tipoDoc)
            };

            // TipoDocumentoOTROS es obligatorio cuando TipoDocumentoOC = 99 (según XSD v4.4)
            if (tipoDoc == "99")
            {
                var tipoOtro = !string.IsNullOrWhiteSpace(primerCargo.TipoDocumentoOtro)
                    ? primerCargo.TipoDocumentoOtro
                    : primerCargo.Detalle ?? "Otros cargos";
                elementos.Add(new XElement(ns + "TipoDocumentoOTROS", tipoOtro));
            }

            elementos.Add(new XElement(ns + "Detalle", primerCargo.Detalle ?? "Otros cargos"));
            elementos.Add(new XElement(ns + "MontoCargo", FormatearDecimal(primerCargo.Monto, 5)));

            return new XElement(ns + "OtrosCargos", elementos.ToArray());
        }

        // Fallback: Si solo hay TotalOtrosCargos sin detalle, generar cargo genérico
        if (doc.TotalOtrosCargos > 0)
        {
            return new XElement(ns + "OtrosCargos",
                new XElement(ns + "TipoDocumentoOC", "99"),
                new XElement(ns + "TipoDocumentoOTROS", "Otros cargos"), // Obligatorio cuando TipoDocumentoOC = 99 (XSD v4.4)
                new XElement(ns + "Detalle", "Otros cargos"),
                new XElement(ns + "MontoCargo", FormatearDecimal(doc.TotalOtrosCargos, 5))
            );
        }

        return null;
    }

    /// <summary>
    /// v4.4 - Genera todos los elementos OtrosCargos como lista
    /// </summary>
    private List<XElement> GenerarTodosOtrosCargos(Documento doc, XNamespace ns)
    {
        var elementos = new List<XElement>();

        if (doc.OtrosCargos != null && doc.OtrosCargos.Any(c => !c.IsDeleted && c.Monto > 0))
        {
            foreach (var cargo in doc.OtrosCargos.Where(c => !c.IsDeleted && c.Monto > 0))
            {
                var tipoDoc = cargo.TipoDocumento ?? "99";
                var cargoElementos = new List<object>
                {
                    new XElement(ns + "TipoDocumentoOC", tipoDoc)
                };

                // TipoDocumentoOTROS es obligatorio cuando TipoDocumentoOC = 99 (según XSD v4.4)
                if (tipoDoc == "99")
                {
                    var tipoOtro = !string.IsNullOrWhiteSpace(cargo.TipoDocumentoOtro)
                        ? cargo.TipoDocumentoOtro
                        : cargo.Detalle ?? "Otros cargos";
                    cargoElementos.Add(new XElement(ns + "TipoDocumentoOTROS", tipoOtro));
                }

                cargoElementos.Add(new XElement(ns + "Detalle", cargo.Detalle ?? "Otros cargos"));
                cargoElementos.Add(new XElement(ns + "MontoCargo", FormatearDecimal(cargo.Monto, 5)));

                elementos.Add(new XElement(ns + "OtrosCargos", cargoElementos.ToArray()));
            }
        }
        else if (doc.TotalOtrosCargos > 0)
        {
            // Fallback: cargo genérico
            elementos.Add(new XElement(ns + "OtrosCargos",
                new XElement(ns + "TipoDocumentoOC", "99"),
                new XElement(ns + "TipoDocumentoOTROS", "Otros cargos"), // Obligatorio cuando TipoDocumentoOC = 99 (XSD v4.4)
                new XElement(ns + "Detalle", "Otros cargos"),
                new XElement(ns + "MontoCargo", FormatearDecimal(doc.TotalOtrosCargos, 5))
            ));
        }

        return elementos;
    }

    private XElement GenerarNumeroConsecutivo(Documento doc, XNamespace ns)
    {
        return new XElement(ns + "NumeroConsecutivo", doc.NumeroConsecutivo);
    }

    private XElement GenerarFechaEmision(Documento doc, XNamespace ns)
    {
        // Formato: 2025-01-15T10:30:00-06:00
        var fechaCostaRica = FechaCostaRicaHelper.AsignarOffsetCostaRica(doc.FechaEmision);
        string fecha = fechaCostaRica.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
        return new XElement(ns + "FechaEmision", fecha);
    }

    /// <summary>
    /// v4.4 - OBLIGATORIO: Genera el elemento ProveedorSistemas
    /// Es un simple string con el numero de identificacion del proveedor (max 20 chars)
    /// CORREGIDO: Ya no retorna null, siempre genera un valor valido
    /// </summary>
    private XElement GenerarProveedorSistemas(Documento doc, XNamespace ns)
    {
        var empresa = doc.Empresa;

        // ProveedorSistemas es OBLIGATORIO en v4.4
        // Si no hay datos del proveedor, usar la cedula de la empresa como proveedor
        string proveedorId;

        if (!string.IsNullOrWhiteSpace(empresa?.ProveedorSistemasIdentificacion))
        {
            proveedorId = empresa.ProveedorSistemasIdentificacion;
        }
        else if (!string.IsNullOrWhiteSpace(empresa?.NumeroIdentificacion))
        {
            // Usar cedula de la empresa como fallback
            proveedorId = empresa.NumeroIdentificacion;
        }
        else
        {
            // Ultimo recurso: lanzar excepcion porque es obligatorio
            throw new InvalidOperationException(
                "ProveedorSistemas es obligatorio en v4.4. Configure el campo ProveedorSistemasIdentificacion en la empresa.");
        }

        // Limitar a 20 caracteres segun XSD
        if (proveedorId.Length > 20)
        {
            proveedorId = proveedorId.Substring(0, 20);
        }

        // v4.4: ProveedorSistemas es un simpleType (solo texto, max 20 caracteres)
        return new XElement(ns + "ProveedorSistemas", proveedorId);
    }

    private XElement GenerarEmisor(Documento doc, XNamespace ns)
    {
        var empresa = doc.Empresa!;

        var emisor = new XElement(ns + "Emisor",
            new XElement(ns + "Nombre", empresa.RazonSocial),
            new XElement(ns + "Identificacion",
                new XElement(ns + "Tipo", ObtenerCodigoTipoIdentificacion(empresa.TipoIdentificacion)),
                new XElement(ns + "Numero", empresa.NumeroIdentificacion)
            )
        );

        // Nombre Comercial (opcional)
        if (!string.IsNullOrWhiteSpace(empresa.NombreComercial))
        {
            emisor.Add(new XElement(ns + "NombreComercial", empresa.NombreComercial));
        }

        // Ubicación - v4.4: Canton y Distrito son 2 dígitos (local, sin código de provincia)
        // Si el valor almacenado incluye provincia (ej: 101, 10103), extraer solo los últimos 2 dígitos
        var cantonStr = (empresa.Canton % 100).ToString("D2"); // Últimos 2 dígitos
        var distritoStr = (empresa.Distrito % 100).ToString("D2"); // Últimos 2 dígitos

        var ubicacionEmisor = new XElement(ns + "Ubicacion",
            new XElement(ns + "Provincia", empresa.Provincia.ToString()),
            new XElement(ns + "Canton", cantonStr),
            new XElement(ns + "Distrito", distritoStr)
        );

        // v4.4: Barrio es OPCIONAL (minLength=5, maxLength=50)
        // TODO: Agregar soporte cuando se haga la migración de DB para el campo Barrio
        // Por ahora se omite ya que el XSD lo marca como opcional

        // v4.4: OtrasSenas es OBLIGATORIO en Emisor (minLength=5)
        // Si está vacío, usar valor por defecto
        var otrasSenas = !string.IsNullOrWhiteSpace(empresa.OtrasSenas)
            ? empresa.OtrasSenas
            : "Sin otras señas";
        ubicacionEmisor.Add(new XElement(ns + "OtrasSenas", otrasSenas));

        emisor.Add(ubicacionEmisor);

        // Teléfono (opcional)
        var telefono = empresa.Telefonos?.FirstOrDefault();
        if (telefono != null)
        {
            emisor.Add(new XElement(ns + "Telefono",
                new XElement(ns + "CodigoPais", telefono.CodigoPais ?? "506"),
                new XElement(ns + "NumTelefono", telefono.NumeroTelefono)
            ));
        }

        // v4.4: CorreoElectronico es OBLIGATORIO en Emisor
        // Hasta 4 emails permitidos, usamos el primero disponible
        var email = empresa.Emails?.FirstOrDefault();
        var emailAddress = email?.DireccionEmail ?? "facturacion@empresa.com";
        emisor.Add(new XElement(ns + "CorreoElectronico", emailAddress));

        return emisor;
    }

    /// <summary>
    /// v4.4: Receptor opcional para Tiquete Electrónico
    /// Si hay datos del receptor, los incluye; si no, retorna null
    /// En TE: Identificación es OPCIONAL
    /// </summary>
    private XElement? GenerarReceptorTiquete(Documento doc, XNamespace ns)
    {
        // Si no hay receptor, retornar null (es opcional en Tiquete)
        if (doc.Cliente == null && string.IsNullOrWhiteSpace(doc.ReceptorNombre))
            return null;

        // Para Tiquete: Identificacion es opcional
        return GenerarReceptorInterno(doc, ns, identificacionRequerida: false);
    }

    private XElement? GenerarReceptor(Documento doc, XNamespace ns)
    {
        // Si no hay receptor, retornar null
        if (doc.Cliente == null && string.IsNullOrWhiteSpace(doc.ReceptorNombre))
            return null;

        // Para FE, NC, ND: Identificacion es OBLIGATORIO en el Receptor
        return GenerarReceptorInterno(doc, ns, identificacionRequerida: true);
    }

    /// <summary>
    /// Genera el elemento Receptor interno con control de campos obligatorios
    /// </summary>
    /// <param name="identificacionRequerida">true para FE/NC/ND (requerido), false para TE/FEE (opcional)</param>
    /// <param name="ubicacionConOtrasSenasRequerido">true si OtrasSenas es obligatorio cuando hay Ubicacion</param>
    private XElement? GenerarReceptorInterno(Documento doc, XNamespace ns, bool identificacionRequerida = false, bool ubicacionConOtrasSenasRequerido = true)
    {
        // Si no hay receptor, retornar null
        if (doc.Cliente == null && string.IsNullOrWhiteSpace(doc.ReceptorNombre))
            return null;

        var receptor = new XElement(ns + "Receptor");

        // Nombre (OBLIGATORIO, minLength=3)
        string nombreReceptor = doc.ReceptorNombre ?? doc.Cliente?.Nombre ?? "Cliente";
        receptor.Add(new XElement(ns + "Nombre", nombreReceptor));

        // Identificación - v4.4: OBLIGATORIO para FE/NC/ND, opcional para TE/FEE
        if (doc.ReceptorTipoIdentificacion.HasValue && !string.IsNullOrWhiteSpace(doc.ReceptorNumeroIdentificacion))
        {
            receptor.Add(new XElement(ns + "Identificacion",
                new XElement(ns + "Tipo", ObtenerCodigoTipoIdentificacion(doc.ReceptorTipoIdentificacion.Value)),
                new XElement(ns + "Numero", doc.ReceptorNumeroIdentificacion)
            ));
        }
        else if (identificacionRequerida)
        {
            // Si es requerido y no hay datos, usar datos del cliente o valores por defecto
            var tipoId = doc.Cliente?.TipoIdentificacion ?? TipoIdentificacion.Fisica;
            var numId = doc.Cliente?.NumeroIdentificacion ?? "000000000";
            receptor.Add(new XElement(ns + "Identificacion",
                new XElement(ns + "Tipo", ObtenerCodigoTipoIdentificacion(tipoId)),
                new XElement(ns + "Numero", numId)
            ));
        }

        // Nombre Comercial (opcional)
        if (!string.IsNullOrWhiteSpace(doc.ReceptorNombreComercial))
        {
            receptor.Add(new XElement(ns + "NombreComercial", doc.ReceptorNombreComercial));
        }

        // NOTA v4.4: ActividadEconomica se movió a CodigoActividadReceptor en el nivel raíz
        // Ya no se incluye aquí en el Receptor

        // Ubicación (opcional) - v4.4: Canton y Distrito son 2 dígitos (local)
        if (doc.ReceptorProvincia.HasValue)
        {
            var cantonReceptor = ((doc.ReceptorCanton ?? 1) % 100).ToString("D2");
            var distritoReceptor = ((doc.ReceptorDistrito ?? 1) % 100).ToString("D2");

            var ubicacionReceptor = new XElement(ns + "Ubicacion",
                new XElement(ns + "Provincia", doc.ReceptorProvincia.Value.ToString()),
                new XElement(ns + "Canton", cantonReceptor),
                new XElement(ns + "Distrito", distritoReceptor)
            );

            // v4.4: OtrasSenas es OBLIGATORIO en Ubicacion (minLength=5)
            // Si hay ubicación, debe tener OtrasSenas
            if (ubicacionConOtrasSenasRequerido)
            {
                var otrasSenas = !string.IsNullOrWhiteSpace(doc.ReceptorOtrasSenas)
                    ? doc.ReceptorOtrasSenas
                    : "Sin otras señas";
                ubicacionReceptor.Add(new XElement(ns + "OtrasSenas", otrasSenas));
            }
            else if (!string.IsNullOrWhiteSpace(doc.ReceptorOtrasSenas))
            {
                ubicacionReceptor.Add(new XElement(ns + "OtrasSenas", doc.ReceptorOtrasSenas));
            }

            receptor.Add(ubicacionReceptor);
        }

        // Teléfono (opcional)
        if (!string.IsNullOrWhiteSpace(doc.ReceptorTelefono))
        {
            receptor.Add(new XElement(ns + "Telefono",
                new XElement(ns + "CodigoPais", "506"),
                new XElement(ns + "NumTelefono", doc.ReceptorTelefono)
            ));
        }

        // Email (opcional - hasta 4 emails en v4.4)
        if (!string.IsNullOrWhiteSpace(doc.ReceptorEmails))
        {
            var emails = doc.ReceptorEmails.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (emails.Any())
            {
                receptor.Add(new XElement(ns + "CorreoElectronico", emails[0].Trim()));
            }
        }

        return receptor;
    }

    private XElement GenerarCondicionVenta(Documento doc, XNamespace ns)
    {
        return new XElement(ns + "CondicionVenta", doc.CondicionVenta);
    }

    private XElement? GenerarPlazoCredito(Documento doc, XNamespace ns)
    {
        // Solo si la condición de venta es crédito (02)
        if (doc.CondicionVenta == "02" && doc.PlazoCreditoDias.HasValue)
        {
            return new XElement(ns + "PlazoCredito", doc.PlazoCreditoDias.Value.ToString());
        }
        return null;
    }

    /// <summary>
    /// OBSOLETO v4.4: MedioPago ya no se genera a nivel de documento.
    /// Ahora se genera dentro de ResumenFactura usando GenerarMediosPagoResumen()
    /// Este metodo se mantiene por compatibilidad pero no debe usarse.
    /// </summary>
    [Obsolete("v4.4: Usar GenerarMediosPagoResumen() en ResumenFactura en su lugar")]
    private XElement GenerarMedioPago(Documento doc, XNamespace ns)
    {
        // NOTA: Este metodo ya no se usa en v4.4
        // MedioPago ahora va dentro de ResumenFactura con estructura compleja
        return new XElement(ns + "MedioPago", doc.MedioPago);
    }

    /// <summary>
    /// Genera el elemento DetalleServicio con todas las líneas del documento
    /// v4.4: Orden correcto de elementos según XSD:
    /// NumeroLinea, CodigoCABYS, CodigoComercial, Cantidad, UnidadMedida, TipoTransaccion,
    /// UnidadMedidaComercial, Detalle, NumeroVINoSerie, RegistroMedicamento, FormaFarmaceutica, etc.
    /// </summary>
    /// <param name="doc">Documento</param>
    /// <param name="ns">Namespace XML</param>
    /// <param name="esParaTiquete">Si es true, NO incluye TipoTransaccion (Tiquetes no llevan este campo)</param>
    private XElement GenerarDetalleServicio(Documento doc, XNamespace ns, bool esParaTiquete = false)
    {
        var detalleServicio = new XElement(ns + "DetalleServicio");

        foreach (var linea in doc.Detalles.OrderBy(d => d.NumeroLinea))
        {
            var lineaDetalle = new XElement(ns + "LineaDetalle",
                new XElement(ns + "NumeroLinea", linea.NumeroLinea)
            );

            // v4.4: CodigoCABYS - OBLIGATORIO (elemento standalone, no complejo)
            // Debe ir ANTES de CodigoComercial
            if (!string.IsNullOrWhiteSpace(linea.CodigoCabys))
            {
                lineaDetalle.Add(new XElement(ns + "CodigoCABYS", linea.CodigoCabys));
            }
            else
            {
                // Si no hay código CABYS, usar uno genérico (esto debería validarse antes)
                lineaDetalle.Add(new XElement(ns + "CodigoCABYS", "0000000000000"));
            }

            // v4.4: CodigoComercial (opcional) - elemento complejo con Tipo y Codigo
            var codigoComercial = GenerarCodigoComercialLinea(linea, ns);
            if (codigoComercial != null)
            {
                lineaDetalle.Add(codigoComercial);
            }

            // Campos obligatorios en orden XSD
            lineaDetalle.Add(new XElement(ns + "Cantidad", FormatearDecimal(linea.Cantidad, 3)));
            lineaDetalle.Add(new XElement(ns + "UnidadMedida", linea.UnidadMedida?.Codigo ?? "Unid"));

            // v4.4: TipoTransaccion (01-13) - Opcional, va DESPUÉS de UnidadMedida
            // NO incluir en Tiquetes
            if (!esParaTiquete && linea.TipoTransaccion.HasValue)
            {
                lineaDetalle.Add(new XElement(ns + "TipoTransaccion",
                    ((int)linea.TipoTransaccion.Value).ToString("D2")));
            }

            // Detalle (descripción) - OBLIGATORIO
            lineaDetalle.Add(new XElement(ns + "Detalle", linea.Descripcion));

            // NUEVO v4.4 - M6: Múltiples Números VIN (hasta 1000 para vehículos)
            if (linea.NumerosVIN != null && linea.NumerosVIN.Any())
            {
                foreach (var vin in linea.NumerosVIN.OrderBy(v => v.NumeroOrden))
                {
                    lineaDetalle.Add(new XElement(ns + "NumeroVINoSerie", vin.NumeroVIN));
                }
            }
            // Soporte legacy: VIN único en campo antiguo
            else if (!string.IsNullOrWhiteSpace(linea.NumeroVIN))
            {
                lineaDetalle.Add(new XElement(ns + "NumeroVINoSerie", linea.NumeroVIN));
            }

            // NUEVO v4.4 - M7: Número de Registro de Medicamento (para productos farmacéuticos)
            if (!string.IsNullOrWhiteSpace(linea.NumeroRegistroMedicamento))
            {
                lineaDetalle.Add(new XElement(ns + "RegistroMedicamento", linea.NumeroRegistroMedicamento));
            }

            // NUEVO v4.4 - M7: Forma Farmacéutica (para productos farmacéuticos)
            // Usar FK si está disponible, sino usar campo legacy
            var formaFarmaceutica = linea.FormaFarmaceuticaNavigation?.Codigo ?? linea.FormaFarmaceutica;
            if (!string.IsNullOrWhiteSpace(formaFarmaceutica))
            {
                lineaDetalle.Add(new XElement(ns + "FormaFarmaceutica", formaFarmaceutica));
            }

            // NUEVO v4.4 - M1: Detalle de Surtido (para combos/paquetes)
            if (!string.IsNullOrWhiteSpace(linea.DetalleSurtido))
            {
                lineaDetalle.Add(new XElement(ns + "DetalleSurtido", linea.DetalleSurtido));
            }

            // Precios
            lineaDetalle.Add(new XElement(ns + "PrecioUnitario", FormatearDecimal(linea.PrecioUnitario, 5)));
            lineaDetalle.Add(new XElement(ns + "MontoTotal", FormatearDecimal(linea.MontoTotal, 5)));

            // Descuentos (opcional)
            if (linea.Descuentos != null && linea.Descuentos.Any())
            {
                foreach (var desc in linea.Descuentos)
                {
                    // v4.4: ORDEN EXACTO según XSD: MontoDescuento, CodigoDescuento, NaturalezaDescuento
                    lineaDetalle.Add(new XElement(ns + "Descuento",
                        new XElement(ns + "MontoDescuento", FormatearDecimal(desc.MontoDescuento, 5)),
                        new XElement(ns + "CodigoDescuento", desc.CodigoDescuento ?? "07"), // v4.4 OBLIGATORIO - Default: 07 Comercial
                        new XElement(ns + "NaturalezaDescuento", desc.NaturalezaDescuento ?? "Descuento comercial")
                    ));
                }
            }

            lineaDetalle.Add(new XElement(ns + "SubTotal", FormatearDecimal(linea.Subtotal, 5)));

            // Base Imponible (opcional)
            if (linea.BaseImponible.HasValue && linea.BaseImponible.Value > 0)
            {
                lineaDetalle.Add(new XElement(ns + "BaseImponible", FormatearDecimal(linea.BaseImponible.Value, 5)));
            }

            // v4.4: Impuestos (minOccurs=1 - al menos uno requerido)
            // Orden de elementos en ImpuestoType: Codigo, CodigoImpuestoOTRO, CodigoTarifaIVA, Tarifa, FactorCalculoIVA, DatosImpuestoEspecifico, Monto, Exoneracion
            if (linea.Impuestos != null && linea.Impuestos.Any())
            {
                foreach (var imp in linea.Impuestos)
                {
                    var impuestoElement = new XElement(ns + "Impuesto",
                        new XElement(ns + "Codigo", imp.CodigoImpuesto)
                    );

                    // CodigoImpuestoOTRO (opcional - solo si Codigo es 99)
                    // Nota: Si se necesita, agregar propiedad CodigoImpuestoOtro a DocumentoDetalleImpuesto

                    // v4.4: CodigoTarifaIVA (obligatorio cuando Codigo es 01 - IVA)
                    if (!string.IsNullOrWhiteSpace(imp.CodigoTarifa))
                    {
                        impuestoElement.Add(new XElement(ns + "CodigoTarifaIVA", imp.CodigoTarifa));
                    }

                    // Tarifa (opcional)
                    impuestoElement.Add(new XElement(ns + "Tarifa", FormatearDecimal(imp.Tarifa, 2)));

                    // v4.4: FactorCalculoIVA (opcional - para IVA con factor)
                    if (imp.FactorIVA.HasValue && imp.FactorIVA.Value > 0)
                    {
                        impuestoElement.Add(new XElement(ns + "FactorCalculoIVA", FormatearDecimal(imp.FactorIVA.Value, 4)));
                    }

                    // Monto (obligatorio)
                    impuestoElement.Add(new XElement(ns + "Monto", FormatearDecimal(imp.MontoImpuesto, 5)));

                    // Exoneracion (opcional) - agregar si existe
                    if (imp.TieneExoneracion && !string.IsNullOrWhiteSpace(imp.TipoDocumentoExoneracion))
                    {
                        var exoneracion = new XElement(ns + "Exoneracion",
                            new XElement(ns + "TipoDocumento", imp.TipoDocumentoExoneracion),
                            new XElement(ns + "NumeroDocumento", imp.NumeroDocumentoExoneracion ?? ""),
                            new XElement(ns + "NombreInstitucion", imp.InstitucionExoneracion ?? ""),
                            new XElement(ns + "FechaEmision", imp.FechaEmisionExoneracion?.ToString("yyyy-MM-ddTHH:mm:ss") ?? DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")),
                            new XElement(ns + "PorcentajeExoneracion", FormatearDecimal(imp.PorcentajeExoneracion ?? 0, 2)),
                            new XElement(ns + "MontoExoneracion", FormatearDecimal(imp.MontoExoneracion ?? 0, 5))
                        );
                        impuestoElement.Add(exoneracion);
                    }

                    lineaDetalle.Add(impuestoElement);
                }
            }
            else
            {
                // v4.4: Impuesto es OBLIGATORIO (minOccurs=1), agregar impuesto exento si no hay impuestos
                var impuestoExento = new XElement(ns + "Impuesto",
                    new XElement(ns + "Codigo", "01"), // IVA
                    new XElement(ns + "CodigoTarifaIVA", "08"), // Tarifa 0% (Exento)
                    new XElement(ns + "Tarifa", "0.00"),
                    new XElement(ns + "Monto", "0.00000")
                );
                lineaDetalle.Add(impuestoExento);
            }

            // v4.4: ImpuestoAsumidoEmisorFabrica (OBLIGATORIO según XSD - minOccurs=1)
            // Si no hay impuesto asumido por el emisor o cobrado a nivel de fábrica, agregar con valor 0
            // TODO: Si se implementa esta funcionalidad, agregar propiedad ImpuestoAsumidoEmisor al DocumentoDetalle
            lineaDetalle.Add(new XElement(ns + "ImpuestoAsumidoEmisorFabrica", FormatearDecimal(0m, 5)));

            // v4.4: ImpuestoNeto (OBLIGATORIO según XSD - minOccurs=1)
            lineaDetalle.Add(new XElement(ns + "ImpuestoNeto", FormatearDecimal(linea.ImpuestoNeto ?? linea.MontoImpuesto, 5)));
            lineaDetalle.Add(new XElement(ns + "MontoTotalLinea", FormatearDecimal(linea.MontoTotalLinea, 5)));

            detalleServicio.Add(lineaDetalle);
        }

        return detalleServicio;
    }

    /// <summary>
    /// v4.4: Genera el elemento CodigoComercial (opcional) para una línea de detalle
    /// Este es un elemento complejo con Tipo y Codigo
    /// </summary>
    private XElement? GenerarCodigoComercialLinea(DocumentoDetalle linea, XNamespace ns)
    {
        if (linea.Producto == null || string.IsNullOrWhiteSpace(linea.Producto.Codigo))
            return null;

        // CodigoComercial tiene subelementos Tipo y Codigo
        var codigoComercial = new XElement(ns + "CodigoComercial",
            new XElement(ns + "Tipo", "01"), // 01 = Código del producto del vendedor
            new XElement(ns + "Codigo", linea.Producto.Codigo)
        );

        return codigoComercial;
    }

    /// <summary>
    /// Genera el elemento ResumenFactura segun XSD v4.4
    /// ORDEN EXACTO de elementos segun XSD:
    /// 1. CodigoTipoMoneda
    /// 2. TotalServGravados
    /// 3. TotalServExentos
    /// 4. TotalServExonerado
    /// 5. TotalMercanciasGravadas
    /// 6. TotalMercanciasExentas
    /// 7. TotalMercExonerada
    /// 8. TotalGravado
    /// 9. TotalExento
    /// 10. TotalExonerado
    /// 11. TotalVenta
    /// 12. TotalDescuentos
    /// 13. TotalVentaNeta
    /// 14. TotalDesgloseImpuesto (puede haber varios)
    /// 15. TotalImpuesto
    /// 16. TotalIVADevuelto (opcional)
    /// 17. TotalOtrosCargos (opcional)
    /// 18. MedioPago (puede haber varios, obligatorio excepto creditos)
    /// 19. TotalComprobante
    /// </summary>
    private XElement GenerarResumenFactura(Documento doc, XNamespace ns)
    {
        var resumen = new XElement(ns + "ResumenFactura");

        // 1. Moneda y tipo de cambio
        resumen.Add(new XElement(ns + "CodigoTipoMoneda",
            new XElement(ns + "CodigoMoneda", ObtenerCodigoMoneda(doc.Moneda)),
            new XElement(ns + "TipoCambio", FormatearDecimal(doc.TipoCambio ?? 1.00m, 5))
        ));

        // 2-4. Totales de Servicios
        resumen.Add(new XElement(ns + "TotalServGravados", FormatearDecimal(doc.TotalServiciosGravados, 5)));
        resumen.Add(new XElement(ns + "TotalServExentos", FormatearDecimal(doc.TotalServiciosExentos, 5)));
        resumen.Add(new XElement(ns + "TotalServExonerado", FormatearDecimal(doc.TotalServiciosExonerados, 5)));

        // 5-7. Totales de Mercancias
        resumen.Add(new XElement(ns + "TotalMercanciasGravadas", FormatearDecimal(doc.TotalMercanciasGravadas, 5)));
        resumen.Add(new XElement(ns + "TotalMercanciasExentas", FormatearDecimal(doc.TotalMercanciasExentas, 5)));
        resumen.Add(new XElement(ns + "TotalMercExonerada", FormatearDecimal(doc.TotalMercanciasExoneradas, 5)));

        // 8-10. Totales Consolidados
        resumen.Add(new XElement(ns + "TotalGravado", FormatearDecimal(doc.TotalGravado, 5)));
        resumen.Add(new XElement(ns + "TotalExento", FormatearDecimal(doc.TotalExento, 5)));
        resumen.Add(new XElement(ns + "TotalExonerado", FormatearDecimal(doc.TotalExonerado, 5)));

        // 11-13. Totales de Venta
        // v4.4: TotalVenta = TotalGravado + TotalExento + TotalExonerado (usando MontoTotal, ANTES de descuentos)
        resumen.Add(new XElement(ns + "TotalVenta", FormatearDecimal(doc.TotalVenta, 5)));

        // TotalDescuentos = suma de TODOS los descuentos (línea + documento)
        resumen.Add(new XElement(ns + "TotalDescuentos", FormatearDecimal(doc.TotalDescuentos, 5)));

        // TotalVentaNeta = TotalVenta - TotalDescuentos (resultado neto después de descuentos)
        var totalVentaNeta = doc.TotalVenta - doc.TotalDescuentos;
        resumen.Add(new XElement(ns + "TotalVentaNeta", FormatearDecimal(totalVentaNeta, 5)));

        // ========================================
        // 14. v4.4 - TotalDesgloseImpuesto (OBLIGATORIO cuando hay impuestos en detalles)
        // Error -487: "El documento posee detalle de Impuesto pero carece del campo Total Desglose Impuestos"
        // IMPORTANTE: Debe ir ANTES de TotalImpuesto segun orden XSD
        // ========================================
        var desgloseImpuestos = GenerarTotalDesgloseImpuesto(doc, ns);
        if (desgloseImpuestos != null && desgloseImpuestos.Any())
        {
            foreach (var desglose in desgloseImpuestos)
            {
                resumen.Add(desglose);
            }
        }

        // 15. Total Impuesto (suma de todos los impuestos)
        resumen.Add(new XElement(ns + "TotalImpuesto", FormatearDecimal(doc.TotalImpuestos, 5)));

        // 16. IVA Devuelto (opcional)
        if (doc.IVADevuelto.HasValue && doc.IVADevuelto.Value > 0)
        {
            resumen.Add(new XElement(ns + "TotalIVADevuelto", FormatearDecimal(doc.IVADevuelto.Value, 5)));
        }

        // 17. Otros cargos (opcional)
        if (doc.TotalOtrosCargos > 0)
        {
            resumen.Add(new XElement(ns + "TotalOtrosCargos", FormatearDecimal(doc.TotalOtrosCargos, 5)));
        }

        // ========================================
        // 18. v4.4 - MedioPago (OBLIGATORIO excepto cuando CondicionVenta es 02, 08 o 10 - creditos)
        // Error -517: "El nodo Medio de Pago es obligatorio, excepto cuando se utilice en el campo
        // condicion de la venta, los codigos 02, 08 y 10, correspondientes a creditos"
        // IMPORTANTE: Debe ir ANTES de TotalComprobante segun orden XSD
        // ========================================
        var mediosPagoElements = GenerarMediosPagoResumen(doc, ns);
        if (mediosPagoElements != null && mediosPagoElements.Any())
        {
            foreach (var medioPago in mediosPagoElements)
            {
                resumen.Add(medioPago);
            }
        }

        // 19. Total Comprobante (el monto final)
        // TotalComprobante = TotalVentaNeta + TotalImpuesto + TotalOtrosCargos - TotalIVADevuelto
        var totalComprobante = totalVentaNeta + doc.TotalImpuestos + doc.TotalOtrosCargos - (doc.IVADevuelto ?? 0m);
        resumen.Add(new XElement(ns + "TotalComprobante", FormatearDecimal(totalComprobante, 5)));

        return resumen;
    }

    /// <summary>
    /// v4.4 - Genera los elementos TotalDesgloseImpuesto para el ResumenFactura
    /// Agrupa los impuestos de todas las lineas por Codigo y CodigoTarifaIVA
    /// Estructura segun XSD:
    ///   - Codigo: Codigo del impuesto (01=IVA, etc.)
    ///   - CodigoTarifaIVA: Codigo de tarifa (opcional, solo para IVA)
    ///   - TotalMontoImpuesto: Suma total del monto del impuesto
    /// </summary>
    private List<XElement>? GenerarTotalDesgloseImpuesto(Documento doc, XNamespace ns)
    {
        // Verificar si hay detalles con impuestos
        if (doc.Detalles == null || !doc.Detalles.Any())
            return null;

        var todosLosImpuestos = doc.Detalles
            .Where(d => d.Impuestos != null)
            .SelectMany(d => d.Impuestos!)
            .ToList();

        if (!todosLosImpuestos.Any())
            return null;

        // Agrupar impuestos por Codigo y CodigoTarifaIVA
        var impuestosAgrupados = todosLosImpuestos
            .GroupBy(i => new { i.CodigoImpuesto, i.CodigoTarifa })
            .Select(g => new
            {
                Codigo = g.Key.CodigoImpuesto,
                CodigoTarifaIVA = g.Key.CodigoTarifa,
                TotalMonto = g.Sum(i => i.MontoImpuesto)
            })
            .OrderBy(x => x.Codigo)
            .ThenBy(x => x.CodigoTarifaIVA)
            .ToList();

        var elementos = new List<XElement>();

        foreach (var grupo in impuestosAgrupados)
        {
            var desglose = new XElement(ns + "TotalDesgloseImpuesto",
                new XElement(ns + "Codigo", grupo.Codigo)
            );

            // CodigoTarifaIVA es opcional, pero se incluye si esta disponible
            // Solo aplica cuando el codigo de impuesto es IVA (01)
            if (!string.IsNullOrWhiteSpace(grupo.CodigoTarifaIVA) && grupo.Codigo == "01")
            {
                desglose.Add(new XElement(ns + "CodigoTarifaIVA", grupo.CodigoTarifaIVA));
            }

            desglose.Add(new XElement(ns + "TotalMontoImpuesto", FormatearDecimal(grupo.TotalMonto, 5)));

            elementos.Add(desglose);
        }

        return elementos;
    }

    /// <summary>
    /// v4.4 - Genera los elementos MedioPago para el ResumenFactura
    /// OBLIGATORIO excepto cuando CondicionVenta es:
    ///   - 02: Credito
    ///   - 08: Servicios prestados al Estado a credito
    ///   - 10: Mercancia no nacionalizada
    ///
    /// Estructura segun XSD:
    ///   - TipoMedioPago: Codigo del medio de pago (01-Efectivo, 02-Tarjeta, etc.)
    ///   - MedioPagoOtros: Descripcion cuando TipoMedioPago es 99
    ///   - TotalMedioPago: Monto pagado con este medio
    /// </summary>
    private List<XElement>? GenerarMediosPagoResumen(Documento doc, XNamespace ns)
    {
        // Condiciones de venta que NO requieren MedioPago (son creditos)
        var condicionesCredito = new[] { "02", "08", "10" };

        // Si es credito, no se requiere MedioPago
        if (condicionesCredito.Contains(doc.CondicionVenta))
            return null;

        var elementos = new List<XElement>();

        // Si hay medios de pago registrados, usarlos
        if (doc.MediosPago != null && doc.MediosPago.Any())
        {
            foreach (var medio in doc.MediosPago)
            {
                var medioPagoElement = new XElement(ns + "MedioPago",
                    new XElement(ns + "TipoMedioPago", medio.CodigoMedioPago)
                );

                // Si es "Otros" (99), agregar descripcion
                if (medio.CodigoMedioPago == "99" && !string.IsNullOrWhiteSpace(medio.Descripcion))
                {
                    medioPagoElement.Add(new XElement(ns + "MedioPagoOtros", medio.Descripcion));
                }

                // Monto del medio de pago
                medioPagoElement.Add(new XElement(ns + "TotalMedioPago", FormatearDecimal(medio.Monto, 5)));

                elementos.Add(medioPagoElement);
            }
        }
        else
        {
            // Si no hay medios de pago registrados, usar el MedioPago principal del documento
            // Esto asegura compatibilidad con documentos que usan el campo simple MedioPago
            if (!string.IsNullOrWhiteSpace(doc.MedioPago))
            {
                var medioPagoElement = new XElement(ns + "MedioPago",
                    new XElement(ns + "TipoMedioPago", doc.MedioPago),
                    new XElement(ns + "TotalMedioPago", FormatearDecimal(doc.TotalVenta, 5))
                );

                elementos.Add(medioPagoElement);
            }
        }

        return elementos.Any() ? elementos : null;
    }

    /// <summary>
    /// Genera elementos InformacionReferencia segun XSD v4.4
    /// CORREGIDO: El XSD define que InformacionReferencia contiene directamente los campos,
    /// NO un elemento "Referencia" intermedio. Cada referencia genera un elemento InformacionReferencia separado.
    /// Retorna object para compatibilidad con el constructor de XElement que acepta params object[]
    /// </summary>
    private object? GenerarInformacionReferencia(Documento doc, XNamespace ns)
    {
        if (doc.Referencias == null || !doc.Referencias.Any())
            return null;

        var referencias = new List<XElement>();

        // Segun XSD v4.4, cada referencia es un elemento InformacionReferencia independiente
        // que contiene directamente: TipoDoc, Numero, FechaEmision, Codigo, Razon
        // NO hay un elemento "Referencia" intermedio
        foreach (var referencia in doc.Referencias)
        {
            var infoRef = new XElement(ns + "InformacionReferencia",
                new XElement(ns + "TipoDoc", referencia.TipoDocumentoReferenciado),
                new XElement(ns + "Numero", referencia.NumeroDocumentoReferenciado),
                new XElement(ns + "FechaEmision",
                    referencia.FechaEmisionDocumentoReferenciado.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)),
                new XElement(ns + "Codigo", ((int)referencia.CodigoReferencia).ToString("D2")),
                new XElement(ns + "Razon", referencia.RazonReferencia)
            );

            referencias.Add(infoRef);
        }

        // Retornar el array de elementos para que XElement los expanda como hijos
        return referencias.ToArray();
    }

    private XElement GenerarNormativa(Documento doc, XNamespace ns)
    {
        return new XElement(ns + "Normativa",
            new XElement(ns + "NumeroResolucion", "DGT-R-48-2016"),
            new XElement(ns + "FechaResolucion", "07-10-2016 08:00:00")
        );
    }

    private XElement? GenerarOtros(Documento doc, XNamespace ns)
    {
        if (doc.OtraInformacion == null || !doc.OtraInformacion.Any())
            return null;

        var otros = new XElement(ns + "Otros");

        foreach (var info in doc.OtraInformacion)
        {
            otros.Add(new XElement(ns + "OtraInformacion",
                new XElement(ns + "Codigo", info.Clave ?? ""),
                new XElement(ns + "Texto", info.Valor ?? "")
            ));
        }

        return otros;
    }

    #endregion

    #region Helpers

    private string ObtenerCodigoTipoIdentificacion(TipoIdentificacion tipo)
    {
        return tipo switch
        {
            TipoIdentificacion.Fisica => "01",
            TipoIdentificacion.Juridica => "02",
            TipoIdentificacion.DIMEX => "03",
            TipoIdentificacion.NITE => "04",
            TipoIdentificacion.Pasaporte => "04",
            TipoIdentificacion.Extranjera => "05",
            _ => "01"
        };
    }

    private string ObtenerCodigoMoneda(TipoMoneda moneda)
    {
        return moneda switch
        {
            TipoMoneda.CRC => "CRC",
            TipoMoneda.USD => "USD",
            TipoMoneda.EUR => "EUR",
            _ => "CRC"
        };
    }

    private string FormatearDecimal(decimal valor, int decimales)
    {
        return valor.ToString($"F{decimales}", CultureInfo.InvariantCulture);
    }

    #endregion

    #region Mensaje Receptor (MR)

    /// <summary>
    /// Genera el XML de un Mensaje Receptor (tipos 05, 06, 07)
    /// </summary>
    public async Task<string> GenerarMensajeReceptorAsync(DocumentoReceptorMensaje mensaje, Documento documentoOriginal)
    {
        // Cargar relaciones necesarias del documento original
        await CargarRelacionesAsync(documentoOriginal);

        // Namespace del Mensaje Receptor v4.4
        XNamespace ns = NamespaceMensajeReceptor;

        // Crear documento XML
        var xmlDoc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(ns + "MensajeReceptor",
                // Clave del mensaje (50 dígitos)
                new XElement(ns + "Clave", mensaje.ClaveMensaje),

                // NumeroCedulaEmisor: Cédula del receptor del documento original (nosotros)
                // En el MR, nosotros somos el emisor del mensaje
                new XElement(ns + "NumeroCedulaEmisor", documentoOriginal.ReceptorNumeroIdentificacion),

                // FechaEmisionDoc: Fecha de emisión del mensaje
                new XElement(ns + "FechaEmisionDoc", mensaje.FechaEmision.ToString("yyyy-MM-ddTHH:mm:sszzz")),

                // Mensaje: Tipo de mensaje (1=Aceptación, 2=Aceptación Parcial, 3=Rechazo)
                new XElement(ns + "Mensaje", mensaje.TipoMensaje.ToString()),

                // DetalleMensaje: Detalle opcional (obligatorio para rechazo y aceptación parcial)
                mensaje.DetalleMensaje != null
                    ? new XElement(ns + "DetalleMensaje", mensaje.DetalleMensaje)
                    : null,

                // MontoTotalImpuesto: Solo para aceptación parcial (tipo 2)
                mensaje.TipoMensaje == 2 && mensaje.MontoTotalImpuestoAceptado.HasValue
                    ? new XElement(ns + "MontoTotalImpuesto", FormatearDecimal(mensaje.MontoTotalImpuestoAceptado.Value, 5))
                    : null,

                // TotalFactura: Solo para aceptación parcial (tipo 2)
                mensaje.TipoMensaje == 2 && mensaje.MontoTotalAceptado.HasValue
                    ? new XElement(ns + "TotalFactura", FormatearDecimal(mensaje.MontoTotalAceptado.Value, 5))
                    : null,

                // NumeroCedulaReceptor: Cédula del emisor del documento original (el proveedor)
                // En el MR, el proveedor es el receptor del mensaje
                new XElement(ns + "NumeroCedulaReceptor",
                    ObtenerNumeroIdentificacionEmisor(documentoOriginal)),

                // NumeroConsecutivoReceptor: Consecutivo del mensaje
                new XElement(ns + "NumeroConsecutivoReceptor", mensaje.NumeroConsecutivo)
            )
        );

        // Convertir a string con formato
        return xmlDoc.Declaration?.ToString() + Environment.NewLine + xmlDoc.ToString();
    }

    /// <summary>
    /// Obtiene el número de identificación del emisor del documento
    /// </summary>
    private string ObtenerNumeroIdentificacionEmisor(Documento documento)
    {
        // Si el documento es recibido, el emisor es el proveedor
        if (documento.EsDocumentoRecibido && documento.Proveedor != null)
        {
            return documento.Proveedor.NumeroIdentificacion;
        }

        // Si el documento es emitido, el emisor es la empresa
        if (documento.Empresa != null)
        {
            return documento.Empresa.NumeroIdentificacion;
        }

        throw new InvalidOperationException("No se pudo determinar el número de identificación del emisor");
    }

    #endregion

    #region Generar REP (Recibo Electrónico de Pago)

    /// <summary>
    /// Genera el XML de un Recibo Electrónico de Pago (REP) - NUEVO en v4.4
    /// </summary>
    public async Task<string> GenerarREPAsync(Documento documentoREP, ReciboPago reciboPago)
    {
        // Cargar todas las relaciones necesarias
        await CargarRelacionesAsync(documentoREP);

        // Cargar documento original referenciado
        var documentoOriginal = await _context.Documentos
            .Include(d => d.Empresa)
            .FirstOrDefaultAsync(d => d.Id == reciboPago.DocumentoOriginalId)
            ?? throw new InvalidOperationException("No se encontró el documento original");

        // Cargar medios de pago
        var mediosPago = await _context.DocumentoMediosPago
            .Where(m => m.DocumentoId == documentoREP.Id)
            .ToListAsync();

        // Crear documento XML con namespace v4.4 para REP
        var ns = XNamespace.Get("https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/reciboElectronicoPago");

        var xml = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            new XElement(ns + "ReciboElectronicoPago",
                // 1. Clave del REP (50 dígitos)
                new XElement(ns + "Clave", documentoREP.Clave),

                // 2. ProveedorSistemas (v4.4 - OBLIGATORIO) - REP no tiene CodigoActividad
                GenerarProveedorSistemas(documentoREP, ns),

                // 3. NumeroConsecutivo del REP
                new XElement(ns + "NumeroConsecutivo", documentoREP.NumeroConsecutivo),

                // 4. FechaEmision (ISO 8601 con timezone)
                new XElement(ns + "FechaEmision", documentoREP.FechaEmision.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)),

                // 5. Emisor (la empresa que recibe el pago)
                GenerarEmisor(documentoREP, ns),

                // 6. Receptor (el cliente que paga)
                GenerarReceptor(documentoREP, ns),

                // 7. DocumentoReferencia (enlace al documento original)
                new XElement(ns + "DocumentoReferencia",
                    new XElement(ns + "TipoDoc", reciboPago.TipoDocumentoOriginal),
                    new XElement(ns + "Numero", reciboPago.NumeroConsecutivoOriginal),
                    new XElement(ns + "FechaEmision", documentoOriginal.FechaEmision.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)),
                    new XElement(ns + "Codigo", "01"), // 01 = Anula documento de referencia (para REP siempre es 01)
                    new XElement(ns + "Razon", $"Pago recibido por {FormatearDecimal(reciboPago.MontoPagado, 5)}")
                ),

                // 8. CodigoMoneda (CRC, USD, EUR)
                new XElement(ns + "CodigoMoneda", documentoREP.Moneda.ToString()),

                // 9. TipoCambio (si no es CRC)
                documentoREP.TipoCambio.HasValue
                    ? new XElement(ns + "TipoCambio", FormatearDecimal(documentoREP.TipoCambio.Value, 5))
                    : null,

                // 10. TotalComprobante (monto total pagado en este REP)
                new XElement(ns + "TotalComprobante", FormatearDecimal(reciboPago.MontoPagado, 5)),

                // 11. MedioPago (puede haber varios)
                from medio in mediosPago
                select new XElement(ns + "MedioPago",
                    new XElement(ns + "Medio", medio.CodigoMedioPago),
                    new XElement(ns + "Monto", FormatearDecimal(medio.Monto, 5)),
                    !string.IsNullOrWhiteSpace(medio.NumeroReferencia)
                        ? new XElement(ns + "NumeroReferencia", medio.NumeroReferencia)
                        : null
                ),

                // 12. SaldoPendiente (saldo que queda después de este pago)
                new XElement(ns + "SaldoPendiente", FormatearDecimal(reciboPago.SaldoPendiente, 5)),

                // 13. Otros (información adicional)
                !string.IsNullOrWhiteSpace(documentoREP.Observaciones)
                    ? new XElement(ns + "Otros",
                        new XElement(ns + "OtroTexto", documentoREP.Observaciones)
                    )
                    : null
            )
        );

        return xml.Declaration?.ToString() + Environment.NewLine + xml.ToString();
    }

    #endregion
}
