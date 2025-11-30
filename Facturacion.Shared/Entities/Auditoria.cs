using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Entidad para el registro completo de auditoría del sistema
/// Registra TODAS las acciones realizadas por los usuarios
/// Recomendación 12 de Hacienda v4.4
/// </summary>
public class Auditoria
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// ID de la empresa donde ocurrió la acción (multi-tenant)
    /// </summary>
    [Display(Name = "Empresa")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid EmpresaId { get; set; }

    /// <summary>
    /// ID del usuario que realizó la acción
    /// </summary>
    [Display(Name = "Usuario")]
    [MaxLength(450, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string UsuarioId { get; set; } = null!;

    /// <summary>
    /// Nombre del usuario para referencia rápida
    /// </summary>
    [Display(Name = "Nombre Usuario")]
    [MaxLength(256, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NombreUsuario { get; set; }

    /// <summary>
    /// Nombre de la tabla/entidad afectada
    /// </summary>
    [Display(Name = "Tabla")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Tabla { get; set; } = null!;

    /// <summary>
    /// ID del registro afectado (puede ser Guid o string)
    /// </summary>
    [Display(Name = "ID Registro")]
    [MaxLength(450, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? RegistroId { get; set; }

    /// <summary>
    /// Tipo de acción realizada
    /// </summary>
    [Display(Name = "Acción")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Accion { get; set; } = null!; // CREATE, UPDATE, DELETE, LOGIN, LOGOUT, VIEW, EXPORT, SEND_HACIENDA, etc.

    /// <summary>
    /// Valores anteriores del registro (JSON)
    /// Solo para UPDATE y DELETE
    /// </summary>
    [Display(Name = "Valores Anteriores")]
    public string? ValoresAnteriores { get; set; }

    /// <summary>
    /// Valores nuevos del registro (JSON)
    /// Para CREATE y UPDATE
    /// </summary>
    [Display(Name = "Valores Nuevos")]
    public string? ValoresNuevos { get; set; }

    /// <summary>
    /// Descripción adicional de la acción
    /// </summary>
    [Display(Name = "Descripción")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    /// <summary>
    /// Dirección IP desde donde se realizó la acción
    /// </summary>
    [Display(Name = "Dirección IP")]
    [MaxLength(45, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? DireccionIP { get; set; }

    /// <summary>
    /// User-Agent del navegador/cliente
    /// </summary>
    [Display(Name = "User Agent")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Nombre del controlador/endpoint que procesó la acción
    /// </summary>
    [Display(Name = "Endpoint")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Endpoint { get; set; }

    /// <summary>
    /// Método HTTP utilizado (GET, POST, PUT, DELETE)
    /// </summary>
    [Display(Name = "Método HTTP")]
    [MaxLength(10, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? MetodoHttp { get; set; }

    /// <summary>
    /// Indica si la acción fue exitosa
    /// </summary>
    [Display(Name = "Exitoso")]
    public bool Exitoso { get; set; }

    /// <summary>
    /// Mensaje de error si la acción falló
    /// </summary>
    [Display(Name = "Error")]
    [MaxLength(2000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? MensajeError { get; set; }

    /// <summary>
    /// Fecha y hora de la acción
    /// </summary>
    [Display(Name = "Fecha")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Duración de la operación en milisegundos
    /// </summary>
    [Display(Name = "Duración (ms)")]
    public long? DuracionMs { get; set; }

    // Navegación
    public Empresa? Empresa { get; set; }
    public User? Usuario { get; set; }
}
