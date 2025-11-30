using Facturacion.Shared.DTOs;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para generar métricas y estadísticas del dashboard empresarial
/// Este servicio agrega datos de múltiples módulos (documentos, gastos, inventario, clientes, proveedores)
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Obtiene el resumen general del dashboard con todas las métricas principales
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>DTO con resumen completo del dashboard</returns>
    Task<DashboardResumenDTO> GetResumenAsync(Guid empresaId);

    /// <summary>
    /// Obtiene las ventas agrupadas por mes para un año específico
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="ano">Año a consultar</param>
    /// <returns>Lista de ventas mensuales</returns>
    Task<IEnumerable<VentasPorMesDTO>> GetVentasPorMesAsync(Guid empresaId, int ano);

    /// <summary>
    /// Obtiene la distribución de ventas por tipo de documento en un rango de fechas
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="fechaInicio">Fecha inicial</param>
    /// <param name="fechaFin">Fecha final</param>
    /// <returns>Lista de ventas por tipo de documento con porcentajes</returns>
    Task<IEnumerable<VentasPorTipoDocumentoDTO>> GetVentasPorTipoDocumentoAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin);

    /// <summary>
    /// Obtiene los principales clientes ordenados por volumen de compras
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="top">Cantidad de clientes a retornar (predeterminado: 10)</param>
    /// <param name="fechaInicio">Fecha inicial opcional para filtrar</param>
    /// <param name="fechaFin">Fecha final opcional para filtrar</param>
    /// <returns>Lista de top clientes</returns>
    Task<IEnumerable<TopClientesDTO>> GetTopClientesAsync(Guid empresaId, int top = 10, DateTime? fechaInicio = null, DateTime? fechaFin = null);

    /// <summary>
    /// Obtiene los productos más vendidos ordenados por cantidad y monto
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="top">Cantidad de productos a retornar (predeterminado: 10)</param>
    /// <param name="fechaInicio">Fecha inicial opcional para filtrar</param>
    /// <param name="fechaFin">Fecha final opcional para filtrar</param>
    /// <returns>Lista de top productos</returns>
    Task<IEnumerable<TopProductosDTO>> GetTopProductosAsync(Guid empresaId, int top = 10, DateTime? fechaInicio = null, DateTime? fechaFin = null);

    /// <summary>
    /// Obtiene el estado del inventario de productos con alertas de stock
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="soloBajoStock">Si es true, solo retorna productos bajo stock mínimo</param>
    /// <returns>Lista de productos con estado de inventario</returns>
    Task<IEnumerable<EstadoInventarioDTO>> GetEstadoInventarioAsync(Guid empresaId, bool soloBajoStock = false);

    /// <summary>
    /// Obtiene los documentos pendientes de envío y rechazados
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>Lista de documentos pendientes</returns>
    Task<IEnumerable<DocumentosPendientesDTO>> GetDocumentosPendientesAsync(Guid empresaId);

    /// <summary>
    /// Obtiene el flujo de caja agrupado por día en un rango de fechas
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="fechaInicio">Fecha inicial</param>
    /// <param name="fechaFin">Fecha final</param>
    /// <returns>Lista de flujo de caja diario con saldos acumulados</returns>
    Task<IEnumerable<FlujoCajaDTO>> GetFlujoCajaAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin);

    /// <summary>
    /// Obtiene las ventas agrupadas por día en un rango de fechas
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="fechaInicio">Fecha inicial</param>
    /// <param name="fechaFin">Fecha final</param>
    /// <returns>Lista de ventas diarias</returns>
    Task<IEnumerable<VentasPorMesDTO>> GetVentasPorDiaAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin);

    /// <summary>
    /// Obtiene comparativo de ventas: mes actual vs mes anterior
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>Array con 2 elementos: [0]=Mes anterior, [1]=Mes actual</returns>
    Task<VentasPorMesDTO[]> GetComparativoMensualAsync(Guid empresaId);
}
