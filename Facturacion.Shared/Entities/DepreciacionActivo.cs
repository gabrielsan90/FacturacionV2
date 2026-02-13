using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Registro histórico de depreciaciones de activos fijos.
/// Cada registro representa un período de depreciación aplicada.
/// </summary>
public class DepreciacionActivo
{
    [Key]
    public Guid Id { get; set; }

    // =====================================================
    // Relación con Activo
    // =====================================================
    [Display(Name = "Activo Fijo")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid ActivoFijoId { get; set; }

    // =====================================================
    // Período
    // =====================================================
    [Display(Name = "Fecha")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime Fecha { get; set; }

    [Display(Name = "Año")]
    public int Anio { get; set; }

    [Display(Name = "Mes")]
    [Range(1, 12, ErrorMessage = "El mes debe estar entre {1} y {2}.")]
    public int Mes { get; set; }

    // =====================================================
    // Montos
    // =====================================================
    [Display(Name = "Monto Depreciación")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoDepreciacion { get; set; }

    [Display(Name = "Depreciación Acumulada Anterior")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal DepreciacionAcumuladaAnterior { get; set; }

    [Display(Name = "Depreciación Acumulada Nueva")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal DepreciacionAcumuladaNueva { get; set; }

    [Display(Name = "Valor en Libros Anterior")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorLibrosAnterior { get; set; }

    [Display(Name = "Valor en Libros Nuevo")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorLibrosNuevo { get; set; }

    // =====================================================
    // Contabilización (para futura integración)
    // =====================================================
    [Display(Name = "Número Asiento")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroAsiento { get; set; }

    [Display(Name = "Contabilizado")]
    public bool Contabilizado { get; set; }

    [Display(Name = "Fecha Contabilización")]
    public DateTime? FechaContabilizacion { get; set; }

    // =====================================================
    // Estado
    // =====================================================
    /// <summary>
    /// Estado: CAL=Calculada, APL=Aplicada, REV=Reversada
    /// </summary>
    [Display(Name = "Estado")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "CAL";

    [Display(Name = "Observaciones")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    // =====================================================
    // Audit Trail
    // =====================================================
    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Usuario Creación")]
    public string? UsuarioCreacionId { get; set; }

    // =====================================================
    // Propiedades calculadas
    // =====================================================
    [NotMapped]
    public string PeriodoDescripcion => $"{Anio}-{Mes:D2}";

    [NotMapped]
    public string EstadoDescripcion => Estado switch
    {
        "CAL" => "Calculada",
        "APL" => "Aplicada",
        "REV" => "Reversada",
        _ => Estado
    };

    // =====================================================
    // Navigation Properties
    // =====================================================
    public ActivoFijo? ActivoFijo { get; set; }
    public User? UsuarioCreacion { get; set; }
}
