using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para crear un nuevo documento electrónico
/// </summary>
public class CreateDocumentoDTO
{
    // Identificadores de relaciones
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid EmpresaId { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid SucursalId { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid TerminalId { get; set; }

    // Tipo de documento
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DocumentoTipo TipoDocumento { get; set; }

    // Actividad económica
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(6)]
    public string ActividadEconomica { get; set; } = null!;

    // Fecha de emisión
    public DateTime? FechaEmision { get; set; } // Si no se proporciona, se usa DateTime.Now

    // Receptor
    public Guid? ClienteId { get; set; }
    public Guid? ProveedorId { get; set; }

    // Datos del receptor (si no viene de Cliente/Proveedor)
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
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(2)]
    public string CondicionVenta { get; set; } = "01"; // 01-Contado por defecto

    public int? PlazoCreditoDias { get; set; }

    // Medio de pago
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(2)]
    public string MedioPago { get; set; } = "01"; // 01-Efectivo por defecto

    // Moneda y tipo de cambio
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public TipoMoneda Moneda { get; set; } = TipoMoneda.CRC;

    public decimal? TipoCambio { get; set; }

    // Observaciones
    [MaxLength(1000)]
    public string? Observaciones { get; set; }

    // Contingencia
    public bool EsContingencia { get; set; } = false;

    // Detalles (líneas de productos/servicios)
    [Required(ErrorMessage = "El documento debe tener al menos una línea de detalle.")]
    [MinLength(1, ErrorMessage = "El documento debe tener al menos una línea de detalle.")]
    public List<CreateDocumentoDetalleDTO> Detalles { get; set; } = new List<CreateDocumentoDetalleDTO>();

    // Descuentos a nivel de documento
    public List<CreateDocumentoDescuentoDTO>? Descuentos { get; set; }

    // Referencias (para NC/ND)
    public List<CreateDocumentoReferenciaDTO>? Referencias { get; set; }

    // Medios de pago adicionales (para pagos mixtos)
    public List<CreateDocumentoMedioPagoDTO>? MediosPago { get; set; }

    // Otra información (key-value pairs)
    public List<CreateDocumentoOtraInformacionDTO>? OtraInformacion { get; set; }

    // Información de exportación (solo para FEE)
    public CreateDocumentoExportacionDTO? Exportacion { get; set; }
}

/// <summary>
/// DTO para crear una línea de detalle del documento
/// </summary>
public class CreateDocumentoDetalleDTO
{
    public int NumeroLinea { get; set; }
    public Guid? ProductoId { get; set; }

    [MaxLength(2)]
    public string? TipoCodigo { get; set; }

    [MaxLength(50)]
    public string? Codigo { get; set; }

    [MaxLength(13)]
    public string? CodigoCabys { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(500)]
    public string Descripcion { get; set; } = null!;

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal Cantidad { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int UnidadMedidaId { get; set; }

    [MaxLength(100)]
    public string? UnidadMedidaComercial { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal PrecioUnitario { get; set; }

    // Descuentos de la línea
    public List<CreateDocumentoDetalleDescuentoDTO>? Descuentos { get; set; }

    // Impuestos de la línea
    public List<CreateDocumentoDetalleImpuestoDTO>? Impuestos { get; set; }

    // Campos específicos v4.4
    [MaxLength(2)]
    public string? TipoTransaccion { get; set; }

    [MaxLength(1000)]
    public string? DetalleSurtido { get; set; }

    [MaxLength(20)]
    public string? NumeroPartidaArancelaria { get; set; }

    [MaxLength(50)]
    public string? NumeroRegistroMedicamento { get; set; }

    [MaxLength(2)]
    public string? FormaFarmaceutica { get; set; }

    [MaxLength(50)]
    public string? NumeroVIN { get; set; }
}

/// <summary>
/// DTO para crear un descuento de documento
/// </summary>
public class CreateDocumentoDescuentoDTO
{
    [Required]
    [MaxLength(100)]
    public string Naturaleza { get; set; } = null!;

    [Required]
    public decimal Monto { get; set; }
}

/// <summary>
/// DTO para crear un descuento de línea de detalle
/// </summary>
public class CreateDocumentoDetalleDescuentoDTO
{
    [Required]
    [MaxLength(100)]
    public string Naturaleza { get; set; } = null!;

    [Required]
    public decimal Monto { get; set; }
}

/// <summary>
/// DTO para crear un impuesto de línea de detalle
/// </summary>
public class CreateDocumentoDetalleImpuestoDTO
{
    [Required]
    [MaxLength(2)]
    public string CodigoTarifa { get; set; } = null!;

    [Required]
    public decimal Tarifa { get; set; }

    [Required]
    public decimal Monto { get; set; }

    public decimal? FactorIVA { get; set; }

    public decimal? MontoExportacion { get; set; }
}

/// <summary>
/// DTO para crear una referencia a otro documento
/// </summary>
public class CreateDocumentoReferenciaDTO
{
    [Required]
    public TipoReferenciaDocumento TipoReferencia { get; set; }

    [Required]
    [MaxLength(50)]
    public string NumeroDocumento { get; set; } = null!;

    public DateTime? FechaDocumento { get; set; }

    [MaxLength(2)]
    public string? CodigoReferencia { get; set; }

    [MaxLength(200)]
    public string? Razon { get; set; }
}

/// <summary>
/// DTO para crear un medio de pago
/// </summary>
public class CreateDocumentoMedioPagoDTO
{
    [Required]
    [MaxLength(2)]
    public string CodigoMedioPago { get; set; } = null!;

    [Required]
    public decimal Monto { get; set; }

    /// <summary>
    /// Fecha de pago - Requerido para REP según v4.4
    /// </summary>
    public DateTime? FechaPago { get; set; }
}

/// <summary>
/// DTO para crear otra información (key-value)
/// </summary>
public class CreateDocumentoOtraInformacionDTO
{
    [Required]
    [MaxLength(100)]
    public string Clave { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string Valor { get; set; } = null!;
}

/// <summary>
/// DTO para crear información de exportación
/// </summary>
public class CreateDocumentoExportacionDTO
{
    [MaxLength(100)]
    public string? NombreConsignatario { get; set; }

    [MaxLength(500)]
    public string? DireccionConsignatario { get; set; }

    [MaxLength(100)]
    public string? NombreComprador { get; set; }

    [MaxLength(500)]
    public string? DireccionComprador { get; set; }

    [MaxLength(3)]
    public string? CodigoPaisDestino { get; set; }

    [MaxLength(100)]
    public string? NombrePaisDestino { get; set; }

    [MaxLength(100)]
    public string? IncotermVenta { get; set; }
}
