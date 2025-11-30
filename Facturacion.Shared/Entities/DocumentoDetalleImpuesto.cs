using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Impuestos aplicados a una línea de detalle del documento
/// Una línea puede tener múltiples impuestos (IVA, Consumo, etc.)
/// Según Hacienda v4.4
/// </summary>
public class DocumentoDetalleImpuesto
{
    [Key]
    public Guid Id { get; set; }

    // ========================================
    // RELACIÓN CON DETALLE
    // ========================================

    [Display(Name = "Detalle del Documento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid DocumentoDetalleId { get; set; }

    // ========================================
    // TIPO DE IMPUESTO
    // ========================================

    /// <summary>
    /// Relación con el catálogo de impuestos de Hacienda
    /// </summary>
    [Display(Name = "Impuesto")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int ImpuestoId { get; set; }

    /// <summary>
    /// Código del impuesto según catálogo Hacienda
    /// 01-IVA Tarifa General, 02-IVA Tarifa Reducida, 03-IVA Tarifa Cero,
    /// 04-Exento, 05-Ventas sin IVA, 06-No sujeto, 07-Impuesto de Consumo,
    /// 08-IVA Generado por Compras Autorizadas, etc.
    /// </summary>
    [Display(Name = "Código Impuesto")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string CodigoImpuesto { get; set; } = null!;

    /// <summary>
    /// Código de tarifa del impuesto (01-Tarifa 0%, 02-Tarifa reducida 1%,
    /// 03-Tarifa reducida 2%, 04-Tarifa reducida 4%, 05-Transitorio 5%,
    /// 06-Transitorio 10%, 07-Transitorio 15%, 08-Tarifa general 13%, etc.)
    /// </summary>
    [Display(Name = "Código Tarifa")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string CodigoTarifa { get; set; } = null!;

    /// <summary>
    /// Porcentaje de la tarifa (hasta 2 decimales)
    /// Ejemplo: 13.00 para IVA tarifa general
    /// </summary>
    [Display(Name = "Tarifa (%)")]
    [Column(TypeName = "decimal(5, 2)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal Tarifa { get; set; }

    // ========================================
    // FACTOR DE IVA DEVUELTO (CASOS ESPECIALES)
    // ========================================

    /// <summary>
    /// Factor de IVA devuelto (para casos específicos de devolución de IVA)
    /// Generalmente se usa en casos de turismo o exportación
    /// </summary>
    [Display(Name = "Factor IVA Devuelto")]
    [Column(TypeName = "decimal(5, 2)")]
    public decimal? FactorIVADevuelto { get; set; }

    // ========================================
    // MONTOS
    // ========================================

    /// <summary>
    /// Monto base sobre el que se calcula el impuesto (base imponible)
    /// </summary>
    [Display(Name = "Monto Base")]
    [Column(TypeName = "decimal(18, 5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal MontoBase { get; set; }

    /// <summary>
    /// Monto del impuesto calculado
    /// Cálculo: MontoBase * (Tarifa / 100)
    /// </summary>
    [Display(Name = "Monto Impuesto")]
    [Column(TypeName = "decimal(18, 5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal MontoImpuesto { get; set; }

    // ========================================
    // EXONERACIÓN (OPCIONAL)
    // ========================================

    /// <summary>
    /// Indica si esta línea de impuesto tiene exoneración
    /// </summary>
    [Display(Name = "Tiene Exoneración")]
    public bool TieneExoneracion { get; set; }

    /// <summary>
    /// Tipo de documento de exoneración (01-Compras Autorizadas,
    /// 02-Ventas exentas a diplomáticos, 03-Orden de compra,
    /// 04-Exoneraciones Ley 7293, 05-Exoneración Dirección de Aviación Civil,
    /// 06-Exoneración Instituciones Religiosas, 99-Otros)
    /// </summary>
    [Display(Name = "Tipo Documento Exoneración")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? TipoDocumentoExoneracion { get; set; }

    /// <summary>
    /// Número del documento de exoneración
    /// </summary>
    [Display(Name = "Número Documento Exoneración")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroDocumentoExoneracion { get; set; }

    /// <summary>
    /// Nombre de la institución que otorga la exoneración
    /// </summary>
    [Display(Name = "Institución Exoneración")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? InstitucionExoneracion { get; set; }

    /// <summary>
    /// Fecha de emisión del documento de exoneración
    /// </summary>
    [Display(Name = "Fecha Emisión Exoneración")]
    public DateTime? FechaEmisionExoneracion { get; set; }

    /// <summary>
    /// Monto exonerado
    /// </summary>
    [Display(Name = "Monto Exonerado")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal? MontoExoneracion { get; set; }

    /// <summary>
    /// Porcentaje de exoneración del impuesto (hasta 2 decimales)
    /// </summary>
    [Display(Name = "Porcentaje Exoneración")]
    [Column(TypeName = "decimal(5, 2)")]
    public decimal? PorcentajeExoneracion { get; set; }

    // ========================================
    // AUDIT TRAIL
    // ========================================

    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Usuario Creación")]
    public string? UsuarioCreacionId { get; set; }

    [Display(Name = "Fecha de Modificación")]
    public DateTime? FechaModificacion { get; set; }

    [Display(Name = "Usuario Modificación")]
    public string? UsuarioModificacionId { get; set; }

    // ========================================
    // SOFT DELETE
    // ========================================

    [Display(Name = "Eliminado")]
    public bool IsDeleted { get; set; }

    [Display(Name = "Fecha de Eliminación")]
    public DateTime? FechaEliminacion { get; set; }

    [Display(Name = "Usuario Eliminación")]
    public string? UsuarioEliminacionId { get; set; }

    // ========================================
    // NAVIGATION PROPERTIES
    // ========================================

    public DocumentoDetalle? DocumentoDetalle { get; set; }
    public Catalogos.Impuesto? Impuesto { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }
}
