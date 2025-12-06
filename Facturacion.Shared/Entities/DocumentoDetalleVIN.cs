using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Números VIN (Vehicle Identification Number) asociados a una línea de detalle
/// Según v4.4, una línea puede tener hasta 1000 números VIN para vehículos
/// </summary>
public class DocumentoDetalleVIN
{
    [Key]
    public Guid Id { get; set; }

    // ========================================
    // RELACIÓN CON DOCUMENTO DETALLE
    // ========================================

    [Display(Name = "Documento Detalle")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid DocumentoDetalleId { get; set; }

    // ========================================
    // NÚMERO VIN
    // ========================================

    /// <summary>
    /// Número VIN del vehículo (Vehicle Identification Number)
    /// Código alfanumérico de 17 caracteres que identifica de forma única un vehículo
    /// </summary>
    [Display(Name = "Número VIN")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string NumeroVIN { get; set; } = null!;

    /// <summary>
    /// Número de orden del VIN en la lista (1, 2, 3, ... hasta 1000)
    /// </summary>
    [Display(Name = "Número de Orden")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int NumeroOrden { get; set; }

    // ========================================
    // AUDIT TRAIL
    // ========================================

    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Usuario Creación")]
    public string? UsuarioCreacionId { get; set; }

    // ========================================
    // NAVIGATION PROPERTIES
    // ========================================

    public DocumentoDetalle? DocumentoDetalle { get; set; }
    public User? UsuarioCreacion { get; set; }
}
