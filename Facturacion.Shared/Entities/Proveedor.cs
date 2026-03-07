using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

public class Proveedor
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

    [Display(Name = "Otras Señas")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? OtrasSenas { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; }

    // =====================================================
    // Campos Comerciales/Compras (Integración ERP)
    // =====================================================

    [Display(Name = "Días de Crédito")]
    public int DiasCredito { get; set; }

    [Display(Name = "Celular")]
    [MaxLength(20)]
    public string? Celular { get; set; }

    [Display(Name = "Sitio Web")]
    [MaxLength(200)]
    public string? SitioWeb { get; set; }

    // Datos bancarios
    [Display(Name = "Banco")]
    [MaxLength(100)]
    public string? Banco { get; set; }

    [Display(Name = "Cuenta Bancaria")]
    [MaxLength(30)]
    public string? CuentaBancaria { get; set; }

    [Display(Name = "Tipo de Cuenta")]
    [MaxLength(20)]
    public string? TipoCuentaBancaria { get; set; }

    [Display(Name = "IBAN")]
    [MaxLength(30)]
    public string? IBAN { get; set; }

    // Crédito
    [Display(Name = "Límite de Crédito")]
    public decimal LimiteCredito { get; set; }

    [Display(Name = "Saldo Pendiente")]
    public decimal SaldoPendiente { get; set; }

    [Display(Name = "Fecha Último Pago")]
    public DateTime? FechaUltimoPago { get; set; }

    [Display(Name = "Fecha Última Compra")]
    public DateTime? FechaUltimaCompra { get; set; }

    // Comercial
    [Display(Name = "Categoría")]
    [MaxLength(50)]
    public string? Categoria { get; set; }

    [Display(Name = "Productos/Servicios")]
    [MaxLength(500)]
    public string? ProductosServicios { get; set; }

    [Display(Name = "Tiempo de Entrega (días)")]
    public int TiempoEntrega { get; set; }

    [Display(Name = "Pedido Mínimo")]
    public decimal PedidoMinimo { get; set; }

    [Display(Name = "Descuento General %")]
    public decimal DescuentoGeneral { get; set; }

    // Extranjero
    [Display(Name = "Es Extranjero")]
    public bool EsExtranjero { get; set; }

    [Display(Name = "País")]
    [MaxLength(50)]
    public string? Pais { get; set; }

    // Contacto de compras
    [Display(Name = "Contacto de Compras")]
    [MaxLength(100)]
    public string? ContactoCompras { get; set; }

    [Display(Name = "Teléfono del Contacto")]
    [MaxLength(20)]
    public string? TelefonoContacto { get; set; }

    [Display(Name = "Email del Contacto")]
    [MaxLength(100)]
    [EmailAddress]
    public string? EmailContacto { get; set; }

    // Régimen Tributario
    [Display(Name = "Régimen Simplificado")]
    public bool EsRegimenSimplificado { get; set; }

    // Retenciones
    [Display(Name = "Retención IVA %")]
    public decimal RetencionIVA { get; set; }

    [Display(Name = "Retención Renta %")]
    public decimal RetencionRenta { get; set; }

    // Gestión
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
