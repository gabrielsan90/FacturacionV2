using Facturacion.Shared.Enums;
using System.Globalization;

namespace Facturacion.Backend.Helpers;

/// <summary>
/// Helper class for PDF formatting utilities
/// Provides consistent formatting for Costa Rica electronic invoices
/// </summary>
public static class PdfFormatHelper
{
    private static readonly CultureInfo CultureCR = new("es-CR");

    /// <summary>
    /// Format currency amount with symbol based on currency type
    /// </summary>
    public static string FormatMoneda(decimal monto, TipoMoneda moneda)
    {
        var simbolo = moneda switch
        {
            TipoMoneda.CRC => "₡",
            TipoMoneda.USD => "$",
            TipoMoneda.EUR => "€",
            _ => "₡"
        };

        return $"{simbolo}{monto:N2}";
    }

    /// <summary>
    /// Format price with 5 decimals (Hacienda requirement)
    /// </summary>
    public static string FormatPrecio(decimal precio)
    {
        return precio.ToString("N5", CultureCR);
    }

    /// <summary>
    /// Format quantity with 3 decimals (Hacienda requirement)
    /// </summary>
    public static string FormatCantidad(decimal cantidad)
    {
        return cantidad.ToString("N3", CultureCR);
    }

    /// <summary>
    /// Format percentage
    /// </summary>
    public static string FormatPorcentaje(decimal porcentaje)
    {
        return $"{porcentaje:N2}%";
    }

    /// <summary>
    /// Get readable name for document type
    /// </summary>
    public static string GetNombreTipoDocumento(DocumentoTipo tipo)
    {
        return tipo switch
        {
            DocumentoTipo.FacturaElectronica => "FACTURA ELECTRÓNICA",
            DocumentoTipo.NotaDebitoElectronica => "NOTA DE DÉBITO ELECTRÓNICA",
            DocumentoTipo.NotaCreditoElectronica => "NOTA DE CRÉDITO ELECTRÓNICA",
            DocumentoTipo.TiqueteElectronico => "TIQUETE ELECTRÓNICO",
            DocumentoTipo.FacturaElectronicaExportacion => "FACTURA ELECTRÓNICA DE EXPORTACIÓN",
            DocumentoTipo.FacturaElectronicaCompra => "FACTURA ELECTRÓNICA DE COMPRA",
            DocumentoTipo.NotaCreditoElectronicaCompra => "NOTA DE CRÉDITO ELECTRÓNICA DE COMPRA",
            DocumentoTipo.NotaDebitoElectronicaCompra => "NOTA DE DÉBITO ELECTRÓNICA DE COMPRA",
            DocumentoTipo.ComprobanteElectronicoCompra => "COMPROBANTE ELECTRÓNICO DE COMPRA",
            DocumentoTipo.ReciboElectronicoPago => "RECIBO ELECTRÓNICO DE PAGO",
            _ => "DOCUMENTO ELECTRÓNICO"
        };
    }

    /// <summary>
    /// Get short name for document type
    /// </summary>
    public static string GetNombreCortoTipoDocumento(DocumentoTipo tipo)
    {
        return tipo switch
        {
            DocumentoTipo.FacturaElectronica => "FE",
            DocumentoTipo.NotaDebitoElectronica => "ND",
            DocumentoTipo.NotaCreditoElectronica => "NC",
            DocumentoTipo.TiqueteElectronico => "TE",
            DocumentoTipo.FacturaElectronicaExportacion => "FEE",
            DocumentoTipo.FacturaElectronicaCompra => "FEC",
            DocumentoTipo.NotaCreditoElectronicaCompra => "NCC",
            DocumentoTipo.NotaDebitoElectronicaCompra => "NDC",
            DocumentoTipo.ComprobanteElectronicoCompra => "CEC",
            DocumentoTipo.ReciboElectronicoPago => "REP",
            _ => "DOC"
        };
    }

    /// <summary>
    /// Get color hex for document type badge
    /// </summary>
    public static string GetColorTipoDocumento(DocumentoTipo tipo)
    {
        return tipo switch
        {
            DocumentoTipo.FacturaElectronica => "#28a745",      // Green
            DocumentoTipo.NotaDebitoElectronica => "#dc3545",   // Red
            DocumentoTipo.NotaCreditoElectronica => "#fd7e14",  // Orange
            DocumentoTipo.TiqueteElectronico => "#17a2b8",      // Cyan
            DocumentoTipo.FacturaElectronicaExportacion => "#6f42c1", // Purple
            DocumentoTipo.FacturaElectronicaCompra => "#20c997",      // Teal
            DocumentoTipo.ReciboElectronicoPago => "#007bff",   // Blue
            _ => "#6c757d" // Gray
        };
    }

    /// <summary>
    /// Get readable state name with color
    /// </summary>
    public static (string Nombre, string Color) GetEstado(EstadoDocumento estado)
    {
        return estado switch
        {
            EstadoDocumento.Borrador => ("Borrador", "#6c757d"),
            EstadoDocumento.Pendiente => ("Pendiente", "#ffc107"),
            EstadoDocumento.Procesando => ("Procesando", "#17a2b8"),
            EstadoDocumento.Aceptado => ("Aceptado", "#28a745"),
            EstadoDocumento.Rechazado => ("Rechazado", "#dc3545"),
            EstadoDocumento.Contingencia => ("Contingencia", "#fd7e14"),
            EstadoDocumento.Anulado => ("Anulado", "#6c757d"),
            EstadoDocumento.Error => ("Error", "#dc3545"),
            _ => ("Desconocido", "#6c757d")
        };
    }

    /// <summary>
    /// Get readable name for sale condition code
    /// </summary>
    public static string GetCondicionVenta(string? codigo)
    {
        return codigo switch
        {
            "01" => "Contado",
            "02" => "Crédito",
            "03" => "Consignación",
            "04" => "Apartado",
            "05" => "Arrendamiento con opción de compra",
            "06" => "Arrendamiento en función financiera",
            "07" => "Cobro a favor de un tercero",
            "08" => "Servicios prestados al Estado a crédito",
            "09" => "Pago del servicio prestado al Estado",
            "10" => "Mercancía no nacionalizada",
            "99" => "Otros",
            _ => codigo ?? "N/A"
        };
    }

    /// <summary>
    /// Get readable name for payment method code
    /// </summary>
    public static string GetMedioPago(string? codigo)
    {
        return codigo switch
        {
            "01" => "Efectivo",
            "02" => "Tarjeta",
            "03" => "Cheque",
            "04" => "Transferencia / Depósito",
            "05" => "Recaudado por terceros",
            "06" => "SINPE Móvil",
            "99" => "Otros",
            _ => codigo ?? "N/A"
        };
    }

    /// <summary>
    /// Get readable name for identification type
    /// </summary>
    public static string GetTipoIdentificacion(TipoIdentificacion? tipo)
    {
        return tipo switch
        {
            TipoIdentificacion.Fisica => "Cédula Física",
            TipoIdentificacion.Juridica => "Cédula Jurídica",
            TipoIdentificacion.DIMEX => "DIMEX",
            TipoIdentificacion.NITE => "NITE",
            TipoIdentificacion.Pasaporte => "Pasaporte",
            TipoIdentificacion.Extranjera => "Extranjero",
            TipoIdentificacion.NoContribuyente => "No Contribuyente",
            _ => "N/A"
        };
    }

    /// <summary>
    /// Format identification number based on type (e.g., 3-101-123456 for juridica)
    /// </summary>
    public static string FormatIdentificacion(string? numero, TipoIdentificacion? tipo)
    {
        if (string.IsNullOrWhiteSpace(numero))
            return "N/A";

        // For juridica, format as X-XXX-XXXXXX
        if (tipo == TipoIdentificacion.Juridica && numero.Length == 10)
        {
            return $"{numero[0]}-{numero.Substring(1, 3)}-{numero.Substring(4)}";
        }

        // For fisica, format as X-XXXX-XXXX
        if (tipo == TipoIdentificacion.Fisica && numero.Length == 9)
        {
            return $"{numero[0]}-{numero.Substring(1, 4)}-{numero.Substring(5)}";
        }

        return numero;
    }

    /// <summary>
    /// Format date for display in Costa Rica format
    /// </summary>
    public static string FormatFecha(DateTime fecha)
    {
        return fecha.ToString("dd/MM/yyyy", CultureCR);
    }

    /// <summary>
    /// Format date and time for display
    /// </summary>
    public static string FormatFechaHora(DateTime fecha)
    {
        return fecha.ToString("dd/MM/yyyy HH:mm:ss", CultureCR);
    }

    /// <summary>
    /// Get readable name for reference type code
    /// </summary>
    public static string GetTipoReferencia(TipoReferenciaDocumento? tipo)
    {
        return tipo switch
        {
            TipoReferenciaDocumento.AnulaDocumentoReferencia => "Anula documento de referencia",
            TipoReferenciaDocumento.CorrigeTexto => "Corrige texto del documento de referencia",
            TipoReferenciaDocumento.CorrigeMonto => "Corrige monto del documento de referencia",
            TipoReferenciaDocumento.ReferenciaOtroDocumento => "Referencia a otro documento",
            TipoReferenciaDocumento.SustituyeComprobanteProvisional => "Sustituye comprobante provisional por contingencia",
            TipoReferenciaDocumento.Otros => "Otros",
            _ => "N/A"
        };
    }

    /// <summary>
    /// Get readable name for other charge type code
    /// </summary>
    public static string GetTipoOtroCargo(string? codigo)
    {
        return codigo switch
        {
            "01" => "Gastos de transporte (flete)",
            "02" => "Gastos de exportación",
            "03" => "Gastos de cobranza",
            "04" => "Descuento por pronto pago",
            "05" => "Interés por mora",
            "06" => "Gastos administrativos",
            "07" => "Gastos de instalación",
            "08" => "Gastos de embalaje",
            "09" => "Impuestos no incluidos en líneas",
            "10" => "Otros",
            "99" => "Otros",
            _ => codigo ?? "N/A"
        };
    }

    /// <summary>
    /// Truncate text to specified length with ellipsis
    /// </summary>
    public static string TruncarTexto(string? texto, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

        if (texto.Length <= maxLength)
            return texto;

        return texto.Substring(0, maxLength - 3) + "...";
    }
}
