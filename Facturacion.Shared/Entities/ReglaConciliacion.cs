using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Define reglas para la conciliación automática de transacciones bancarias.
/// </summary>
public class ReglaConciliacion
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
    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Descripción")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    /// <summary>
    /// Prioridad de la regla (menor = mayor prioridad)
    /// </summary>
    [Display(Name = "Prioridad")]
    public int Prioridad { get; set; } = 100;

    [Display(Name = "Activa")]
    public bool Activa { get; set; } = true;

    // =====================================================
    // Cuenta Bancaria
    // =====================================================
    /// <summary>
    /// Cuenta bancaria específica o null para todas
    /// </summary>
    [Display(Name = "Cuenta Bancaria")]
    public Guid? CuentaBancariaId { get; set; }

    // =====================================================
    // Criterios de Matching - Monto
    // =====================================================
    [Display(Name = "Comparar Monto")]
    public bool CompararMonto { get; set; } = true;

    /// <summary>
    /// Tolerancia de monto permitida (para comisiones bancarias)
    /// </summary>
    [Display(Name = "Tolerancia Monto")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal ToleranciaMonto { get; set; }

    /// <summary>
    /// Porcentaje de tolerancia (ej: 1% para diferencias cambiarias)
    /// </summary>
    [Display(Name = "Tolerancia Porcentaje")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal? ToleranciaPorcentaje { get; set; }

    // =====================================================
    // Criterios de Matching - Fecha
    // =====================================================
    [Display(Name = "Comparar Fecha")]
    public bool CompararFecha { get; set; } = true;

    /// <summary>
    /// Días de tolerancia en fecha (para cheques en tránsito)
    /// </summary>
    [Display(Name = "Tolerancia Fecha (Días)")]
    public int ToleranciaFechaDias { get; set; } = 3;

    // =====================================================
    // Criterios de Matching - Referencia
    // =====================================================
    [Display(Name = "Comparar Referencia")]
    public bool CompararReferencia { get; set; }

    /// <summary>
    /// Patrón regex para extraer referencia de la descripción del banco
    /// </summary>
    [Display(Name = "Patrón Referencia")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? PatronReferencia { get; set; }

    // =====================================================
    // Criterios de Matching - Descripción
    // =====================================================
    [Display(Name = "Comparar Descripción")]
    public bool CompararDescripcion { get; set; }

    /// <summary>
    /// Palabras clave que debe contener la descripción (separadas por coma)
    /// </summary>
    [Display(Name = "Palabras Clave")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? PalabrasClaveDescripcion { get; set; }

    // =====================================================
    // Acciones Automáticas
    // =====================================================
    /// <summary>
    /// Conciliar automáticamente si cumple criterios
    /// </summary>
    [Display(Name = "Auto Conciliar")]
    public bool AutoConciliar { get; set; }

    /// <summary>
    /// Crear movimiento bancario si no existe
    /// </summary>
    [Display(Name = "Crear Movimiento Faltante")]
    public bool CrearMovimientoFaltante { get; set; }

    /// <summary>
    /// Cuenta contable a usar para movimientos creados automáticamente
    /// </summary>
    [Display(Name = "Cuenta Contable Default")]
    public Guid? CuentaContableDefaultId { get; set; }

    /// <summary>
    /// Tipo de movimiento default: DEP, RET, COM, INT, OTR
    /// </summary>
    [Display(Name = "Tipo Movimiento Default")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? TipoMovimientoDefault { get; set; }

    /// <summary>
    /// Puntaje mínimo de confianza para auto-conciliar (0-100)
    /// </summary>
    [Display(Name = "Confianza Mínima")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal ConfianzaMinima { get; set; } = 95;

    // =====================================================
    // Audit Trail
    // =====================================================
    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Creado Por")]
    public string? CreadoPorId { get; set; }

    [Display(Name = "Fecha de Modificación")]
    public DateTime? FechaModificacion { get; set; }

    [Display(Name = "Modificado Por")]
    public string? ModificadoPorId { get; set; }

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public CuentaBancaria? CuentaBancaria { get; set; }
    public CuentaContable? CuentaContableDefault { get; set; }
    public User? CreadoPor { get; set; }
    public User? ModificadoPor { get; set; }

    public ICollection<LineaExtractoBancario>? LineasConciliadas { get; set; }
}
