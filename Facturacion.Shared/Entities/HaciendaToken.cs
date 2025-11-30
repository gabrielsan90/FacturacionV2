using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Almacena los tokens OAuth2 de autenticación con Hacienda
/// para evitar solicitar un nuevo token en cada operación
/// </summary>
public class HaciendaToken
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Empresa a la que pertenece este token
    /// </summary>
    [Required]
    public Guid EmpresaId { get; set; }

    /// <summary>
    /// Access Token de Hacienda (JWT)
    /// </summary>
    [Required]
    [MaxLength(4000)]
    public string AccessToken { get; set; } = null!;

    /// <summary>
    /// Refresh Token de Hacienda (JWT)
    /// </summary>
    [Required]
    [MaxLength(4000)]
    public string RefreshToken { get; set; } = null!;

    /// <summary>
    /// Fecha y hora en que expira el AccessToken
    /// Se calcula sumando expires_in a la fecha de obtención
    /// </summary>
    [Required]
    public DateTime FechaExpiracionToken { get; set; }

    /// <summary>
    /// Fecha y hora en que expira el RefreshToken
    /// Se calcula sumando refresh_expires_in a la fecha de obtención
    /// </summary>
    [Required]
    public DateTime FechaExpiracionRefreshToken { get; set; }

    /// <summary>
    /// Ambiente para el que se obtuvo el token (Pruebas/Producción)
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Ambiente { get; set; } = null!; // "stag" o "prod"

    /// <summary>
    /// Fecha y hora en que se obtuvo el token
    /// </summary>
    [Required]
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Fecha y hora de la última actualización del token (refresco)
    /// </summary>
    public DateTime? FechaActualizacion { get; set; }

    /// <summary>
    /// Indica si el token está activo o fue revocado/invalidado
    /// </summary>
    public bool Activo { get; set; } = true;

    // Navegación
    public Empresa? Empresa { get; set; }
}
