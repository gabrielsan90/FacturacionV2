using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Catálogo de bancos para gestión de cuentas bancarias y pagos.
/// Incluye bancos nacionales de Costa Rica y bancos extranjeros.
/// </summary>
public class Banco
{
    [Key]
    public Guid Id { get; set; }

    // =====================================================
    // Empresa (multi-tenant)
    // =====================================================
    [Display(Name = "Empresa")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid EmpresaId { get; set; }

    // =====================================================
    // Datos principales
    // =====================================================
    [Display(Name = "Código")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(10, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Codigo { get; set; } = null!;

    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Nombre Corto")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NombreCorto { get; set; }

    [Display(Name = "Código SINPE")]
    [MaxLength(10, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? CodigoSINPE { get; set; }

    [Display(Name = "Código SWIFT/BIC")]
    [MaxLength(11, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? CodigoSWIFT { get; set; }

    // =====================================================
    // Clasificación
    // =====================================================
    [Display(Name = "Es Nacional")]
    public bool EsNacional { get; set; } = true;

    [Display(Name = "Es Estatal")]
    public bool EsEstatal { get; set; }

    [Display(Name = "País")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Pais { get; set; }

    // =====================================================
    // Contacto
    // =====================================================
    [Display(Name = "Sitio Web")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? SitioWeb { get; set; }

    [Display(Name = "Teléfono")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Telefono { get; set; }

    // =====================================================
    // Control
    // =====================================================
    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    // =====================================================
    // Audit Trail
    // =====================================================
    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Usuario Creación")]
    public string? UsuarioCreacionId { get; set; }

    [Display(Name = "Fecha de Modificación")]
    public DateTime? FechaModificacion { get; set; }

    [Display(Name = "Usuario Modificación")]
    public string? UsuarioModificacionId { get; set; }

    // =====================================================
    // Soft Delete
    // =====================================================
    [Display(Name = "Eliminado")]
    public bool IsDeleted { get; set; }

    [Display(Name = "Fecha de Eliminación")]
    public DateTime? FechaEliminacion { get; set; }

    [Display(Name = "Usuario Eliminación")]
    public string? UsuarioEliminacionId { get; set; }

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }

    // Colecciones relacionadas (para futuras fases)
    // public ICollection<CuentaBancaria>? CuentasBancarias { get; set; }
}
