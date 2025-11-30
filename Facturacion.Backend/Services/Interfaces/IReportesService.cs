using Facturacion.Shared.DTOs;
using Facturacion.Shared.Enums;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para generación de reportes empresariales
/// Sistema de Facturación Electrónica - Costa Rica
/// </summary>
public interface IReportesService
{
    /// <summary>
    /// Genera reporte de ventas consolidado por período
    /// </summary>
    /// <param name="empresaId">ID de la empresa (multi-tenant)</param>
    /// <param name="fechaInicio">Fecha inicial del reporte</param>
    /// <param name="fechaFin">Fecha final del reporte</param>
    /// <param name="clienteId">Filtro opcional por cliente</param>
    /// <param name="tipoDocumento">Filtro opcional por tipo de documento</param>
    Task<ActionResponse<ReporteVentasDTO>> GetReporteVentasAsync(
        Guid empresaId,
        DateTime fechaInicio,
        DateTime fechaFin,
        Guid? clienteId = null,
        DocumentoTipo? tipoDocumento = null);

    /// <summary>
    /// Genera reporte de gastos consolidado por período
    /// </summary>
    /// <param name="empresaId">ID de la empresa (multi-tenant)</param>
    /// <param name="fechaInicio">Fecha inicial del reporte</param>
    /// <param name="fechaFin">Fecha final del reporte</param>
    /// <param name="proveedorId">Filtro opcional por proveedor</param>
    /// <param name="categoriaId">Filtro opcional por categoría de gasto</param>
    Task<ActionResponse<ReporteGastosDTO>> GetReporteGastosAsync(
        Guid empresaId,
        DateTime fechaInicio,
        DateTime fechaFin,
        Guid? proveedorId = null,
        int? categoriaId = null);

    /// <summary>
    /// Genera reporte de inventario actual
    /// </summary>
    /// <param name="empresaId">ID de la empresa (multi-tenant)</param>
    /// <param name="sucursalId">Filtro opcional por sucursal</param>
    /// <param name="soloBajoStock">Filtrar solo productos con stock bajo</param>
    Task<ActionResponse<ReporteInventarioDTO>> GetReporteInventarioAsync(
        Guid empresaId,
        Guid? sucursalId = null,
        bool soloBajoStock = false);

    /// <summary>
    /// Genera reporte de impuestos para declaraciones a Hacienda
    /// Calcula IVA ventas, IVA compras y saldo por pagar
    /// </summary>
    /// <param name="empresaId">ID de la empresa (multi-tenant)</param>
    /// <param name="fechaInicio">Fecha inicial del período fiscal</param>
    /// <param name="fechaFin">Fecha final del período fiscal</param>
    Task<ActionResponse<ReporteImpuestosDTO>> GetReporteImpuestosAsync(
        Guid empresaId,
        DateTime fechaInicio,
        DateTime fechaFin);

    /// <summary>
    /// Genera reporte de actividad de clientes
    /// </summary>
    /// <param name="empresaId">ID de la empresa (multi-tenant)</param>
    /// <param name="fechaInicio">Fecha inicial del reporte</param>
    /// <param name="fechaFin">Fecha final del reporte</param>
    /// <param name="soloActivos">Filtrar solo clientes con compras en el período</param>
    Task<ActionResponse<ReporteClientesDTO>> GetReporteClientesAsync(
        Guid empresaId,
        DateTime fechaInicio,
        DateTime fechaFin,
        bool soloActivos = false);

    /// <summary>
    /// Genera reporte de ventas por producto
    /// </summary>
    /// <param name="empresaId">ID de la empresa (multi-tenant)</param>
    /// <param name="fechaInicio">Fecha inicial del reporte</param>
    /// <param name="fechaFin">Fecha final del reporte</param>
    /// <param name="categoriaId">Filtro opcional por categoría de producto</param>
    Task<ActionResponse<ReporteProductosDTO>> GetReporteProductosAsync(
        Guid empresaId,
        DateTime fechaInicio,
        DateTime fechaFin,
        Guid? categoriaId = null);

    /// <summary>
    /// Genera reporte de movimientos de inventario
    /// </summary>
    /// <param name="empresaId">ID de la empresa (multi-tenant)</param>
    /// <param name="fechaInicio">Fecha inicial del reporte</param>
    /// <param name="fechaFin">Fecha final del reporte</param>
    /// <param name="productoId">Filtro opcional por producto</param>
    /// <param name="tipoMovimiento">Filtro opcional por tipo de movimiento</param>
    Task<ActionResponse<ReporteMovimientosInventarioDTO>> GetReporteMovimientosInventarioAsync(
        Guid empresaId,
        DateTime fechaInicio,
        DateTime fechaFin,
        Guid? productoId = null,
        TipoMovimientoInventario? tipoMovimiento = null);

    /// <summary>
    /// Genera Libro de Ventas mensual (requerido por Hacienda)
    /// Lista cronológica de todas las facturas emitidas en el mes
    /// </summary>
    /// <param name="empresaId">ID de la empresa (multi-tenant)</param>
    /// <param name="mes">Mes del libro de ventas (1-12)</param>
    /// <param name="ano">Año del libro de ventas</param>
    Task<ActionResponse<ReporteVentasDTO>> GetReporteLibroVentasAsync(
        Guid empresaId,
        int mes,
        int ano);
}
