using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Notas asociadas a una oportunidad de venta CRM.
/// </summary>
public class NotaOportunidad
{
    [Key]
    public Guid Id { get; set; }

    // =====================================================
    // Relación con oportunidad
    // =====================================================
    [Display(Name = "Oportunidad")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid OportunidadId { get; set; }

    // =====================================================
    // Contenido
    // =====================================================
    [Display(Name = "Contenido")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Contenido { get; set; } = null!;

    [Display(Name = "Es Importante")]
    public bool EsImportante { get; set; }

    // =====================================================
    // Audit Trail
    // =====================================================
    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Usuario Creación")]
    public string? UsuarioCreacionId { get; set; }

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Oportunidad? Oportunidad { get; set; }
    public User? UsuarioCreacion { get; set; }
}
