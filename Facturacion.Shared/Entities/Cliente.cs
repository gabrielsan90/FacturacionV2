using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

public class Cliente
{
    [Key]
    public Guid Id { get; set; }

    [Display(Name = "Empresa")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid EmpresaId { get; set; }

    [Display(Name = "Tipo de Identificación")]
    public TipoIdentificacion TipoIdentificacion { get; set; }

    [Display(Name = "Número de Identificación")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string NumeroIdentificacion { get; set; } = null!;

    [Display(Name = "Nombre")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Nombre Comercial")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NombreComercial { get; set; }

    [Display(Name = "Email Principal")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [EmailAddress(ErrorMessage = "El campo {0} debe ser un email válido.")]
    public string? EmailPrincipal { get; set; }

    [Display(Name = "Teléfono Principal")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? TelefonoPrincipal { get; set; }

    [Display(Name = "Provincia")]
    public int Provincia { get; set; }

    [Display(Name = "Cantón")]
    public int Canton { get; set; }

    [Display(Name = "Distrito")]
    public int Distrito { get; set; }

    /// <summary>
    /// v4.4: Barrio (opcional en XSD, minLength=5, maxLength=50)
    /// Texto descriptivo del barrio, no código numérico
    /// </summary>
    [Display(Name = "Barrio")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Barrio { get; set; }

    [Display(Name = "Otras Señas")]
    [MaxLength(250, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? OtrasSenas { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; }

    [Display(Name = "Saldo Actual")]
    public decimal SaldoActual { get; set; }

    [Display(Name = "Límite de Crédito")]
    public decimal LimiteCredito { get; set; }

    [Display(Name = "Fecha de Registro")]
    public DateTime FechaRegistro { get; set; }

    [Display(Name = "Moneda")]
    public TipoMoneda Moneda { get; set; }

    [Display(Name = "Tipo de Documento")]
    public int TipoDocumento { get; set; }

    [Display(Name = "Tipo de Venta")]
    public int TipoVenta { get; set; }

    [Display(Name = "Tipo de Pago")]
    public int TipoPago { get; set; }

    [Display(Name = "Actividad Económica CIIU v4")]
    [MaxLength(10, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ActividadEconomicaCIIU4 { get; set; }

    [Display(Name = "Actividad Económica CIIU v3")]
    [MaxLength(10, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ActividadEconomicaCIIU3 { get; set; }

    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Usuario Creación")]
    public string? UsuarioCreacionId { get; set; }

    [Display(Name = "Fecha de Modificación")]
    public DateTime? FechaModificacion { get; set; }

    [Display(Name = "Usuario Modificación")]
    public string? UsuarioModificacionId { get; set; }

    // Soft Delete
    [Display(Name = "Eliminado")]
    public bool IsDeleted { get; set; }

    [Display(Name = "Fecha de Eliminación")]
    public DateTime? FechaEliminacion { get; set; }

    [Display(Name = "Usuario Eliminación")]
    public string? UsuarioEliminacionId { get; set; }

    // Navegación
    public Empresa? Empresa { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<Telefono>? Telefonos { get; set; } = new List<Telefono>();
    public ICollection<Email>? Emails { get; set; } = new List<Email>();
    public ICollection<Documento>? Documentos { get; set; } = new List<Documento>();
}
