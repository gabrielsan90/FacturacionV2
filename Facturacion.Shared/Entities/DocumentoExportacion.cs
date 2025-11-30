using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Información específica para Factura Electrónica de Exportación (FEE - tipo 09)
/// Campos adicionales requeridos únicamente para exportaciones
/// Según Hacienda v4.4
/// </summary>
public class DocumentoExportacion
{
    [Key]
    public Guid Id { get; set; }

    // ========================================
    // RELACIÓN CON DOCUMENTO (1:1)
    // ========================================

    [Display(Name = "Documento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid DocumentoId { get; set; }

    // ========================================
    // RECEPTOR (COMPRADOR EXTRANJERO)
    // ========================================

    /// <summary>
    /// Nombre del comprador extranjero
    /// </summary>
    [Display(Name = "Nombre del Comprador")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string NombreComprador { get; set; } = null!;

    /// <summary>
    /// Número de identificación del comprador extranjero
    /// Puede ser Tax ID, VAT, NIF, etc. según el país
    /// </summary>
    [Display(Name = "Identificación del Comprador")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? IdentificacionComprador { get; set; }

    // ========================================
    // UBICACIÓN DEL COMPRADOR
    // ========================================

    /// <summary>
    /// País del comprador (código ISO 3166-1 alpha-2)
    /// Ejemplo: US, MX, PA, ES, etc.
    /// </summary>
    [Display(Name = "País")]
    [MaxLength(2, ErrorMessage = "El campo {0} debe ser un código de país de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Pais { get; set; } = null!;

    /// <summary>
    /// Dirección completa del comprador
    /// </summary>
    [Display(Name = "Dirección")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Direccion { get; set; }

    // ========================================
    // DATOS DE EXPORTACIÓN
    // ========================================

    /// <summary>
    /// Código del Incoterm utilizado en la exportación
    /// EXW, FCA, FAS, FOB, CFR, CIF, CPT, CIP, DAP, DPU, DDP
    /// </summary>
    [Display(Name = "Incoterm")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Incoterm { get; set; }

    /// <summary>
    /// Descripción o lugar del Incoterm
    /// Ejemplo: "FOB Puerto Limón", "CIF Puerto de Miami"
    /// </summary>
    [Display(Name = "Descripción Incoterm")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? DescripcionIncoterm { get; set; }

    /// <summary>
    /// Número de la Declaración Única Aduanera (DUA) o documento de exportación
    /// </summary>
    [Display(Name = "Número DUA")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroDUA { get; set; }

    /// <summary>
    /// Fecha de la DUA o documento de exportación
    /// </summary>
    [Display(Name = "Fecha DUA")]
    public DateTime? FechaDUA { get; set; }

    // ========================================
    // INFORMACIÓN DE TRANSPORTE
    // ========================================

    /// <summary>
    /// Medio de transporte utilizado
    /// 01-Marítimo, 02-Aéreo, 03-Terrestre, 04-Ferroviario, 05-Postal, 99-Otro
    /// </summary>
    [Display(Name = "Medio de Transporte")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? MedioTransporte { get; set; }

    /// <summary>
    /// Nombre de la empresa transportista
    /// </summary>
    [Display(Name = "Transportista")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Transportista { get; set; }

    /// <summary>
    /// Número de contenedor, vuelo, placa del vehículo, etc.
    /// </summary>
    [Display(Name = "Número de Transporte")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroTransporte { get; set; }

    /// <summary>
    /// Puerto, aeropuerto o aduana de salida de Costa Rica
    /// </summary>
    [Display(Name = "Puerto/Aduana de Salida")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? PuertoSalida { get; set; }

    /// <summary>
    /// Puerto, aeropuerto o aduana de destino en el país extranjero
    /// </summary>
    [Display(Name = "Puerto/Aduana de Destino")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? PuertoDestino { get; set; }

    // ========================================
    // INFORMACIÓN COMERCIAL
    // ========================================

    /// <summary>
    /// Número de factura comercial (puede ser diferente al número consecutivo)
    /// </summary>
    [Display(Name = "Número Factura Comercial")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroFacturaComercial { get; set; }

    /// <summary>
    /// Número de la lista de empaque (packing list)
    /// </summary>
    [Display(Name = "Número de Packing List")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroPackingList { get; set; }

    /// <summary>
    /// Número del conocimiento de embarque (Bill of Lading para marítimo o Air Waybill para aéreo)
    /// </summary>
    [Display(Name = "Número BL/AWB")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroBL { get; set; }

    // ========================================
    // TOTALES EN MONEDA EXTRANJERA
    // ========================================

    /// <summary>
    /// Total FOB (Free On Board) en la moneda de la exportación
    /// </summary>
    [Display(Name = "Total FOB")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal? TotalFOB { get; set; }

    /// <summary>
    /// Costo de flete internacional
    /// </summary>
    [Display(Name = "Flete")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal? Flete { get; set; }

    /// <summary>
    /// Costo del seguro internacional
    /// </summary>
    [Display(Name = "Seguro")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal? Seguro { get; set; }

    /// <summary>
    /// Total CIF (Cost, Insurance and Freight) en la moneda de la exportación
    /// </summary>
    [Display(Name = "Total CIF")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal? TotalCIF { get; set; }

    // ========================================
    // OBSERVACIONES
    // ========================================

    /// <summary>
    /// Observaciones o notas adicionales específicas de la exportación
    /// </summary>
    [Display(Name = "Observaciones")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

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
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }
}
