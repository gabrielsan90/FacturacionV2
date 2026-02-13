using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Configuración global del módulo de contabilidad por empresa.
/// Define parámetros de funcionamiento, numeración y cuentas principales.
/// </summary>
public class ConfiguracionContable
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
    // Período Fiscal
    // =====================================================
    /// <summary>
    /// Mes de inicio del período fiscal (1=Enero).
    /// Desde Ley 9635/2020, Costa Rica usa año fiscal calendario (Enero-Diciembre).
    /// </summary>
    [Display(Name = "Mes Inicio Período Fiscal")]
    [Range(1, 12, ErrorMessage = "El mes debe estar entre 1 y 12")]
    public int MesInicioPeriodoFiscal { get; set; } = 1; // Enero - Ley 9635/2020 Costa Rica

    [Display(Name = "Día Inicio Período Fiscal")]
    [Range(1, 31)]
    public int DiaInicioPeriodoFiscal { get; set; } = 1;

    /// <summary>
    /// Número de períodos por año: 12=Mensual, 4=Trimestral, 1=Anual
    /// </summary>
    [Display(Name = "Número de Períodos por Año")]
    [Range(1, 12)]
    public int NumeroPeriodosPorAnio { get; set; } = 12;

    // =====================================================
    // Moneda
    // =====================================================
    /// <summary>
    /// Código de moneda base: CRC, USD
    /// </summary>
    [Display(Name = "Moneda Base")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string MonedaBase { get; set; } = "CRC";

    [Display(Name = "Decimales Moneda")]
    [Range(0, 4)]
    public int DecimalesMoneda { get; set; } = 2;

    [Display(Name = "Decimales Tipo Cambio")]
    [Range(2, 6)]
    public int DecimalesTipoCambio { get; set; } = 4;

    // =====================================================
    // Numeración de Asientos
    // =====================================================
    /// <summary>
    /// Tipo numeración: PER=Por Período, ANO=Por Año, CON=Continua
    /// </summary>
    [Display(Name = "Tipo Numeración Asientos")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string TipoNumeracion { get; set; } = "PER";

    [Display(Name = "Usar Prefijo en Número")]
    public bool UsarPrefijoNumero { get; set; }

    /// <summary>
    /// Formato prefijo (Ej: "ASI-{AAAA}-{MM}-")
    /// </summary>
    [Display(Name = "Formato Prefijo")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? FormatoPrefijo { get; set; }

    /// <summary>
    /// Longitud del número (Ej: 6 para 000001)
    /// </summary>
    [Display(Name = "Longitud Número")]
    [Range(1, 10)]
    public int LongitudNumero { get; set; } = 6;

    // =====================================================
    // Control de Períodos
    // =====================================================
    [Display(Name = "Períodos Abiertos Simultáneos")]
    [Range(1, 12)]
    public int PeriodosAbiertosSimultaneos { get; set; } = 2;

    [Display(Name = "Permitir Movimientos en Períodos Futuros")]
    public bool PermitirMovimientosFuturos { get; set; }

    [Display(Name = "Bloquear Edición de Períodos Cerrados")]
    public bool BloquearPeriodosCerrados { get; set; } = true;

    // =====================================================
    // Generación Automática de Asientos
    // =====================================================
    [Display(Name = "Generar Asientos Automáticos")]
    public bool GenerarAsientosAutomaticos { get; set; } = true;

    /// <summary>
    /// Frecuencia: INM=Inmediata, DIA=Diaria, SEM=Semanal, MEN=Mensual
    /// </summary>
    [Display(Name = "Frecuencia Generación")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string FrecuenciaGeneracion { get; set; } = "INM";

    [Display(Name = "Aprobar Asientos Automáticos")]
    public bool AprobarAsientosAutomaticos { get; set; }

    // =====================================================
    // Cierre Contable
    // =====================================================
    [Display(Name = "Requiere Aprobación para Cierre")]
    public bool RequiereAprobacionCierre { get; set; } = true;

    [Display(Name = "Validar Balance Antes de Cierre")]
    public bool ValidarBalanceAntesCierre { get; set; } = true;

    [Display(Name = "Generar Asiento de Cierre Automático")]
    public bool GenerarAsientoCierreAutomatico { get; set; } = true;

    // =====================================================
    // Diferencias y Ajustes
    // =====================================================
    [Display(Name = "Tolerancia Diferencias de Redondeo")]
    [Column(TypeName = "decimal(10,4)")]
    public decimal ToleranciaDiferencias { get; set; } = 0.01m;

    [Display(Name = "Registrar Diferencias Cambiarias Automáticas")]
    public bool RegistrarDiferenciasCambiarias { get; set; } = true;

    // =====================================================
    // Workflow y Aprobaciones
    // =====================================================
    [Display(Name = "Requiere Aprobación de Asientos")]
    public bool RequiereAprobacionAsientos { get; set; } = true;

    /// <summary>
    /// Monto límite sin aprobación (en moneda base)
    /// </summary>
    [Display(Name = "Monto Límite sin Aprobación")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MontoLimiteSinAprobacion { get; set; } = 100000;

    [Display(Name = "Niveles de Aprobación")]
    [Range(1, 5)]
    public int NivelesAprobacion { get; set; } = 1;

    // =====================================================
    // Cuentas Principales
    // =====================================================
    [Display(Name = "Cuenta Ventas Gravadas")]
    public Guid? CuentaVentasGravadasId { get; set; }

    [Display(Name = "Cuenta Ventas Exentas")]
    public Guid? CuentaVentasExentasId { get; set; }

    [Display(Name = "Cuenta IVA Débito")]
    public Guid? CuentaIvaDebitoId { get; set; }

    [Display(Name = "Cuenta IVA Crédito")]
    public Guid? CuentaIvaCreditoId { get; set; }

    [Display(Name = "Cuenta Clientes")]
    public Guid? CuentaClientesId { get; set; }

    [Display(Name = "Cuenta Proveedores")]
    public Guid? CuentaProveedoresId { get; set; }

    [Display(Name = "Cuenta Inventario")]
    public Guid? CuentaInventarioId { get; set; }

    [Display(Name = "Cuenta Costo Ventas")]
    public Guid? CuentaCostoVentasId { get; set; }

    [Display(Name = "Cuenta Caja General")]
    public Guid? CuentaCajaGeneralId { get; set; }

    [Display(Name = "Cuenta Bancos Colones")]
    public Guid? CuentaBancosColonesId { get; set; }

    [Display(Name = "Cuenta Bancos Dólares")]
    public Guid? CuentaBancosDolaresId { get; set; }

    [Display(Name = "Cuenta Diferencia Cambiaria Ganancia")]
    public Guid? CuentaDifCambiariaGananciaId { get; set; }

    [Display(Name = "Cuenta Diferencia Cambiaria Pérdida")]
    public Guid? CuentaDifCambiariaPerdidaId { get; set; }

    [Display(Name = "Cuenta Utilidad del Ejercicio")]
    public Guid? CuentaUtilidadEjercicioId { get; set; }

    [Display(Name = "Cuenta Pérdida del Ejercicio")]
    public Guid? CuentaPerdidaEjercicioId { get; set; }

    // =====================================================
    // Cuentas de Planilla (Costa Rica)
    // =====================================================
    /// <summary>
    /// Cuenta para salarios por pagar (pasivo)
    /// </summary>
    [Display(Name = "Cuenta Salarios por Pagar")]
    public Guid? CuentaSalariosPorPagarId { get; set; }

    /// <summary>
    /// Cuenta para provisión de aguinaldo (pasivo)
    /// </summary>
    [Display(Name = "Cuenta Aguinaldo por Pagar")]
    public Guid? CuentaAguinaldoPorPagarId { get; set; }

    /// <summary>
    /// Cuenta para provisión de vacaciones (pasivo)
    /// </summary>
    [Display(Name = "Cuenta Vacaciones por Pagar")]
    public Guid? CuentaVacacionesPorPagarId { get; set; }

    /// <summary>
    /// Cuenta para provisión de cesantía/preaviso (pasivo)
    /// </summary>
    [Display(Name = "Cuenta Cesantía por Pagar")]
    public Guid? CuentaCesantiaPorPagarId { get; set; }

    /// <summary>
    /// Cuenta para CCSS patronal por pagar (pasivo)
    /// </summary>
    [Display(Name = "Cuenta CCSS Patronal por Pagar")]
    public Guid? CuentaCCSSPatronalId { get; set; }

    /// <summary>
    /// Cuenta para retenciones CCSS obrero (pasivo)
    /// </summary>
    [Display(Name = "Cuenta CCSS Obrero por Pagar")]
    public Guid? CuentaCCSSObreroId { get; set; }

    /// <summary>
    /// Cuenta para INS patronal por pagar (pasivo)
    /// </summary>
    [Display(Name = "Cuenta INS Patronal por Pagar")]
    public Guid? CuentaINSPatronalId { get; set; }

    /// <summary>
    /// Cuenta para retención impuesto sobre la renta (pasivo)
    /// </summary>
    [Display(Name = "Cuenta Retención ISR")]
    public Guid? CuentaRetencionISRId { get; set; }

    /// <summary>
    /// Cuenta para gasto de salarios (gasto)
    /// </summary>
    [Display(Name = "Cuenta Gasto Salarios")]
    public Guid? CuentaGastoSalariosId { get; set; }

    /// <summary>
    /// Cuenta para gasto de cargas sociales patronales (gasto)
    /// </summary>
    [Display(Name = "Cuenta Gasto Cargas Sociales")]
    public Guid? CuentaGastoCargasSocialesId { get; set; }

    /// <summary>
    /// Cuenta para gasto de aguinaldo (gasto)
    /// </summary>
    [Display(Name = "Cuenta Gasto Aguinaldo")]
    public Guid? CuentaGastoAguinaldoId { get; set; }

    /// <summary>
    /// Cuenta para gasto de vacaciones (gasto)
    /// </summary>
    [Display(Name = "Cuenta Gasto Vacaciones")]
    public Guid? CuentaGastoVacacionesId { get; set; }

    /// <summary>
    /// Cuenta para gasto de cesantía/preaviso (gasto)
    /// </summary>
    [Display(Name = "Cuenta Gasto Cesantía")]
    public Guid? CuentaGastoCesantiaId { get; set; }

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
    public CuentaContable? CuentaVentasGravadas { get; set; }
    public CuentaContable? CuentaVentasExentas { get; set; }
    public CuentaContable? CuentaIvaDebito { get; set; }
    public CuentaContable? CuentaIvaCredito { get; set; }
    public CuentaContable? CuentaClientes { get; set; }
    public CuentaContable? CuentaProveedores { get; set; }
    public CuentaContable? CuentaInventario { get; set; }
    public CuentaContable? CuentaCostoVentas { get; set; }
    public CuentaContable? CuentaCajaGeneral { get; set; }
    public CuentaContable? CuentaBancosColones { get; set; }
    public CuentaContable? CuentaBancosDolares { get; set; }
    public CuentaContable? CuentaDifCambiariaGanancia { get; set; }
    public CuentaContable? CuentaDifCambiariaPerdida { get; set; }
    public CuentaContable? CuentaUtilidadEjercicio { get; set; }
    public CuentaContable? CuentaPerdidaEjercicio { get; set; }

    // Cuentas de Planilla
    public CuentaContable? CuentaSalariosPorPagar { get; set; }
    public CuentaContable? CuentaAguinaldoPorPagar { get; set; }
    public CuentaContable? CuentaVacacionesPorPagar { get; set; }
    public CuentaContable? CuentaCesantiaPorPagar { get; set; }
    public CuentaContable? CuentaCCSSPatronal { get; set; }
    public CuentaContable? CuentaCCSSObrero { get; set; }
    public CuentaContable? CuentaINSPatronal { get; set; }
    public CuentaContable? CuentaRetencionISR { get; set; }
    public CuentaContable? CuentaGastoSalarios { get; set; }
    public CuentaContable? CuentaGastoCargasSociales { get; set; }
    public CuentaContable? CuentaGastoAguinaldo { get; set; }
    public CuentaContable? CuentaGastoVacaciones { get; set; }
    public CuentaContable? CuentaGastoCesantia { get; set; }

    public User? CreadoPor { get; set; }
    public User? ModificadoPor { get; set; }
}
