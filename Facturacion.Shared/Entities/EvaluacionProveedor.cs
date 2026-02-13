using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Evaluación periódica de desempeño de proveedores.
/// Permite clasificarlos y tomar decisiones de compra informadas.
/// </summary>
public class EvaluacionProveedor
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
    // Proveedor y Período
    // =====================================================
    [Display(Name = "Proveedor")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid ProveedorId { get; set; }

    [Display(Name = "Año")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int Anio { get; set; }

    [Display(Name = "Trimestre")]
    [Range(1, 4, ErrorMessage = "El trimestre debe estar entre {1} y {2}.")]
    public int? Trimestre { get; set; }

    [Display(Name = "Mes")]
    [Range(1, 12, ErrorMessage = "El mes debe estar entre {1} y {2}.")]
    public int? Mes { get; set; }

    // =====================================================
    // Criterios de Evaluación (1-5)
    // =====================================================
    [Display(Name = "Calidad del Producto")]
    [Range(1, 5, ErrorMessage = "El valor debe estar entre {1} y {2}.")]
    public int CalidadProducto { get; set; }

    [Display(Name = "Tiempo de Entrega")]
    [Range(1, 5, ErrorMessage = "El valor debe estar entre {1} y {2}.")]
    public int TiempoEntrega { get; set; }

    [Display(Name = "Precios")]
    [Range(1, 5, ErrorMessage = "El valor debe estar entre {1} y {2}.")]
    public int Precios { get; set; }

    [Display(Name = "Servicio Postventa")]
    [Range(1, 5, ErrorMessage = "El valor debe estar entre {1} y {2}.")]
    public int ServicioPostventa { get; set; }

    [Display(Name = "Documentación")]
    [Range(1, 5, ErrorMessage = "El valor debe estar entre {1} y {2}.")]
    public int DocumentacionCompleta { get; set; }

    [Display(Name = "Comunicación")]
    [Range(1, 5, ErrorMessage = "El valor debe estar entre {1} y {2}.")]
    public int Comunicacion { get; set; }

    [Display(Name = "Flexibilidad")]
    [Range(1, 5, ErrorMessage = "El valor debe estar entre {1} y {2}.")]
    public int Flexibilidad { get; set; }

    [Display(Name = "Cumplimiento de Especificaciones")]
    [Range(1, 5, ErrorMessage = "El valor debe estar entre {1} y {2}.")]
    public int CumplimientoEspecificaciones { get; set; }

    // =====================================================
    // Resultado
    // =====================================================
    [Display(Name = "Promedio General")]
    [Column(TypeName = "decimal(3,2)")]
    public decimal PromedioGeneral { get; set; }

    /// <summary>
    /// Clasificación: A=Excelente, B=Bueno, C=Regular, D=Deficiente
    /// </summary>
    [Display(Name = "Clasificación")]
    [MaxLength(1, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Clasificacion { get; set; } = null!;

    // =====================================================
    // Estadísticas del Período
    // =====================================================
    [Display(Name = "Cantidad de Órdenes")]
    public int CantidadOrdenes { get; set; }

    [Display(Name = "Monto Total Compras")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoTotalCompras { get; set; }

    [Display(Name = "Cantidad Devoluciones")]
    public int CantidadDevoluciones { get; set; }

    [Display(Name = "Monto Devoluciones")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoDevoluciones { get; set; }

    [Display(Name = "% Entregas a Tiempo")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal PorcentajeEntregasATiempo { get; set; }

    [Display(Name = "Cantidad No Conformidades")]
    public int CantidadNoConformidades { get; set; }

    // =====================================================
    // Observaciones y Recomendaciones
    // =====================================================
    [Display(Name = "Observaciones")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    [Display(Name = "Acciones Correctivas")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? AccionesCorrectivas { get; set; }

    /// <summary>
    /// Recomendación: MAN=Mantener, MEJ=Mejorar, SUS=Suspender, ELI=Eliminar
    /// </summary>
    [Display(Name = "Recomendación")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Recomendacion { get; set; }

    // =====================================================
    // Audit Trail
    // =====================================================
    [Display(Name = "Fecha Evaluación")]
    public DateTime FechaEvaluacion { get; set; }

    [Display(Name = "Evaluado Por")]
    public string? EvaluadoPorId { get; set; }

    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    // =====================================================
    // Propiedades calculadas
    // =====================================================
    [NotMapped]
    public string ClasificacionDescripcion => Clasificacion switch
    {
        "A" => "Excelente",
        "B" => "Bueno",
        "C" => "Regular",
        "D" => "Deficiente",
        _ => Clasificacion
    };

    [NotMapped]
    public string RecomendacionDescripcion => Recomendacion switch
    {
        "MAN" => "Mantener",
        "MEJ" => "Mejorar",
        "SUS" => "Suspender",
        "ELI" => "Eliminar",
        _ => Recomendacion ?? ""
    };

    [NotMapped]
    public string PeriodoDescripcion
    {
        get
        {
            if (Mes.HasValue) return $"{Anio}-{Mes:D2}";
            if (Trimestre.HasValue) return $"{Anio} Q{Trimestre}";
            return Anio.ToString();
        }
    }

    [NotMapped]
    public decimal PorcentajeDevoluciones => MontoTotalCompras > 0
        ? (MontoDevoluciones / MontoTotalCompras) * 100
        : 0;

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public Proveedor? Proveedor { get; set; }
    public User? EvaluadoPor { get; set; }
}
