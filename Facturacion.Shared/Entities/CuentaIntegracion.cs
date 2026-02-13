using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Mapeo de cuentas contables para integración automática con módulos.
/// Define qué cuentas se usan para cada tipo de operación.
/// </summary>
public class CuentaIntegracion
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
    // Identificación del Mapeo
    // =====================================================
    /// <summary>
    /// Módulo del sistema: VEN, COM, INV, CXC, CXP, BAN, NOM, ACT
    /// </summary>
    [Display(Name = "Módulo")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Modulo { get; set; } = null!;

    /// <summary>
    /// Tipo de operación dentro del módulo.
    /// VEN: VENTA_CONTADO, VENTA_CREDITO, NOTA_CREDITO, NOTA_DEBITO
    /// COM: COMPRA_CONTADO, COMPRA_CREDITO
    /// INV: ENTRADA, SALIDA, AJUSTE_POSITIVO, AJUSTE_NEGATIVO
    /// CXC: RECIBO_PAGO, NOTA_DEBITO_INTERES
    /// CXP: PAGO_PROVEEDOR
    /// BAN: DEPOSITO, RETIRO, COMISION, INTERES
    /// NOM: PAGO_PLANILLA, PROVISION
    /// ACT: DEPRECIACION, BAJA, VENTA
    /// </summary>
    [Display(Name = "Tipo de Operación")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string TipoOperacion { get; set; } = null!;

    /// <summary>
    /// Concepto contable que representa este movimiento.
    /// Ej: CUENTA_VENTAS, CUENTA_IVA_DEBITO, CUENTA_CLIENTES
    /// </summary>
    [Display(Name = "Concepto Contable")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string ConceptoContable { get; set; } = null!;

    [Display(Name = "Descripción")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    // =====================================================
    // Cuenta Contable
    // =====================================================
    [Display(Name = "Cuenta Contable")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid CuentaContableId { get; set; }

    /// <summary>
    /// Tipo de movimiento: D=Débito, H=Haber
    /// </summary>
    [Display(Name = "Tipo Movimiento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(1, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string TipoMovimiento { get; set; } = "D";

    // =====================================================
    // Configuración Adicional
    // =====================================================
    /// <summary>
    /// Porcentaje a aplicar (para casos especiales como comisiones)
    /// </summary>
    [Display(Name = "Porcentaje")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal? Porcentaje { get; set; }

    /// <summary>
    /// Condiciones de aplicación en formato JSON
    /// Ej: {"Moneda": "USD", "TipoCliente": "LOCAL"}
    /// </summary>
    [Display(Name = "Condición Aplicación")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? CondicionAplicacion { get; set; }

    /// <summary>
    /// Orden del movimiento dentro del asiento
    /// </summary>
    [Display(Name = "Orden")]
    public int Orden { get; set; } = 1;

    // =====================================================
    // Estado
    // =====================================================
    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    [Display(Name = "Observaciones")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

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
    // Propiedades Calculadas
    // =====================================================
    [NotMapped]
    public string ModuloDescripcion => Modulo switch
    {
        "VEN" => "Ventas",
        "COM" => "Compras",
        "INV" => "Inventario",
        "CXC" => "Cuentas por Cobrar",
        "CXP" => "Cuentas por Pagar",
        "BAN" => "Bancos",
        "NOM" => "Nómina",
        "ACT" => "Activos Fijos",
        _ => Modulo
    };

    [NotMapped]
    public string TipoMovimientoDescripcion => TipoMovimiento switch
    {
        "D" => "Débito",
        "H" => "Haber",
        _ => TipoMovimiento
    };

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public CuentaContable? CuentaContable { get; set; }
    public User? CreadoPor { get; set; }
    public User? ModificadoPor { get; set; }
}

/// <summary>
/// Constantes para los módulos del sistema
/// </summary>
public static class ModulosContables
{
    public const string Ventas = "VEN";
    public const string Compras = "COM";
    public const string Inventario = "INV";
    public const string CuentasPorCobrar = "CXC";
    public const string CuentasPorPagar = "CXP";
    public const string Bancos = "BAN";
    public const string Nomina = "NOM";
    public const string ActivosFijos = "ACT";
}

/// <summary>
/// Constantes para tipos de operación por módulo
/// </summary>
public static class TiposOperacionContable
{
    // Ventas
    public const string VentaContado = "VENTA_CONTADO";
    public const string VentaCredito = "VENTA_CREDITO";
    public const string NotaCreditoVenta = "NOTA_CREDITO_VENTA";
    public const string NotaDebitoVenta = "NOTA_DEBITO_VENTA";

    // Compras
    public const string CompraContado = "COMPRA_CONTADO";
    public const string CompraCredito = "COMPRA_CREDITO";
    public const string NotaCreditoCompra = "NOTA_CREDITO_COMPRA";

    // Inventario
    public const string EntradaInventario = "ENTRADA";
    public const string SalidaInventario = "SALIDA";
    public const string AjustePositivo = "AJUSTE_POSITIVO";
    public const string AjusteNegativo = "AJUSTE_NEGATIVO";

    // CxC
    public const string ReciboPago = "RECIBO_PAGO";
    public const string NotaDebitoInteres = "NOTA_DEBITO_INTERES";

    // CxP
    public const string PagoProveedor = "PAGO_PROVEEDOR";

    // Bancos
    public const string Deposito = "DEPOSITO";
    public const string Retiro = "RETIRO";
    public const string ComisionBancaria = "COMISION_BANCARIA";
    public const string InteresBancario = "INTERES_BANCARIO";

    // Nómina
    public const string PagoPlanilla = "PAGO_PLANILLA";
    public const string ProvisionPlanilla = "PROVISION_PLANILLA";

    // Activos Fijos
    public const string Depreciacion = "DEPRECIACION";
    public const string BajaActivo = "BAJA_ACTIVO";
    public const string VentaActivo = "VENTA_ACTIVO";
}

/// <summary>
/// Constantes para conceptos contables
/// </summary>
public static class ConceptosContables
{
    public const string CuentaVentas = "CUENTA_VENTAS";
    public const string CuentaVentasExentas = "CUENTA_VENTAS_EXENTAS";
    public const string CuentaIvaDebito = "CUENTA_IVA_DEBITO";
    public const string CuentaIvaCredito = "CUENTA_IVA_CREDITO";
    public const string CuentaClientes = "CUENTA_CLIENTES";
    public const string CuentaProveedores = "CUENTA_PROVEEDORES";
    public const string CuentaInventario = "CUENTA_INVENTARIO";
    public const string CuentaCostoVentas = "CUENTA_COSTO_VENTAS";
    public const string CuentaBancos = "CUENTA_BANCOS";
    public const string CuentaCaja = "CUENTA_CAJA";
    public const string CuentaDescuentos = "CUENTA_DESCUENTOS";
    public const string CuentaDevoluciones = "CUENTA_DEVOLUCIONES";
    public const string CuentaComisiones = "CUENTA_COMISIONES";
    public const string CuentaIntereses = "CUENTA_INTERESES";
    public const string CuentaDifCambiariaGanancia = "CUENTA_DIF_CAMBIARIA_GANANCIA";
    public const string CuentaDifCambiariaPerdida = "CUENTA_DIF_CAMBIARIA_PERDIDA";
    public const string CuentaDepreciacion = "CUENTA_DEPRECIACION";
    public const string CuentaDepreciacionAcumulada = "CUENTA_DEPRECIACION_ACUMULADA";
}
