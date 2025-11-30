using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

public class Empresa
{
    [Key]
    public Guid Id { get; set; }

    [Display(Name = "Tipo de Identificación")]
    public TipoIdentificacion TipoIdentificacion { get; set; }

    [Display(Name = "Número de Identificación")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string NumeroIdentificacion { get; set; } = null!;

    [Display(Name = "Nombre Comercial")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string NombreComercial { get; set; } = null!;

    [Display(Name = "Razón Social")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string RazonSocial { get; set; } = null!;

    [Display(Name = "Provincia")]
    public int Provincia { get; set; }

    [Display(Name = "Cantón")]
    public int Canton { get; set; }

    [Display(Name = "Distrito")]
    public int Distrito { get; set; }

    [Display(Name = "Otras Señas")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? OtrasSenas { get; set; }

    [Display(Name = "Logo")]
    public string? Logo { get; set; }

    // ========================================
    // HACIENDA - FIRMA DIGITAL
    // ========================================

    /// <summary>
    /// Certificado digital en formato PKCS#12 (.p12/.pfx)
    /// Incluye clave pública y privada para firma XAdES-BES
    /// </summary>
    [Display(Name = "Certificado Digital")]
    public byte[]? CertificadoDigital { get; set; }

    /// <summary>
    /// PIN o contraseña del certificado digital
    /// Requerido para desencriptar la clave privada
    /// </summary>
    [Display(Name = "PIN Certificado")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? PinCertificado { get; set; }

    // ========================================
    // HACIENDA - CREDENCIALES ATV
    // ========================================

    /// <summary>
    /// Usuario de la API de Hacienda (ATV - Administración Tributaria Virtual)
    /// Se obtiene del Ministerio de Hacienda
    /// </summary>
    [Display(Name = "Usuario ATV")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? UsuarioHacienda { get; set; }

    /// <summary>
    /// Contraseña de la API de Hacienda (ATV)
    /// Se obtiene del Ministerio de Hacienda
    /// </summary>
    [Display(Name = "Clave ATV")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ClaveHacienda { get; set; }

    /// <summary>
    /// Ambiente de Hacienda: Pruebas (stag) o Producción (prod)
    /// Por defecto inicia en Pruebas
    /// </summary>
    [Display(Name = "Ambiente Hacienda")]
    public Ambiente Ambiente { get; set; } = Ambiente.Pruebas;

    [Display(Name = "Servidor SMTP")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ServidorSMTP { get; set; }

    [Display(Name = "Puerto SMTP")]
    public int? PuertoSMTP { get; set; }

    [Display(Name = "Usuario SMTP")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? UsuarioSMTP { get; set; }

    [Display(Name = "Clave SMTP")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ClaveSMTP { get; set; }

    [Display(Name = "Activa")]
    public bool Activa { get; set; }

    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Usuario Creación")]
    public string? UsuarioCreacionId { get; set; }

    [Display(Name = "Fecha de Modificación")]
    public DateTime? FechaModificacion { get; set; }

    [Display(Name = "Usuario Modificación")]
    public string? UsuarioModificacionId { get; set; }

    // Soft Delete
    [Display(Name = "Eliminada")]
    public bool IsDeleted { get; set; }

    [Display(Name = "Fecha de Eliminación")]
    public DateTime? FechaEliminacion { get; set; }

    [Display(Name = "Usuario Eliminación")]
    public string? UsuarioEliminacionId { get; set; }

    // Navegación
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<Telefono>? Telefonos { get; set; } = new List<Telefono>();
    public ICollection<Email>? Emails { get; set; } = new List<Email>();
    public ICollection<EmpresaActividadEconomica>? ActividadesEconomicas { get; set; } = new List<EmpresaActividadEconomica>();
    public ICollection<UsuarioEmpresa>? Usuarios { get; set; } = new List<UsuarioEmpresa>();
    public ICollection<Cliente>? Clientes { get; set; } = new List<Cliente>();
    public ICollection<Proveedor>? Proveedores { get; set; } = new List<Proveedor>();
    public ICollection<Categoria>? Categorias { get; set; } = new List<Categoria>();
    public ICollection<Producto>? Productos { get; set; } = new List<Producto>();
    public ICollection<Sucursal>? Sucursales { get; set; } = new List<Sucursal>();
    public ICollection<Documento>? Documentos { get; set; } = new List<Documento>();
}
