using Facturacion.Shared.Enums;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para documentos recibidos de proveedores (lado compras)
/// Representa información parseada de XML recibido
/// </summary>
public class DocumentoRecibido
{
    /// <summary>
    /// Clave numérica de 50 dígitos del documento original
    /// </summary>
    public string Clave { get; set; } = null!;

    /// <summary>
    /// Número consecutivo del documento original
    /// </summary>
    public string NumeroConsecutivo { get; set; } = null!;

    /// <summary>
    /// Tipo de documento recibido (FE, ND, NC, TE, FEE)
    /// </summary>
    public DocumentoTipo TipoDocumento { get; set; }

    /// <summary>
    /// Fecha de emisión del documento
    /// </summary>
    public DateTime FechaEmision { get; set; }

    // ========================================
    // EMISOR (El proveedor que nos envió el documento)
    // ========================================

    /// <summary>
    /// Tipo de identificación del emisor
    /// </summary>
    public TipoIdentificacion EmisorTipoIdentificacion { get; set; }

    /// <summary>
    /// Número de identificación del emisor
    /// </summary>
    public string EmisorNumeroIdentificacion { get; set; } = null!;

    /// <summary>
    /// Nombre del emisor
    /// </summary>
    public string EmisorNombre { get; set; } = null!;

    /// <summary>
    /// Nombre comercial del emisor
    /// </summary>
    public string? EmisorNombreComercial { get; set; }

    /// <summary>
    /// Actividad económica del emisor (CIIU4 - 6 dígitos)
    /// </summary>
    public string? EmisorActividadEconomica { get; set; }

    // ========================================
    // RECEPTOR (Nosotros - la empresa que recibe)
    // ========================================

    /// <summary>
    /// Tipo de identificación del receptor
    /// </summary>
    public TipoIdentificacion? ReceptorTipoIdentificacion { get; set; }

    /// <summary>
    /// Número de identificación del receptor
    /// </summary>
    public string? ReceptorNumeroIdentificacion { get; set; }

    /// <summary>
    /// Nombre del receptor
    /// </summary>
    public string? ReceptorNombre { get; set; }

    /// <summary>
    /// Actividad económica del receptor (CIIU4 - 6 dígitos) - NUEVO v4.4
    /// </summary>
    public string? ReceptorActividadEconomica { get; set; }

    // ========================================
    // DETALLES FINANCIEROS
    // ========================================

    /// <summary>
    /// Moneda del documento
    /// </summary>
    public TipoMoneda Moneda { get; set; }

    /// <summary>
    /// Tipo de cambio (si moneda != CRC)
    /// </summary>
    public decimal TipoCambio { get; set; }

    /// <summary>
    /// Total de ventas gravadas
    /// </summary>
    public decimal TotalGravado { get; set; }

    /// <summary>
    /// Total de ventas exentas
    /// </summary>
    public decimal TotalExento { get; set; }

    /// <summary>
    /// Total de ventas exoneradas
    /// </summary>
    public decimal TotalExonerado { get; set; }

    /// <summary>
    /// Subtotal (antes de impuestos y descuentos)
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Total de descuentos
    /// </summary>
    public decimal TotalDescuentos { get; set; }

    /// <summary>
    /// Total de impuestos
    /// </summary>
    public decimal TotalImpuestos { get; set; }

    /// <summary>
    /// Total de venta (monto final)
    /// </summary>
    public decimal TotalVenta { get; set; }

    /// <summary>
    /// Condición de venta (código catálogo)
    /// </summary>
    public string CondicionVenta { get; set; } = null!;

    /// <summary>
    /// Plazo de crédito en días (si condición venta = 02)
    /// </summary>
    public int? PlazoCreditoDias { get; set; }

    /// <summary>
    /// Medio de pago (código catálogo)
    /// </summary>
    public string MedioPago { get; set; } = null!;

    /// <summary>
    /// Observaciones del documento
    /// </summary>
    public string? Observaciones { get; set; }

    // ========================================
    // XML ORIGINAL
    // ========================================

    /// <summary>
    /// XML original completo recibido
    /// </summary>
    public string XmlOriginal { get; set; } = null!;

    // ========================================
    // LÍNEAS DE DETALLE (Simplificado)
    // ========================================

    /// <summary>
    /// Líneas de detalle del documento
    /// </summary>
    public List<DetalleRecibido> Detalles { get; set; } = new();
}

/// <summary>
/// Detalle de línea de un documento recibido
/// </summary>
public class DetalleRecibido
{
    /// <summary>
    /// Número de línea
    /// </summary>
    public int NumeroLinea { get; set; }

    /// <summary>
    /// Código del producto/servicio
    /// </summary>
    public string? Codigo { get; set; }

    /// <summary>
    /// Código CAByS (13 dígitos)
    /// </summary>
    public string? CodigoCabys { get; set; }

    /// <summary>
    /// Descripción del producto/servicio
    /// </summary>
    public string Descripcion { get; set; } = null!;

    /// <summary>
    /// Unidad de medida
    /// </summary>
    public string? UnidadMedida { get; set; }

    /// <summary>
    /// Cantidad
    /// </summary>
    public decimal Cantidad { get; set; }

    /// <summary>
    /// Precio unitario
    /// </summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Monto total antes de descuentos e impuestos
    /// </summary>
    public decimal MontoTotal { get; set; }

    /// <summary>
    /// Monto de descuento
    /// </summary>
    public decimal MontoDescuento { get; set; }

    /// <summary>
    /// Subtotal después de descuento
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Monto de impuesto
    /// </summary>
    public decimal MontoImpuesto { get; set; }

    /// <summary>
    /// Monto total de la línea (subtotal + impuesto)
    /// </summary>
    public decimal MontoTotalLinea { get; set; }
}
