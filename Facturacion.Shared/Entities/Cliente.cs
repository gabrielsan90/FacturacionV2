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

    // =====================================================
    // Campos CRM/Comerciales (Integración ERP)
    // =====================================================

    [Display(Name = "Días de Crédito")]
    public int DiasCredito { get; set; }

    [Display(Name = "Exento de IVA")]
    public bool ExentoIVA { get; set; }

    // Exoneración (v4.4)
    [Display(Name = "Tipo de Exoneración")]
    [MaxLength(2)]
    public string? TipoExoneracion { get; set; }

    [Display(Name = "Número de Exoneración")]
    [MaxLength(40)]
    public string? NumeroExoneracion { get; set; }

    [Display(Name = "Institución Exoneración")]
    [MaxLength(160)]
    public string? NombreInstitucionExoneracion { get; set; }

    [Display(Name = "Fecha de Exoneración")]
    public DateTime? FechaExoneracion { get; set; }

    [Display(Name = "Porcentaje de Exoneración")]
    public decimal PorcentajeExoneracion { get; set; }

    // Comercial
    [Display(Name = "Descuento General %")]
    public decimal DescuentoGeneral { get; set; }

    [Display(Name = "Fecha Último Pago")]
    public DateTime? FechaUltimoPago { get; set; }

    [Display(Name = "Fecha Última Compra")]
    public DateTime? FechaUltimaCompra { get; set; }

    [Display(Name = "Categoría")]
    [MaxLength(50)]
    public string? Categoria { get; set; }

    [Display(Name = "Requiere Orden de Compra")]
    public bool RequiereOrdenCompra { get; set; }

    // Contacto
    [Display(Name = "Contacto Principal")]
    [MaxLength(100)]
    public string? Contacto { get; set; }

    [Display(Name = "Teléfono del Contacto")]
    [MaxLength(20)]
    public string? TelefonoContacto { get; set; }

    // Gestión
    [Display(Name = "Zona")]
    [MaxLength(50)]
    public string? Zona { get; set; }

    [Display(Name = "En Mora")]
    public bool EnMora { get; set; }

    [Display(Name = "Bloqueado")]
    public bool Bloqueado { get; set; }

    [Display(Name = "Motivo de Bloqueo")]
    [MaxLength(200)]
    public string? MotivoBloqueo { get; set; }

    [Display(Name = "Notas")]
    [MaxLength(500)]
    public string? Notas { get; set; }

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
