using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Líneas de detalle de un documento electrónico según Hacienda v4.4
/// Representa productos o servicios incluidos en el documento
/// </summary>
public class DocumentoDetalle
{
    [Key]
    public Guid Id { get; set; }

    // ========================================
    // RELACIÓN CON DOCUMENTO
    // ========================================

    [Display(Name = "Documento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid DocumentoId { get; set; }

    // ========================================
    // NÚMERO DE LÍNEA
    // ========================================

    /// <summary>
    /// Número secuencial de la línea (1, 2, 3, ...)
    /// </summary>
    [Display(Name = "Número de Línea")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int NumeroLinea { get; set; }

    // ========================================
    // PRODUCTO/SERVICIO
    // ========================================

    /// <summary>
    /// Relación con el catálogo de productos (opcional, puede ser producto libre)
    /// </summary>
    [Display(Name = "Producto")]
    public Guid? ProductoId { get; set; }

    /// <summary>
    /// Tipo de código (01-Código del producto del vendedor, 02-Código del comprador,
    /// 03-Código asignado por la industria, 04-Código uso interno, 99-Otros)
    /// </summary>
    [Display(Name = "Tipo de Código")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? TipoCodigo { get; set; }

    /// <summary>
    /// Código del producto o servicio (código comercial, SKU, etc.)
    /// </summary>
    [Display(Name = "Código")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Codigo { get; set; }

    /// <summary>
    /// OBLIGATORIO desde 01/06/2025: Código de producto/servicio CAByS (13 dígitos)
    /// Clasificador de Actividades y Bienes y Servicios
    /// </summary>
    [Display(Name = "Código CAByS")]
    [MaxLength(13, ErrorMessage = "El campo {0} debe tener exactamente {1} caracteres.")]
    [MinLength(13, ErrorMessage = "El campo {0} debe tener exactamente {1} caracteres.")]
    public string? CodigoCabys { get; set; }

    /// <summary>
    /// Descripción detallada del producto o servicio
    /// </summary>
    [Display(Name = "Descripción")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Descripcion { get; set; } = null!;

    // ========================================
    // CANTIDAD Y UNIDAD
    // ========================================

    /// <summary>
    /// Cantidad del producto o servicio (hasta 3 decimales según Hacienda)
    /// </summary>
    [Display(Name = "Cantidad")]
    [Column(TypeName = "decimal(18, 3)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal Cantidad { get; set; }

    /// <summary>
    /// Código de unidad de medida según catálogo Hacienda
    /// </summary>
    [Display(Name = "Unidad de Medida")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int UnidadMedidaId { get; set; }

    /// <summary>
    /// Unidad de medida comercial (texto libre, ejemplo: "Caja de 12 unidades")
    /// </summary>
    [Display(Name = "Unidad Medida Comercial")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? UnidadMedidaComercial { get; set; }

    // ========================================
    // PRECIOS (HASTA 5 DECIMALES)
    // ========================================

    /// <summary>
    /// Precio unitario del producto/servicio (hasta 5 decimales)
    /// </summary>
    [Display(Name = "Precio Unitario")]
    [Column(TypeName = "decimal(18, 5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Monto total de la línea antes de impuestos y descuentos
    /// Cálculo: Cantidad * PrecioUnitario
    /// </summary>
    [Display(Name = "Monto Total")]
    [Column(TypeName = "decimal(18, 5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal MontoTotal { get; set; }

    // ========================================
    // DESCUENTOS A NIVEL DE LÍNEA
    // ========================================

    /// <summary>
    /// Monto total de descuentos aplicados a esta línea
    /// </summary>
    [Display(Name = "Monto Descuento")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal MontoDescuento { get; set; }

    /// <summary>
    /// Naturaleza del descuento (texto descriptivo)
    /// </summary>
    [Display(Name = "Naturaleza Descuento")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NaturalezaDescuento { get; set; }

    /// <summary>
    /// Subtotal de la línea después de descuentos, antes de impuestos
    /// Cálculo: MontoTotal - MontoDescuento
    /// </summary>
    [Display(Name = "Subtotal")]
    [Column(TypeName = "decimal(18, 5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal Subtotal { get; set; }

    // ========================================
    // BASE IMPONIBLE
    // ========================================

    /// <summary>
    /// Base imponible para el cálculo de impuestos
    /// Generalmente igual a Subtotal, pero puede variar en casos especiales
    /// </summary>
    [Display(Name = "Base Imponible")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal? BaseImponible { get; set; }

    // ========================================
    // IMPUESTOS
    // ========================================

    /// <summary>
    /// Monto total de impuestos aplicados a esta línea
    /// Suma de todos los impuestos (IVA, consumo, etc.)
    /// </summary>
    [Display(Name = "Monto Impuesto")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal MontoImpuesto { get; set; }

    /// <summary>
    /// Impuesto neto de la línea (puede incluir ajustes)
    /// </summary>
    [Display(Name = "Impuesto Neto")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal? ImpuestoNeto { get; set; }

    /// <summary>
    /// Monto total de la línea incluyendo impuestos
    /// Cálculo: Subtotal + MontoImpuesto
    /// </summary>
    [Display(Name = "Monto Total Línea")]
    [Column(TypeName = "decimal(18, 5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal MontoTotalLinea { get; set; }

    // ========================================
    // INFORMACIÓN ADICIONAL PARA PRODUCTOS ESPECÍFICOS
    // ========================================

    /// <summary>
    /// Número de partida arancelaria (para productos importados)
    /// </summary>
    [Display(Name = "Número de Partida Arancelaria")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroPartidaArancelaria { get; set; }

    /// <summary>
    /// OBLIGATORIO desde 01/12/2024 para productos farmacéuticos:
    /// Número de registro del Ministerio de Salud
    /// </summary>
    [Display(Name = "Número de Registro Medicamento")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroRegistroMedicamento { get; set; }

    /// <summary>
    /// OBLIGATORIO desde 01/12/2024 para productos farmacéuticos:
    /// Forma farmacéutica (01-Tableta, 02-Cápsula, 03-Jarabe, etc.)
    /// </summary>
    [Display(Name = "Forma Farmacéutica")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? FormaFarmaceutica { get; set; }

    /// <summary>
    /// OBLIGATORIO para vehículos: Número VIN (Vehicle Identification Number)
    /// </summary>
    [Display(Name = "Número VIN")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroVIN { get; set; }

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

    public Documento? Documento { get; set; }
    public Producto? Producto { get; set; }
    public Catalogos.UnidadMedida? UnidadMedida { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }

    /// <summary>
    /// Impuestos aplicados a esta línea de detalle
    /// Una línea puede tener múltiples impuestos (IVA 13%, Consumo 5%, etc.)
    /// </summary>
    public ICollection<DocumentoDetalleImpuesto> Impuestos { get; set; } = new List<DocumentoDetalleImpuesto>();

    /// <summary>
    /// Descuentos aplicados a esta línea de detalle
    /// Permite múltiples descuentos por línea
    /// </summary>
    public ICollection<DocumentoDetalleDescuento> Descuentos { get; set; } = new List<DocumentoDetalleDescuento>();
}
