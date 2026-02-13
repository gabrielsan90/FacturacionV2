using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Línea de presupuesto con montos mensuales por cuenta contable y centro de costo.
/// </summary>
public class LineaPresupuesto
{
    [Key]
    public Guid Id { get; set; }

    // =====================================================
    // Multi-Tenancy
    // =====================================================
    [Display(Name = "Empresa")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid EmpresaId { get; set; }

    // =====================================================
    // Presupuesto
    // =====================================================
    [Display(Name = "Presupuesto")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid PresupuestoId { get; set; }

    // =====================================================
    // Cuenta y Centro de Costo
    // =====================================================
    [Display(Name = "Cuenta Contable")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid CuentaContableId { get; set; }

    [Display(Name = "Centro de Costo")]
    public Guid? CentroCostoId { get; set; }

    /// <summary>
    /// Sucursal para presupuestos por ubicación
    /// </summary>
    [Display(Name = "Sucursal")]
    public Guid? SucursalId { get; set; }

    // =====================================================
    // Montos por Mes
    // =====================================================
    [Display(Name = "Enero")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoEnero { get; set; }

    [Display(Name = "Febrero")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoFebrero { get; set; }

    [Display(Name = "Marzo")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoMarzo { get; set; }

    [Display(Name = "Abril")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoAbril { get; set; }

    [Display(Name = "Mayo")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoMayo { get; set; }

    [Display(Name = "Junio")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoJunio { get; set; }

    [Display(Name = "Julio")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoJulio { get; set; }

    [Display(Name = "Agosto")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoAgosto { get; set; }

    [Display(Name = "Septiembre")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoSeptiembre { get; set; }

    [Display(Name = "Octubre")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoOctubre { get; set; }

    [Display(Name = "Noviembre")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoNoviembre { get; set; }

    [Display(Name = "Diciembre")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoDiciembre { get; set; }

    // =====================================================
    // Ejecución por Mes
    // =====================================================
    [Display(Name = "Ejecutado Enero")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal EjecutadoEnero { get; set; }

    [Display(Name = "Ejecutado Febrero")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal EjecutadoFebrero { get; set; }

    [Display(Name = "Ejecutado Marzo")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal EjecutadoMarzo { get; set; }

    [Display(Name = "Ejecutado Abril")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal EjecutadoAbril { get; set; }

    [Display(Name = "Ejecutado Mayo")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal EjecutadoMayo { get; set; }

    [Display(Name = "Ejecutado Junio")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal EjecutadoJunio { get; set; }

    [Display(Name = "Ejecutado Julio")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal EjecutadoJulio { get; set; }

    [Display(Name = "Ejecutado Agosto")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal EjecutadoAgosto { get; set; }

    [Display(Name = "Ejecutado Septiembre")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal EjecutadoSeptiembre { get; set; }

    [Display(Name = "Ejecutado Octubre")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal EjecutadoOctubre { get; set; }

    [Display(Name = "Ejecutado Noviembre")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal EjecutadoNoviembre { get; set; }

    [Display(Name = "Ejecutado Diciembre")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal EjecutadoDiciembre { get; set; }

    // =====================================================
    // Notas
    // =====================================================
    [Display(Name = "Notas")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Notas { get; set; }

    // =====================================================
    // Propiedades Calculadas
    // =====================================================
    [NotMapped]
    public decimal MontoAnual => MontoEnero + MontoFebrero + MontoMarzo + MontoAbril +
                                  MontoMayo + MontoJunio + MontoJulio + MontoAgosto +
                                  MontoSeptiembre + MontoOctubre + MontoNoviembre + MontoDiciembre;

    [NotMapped]
    public decimal EjecutadoAnual => EjecutadoEnero + EjecutadoFebrero + EjecutadoMarzo + EjecutadoAbril +
                                      EjecutadoMayo + EjecutadoJunio + EjecutadoJulio + EjecutadoAgosto +
                                      EjecutadoSeptiembre + EjecutadoOctubre + EjecutadoNoviembre + EjecutadoDiciembre;

    [NotMapped]
    public decimal DisponibleAnual => MontoAnual - EjecutadoAnual;

    [NotMapped]
    public decimal PorcentajeEjecucion => MontoAnual > 0 ? Math.Round((EjecutadoAnual / MontoAnual) * 100, 2) : 0;

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public Presupuesto? Presupuesto { get; set; }
    public CuentaContable? CuentaContable { get; set; }
    public CentroCosto? CentroCosto { get; set; }
    public Sucursal? Sucursal { get; set; }
}
