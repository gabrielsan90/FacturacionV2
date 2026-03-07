using ClosedXML.Excel;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Servicio de generación de reportes empresariales
/// Sistema de Facturación Electrónica - Costa Rica
/// </summary>
public class ReportesService : IReportesService
{
    private readonly DataContext _context;

    public ReportesService(DataContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Genera reporte de ventas consolidado por período
    /// Incluye documentos válidos (excluye Borrador, Rechazado, Anulado y Error)
    /// </summary>
    public async Task<ActionResponse<ReporteVentasDTO>> GetReporteVentasAsync(
        Guid empresaId,
        DateTime fechaInicio,
        DateTime fechaFin,
        Guid? clienteId = null,
        DocumentoTipo? tipoDocumento = null)
    {
        try
        {
            // Query base: documentos de venta válidos (no borrador, rechazado, anulado ni error)
            var estadosExcluidos = new[] { EstadoDocumento.Borrador, EstadoDocumento.Rechazado, EstadoDocumento.Anulado, EstadoDocumento.Error };
            var query = _context.Documentos
                .Where(d => !d.IsDeleted
                    && d.EmpresaId == empresaId
                    && !d.EsDocumentoRecibido
                    && !estadosExcluidos.Contains(d.Estado)
                    && d.FechaEmision >= fechaInicio
                    && d.FechaEmision <= fechaFin);

            // Filtros opcionales
            if (clienteId.HasValue)
            {
                query = query.Where(d => d.ClienteId == clienteId.Value);
            }

            if (tipoDocumento.HasValue)
            {
                query = query.Where(d => d.TipoDocumento == tipoDocumento.Value);
            }

            // Ejecutar query con navegación
            var documentos = await query
                .Include(d => d.Cliente)
                .AsNoTracking()
                .OrderBy(d => d.FechaEmision)
                .ThenBy(d => d.NumeroConsecutivo)
                .ToListAsync();

            // Calcular totales
            var totalVentas = documentos.Sum(d => d.TotalVenta);
            var totalImpuestos = documentos.Sum(d => d.TotalImpuestos);
            var totalDescuentos = documentos.Sum(d => d.TotalDescuentos);

            // Construir detalles
            var detalles = documentos.Select(d => new ReporteVentasDetalleDTO
            {
                FechaEmision = d.FechaEmision,
                NumeroConsecutivo = d.NumeroConsecutivo,
                TipoDocumento = ObtenerNombreTipoDocumento(d.TipoDocumento),
                Cliente = d.ReceptorNombre ?? d.Cliente?.Nombre ?? "Cliente no especificado",
                Subtotal = d.Subtotal,
                Impuestos = d.TotalImpuestos,
                Descuentos = d.TotalDescuentos,
                Total = d.TotalVenta,
                Moneda = d.Moneda.ToString()
            }).ToList();

            var reporte = new ReporteVentasDTO
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                TotalVentas = totalVentas,
                TotalImpuestos = totalImpuestos,
                TotalDescuentos = totalDescuentos,
                CantidadDocumentos = documentos.Count,
                Detalles = detalles
            };

            return new ActionResponse<ReporteVentasDTO>
            {
                WasSuccess = true,
                Result = reporte
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<ReporteVentasDTO>
            {
                WasSuccess = false,
                Message = $"Error al generar reporte de ventas: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Genera reporte de gastos consolidado por período
    /// Solo incluye gastos aprobados
    /// </summary>
    public async Task<ActionResponse<ReporteGastosDTO>> GetReporteGastosAsync(
        Guid empresaId,
        DateTime fechaInicio,
        DateTime fechaFin,
        Guid? proveedorId = null,
        int? categoriaId = null)
    {
        try
        {
            // Query base: gastos aprobados no eliminados
            var query = _context.Gastos
                .Where(g => !g.IsDeleted
                    && g.EmpresaId == empresaId
                    && g.Aprobado
                    && g.FechaGasto >= fechaInicio
                    && g.FechaGasto <= fechaFin);

            // Filtros opcionales
            if (proveedorId.HasValue)
            {
                query = query.Where(g => g.ProveedorId == proveedorId.Value);
            }

            if (categoriaId.HasValue)
            {
                query = query.Where(g => g.CategoriaGastoId == categoriaId.Value);
            }

            // Ejecutar query con navegación
            var gastos = await query
                .Include(g => g.Proveedor)
                .Include(g => g.CategoriaGasto)
                .AsNoTracking()
                .OrderBy(g => g.FechaGasto)
                .ThenBy(g => g.NumeroDocumento)
                .ToListAsync();

            // Calcular totales
            var totalGastos = gastos.Sum(g => g.MontoTotal);
            var totalImpuestos = gastos.Sum(g => g.MontoImpuesto);

            // Construir detalles
            var detalles = gastos.Select(g => new ReporteGastosDetalleDTO
            {
                FechaGasto = g.FechaGasto,
                NumeroDocumento = g.NumeroDocumento,
                Proveedor = g.Proveedor?.Nombre ?? "Proveedor no especificado",
                Categoria = g.CategoriaGasto?.Nombre ?? "Sin categoría",
                Descripcion = g.Descripcion,
                Subtotal = g.MontoSubtotal,
                Impuesto = g.MontoImpuesto,
                Total = g.MontoTotal,
                Moneda = g.Moneda.ToString()
            }).ToList();

            var reporte = new ReporteGastosDTO
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                TotalGastos = totalGastos,
                TotalImpuestos = totalImpuestos,
                CantidadGastos = gastos.Count,
                Detalles = detalles
            };

            return new ActionResponse<ReporteGastosDTO>
            {
                WasSuccess = true,
                Result = reporte
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<ReporteGastosDTO>
            {
                WasSuccess = false,
                Message = $"Error al generar reporte de gastos: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Genera reporte de inventario actual
    /// </summary>
    public async Task<ActionResponse<ReporteInventarioDTO>> GetReporteInventarioAsync(
        Guid empresaId,
        Guid? sucursalId = null,
        bool soloBajoStock = false)
    {
        try
        {
            // Query base: inventarios no eliminados de productos de la empresa
            var query = _context.Inventarios
                .Where(i => !i.IsDeleted
                    && i.Producto != null
                    && !i.Producto.IsDeleted
                    && i.Producto.EmpresaId == empresaId);

            // Filtro por sucursal
            if (sucursalId.HasValue)
            {
                query = query.Where(i => i.SucursalId == sucursalId.Value);
            }

            // Ejecutar query con navegación
            var inventarios = await query
                .Include(i => i.Producto)
                .Include(i => i.Sucursal)
                .AsNoTracking()
                .ToListAsync();

            // Filtro de bajo stock (en memoria, ya que depende de propiedad calculada)
            if (soloBajoStock)
            {
                inventarios = inventarios
                    .Where(i => i.Producto != null && i.CantidadActual <= i.Producto.StockMinimo)
                    .ToList();
            }

            // Calcular totales
            var valorTotal = inventarios.Sum(i => i.CantidadActual * (i.Producto?.Costo ?? 0));

            // Construir detalles
            var detalles = inventarios.Select(i => new ReporteInventarioDetalleDTO
            {
                CodigoProducto = i.Producto?.Codigo ?? "N/A",
                NombreProducto = i.Producto?.Nombre ?? "Producto sin nombre",
                Sucursal = i.Sucursal?.Nombre ?? "Sucursal no especificada",
                StockActual = i.CantidadActual,
                CostoUnitario = i.Producto?.Costo ?? 0,
                ValorTotal = i.CantidadActual * (i.Producto?.Costo ?? 0),
                EstadoStock = DeterminarEstadoStock(i.CantidadActual, i.Producto?.StockMinimo ?? 0, 0)
            }).ToList();

            var reporte = new ReporteInventarioDTO
            {
                FechaReporte = FechaCostaRicaHelper.Ahora,
                TotalProductos = detalles.Count(),
                ValorTotalInventario = valorTotal,
                Detalles = detalles
            };

            return new ActionResponse<ReporteInventarioDTO>
            {
                WasSuccess = true,
                Result = reporte
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<ReporteInventarioDTO>
            {
                WasSuccess = false,
                Message = $"Error al generar reporte de inventario: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Genera reporte de impuestos para declaraciones a Hacienda
    /// Calcula IVA de ventas, IVA de compras y saldo por pagar
    /// </summary>
    public async Task<ActionResponse<ReporteImpuestosDTO>> GetReporteImpuestosAsync(
        Guid empresaId,
        DateTime fechaInicio,
        DateTime fechaFin)
    {
        try
        {
            // 1. Obtener documentos de venta aceptados/contingencia con detalle de impuestos
            var documentosVenta = await _context.Documentos
                .Where(d => !d.IsDeleted
                    && d.EmpresaId == empresaId
                    && !d.EsDocumentoRecibido
                    && (d.Estado == EstadoDocumento.Aceptado || d.Estado == EstadoDocumento.Contingencia)
                    && d.FechaEmision >= fechaInicio
                    && d.FechaEmision <= fechaFin)
                .AsNoTracking()
                .ToListAsync();

            // 2. Obtener impuestos por línea de los documentos de venta
            var docVentaIds = documentosVenta.Select(d => d.Id).ToList();
            var impuestosVenta = await _context.Set<DocumentoDetalleImpuesto>()
                .Where(i => !i.IsDeleted && docVentaIds.Contains(i.DocumentoDetalle!.DocumentoId))
                .AsNoTracking()
                .ToListAsync();

            // 3. Obtener gastos aprobados (compras con IVA crédito)
            var gastos = await _context.Gastos
                .Where(g => !g.IsDeleted
                    && g.EmpresaId == empresaId
                    && g.Aprobado
                    && g.FechaGasto >= fechaInicio
                    && g.FechaGasto <= fechaFin)
                .AsNoTracking()
                .ToListAsync();

            // 4. Calcular totales de ventas
            var totalVentasGravadas = documentosVenta
                .Where(d => d.TotalImpuestos > 0)
                .Sum(d => d.TotalGravado);

            var totalVentasExentas = documentosVenta
                .Sum(d => d.TotalExento + d.TotalExonerado);

            var ivaVentas = documentosVenta.Sum(d => d.TotalImpuestos);

            // 5. Calcular IVA de compras (gastos)
            var ivaCompras = gastos.Sum(g => g.MontoImpuesto);

            // 6. IVA por pagar (ventas - compras)
            var ivaPorPagar = ivaVentas - ivaCompras;

            // 7. Desglose por tarifa IVA - Ventas
            var detallesPorTarifa = new List<ReporteImpuestosTarifaDTO>();

            var ventasPorTarifa = impuestosVenta
                .GroupBy(i => i.Tarifa)
                .OrderByDescending(g => g.Key)
                .Select(g => new ReporteImpuestosTarifaDTO
                {
                    Tipo = "Ventas",
                    TipoImpuesto = g.Key == 0 ? "Exento" : $"IVA {g.Key:0.##}%",
                    Tarifa = g.Key,
                    BaseImponible = g.Sum(i => i.MontoBase),
                    MontoImpuesto = g.Sum(i => i.MontoImpuesto),
                    CantidadDocumentos = g.Select(i => i.DocumentoDetalleId).Distinct().Count()
                })
                .ToList();

            detallesPorTarifa.AddRange(ventasPorTarifa);

            // Ventas exentas (documentos sin impuestos)
            var ventasExentas = documentosVenta.Sum(d => d.TotalExento + d.TotalExonerado);
            if (ventasExentas > 0)
            {
                var yaExisteExento = detallesPorTarifa.Any(d => d.Tipo == "Ventas" && d.Tarifa == 0);
                if (!yaExisteExento)
                {
                    detallesPorTarifa.Add(new ReporteImpuestosTarifaDTO
                    {
                        Tipo = "Ventas",
                        TipoImpuesto = "Exento",
                        Tarifa = 0,
                        BaseImponible = ventasExentas,
                        MontoImpuesto = 0,
                        CantidadDocumentos = documentosVenta.Count(d => d.TotalExento > 0 || d.TotalExonerado > 0)
                    });
                }
            }

            // Compras - agrupado como total (gastos no tienen desglose por tarifa)
            if (ivaCompras > 0)
            {
                detallesPorTarifa.Add(new ReporteImpuestosTarifaDTO
                {
                    Tipo = "Compras",
                    TipoImpuesto = "IVA Crédito Fiscal",
                    Tarifa = 0,
                    BaseImponible = gastos.Sum(g => g.MontoSubtotal),
                    MontoImpuesto = ivaCompras,
                    CantidadDocumentos = gastos.Count(g => g.MontoImpuesto > 0)
                });
            }

            var reporte = new ReporteImpuestosDTO
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                TotalVentasGravadas = totalVentasGravadas,
                TotalVentasExentas = totalVentasExentas,
                IVAVentas = ivaVentas,
                IVACompras = ivaCompras,
                IVAPorPagar = ivaPorPagar,
                CantidadDocumentosVenta = documentosVenta.Count,
                CantidadDocumentosCompra = gastos.Count,
                DetallesPorTarifa = detallesPorTarifa
            };

            return new ActionResponse<ReporteImpuestosDTO>
            {
                WasSuccess = true,
                Result = reporte
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<ReporteImpuestosDTO>
            {
                WasSuccess = false,
                Message = $"Error al generar reporte de impuestos: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Genera reporte de actividad de clientes
    /// </summary>
    public async Task<ActionResponse<ReporteClientesDTO>> GetReporteClientesAsync(
        Guid empresaId,
        DateTime fechaInicio,
        DateTime fechaFin,
        bool soloActivos = false)
    {
        try
        {
            // 1. Obtener todos los clientes de la empresa
            var clientes = await _context.Clientes
                .Where(c => !c.IsDeleted && c.EmpresaId == empresaId)
                .AsNoTracking()
                .ToListAsync();

            // 2. Obtener documentos del período (excluye Borrador, Rechazado, Anulado y Error)
            var estadosExcluidos = new[] { EstadoDocumento.Borrador, EstadoDocumento.Rechazado, EstadoDocumento.Anulado, EstadoDocumento.Error };
            var documentos = await _context.Documentos
                .Where(d => !d.IsDeleted
                    && d.EmpresaId == empresaId
                    && !d.EsDocumentoRecibido
                    && !estadosExcluidos.Contains(d.Estado)
                    && d.FechaEmision >= fechaInicio
                    && d.FechaEmision <= fechaFin
                    && d.ClienteId != null)
                .AsNoTracking()
                .ToListAsync();

            // 3. Obtener documentos de crédito (condición venta = 02)
            var documentosCredito = await _context.Documentos
                .Where(d => !d.IsDeleted
                    && d.EmpresaId == empresaId
                    && !d.EsDocumentoRecibido
                    && d.CondicionVenta == "02"
                    && !estadosExcluidos.Contains(d.Estado)
                    && d.ClienteId != null)
                .AsNoTracking()
                .ToListAsync();

            // 4. Construir detalles por cliente
            var detalles = new List<ReporteClientesDetalleDTO>();

            foreach (var cliente in clientes)
            {
                var compras = documentos.Where(d => d.ClienteId == cliente.Id).ToList();
                var cantidadCompras = compras.Count;
                var totalCompras = compras.Sum(d => d.TotalVenta);
                var ultimaCompra = compras.Any() ? compras.Max(d => d.FechaEmision) : (DateTime?)null;

                // Calcular saldo pendiente (créditos sin pagar completamente)
                var creditosCliente = documentosCredito.Where(d => d.ClienteId == cliente.Id).ToList();
                var saldoPendiente = creditosCliente.Sum(d => d.TotalVenta); // Simplificado - se puede mejorar con tabla de pagos

                // Filtrar solo activos si se requiere
                if (soloActivos && cantidadCompras == 0)
                {
                    continue;
                }

                detalles.Add(new ReporteClientesDetalleDTO
                {
                    ClienteId = cliente.Id,
                    NombreCliente = cliente.Nombre,
                    Identificacion = cliente.NumeroIdentificacion,
                    CantidadCompras = cantidadCompras,
                    TotalCompras = totalCompras,
                    UltimaCompra = ultimaCompra,
                    SaldoPendiente = saldoPendiente
                });
            }

            // Ordenar por total de compras descendente
            detalles = detalles.OrderByDescending(d => d.TotalCompras).ToList();

            var reporte = new ReporteClientesDTO
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                TotalClientes = detalles.Count,
                ClientesActivos = detalles.Count(d => d.CantidadCompras > 0),
                Detalles = detalles
            };

            return new ActionResponse<ReporteClientesDTO>
            {
                WasSuccess = true,
                Result = reporte
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<ReporteClientesDTO>
            {
                WasSuccess = false,
                Message = $"Error al generar reporte de clientes: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Genera reporte de ventas por producto
    /// </summary>
    public async Task<ActionResponse<ReporteProductosDTO>> GetReporteProductosAsync(
        Guid empresaId,
        DateTime fechaInicio,
        DateTime fechaFin,
        Guid? categoriaId = null)
    {
        try
        {
            // 1. Obtener detalles de documentos del período (excluye Borrador, Rechazado, Anulado y Error)
            var estadosExcluidos = new[] { EstadoDocumento.Borrador, EstadoDocumento.Rechazado, EstadoDocumento.Anulado, EstadoDocumento.Error };
            var detallesQuery = _context.DocumentoDetalles
                .Where(dd => !dd.IsDeleted
                    && dd.Documento != null
                    && !dd.Documento.IsDeleted
                    && dd.Documento.EmpresaId == empresaId
                    && !dd.Documento.EsDocumentoRecibido
                    && !estadosExcluidos.Contains(dd.Documento.Estado)
                    && dd.Documento.FechaEmision >= fechaInicio
                    && dd.Documento.FechaEmision <= fechaFin);

            // Filtro por categoría
            if (categoriaId.HasValue)
            {
                detallesQuery = detallesQuery.Where(dd => dd.Producto != null && dd.Producto.CategoriaId == categoriaId.Value);
            }

            var detalles = await detallesQuery
                .Include(dd => dd.Producto)
                .AsNoTracking()
                .ToListAsync();

            // 2. Agrupar por producto
            var productosVendidos = detalles
                .Where(dd => dd.Producto != null)
                .GroupBy(dd => dd.ProductoId)
                .Select(g => new ReporteProductosDetalleDTO
                {
                    ProductoId = g.Key ?? Guid.Empty,
                    CodigoProducto = g.First().Producto?.Codigo ?? "N/A",
                    NombreProducto = g.First().Producto?.Nombre ?? "Producto sin nombre",
                    CantidadVendida = g.Sum(dd => dd.Cantidad),
                    TotalVentas = g.Sum(dd => dd.MontoTotalLinea),
                    PromedioVenta = g.Average(dd => dd.PrecioUnitario)
                })
                .OrderByDescending(p => p.TotalVentas)
                .ToList();

            var reporte = new ReporteProductosDTO
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                TotalProductos = productosVendidos.Count,
                Detalles = productosVendidos
            };

            return new ActionResponse<ReporteProductosDTO>
            {
                WasSuccess = true,
                Result = reporte
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<ReporteProductosDTO>
            {
                WasSuccess = false,
                Message = $"Error al generar reporte de productos: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Genera reporte de movimientos de inventario
    /// </summary>
    public async Task<ActionResponse<ReporteMovimientosInventarioDTO>> GetReporteMovimientosInventarioAsync(
        Guid empresaId,
        DateTime fechaInicio,
        DateTime fechaFin,
        Guid? productoId = null,
        TipoMovimientoInventario? tipoMovimiento = null)
    {
        try
        {
            // Query base: movimientos del período de productos de la empresa
            var query = _context.MovimientosInventario
                .Where(m => m.Inventario != null
                    && m.Inventario.Producto != null
                    && m.Inventario.Producto.EmpresaId == empresaId
                    && m.Fecha >= fechaInicio
                    && m.Fecha <= fechaFin);

            // Filtros opcionales
            if (productoId.HasValue)
            {
                query = query.Where(m => m.Inventario != null && m.Inventario.ProductoId == productoId.Value);
            }

            if (tipoMovimiento.HasValue)
            {
                query = query.Where(m => m.TipoMovimiento == tipoMovimiento.Value);
            }

            // Ejecutar query con navegación
            var movimientos = await query
                .Include(m => m.Inventario)
                    .ThenInclude(i => i!.Producto)
                .Include(m => m.UsuarioCreacion)
                .AsNoTracking()
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();

            // Construir detalles
            var detalles = movimientos.Select(m => new ReporteMovimientosDetalleDTO
            {
                Fecha = m.Fecha,
                TipoMovimiento = ObtenerNombreTipoMovimiento(m.TipoMovimiento),
                Producto = m.Inventario?.Producto?.Nombre ?? "Producto no especificado",
                Cantidad = m.Cantidad,
                Referencia = m.Referencia ?? "Sin referencia",
                Usuario = m.UsuarioCreacion?.Email ?? "Usuario no especificado"
            }).ToList();

            var reporte = new ReporteMovimientosInventarioDTO
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                TotalMovimientos = detalles.Count,
                Detalles = detalles
            };

            return new ActionResponse<ReporteMovimientosInventarioDTO>
            {
                WasSuccess = true,
                Result = reporte
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<ReporteMovimientosInventarioDTO>
            {
                WasSuccess = false,
                Message = $"Error al generar reporte de movimientos de inventario: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Genera Libro de Ventas mensual (requerido por Hacienda)
    /// Lista cronológica de todas las facturas emitidas en el mes
    /// </summary>
    public async Task<ActionResponse<ReporteVentasDTO>> GetReporteLibroVentasAsync(
        Guid empresaId,
        int mes,
        int ano)
    {
        try
        {
            // Validar mes
            if (mes < 1 || mes > 12)
            {
                return new ActionResponse<ReporteVentasDTO>
                {
                    WasSuccess = false,
                    Message = "El mes debe estar entre 1 y 12"
                };
            }

            // Calcular fechas del mes
            var fechaInicio = new DateTime(ano, mes, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            // Obtener todos los documentos emitidos en el mes (aceptados y contingencia)
            var documentos = await _context.Documentos
                .Where(d => !d.IsDeleted
                    && d.EmpresaId == empresaId
                    && !d.EsDocumentoRecibido
                    && (d.Estado == EstadoDocumento.Aceptado || d.Estado == EstadoDocumento.Contingencia)
                    && d.FechaEmision >= fechaInicio
                    && d.FechaEmision <= fechaFin)
                .Include(d => d.Cliente)
                .AsNoTracking()
                .OrderBy(d => d.FechaEmision)
                .ThenBy(d => d.NumeroConsecutivo)
                .ToListAsync();

            // Calcular totales
            var totalVentas = documentos.Sum(d => d.TotalVenta);
            var totalImpuestos = documentos.Sum(d => d.TotalImpuestos);
            var totalDescuentos = documentos.Sum(d => d.TotalDescuentos);

            // Construir detalles agrupados por tipo de documento
            var detalles = documentos.Select(d => new ReporteVentasDetalleDTO
            {
                FechaEmision = d.FechaEmision,
                NumeroConsecutivo = d.NumeroConsecutivo,
                TipoDocumento = ObtenerNombreTipoDocumento(d.TipoDocumento),
                Cliente = $"{d.ReceptorNombre ?? d.Cliente?.Nombre ?? "Cliente no especificado"} - {d.ReceptorNumeroIdentificacion ?? "Sin ID"}",
                Subtotal = d.Subtotal,
                Impuestos = d.TotalImpuestos,
                Descuentos = d.TotalDescuentos,
                Total = d.TotalVenta,
                Moneda = d.Moneda.ToString()
            }).ToList();

            var reporte = new ReporteVentasDTO
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                TotalVentas = totalVentas,
                TotalImpuestos = totalImpuestos,
                TotalDescuentos = totalDescuentos,
                CantidadDocumentos = documentos.Count,
                Detalles = detalles
            };

            return new ActionResponse<ReporteVentasDTO>
            {
                WasSuccess = true,
                Result = reporte
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<ReporteVentasDTO>
            {
                WasSuccess = false,
                Message = $"Error al generar Libro de Ventas: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Genera reporte de retenciones de renta (D-103) por período
    /// Agrupa gastos con retención por proveedor
    /// </summary>
    public async Task<ActionResponse<ReporteRetencionesDTO>> GetReporteRetencionesAsync(
        Guid empresaId,
        DateTime fechaInicio,
        DateTime fechaFin)
    {
        try
        {
            var gastos = await _context.Gastos
                .Where(g => !g.IsDeleted
                    && g.EmpresaId == empresaId
                    && g.FechaGasto >= fechaInicio
                    && g.FechaGasto <= fechaFin
                    && g.MontoRetencionRenta > 0)
                .Include(g => g.Proveedor)
                .AsNoTracking()
                .ToListAsync();

            var detalles = gastos
                .GroupBy(g => g.ProveedorId)
                .Select(grp =>
                {
                    var primero = grp.First();
                    var baseImponible = grp.Sum(g => g.MontoSubtotal);
                    var montoRetencion = grp.Sum(g => g.MontoRetencionRenta);
                    var montoRetencionIva = grp.Sum(g => g.MontoRetencionIVA);
                    var tarifa = baseImponible > 0
                        ? Math.Round((montoRetencion / baseImponible) * 100, 2)
                        : 0;

                    return new RetencionDetalleDTO
                    {
                        ProveedorNombre = primero.Proveedor?.Nombre ?? "Proveedor no especificado",
                        ProveedorIdentificacion = primero.Proveedor?.NumeroIdentificacion ?? "",
                        TipoIdentificacion = primero.Proveedor?.TipoIdentificacion.ToString() ?? "",
                        BaseImponible = baseImponible,
                        TarifaRetencion = tarifa,
                        MontoRetencion = montoRetencion,
                        MontoRetencionIVA = montoRetencionIva,
                        CantidadDocumentos = grp.Count()
                    };
                })
                .OrderByDescending(d => d.MontoRetencion)
                .ToList();

            var reporte = new ReporteRetencionesDTO
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                TotalRetencionRenta = detalles.Sum(d => d.MontoRetencion),
                TotalRetencionIVA = detalles.Sum(d => d.MontoRetencionIVA),
                TotalBaseImponible = detalles.Sum(d => d.BaseImponible),
                CantidadDocumentos = gastos.Count,
                CantidadProveedores = detalles.Count,
                Detalles = detalles
            };

            return new ActionResponse<ReporteRetencionesDTO>
            {
                WasSuccess = true,
                Result = reporte
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<ReporteRetencionesDTO>
            {
                WasSuccess = false,
                Message = $"Error al generar reporte de retenciones: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Exporta reporte de retenciones a Excel (formato D-103)
    /// </summary>
    public async Task<ActionResponse<byte[]>> ExportarRetencionesExcelAsync(
        Guid empresaId,
        DateTime fechaInicio,
        DateTime fechaFin)
    {
        try
        {
            var reporteResponse = await GetReporteRetencionesAsync(empresaId, fechaInicio, fechaFin);
            if (!reporteResponse.WasSuccess || reporteResponse.Result == null)
            {
                return new ActionResponse<byte[]>
                {
                    WasSuccess = false,
                    Message = reporteResponse.Message ?? "Error al generar reporte"
                };
            }

            var reporte = reporteResponse.Result;

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("D-103 Retenciones");

            // Header
            ws.Cell("A1").Value = "Declaración D-103 - Retenciones de Renta";
            ws.Cell("A1").Style.Font.Bold = true;
            ws.Cell("A1").Style.Font.FontSize = 14;
            ws.Range("A1:F1").Merge();

            ws.Cell("A2").Value = $"Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}";
            ws.Range("A2:F2").Merge();

            // Column headers
            int row = 4;
            var headers = new[] { "Tipo ID", "Identificación", "Nombre Proveedor", "Base Imponible", "Tarifa %", "Retención Renta", "Retención IVA", "# Docs" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(row, i + 1).Value = headers[i];
                ws.Cell(row, i + 1).Style.Font.Bold = true;
                ws.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 51, 102);
                ws.Cell(row, i + 1).Style.Font.FontColor = XLColor.White;
            }

            // Data
            row++;
            foreach (var det in reporte.Detalles)
            {
                ws.Cell(row, 1).Value = det.TipoIdentificacion;
                ws.Cell(row, 2).Value = det.ProveedorIdentificacion;
                ws.Cell(row, 3).Value = det.ProveedorNombre;
                ws.Cell(row, 4).Value = det.BaseImponible;
                ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 5).Value = det.TarifaRetencion;
                ws.Cell(row, 5).Style.NumberFormat.Format = "0.00";
                ws.Cell(row, 6).Value = det.MontoRetencion;
                ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 7).Value = det.MontoRetencionIVA;
                ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 8).Value = det.CantidadDocumentos;
                row++;
            }

            // Totals
            ws.Cell(row, 3).Value = "TOTALES";
            ws.Cell(row, 3).Style.Font.Bold = true;
            ws.Cell(row, 4).Value = reporte.TotalBaseImponible;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 4).Style.Font.Bold = true;
            ws.Cell(row, 6).Value = reporte.TotalRetencionRenta;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 6).Style.Font.Bold = true;
            ws.Cell(row, 7).Value = reporte.TotalRetencionIVA;
            ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 7).Style.Font.Bold = true;
            ws.Cell(row, 8).Value = reporte.CantidadDocumentos;
            ws.Cell(row, 8).Style.Font.Bold = true;

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);

            return new ActionResponse<byte[]>
            {
                WasSuccess = true,
                Result = ms.ToArray()
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<byte[]>
            {
                WasSuccess = false,
                Message = $"Error al exportar retenciones a Excel: {ex.Message}"
            };
        }
    }

    #region Helper Methods

    /// <summary>
    /// Obtiene el nombre descriptivo del tipo de documento
    /// </summary>
    private string ObtenerNombreTipoDocumento(DocumentoTipo tipo)
    {
        return tipo switch
        {
            DocumentoTipo.FacturaElectronica => "Factura Electrónica",
            DocumentoTipo.NotaDebitoElectronica => "Nota de Débito",
            DocumentoTipo.NotaCreditoElectronica => "Nota de Crédito",
            DocumentoTipo.TiqueteElectronico => "Tiquete Electrónico",
            DocumentoTipo.FacturaElectronicaExportacion => "Factura de Exportación",
            DocumentoTipo.FacturaElectronicaCompra => "Factura Electrónica de Compra",
            DocumentoTipo.NotaCreditoElectronicaCompra => "Nota de Crédito de Compra",
            DocumentoTipo.NotaDebitoElectronicaCompra => "Nota de Débito de Compra",
            DocumentoTipo.ComprobanteElectronicoCompra => "Comprobante de Compra",
            DocumentoTipo.ReciboElectronicoPago => "Recibo Electrónico de Pago",
            _ => "Documento desconocido"
        };
    }

    /// <summary>
    /// Obtiene el nombre descriptivo del tipo de movimiento de inventario
    /// </summary>
    private string ObtenerNombreTipoMovimiento(TipoMovimientoInventario tipo)
    {
        return tipo switch
        {
            TipoMovimientoInventario.Compra => "Compra de Proveedor",
            TipoMovimientoInventario.Venta => "Venta a Cliente",
            TipoMovimientoInventario.AjusteEntrada => "Ajuste de Entrada",
            TipoMovimientoInventario.AjusteSalida => "Ajuste de Salida",
            TipoMovimientoInventario.TrasladoEntrada => "Traslado Entrada",
            TipoMovimientoInventario.TrasladoSalida => "Traslado Salida",
            TipoMovimientoInventario.DevolucionCliente => "Devolución de Cliente",
            TipoMovimientoInventario.DevolucionProveedor => "Devolución a Proveedor",
            TipoMovimientoInventario.Merma => "Merma o Pérdida",
            _ => "Movimiento desconocido"
        };
    }

    /// <summary>
    /// Determina el estado del stock basado en niveles mínimo y máximo
    /// </summary>
    private string DeterminarEstadoStock(decimal stockActual, decimal stockMinimo, decimal stockMaximo)
    {
        if (stockActual <= 0)
        {
            return "Agotado";
        }
        else if (stockActual <= stockMinimo)
        {
            return "Bajo";
        }
        else if (stockMaximo > 0 && stockActual >= stockMaximo)
        {
            return "Exceso";
        }
        else
        {
            return "Normal";
        }
    }

    #endregion
}
