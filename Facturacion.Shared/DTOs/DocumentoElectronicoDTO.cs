using Facturacion.Shared.Enums;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para documentos electrónicos (vista simplificada)
/// </summary>
public class DocumentoElectronicoDTO
{
    public Guid Id { get; set; }
    public string Clave { get; set; } = null!;
    public string NumeroConsecutivo { get; set; } = null!;
    public DocumentoTipo TipoDocumento { get; set; }
    public EstadoDocumento Estado { get; set; }
    public DateTime FechaEmision { get; set; }

    // Receptor (Cliente o Proveedor)
    public string? ReceptorNombre { get; set; }
    public string? ReceptorNumeroIdentificacion { get; set; }

    // Totales
    public decimal Subtotal { get; set; }
    public decimal TotalDescuentos { get; set; }
    public decimal TotalImpuestos { get; set; }
    public decimal TotalVenta { get; set; }

    // Moneda
    public TipoMoneda Moneda { get; set; }
    public decimal? TipoCambio { get; set; }

    // Condición de venta
    public string CondicionVenta { get; set; } = null!;
    public int? PlazoCreditoDias { get; set; }

    // XML
    public string? XmlGenerado { get; set; }
    public string? XmlFirmado { get; set; }

    // Observaciones
    public string? Observaciones { get; set; }
}
