using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para actualizar un documento existente
/// Solo permite actualizar documentos en estado Borrador
/// </summary>
public class UpdateDocumentoDTO
{
    [Required]
    public Guid Id { get; set; }

    // Relaciones (solo se permiten cambiar si están en Borrador)
    public Guid? SucursalId { get; set; }
    public Guid? TerminalId { get; set; }

    // Actividad económica
    [MaxLength(6)]
    public string? ActividadEconomica { get; set; }

    // Fecha de emisión
    public DateTime? FechaEmision { get; set; }

    // Receptor
    public Guid? ClienteId { get; set; }
    public Guid? ProveedorId { get; set; }

    // Datos del receptor
    public TipoIdentificacion? ReceptorTipoIdentificacion { get; set; }
    [MaxLength(20)]
    public string? ReceptorNumeroIdentificacion { get; set; }
    [MaxLength(200)]
    public string? ReceptorNombre { get; set; }
    [MaxLength(200)]
    public string? ReceptorNombreComercial { get; set; }
    [MaxLength(6)]
    public string? ReceptorActividadEconomica { get; set; }
    public int? ReceptorProvincia { get; set; }
    public int? ReceptorCanton { get; set; }
    public int? ReceptorDistrito { get; set; }
    public int? ReceptorBarrio { get; set; }
    [MaxLength(500)]
    public string? ReceptorOtrasSenas { get; set; }
    [MaxLength(400)]
    public string? ReceptorEmails { get; set; }
    [MaxLength(20)]
    public string? ReceptorTelefono { get; set; }

    // Condición de venta
    [MaxLength(2)]
    public string? CondicionVenta { get; set; }
    public int? PlazoCreditoDias { get; set; }

    // Medio de pago
    [MaxLength(2)]
    public string? MedioPago { get; set; }

    // Moneda y tipo de cambio
    public TipoMoneda? Moneda { get; set; }
    public decimal? TipoCambio { get; set; }

    // Observaciones
    [MaxLength(1000)]
    public string? Observaciones { get; set; }

    // Detalles actualizados (reemplaza todos los detalles existentes)
    public List<CreateDocumentoDetalleDTO>? Detalles { get; set; }

    // Descuentos actualizados
    public List<CreateDocumentoDescuentoDTO>? Descuentos { get; set; }

    // Referencias actualizadas
    public List<CreateDocumentoReferenciaDTO>? Referencias { get; set; }

    // Medios de pago actualizados
    public List<CreateDocumentoMedioPagoDTO>? MediosPago { get; set; }

    // Otra información actualizada
    public List<CreateDocumentoOtraInformacionDTO>? OtraInformacion { get; set; }

    // Información de exportación actualizada
    public CreateDocumentoExportacionDTO? Exportacion { get; set; }
}
