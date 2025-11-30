using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// Resumen general del dashboard con métricas principales del negocio
/// </summary>
public class DashboardResumenDTO
{
    // Métricas de Ventas
    [Display(Name = "Total Ventas Hoy")]
    public decimal TotalVentasHoy { get; set; }

    [Display(Name = "Total Ventas del Mes")]
    public decimal TotalVentasMes { get; set; }

    [Display(Name = "Total Ventas del Año")]
    public decimal TotalVentasAno { get; set; }

    [Display(Name = "Cantidad de Documentos Hoy")]
    public int CantidadDocumentosHoy { get; set; }

    [Display(Name = "Cantidad de Documentos del Mes")]
    public int CantidadDocumentosMes { get; set; }

    // Métricas de Gastos
    [Display(Name = "Total Gastos Hoy")]
    public decimal TotalGastosHoy { get; set; }

    [Display(Name = "Total Gastos del Mes")]
    public decimal TotalGastosMes { get; set; }

    [Display(Name = "Total Gastos del Año")]
    public decimal TotalGastosAno { get; set; }

    // Métricas de Inventario
    [Display(Name = "Saldo Total de Inventario")]
    public decimal SaldoInventarioTotal { get; set; }

    [Display(Name = "Productos Bajo Stock")]
    public int ProductosBajoStock { get; set; }

    // Métricas de Documentos
    [Display(Name = "Documentos Pendientes de Envío")]
    public int DocumentosPendientesEnvio { get; set; }

    [Display(Name = "Documentos Rechazados")]
    public int DocumentosRechazados { get; set; }

    // Métricas de Clientes y Proveedores
    [Display(Name = "Clientes Activos")]
    public int ClientesActivos { get; set; }

    [Display(Name = "Proveedores Activos")]
    public int ProveedoresActivos { get; set; }

    // Métricas Financieras
    [Display(Name = "Pagos Pendientes")]
    public decimal PagosPendientes { get; set; }

    [Display(Name = "Gastos Pendientes de Pago")]
    public decimal GastosPendientesPago { get; set; }
}
