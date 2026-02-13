using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Entidad para el mapeo de IDs durante la migración de datos desde el ERP anterior hacia FacturacionV2
/// Permite rastrear la correspondencia entre los IDs originales (int) del sistema anterior
/// y los nuevos IDs (Guid) del sistema actual para cada entidad migrada
/// </summary>
public class MigracionIdMapping
{
    /// <summary>
    /// ID único del registro de mapeo
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre de la entidad migrada (ej: "Producto", "Cliente", "Categoria", "Proveedor")
    /// Debe coincidir exactamente con el nombre de la clase de la entidad
    /// </summary>
    [Display(Name = "Nombre Entidad")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string NombreEntidad { get; set; } = null!;

    /// <summary>
    /// ID original del registro en el sistema ERP anterior (tipo int)
    /// </summary>
    [Display(Name = "ID Anterior")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int IdAnterior { get; set; }

    /// <summary>
    /// Nuevo ID del registro en FacturacionV2 (tipo Guid)
    /// </summary>
    [Display(Name = "ID Nuevo")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid IdNuevo { get; set; }

    /// <summary>
    /// Fecha y hora en que se realizó la migración del registro
    /// </summary>
    [Display(Name = "Fecha Migración")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaMigracion { get; set; }

    /// <summary>
    /// Observaciones opcionales sobre la migración del registro
    /// Puede incluir advertencias, datos omitidos, transformaciones aplicadas, etc.
    /// </summary>
    [Display(Name = "Observaciones")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }
}
