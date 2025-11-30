using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Implementación del servicio de dashboard con agregaciones de datos empresariales
/// Lee datos de múltiples módulos sin modificar entidades
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly DataContext _context;

    public DashboardService(DataContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene el resumen general del dashboard con todas las métricas principales
    /// </summary>
    public async Task<DashboardResumenDTO> GetResumenAsync(Guid empresaId)
    {
        var hoy = DateTime.Today;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
        var inicioAno = new DateTime(hoy.Year, 1, 1);

        // Obtener documentos de venta aceptados (no rechazados ni borrador)
        var documentosVenta = await _context.Documentos
            .Where(d => d.EmpresaId == empresaId
                && !d.IsDeleted
                && !d.EsDocumentoRecibido
                && (d.Estado == EstadoDocumento.Aceptado || d.Estado == EstadoDocumento.Contingencia))
            .AsNoTracking()
            .ToListAsync();

        // Métricas de ventas
        var ventasHoy = documentosVenta
            .Where(d => d.FechaEmision.Date == hoy)
            .Sum(d => d.TotalVenta);

        var ventasMes = documentosVenta
            .Where(d => d.FechaEmision >= inicioMes)
            .Sum(d => d.TotalVenta);

        var ventasAno = documentosVenta
            .Where(d => d.FechaEmision >= inicioAno)
            .Sum(d => d.TotalVenta);

        var cantidadDocHoy = documentosVenta
            .Count(d => d.FechaEmision.Date == hoy);

        var cantidadDocMes = documentosVenta
            .Count(d => d.FechaEmision >= inicioMes);

        // Métricas de gastos aprobados
        var gastos = await _context.Gastos
            .Where(g => g.EmpresaId == empresaId && !g.IsDeleted && g.Aprobado)
            .AsNoTracking()
            .ToListAsync();

        var gastosHoy = gastos
            .Where(g => g.FechaGasto.Date == hoy)
            .Sum(g => g.MontoTotal);

        var gastosMes = gastos
            .Where(g => g.FechaGasto >= inicioMes)
            .Sum(g => g.MontoTotal);

        var gastosAno = gastos
            .Where(g => g.FechaGasto >= inicioAno)
            .Sum(g => g.MontoTotal);

        // Métricas de inventario
        var inventarios = await _context.Inventarios
            .Where(i => i.Producto!.EmpresaId == empresaId
                && !i.IsDeleted
                && !i.Producto.IsDeleted
                && i.Producto.ControlarInventario)
            .Include(i => i.Producto)
            .AsNoTracking()
            .ToListAsync();

        var saldoInventarioTotal = inventarios
            .Sum(i => i.CantidadActual * (i.Producto!.Costo ?? 0));

        var productosBajoStock = inventarios
            .Count(i => i.Producto!.StockMinimo.HasValue
                && i.CantidadActual <= i.Producto.StockMinimo.Value);

        // Métricas de documentos pendientes y rechazados
        var documentosPendientes = await _context.Documentos
            .Where(d => d.EmpresaId == empresaId
                && !d.IsDeleted
                && !d.EsDocumentoRecibido
                && (d.Estado == EstadoDocumento.Borrador
                    || d.Estado == EstadoDocumento.Pendiente
                    || d.Estado == EstadoDocumento.Procesando))
            .CountAsync();

        var documentosRechazados = await _context.Documentos
            .Where(d => d.EmpresaId == empresaId
                && !d.IsDeleted
                && !d.EsDocumentoRecibido
                && d.Estado == EstadoDocumento.Rechazado)
            .CountAsync();

        // Métricas de clientes y proveedores activos
        var clientesActivos = await _context.Clientes
            .Where(c => c.EmpresaId == empresaId && !c.IsDeleted && c.Activo)
            .CountAsync();

        var proveedoresActivos = await _context.Proveedores
            .Where(p => p.EmpresaId == empresaId && !p.IsDeleted && p.Activo)
            .CountAsync();

        // Pagos pendientes (documentos a crédito con saldo pendiente)
        var documentosCredito = await _context.Documentos
            .Where(d => d.EmpresaId == empresaId
                && !d.IsDeleted
                && !d.EsDocumentoRecibido
                && d.CondicionVenta == "02"
                && (d.Estado == EstadoDocumento.Aceptado || d.Estado == EstadoDocumento.Contingencia))
            .AsNoTracking()
            .ToListAsync();

        // Calcular pagos recibidos por documento
        var documentoIds = documentosCredito.Select(d => d.Id).ToList();
        var pagosRecibidos = await _context.RecibosPago
            .Where(r => documentoIds.Contains(r.DocumentoOriginalId) && !r.IsDeleted)
            .GroupBy(r => r.DocumentoOriginalId)
            .Select(g => new { DocumentoId = g.Key, TotalPagado = g.Sum(r => r.MontoPagado) })
            .ToListAsync();

        var pagosPendientes = documentosCredito.Sum(d => d.TotalVenta)
            - pagosRecibidos.Sum(p => p.TotalPagado);

        // Gastos pendientes de pago
        var gastosPendientesPago = await _context.Gastos
            .Where(g => g.EmpresaId == empresaId
                && !g.IsDeleted
                && g.Aprobado
                && g.EstadoPago != EstadoPago.Pagado)
            .SumAsync(g => g.SaldoPendiente);

        return new DashboardResumenDTO
        {
            TotalVentasHoy = ventasHoy,
            TotalVentasMes = ventasMes,
            TotalVentasAno = ventasAno,
            CantidadDocumentosHoy = cantidadDocHoy,
            CantidadDocumentosMes = cantidadDocMes,
            TotalGastosHoy = gastosHoy,
            TotalGastosMes = gastosMes,
            TotalGastosAno = gastosAno,
            SaldoInventarioTotal = saldoInventarioTotal,
            ProductosBajoStock = productosBajoStock,
            DocumentosPendientesEnvio = documentosPendientes,
            DocumentosRechazados = documentosRechazados,
            ClientesActivos = clientesActivos,
            ProveedoresActivos = proveedoresActivos,
            PagosPendientes = pagosPendientes,
            GastosPendientesPago = gastosPendientesPago
        };
    }

    /// <summary>
    /// Obtiene las ventas agrupadas por mes para un año específico
    /// </summary>
    public async Task<IEnumerable<VentasPorMesDTO>> GetVentasPorMesAsync(Guid empresaId, int ano)
    {
        var inicioAno = new DateTime(ano, 1, 1);
        var finAno = new DateTime(ano, 12, 31, 23, 59, 59);

        var ventasPorMes = await _context.Documentos
            .Where(d => d.EmpresaId == empresaId
                && !d.IsDeleted
                && !d.EsDocumentoRecibido
                && (d.Estado == EstadoDocumento.Aceptado || d.Estado == EstadoDocumento.Contingencia)
                && d.FechaEmision >= inicioAno
                && d.FechaEmision <= finAno)
            .GroupBy(d => d.FechaEmision.Month)
            .Select(g => new VentasPorMesDTO
            {
                Mes = g.Key,
                Ano = ano,
                TotalVentas = g.Sum(d => d.TotalVenta),
                CantidadDocumentos = g.Count(),
                PromedioVenta = g.Average(d => d.TotalVenta)
            })
            .OrderBy(v => v.Mes)
            .AsNoTracking()
            .ToListAsync();

        // Asegurar que todos los meses estén presentes (incluso con valor 0)
        var mesesCompletos = Enumerable.Range(1, 12)
            .Select(mes => ventasPorMes.FirstOrDefault(v => v.Mes == mes)
                ?? new VentasPorMesDTO
                {
                    Mes = mes,
                    Ano = ano,
                    TotalVentas = 0,
                    CantidadDocumentos = 0,
                    PromedioVenta = 0
                })
            .ToList();

        return mesesCompletos;
    }

    /// <summary>
    /// Obtiene la distribución de ventas por tipo de documento en un rango de fechas
    /// </summary>
    public async Task<IEnumerable<VentasPorTipoDocumentoDTO>> GetVentasPorTipoDocumentoAsync(
        Guid empresaId, DateTime fechaInicio, DateTime fechaFin)
    {
        var ventasPorTipo = await _context.Documentos
            .Where(d => d.EmpresaId == empresaId
                && !d.IsDeleted
                && !d.EsDocumentoRecibido
                && (d.Estado == EstadoDocumento.Aceptado || d.Estado == EstadoDocumento.Contingencia)
                && d.FechaEmision >= fechaInicio
                && d.FechaEmision <= fechaFin)
            .GroupBy(d => d.TipoDocumento)
            .Select(g => new
            {
                TipoDocumento = g.Key,
                Cantidad = g.Count(),
                Total = g.Sum(d => d.TotalVenta)
            })
            .AsNoTracking()
            .ToListAsync();

        var totalGeneral = ventasPorTipo.Sum(v => v.Total);

        var resultado = ventasPorTipo.Select(v => new VentasPorTipoDocumentoDTO
        {
            TipoDocumento = ObtenerNombreTipoDocumento(v.TipoDocumento),
            Cantidad = v.Cantidad,
            Total = v.Total,
            Porcentaje = totalGeneral > 0 ? (v.Total / totalGeneral) * 100 : 0
        })
        .OrderByDescending(v => v.Total)
        .ToList();

        return resultado;
    }

    /// <summary>
    /// Obtiene los principales clientes ordenados por volumen de compras
    /// </summary>
    public async Task<IEnumerable<TopClientesDTO>> GetTopClientesAsync(
        Guid empresaId, int top = 10, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        var query = _context.Documentos
            .Where(d => d.EmpresaId == empresaId
                && !d.IsDeleted
                && !d.EsDocumentoRecibido
                && d.ClienteId.HasValue
                && (d.Estado == EstadoDocumento.Aceptado || d.Estado == EstadoDocumento.Contingencia));

        if (fechaInicio.HasValue)
            query = query.Where(d => d.FechaEmision >= fechaInicio.Value);

        if (fechaFin.HasValue)
            query = query.Where(d => d.FechaEmision <= fechaFin.Value);

        var topClientes = await query
            .GroupBy(d => new { d.ClienteId, d.Cliente!.Nombre })
            .Select(g => new TopClientesDTO
            {
                ClienteId = g.Key.ClienteId!.Value,
                NombreCliente = g.Key.Nombre,
                TotalCompras = g.Sum(d => d.TotalVenta),
                CantidadDocumentos = g.Count(),
                UltimaCompra = g.Max(d => d.FechaEmision)
            })
            .OrderByDescending(c => c.TotalCompras)
            .Take(top)
            .AsNoTracking()
            .ToListAsync();

        return topClientes;
    }

    /// <summary>
    /// Obtiene los productos más vendidos ordenados por cantidad y monto
    /// </summary>
    public async Task<IEnumerable<TopProductosDTO>> GetTopProductosAsync(
        Guid empresaId, int top = 10, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        var query = _context.DocumentoDetalles
            .Where(dd => dd.Documento!.EmpresaId == empresaId
                && !dd.IsDeleted
                && !dd.Documento.IsDeleted
                && !dd.Documento.EsDocumentoRecibido
                && dd.ProductoId.HasValue
                && (dd.Documento.Estado == EstadoDocumento.Aceptado
                    || dd.Documento.Estado == EstadoDocumento.Contingencia));

        if (fechaInicio.HasValue)
            query = query.Where(dd => dd.Documento!.FechaEmision >= fechaInicio.Value);

        if (fechaFin.HasValue)
            query = query.Where(dd => dd.Documento!.FechaEmision <= fechaFin.Value);

        var topProductos = await query
            .GroupBy(dd => new { dd.ProductoId, dd.Producto!.Nombre })
            .Select(g => new TopProductosDTO
            {
                ProductoId = g.Key.ProductoId!.Value,
                NombreProducto = g.Key.Nombre,
                CantidadVendida = g.Sum(dd => dd.Cantidad),
                TotalVentas = g.Sum(dd => dd.MontoTotalLinea),
                UltimaVenta = g.Max(dd => dd.Documento!.FechaEmision)
            })
            .OrderByDescending(p => p.TotalVentas)
            .Take(top)
            .AsNoTracking()
            .ToListAsync();

        return topProductos;
    }

    /// <summary>
    /// Obtiene el estado del inventario de productos con alertas de stock
    /// </summary>
    public async Task<IEnumerable<EstadoInventarioDTO>> GetEstadoInventarioAsync(
        Guid empresaId, bool soloBajoStock = false)
    {
        // Agrupar inventario por producto (sumar todas las sucursales)
        var query = _context.Inventarios
            .Where(i => i.Producto!.EmpresaId == empresaId
                && !i.IsDeleted
                && !i.Producto.IsDeleted
                && i.Producto.ControlarInventario);

        var inventarioAgrupado = await query
            .GroupBy(i => new
            {
                i.ProductoId,
                i.Producto!.Nombre,
                i.Producto.StockMinimo,
                i.Producto.Costo
            })
            .Select(g => new
            {
                ProductoId = g.Key.ProductoId,
                NombreProducto = g.Key.Nombre,
                StockActual = g.Sum(i => i.CantidadActual),
                StockMinimo = g.Key.StockMinimo ?? 0,
                Costo = g.Key.Costo ?? 0
            })
            .AsNoTracking()
            .ToListAsync();

        var resultado = inventarioAgrupado.Select(i => new EstadoInventarioDTO
        {
            ProductoId = i.ProductoId,
            NombreProducto = i.NombreProducto,
            StockActual = i.StockActual,
            StockMinimo = i.StockMinimo,
            StockMaximo = i.StockMinimo * 3, // Asumimos stock máximo como 3x el mínimo
            ValorInventario = i.StockActual * i.Costo,
            EstadoStock = DeterminarEstadoStock(i.StockActual, i.StockMinimo)
        })
        .Where(i => !soloBajoStock || i.EstadoStock == "Bajo")
        .OrderBy(i => i.StockActual)
        .ToList();

        return resultado;
    }

    /// <summary>
    /// Obtiene los documentos pendientes de envío y rechazados
    /// </summary>
    public async Task<IEnumerable<DocumentosPendientesDTO>> GetDocumentosPendientesAsync(Guid empresaId)
    {
        var documentosPendientes = await _context.Documentos
            .Where(d => d.EmpresaId == empresaId
                && !d.IsDeleted
                && !d.EsDocumentoRecibido
                && (d.Estado == EstadoDocumento.Borrador
                    || d.Estado == EstadoDocumento.Pendiente
                    || d.Estado == EstadoDocumento.Procesando
                    || d.Estado == EstadoDocumento.Rechazado))
            .Select(d => new DocumentosPendientesDTO
            {
                DocumentoId = d.Id,
                NumeroConsecutivo = d.NumeroConsecutivo,
                TipoDocumento = ObtenerNombreTipoDocumento(d.TipoDocumento),
                Clave = d.Clave,
                FechaEmision = d.FechaEmision,
                Estado = d.Estado.ToString(),
                Total = d.TotalVenta,
                MotivoRechazo = d.MensajeHacienda
            })
            .OrderByDescending(d => d.FechaEmision)
            .AsNoTracking()
            .ToListAsync();

        return documentosPendientes;
    }

    /// <summary>
    /// Obtiene el flujo de caja agrupado por día en un rango de fechas
    /// </summary>
    public async Task<IEnumerable<FlujoCajaDTO>> GetFlujoCajaAsync(
        Guid empresaId, DateTime fechaInicio, DateTime fechaFin)
    {
        // Ingresos por ventas (documentos aceptados)
        var ingresosPorDia = await _context.Documentos
            .Where(d => d.EmpresaId == empresaId
                && !d.IsDeleted
                && !d.EsDocumentoRecibido
                && (d.Estado == EstadoDocumento.Aceptado || d.Estado == EstadoDocumento.Contingencia)
                && d.FechaEmision >= fechaInicio
                && d.FechaEmision <= fechaFin)
            .GroupBy(d => d.FechaEmision.Date)
            .Select(g => new
            {
                Fecha = g.Key,
                Total = g.Sum(d => d.TotalVenta)
            })
            .AsNoTracking()
            .ToListAsync();

        // Egresos por gastos aprobados y pagados
        var egresosPorDia = await _context.Gastos
            .Where(g => g.EmpresaId == empresaId
                && !g.IsDeleted
                && g.Aprobado
                && g.FechaPago.HasValue
                && g.FechaPago.Value >= fechaInicio
                && g.FechaPago.Value <= fechaFin)
            .GroupBy(g => g.FechaPago!.Value.Date)
            .Select(g => new
            {
                Fecha = g.Key,
                Total = g.Sum(gasto => gasto.MontoPagado)
            })
            .AsNoTracking()
            .ToListAsync();

        // Combinar ingresos y egresos por día
        var todasLasFechas = ingresosPorDia.Select(i => i.Fecha)
            .Union(egresosPorDia.Select(e => e.Fecha))
            .Distinct()
            .OrderBy(f => f)
            .ToList();

        var flujoCaja = new List<FlujoCajaDTO>();
        decimal saldoAcumulado = 0;

        foreach (var fecha in todasLasFechas)
        {
            var ingresos = ingresosPorDia.FirstOrDefault(i => i.Fecha == fecha)?.Total ?? 0;
            var egresos = egresosPorDia.FirstOrDefault(e => e.Fecha == fecha)?.Total ?? 0;
            var saldoDiario = ingresos - egresos;
            saldoAcumulado += saldoDiario;

            flujoCaja.Add(new FlujoCajaDTO
            {
                Fecha = fecha,
                Ingresos = ingresos,
                Egresos = egresos,
                SaldoDiario = saldoDiario,
                SaldoAcumulado = saldoAcumulado
            });
        }

        return flujoCaja;
    }

    /// <summary>
    /// Obtiene las ventas agrupadas por día en un rango de fechas
    /// </summary>
    public async Task<IEnumerable<VentasPorMesDTO>> GetVentasPorDiaAsync(
        Guid empresaId, DateTime fechaInicio, DateTime fechaFin)
    {
        var ventasPorDia = await _context.Documentos
            .Where(d => d.EmpresaId == empresaId
                && !d.IsDeleted
                && !d.EsDocumentoRecibido
                && (d.Estado == EstadoDocumento.Aceptado || d.Estado == EstadoDocumento.Contingencia)
                && d.FechaEmision >= fechaInicio
                && d.FechaEmision <= fechaFin)
            .GroupBy(d => d.FechaEmision.Date)
            .Select(g => new VentasPorMesDTO
            {
                Mes = g.Key.Day,
                Ano = g.Key.Year,
                TotalVentas = g.Sum(d => d.TotalVenta),
                CantidadDocumentos = g.Count(),
                PromedioVenta = g.Average(d => d.TotalVenta)
            })
            .OrderBy(v => v.Ano).ThenBy(v => v.Mes)
            .AsNoTracking()
            .ToListAsync();

        return ventasPorDia;
    }

    /// <summary>
    /// Obtiene comparativo de ventas: mes actual vs mes anterior
    /// </summary>
    public async Task<VentasPorMesDTO[]> GetComparativoMensualAsync(Guid empresaId)
    {
        var hoy = DateTime.Today;
        var inicioMesActual = new DateTime(hoy.Year, hoy.Month, 1);
        var inicioMesAnterior = inicioMesActual.AddMonths(-1);
        var finMesAnterior = inicioMesActual.AddDays(-1);

        // Ventas mes anterior
        var ventasMesAnterior = await _context.Documentos
            .Where(d => d.EmpresaId == empresaId
                && !d.IsDeleted
                && !d.EsDocumentoRecibido
                && (d.Estado == EstadoDocumento.Aceptado || d.Estado == EstadoDocumento.Contingencia)
                && d.FechaEmision >= inicioMesAnterior
                && d.FechaEmision <= finMesAnterior)
            .AsNoTracking()
            .ToListAsync();

        var totalMesAnterior = ventasMesAnterior.Sum(d => d.TotalVenta);
        var cantidadMesAnterior = ventasMesAnterior.Count;

        // Ventas mes actual
        var ventasMesActual = await _context.Documentos
            .Where(d => d.EmpresaId == empresaId
                && !d.IsDeleted
                && !d.EsDocumentoRecibido
                && (d.Estado == EstadoDocumento.Aceptado || d.Estado == EstadoDocumento.Contingencia)
                && d.FechaEmision >= inicioMesActual)
            .AsNoTracking()
            .ToListAsync();

        var totalMesActual = ventasMesActual.Sum(d => d.TotalVenta);
        var cantidadMesActual = ventasMesActual.Count;

        return new[]
        {
            new VentasPorMesDTO
            {
                Mes = inicioMesAnterior.Month,
                Ano = inicioMesAnterior.Year,
                TotalVentas = totalMesAnterior,
                CantidadDocumentos = cantidadMesAnterior,
                PromedioVenta = cantidadMesAnterior > 0 ? totalMesAnterior / cantidadMesAnterior : 0
            },
            new VentasPorMesDTO
            {
                Mes = inicioMesActual.Month,
                Ano = inicioMesActual.Year,
                TotalVentas = totalMesActual,
                CantidadDocumentos = cantidadMesActual,
                PromedioVenta = cantidadMesActual > 0 ? totalMesActual / cantidadMesActual : 0
            }
        };
    }

    // Métodos auxiliares privados

    private string ObtenerNombreTipoDocumento(DocumentoTipo tipo)
    {
        return tipo switch
        {
            DocumentoTipo.FacturaElectronica => "FE - Factura Electrónica",
            DocumentoTipo.NotaDebitoElectronica => "ND - Nota de Débito",
            DocumentoTipo.NotaCreditoElectronica => "NC - Nota de Crédito",
            DocumentoTipo.TiqueteElectronico => "TE - Tiquete Electrónico",
            DocumentoTipo.FacturaElectronicaExportacion => "FEE - Factura Exportación",
            DocumentoTipo.FacturaElectronicaCompra => "FEC - Factura Compra",
            DocumentoTipo.ComprobanteCompraElectronico => "CCE - Comprobante Compra",
            DocumentoTipo.ReciboElectronicoPago => "REP - Recibo de Pago",
            _ => tipo.ToString()
        };
    }

    private string DeterminarEstadoStock(decimal stockActual, decimal stockMinimo)
    {
        if (stockMinimo == 0)
            return "Normal";

        if (stockActual <= stockMinimo)
            return "Bajo";

        if (stockActual <= stockMinimo * 1.5m)
            return "Medio";

        return "Alto";
    }
}
