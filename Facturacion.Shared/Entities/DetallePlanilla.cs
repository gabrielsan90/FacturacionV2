using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Detalle de planilla por empleado.
/// Contiene cálculo de ingresos, deducciones y cargas sociales según legislación Costa Rica.
/// </summary>
public class DetallePlanilla
{
    [Key]
    public Guid Id { get; set; }

    // =====================================================
    // Relaciones
    // =====================================================
    [Display(Name = "Planilla")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid PlanillaId { get; set; }

    [Display(Name = "Empleado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid EmpleadoId { get; set; }

    // =====================================================
    // Ingresos
    // =====================================================
    [Display(Name = "Salario Base")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SalarioBase { get; set; }

    [Display(Name = "Días Laborados")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal DiasLaborados { get; set; } = 15;

    [Display(Name = "Salario Bruto")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SalarioBruto { get; set; }

    [Display(Name = "Horas Extra")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal HorasExtra { get; set; }

    [Display(Name = "Monto Horas Extra")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoHorasExtra { get; set; }

    [Display(Name = "Comisiones")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Comisiones { get; set; }

    [Display(Name = "Bonificaciones")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Bonificaciones { get; set; }

    [Display(Name = "Otros Ingresos")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal OtrosIngresos { get; set; }

    [Display(Name = "Detalle Otros Ingresos")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? DetalleOtrosIngresos { get; set; }

    [Display(Name = "Total Ingresos")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalIngresos { get; set; }

    // =====================================================
    // Deducciones CCSS Trabajador (Costa Rica)
    // SEM: 5.50%, IVM: 4.17%, Total: 9.67%
    // =====================================================
    [Display(Name = "Deducción CCSS")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal DeduccionCCSS { get; set; }

    // Banco Popular (Ahorro obligatorio trabajador 1%)
    [Display(Name = "Deducción Banco Popular")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal DeduccionBancoPopular { get; set; }

    // Impuesto sobre la Renta
    [Display(Name = "Deducción Renta")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal DeduccionRenta { get; set; }

    // =====================================================
    // Otras deducciones
    // =====================================================
    [Display(Name = "Pensión Alimenticia")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal DeduccionPensionAlimenticia { get; set; }

    [Display(Name = "Embargo")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal DeduccionEmbargo { get; set; }

    [Display(Name = "Préstamo")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal DeduccionPrestamo { get; set; }

    [Display(Name = "Otras Deducciones")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal OtrasDeducciones { get; set; }

    [Display(Name = "Detalle Otras Deducciones")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? DetalleOtrasDeducciones { get; set; }

    [Display(Name = "Total Deducciones")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDeducciones { get; set; }

    // =====================================================
    // Salario Neto
    // =====================================================
    [Display(Name = "Salario Neto")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SalarioNeto { get; set; }

    // =====================================================
    // Cargas Sociales Patronales (Costa Rica)
    // CCSS Patronal: 14.50% (SEM: 9.25%, IVM: 5.25%)
    // Otras: IMAS 0.50%, INA 1.50%, Asig. Fam. 5.00%, BP 0.25%
    // Total Patronal: ~26.50%
    // =====================================================
    [Display(Name = "CCSS Patronal")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CargaCCSSPatronal { get; set; }

    [Display(Name = "Otras Instituciones")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CargaOtrasInstituciones { get; set; }

    [Display(Name = "Provisión Aguinaldo")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CargaAguinaldo { get; set; }

    [Display(Name = "Provisión Vacaciones")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CargaVacaciones { get; set; }

    [Display(Name = "Provisión Cesantía")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CargaCesantia { get; set; }

    [Display(Name = "INS")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CargaINS { get; set; }

    [Display(Name = "Total Cargas Sociales")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCargasSociales { get; set; }

    // =====================================================
    // Estado del pago
    // =====================================================
    /// <summary>
    /// Estado: PEN=Pendiente, PAG=Pagado, REC=Rechazado
    /// </summary>
    [Display(Name = "Estado de Pago")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string EstadoPago { get; set; } = "PEN";

    [Display(Name = "Fecha de Pago")]
    public DateTime? FechaPago { get; set; }

    [Display(Name = "Número de Transferencia")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroTransferencia { get; set; }

    [Display(Name = "Observaciones")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    // =====================================================
    // Propiedades calculadas
    // =====================================================
    [NotMapped]
    public string EstadoPagoDescripcion => EstadoPago switch
    {
        "PEN" => "Pendiente",
        "PAG" => "Pagado",
        "REC" => "Rechazado",
        _ => EstadoPago
    };

    [NotMapped]
    public decimal CostoTotalEmpleador => SalarioBruto + TotalCargasSociales;

    [NotMapped]
    public decimal PorcentajeDeduccion => SalarioBruto > 0
        ? (TotalDeducciones / SalarioBruto) * 100
        : 0;

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Planilla? Planilla { get; set; }
    public Empleado? Empleado { get; set; }
}
