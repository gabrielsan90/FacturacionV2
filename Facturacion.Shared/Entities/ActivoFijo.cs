using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Activos fijos de la empresa.
/// Control de bienes tangibles con depreciación y ubicación.
/// </summary>
public class ActivoFijo
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
    // Identificación
    // =====================================================
    [Display(Name = "Código")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Codigo { get; set; } = null!;

    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Descripción")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    // =====================================================
    // Clasificación
    // =====================================================
    [Display(Name = "Categoría")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid CategoriaActivoId { get; set; }

    // =====================================================
    // Ubicación
    // =====================================================
    [Display(Name = "Sucursal")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid SucursalId { get; set; }

    [Display(Name = "Responsable")]
    public Guid? ResponsableId { get; set; }

    // =====================================================
    // Datos de Adquisición
    // =====================================================
    [Display(Name = "Fecha de Compra")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaCompra { get; set; }

    [Display(Name = "Número de Factura")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroFactura { get; set; }

    [Display(Name = "Proveedor")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Proveedor { get; set; }

    [Display(Name = "Proveedor ID")]
    public Guid? ProveedorId { get; set; }

    // =====================================================
    // Valores
    // =====================================================
    [Display(Name = "Valor Original")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorOriginal { get; set; }

    [Display(Name = "Valor Residual")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorResidual { get; set; } = 0; // Valor de salvamento

    [Display(Name = "Depreciación Acumulada")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal DepreciacionAcumulada { get; set; } = 0;

    // =====================================================
    // Configuración de Depreciación
    // =====================================================
    /// <summary>
    /// Método: LR=Línea Recta, SAD=Suma de Años Dígitos, SDD=Saldo Doble Decreciente
    /// </summary>
    [Display(Name = "Método Depreciación")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string MetodoDepreciacion { get; set; } = "LR";

    [Display(Name = "Vida Útil (Años)")]
    public int VidaUtilAnios { get; set; } = 5;

    [Display(Name = "% Depreciación Anual")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal PorcentajeDepreciacionAnual { get; set; } = 20;

    /// <summary>
    /// Frecuencia: MEN=Mensual, ANU=Anual
    /// </summary>
    [Display(Name = "Frecuencia Depreciación")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string FrecuenciaDepreciacion { get; set; } = "MEN";

    [Display(Name = "Última Depreciación")]
    public DateTime? UltimaDepreciacion { get; set; }

    [Display(Name = "Fecha Inicio Depreciación")]
    public DateTime? FechaInicioDepreciacion { get; set; }

    // =====================================================
    // Estado del Activo
    // =====================================================
    /// <summary>
    /// Estado: ACT=Activo, BAJ=Baja, VEN=Vendido, ROB=Robo/Pérdida, DEP=Depreciado Total
    /// </summary>
    [Display(Name = "Estado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "ACT";

    [Display(Name = "Fecha de Baja")]
    public DateTime? FechaBaja { get; set; }

    [Display(Name = "Motivo de Baja")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? MotivoBaja { get; set; }

    [Display(Name = "Valor de Venta")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? ValorVenta { get; set; }

    // =====================================================
    // Datos Adicionales
    // =====================================================
    [Display(Name = "Marca")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Marca { get; set; }

    [Display(Name = "Modelo")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Modelo { get; set; }

    [Display(Name = "Número de Serie")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroSerie { get; set; }

    [Display(Name = "Placa")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Placa { get; set; } // Para vehículos

    [Display(Name = "Color")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Color { get; set; }

    [Display(Name = "Año de Fabricación")]
    public int? AnioFabricacion { get; set; }

    [Display(Name = "Garantía hasta")]
    public DateTime? GarantiaHasta { get; set; }

    [Display(Name = "Observaciones")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    [Display(Name = "Foto URL")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? FotoUrl { get; set; }

    // =====================================================
    // Audit Trail
    // =====================================================
    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Usuario Creación")]
    public string? UsuarioCreacionId { get; set; }

    [Display(Name = "Fecha de Modificación")]
    public DateTime? FechaModificacion { get; set; }

    [Display(Name = "Usuario Modificación")]
    public string? UsuarioModificacionId { get; set; }

    // =====================================================
    // Soft Delete
    // =====================================================
    [Display(Name = "Eliminado")]
    public bool IsDeleted { get; set; }

    [Display(Name = "Fecha de Eliminación")]
    public DateTime? FechaEliminacion { get; set; }

    [Display(Name = "Usuario Eliminación")]
    public string? UsuarioEliminacionId { get; set; }

    // =====================================================
    // Propiedades calculadas
    // =====================================================
    [NotMapped]
    public decimal ValorActual => ValorOriginal - DepreciacionAcumulada;

    [NotMapped]
    public decimal ValorDepreciable => ValorOriginal - ValorResidual;

    [NotMapped]
    public decimal DepreciacionMensual => VidaUtilAnios > 0
        ? ValorDepreciable / (VidaUtilAnios * 12)
        : 0;

    [NotMapped]
    public decimal PorcentajeDepreciado => ValorDepreciable > 0
        ? (DepreciacionAcumulada / ValorDepreciable) * 100
        : 0;

    [NotMapped]
    public bool EstaDepreciadoTotal => DepreciacionAcumulada >= ValorDepreciable;

    [NotMapped]
    public string MetodoDepreciacionDescripcion => MetodoDepreciacion switch
    {
        "LR" => "Línea Recta",
        "SAD" => "Suma Años Dígitos",
        "SDD" => "Saldo Doble Decreciente",
        _ => MetodoDepreciacion
    };

    [NotMapped]
    public string EstadoDescripcion => Estado switch
    {
        "ACT" => "Activo",
        "BAJ" => "Baja",
        "VEN" => "Vendido",
        "ROB" => "Robo/Pérdida",
        "DEP" => "Depreciado Total",
        _ => Estado
    };

    [NotMapped]
    public string FrecuenciaDescripcion => FrecuenciaDepreciacion switch
    {
        "MEN" => "Mensual",
        "ANU" => "Anual",
        _ => FrecuenciaDepreciacion
    };

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public CategoriaActivo? CategoriaActivo { get; set; }
    public Sucursal? Sucursal { get; set; }
    public Empleado? Responsable { get; set; }
    public Proveedor? ProveedorEntity { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<DepreciacionActivo>? Depreciaciones { get; set; }
    public ICollection<TrasladoActivo>? Traslados { get; set; }
}
