using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Services.Implementations;

public class GastoService : IGastoService
{
    private readonly IGastoRepository _gastoRepository;
    private readonly ICategoriaGastoRepository _categoriaGastoRepository;
    private readonly IProveedorRepository _proveedorRepository;
    private readonly IContabilidadIntegracionService _contabilidadIntegracion;
    private readonly ILogger<GastoService> _logger;

    public GastoService(
        IGastoRepository gastoRepository,
        ICategoriaGastoRepository categoriaGastoRepository,
        IProveedorRepository proveedorRepository,
        IContabilidadIntegracionService contabilidadIntegracion,
        ILogger<GastoService> logger)
    {
        _gastoRepository = gastoRepository;
        _categoriaGastoRepository = categoriaGastoRepository;
        _proveedorRepository = proveedorRepository;
        _contabilidadIntegracion = contabilidadIntegracion;
        _logger = logger;
    }

    public async Task<Gasto> CrearGastoAsync(GastoDTO dto, Guid empresaId, string userId)
    {
        // Validar que la categoría exista y esté activa
        var categoria = await _categoriaGastoRepository.GetAsync(dto.CategoriaGastoId);
        if (categoria == null || !categoria.Activa)
        {
            throw new InvalidOperationException("La categoría de gasto no existe o está inactiva.");
        }

        // Validar proveedor si se especifica
        if (dto.ProveedorId.HasValue)
        {
            var proveedor = await _proveedorRepository.GetAsync(dto.ProveedorId.Value);
            if (proveedor == null || proveedor.EmpresaId != empresaId)
            {
                throw new InvalidOperationException("El proveedor no existe o no pertenece a esta empresa.");
            }
        }

        var gasto = new Gasto
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            ProveedorId = dto.ProveedorId,
            CategoriaGastoId = dto.CategoriaGastoId,
            NumeroDocumento = dto.NumeroDocumento,
            FechaGasto = dto.FechaGasto,
            Descripcion = dto.Descripcion,
            MontoSubtotal = dto.MontoSubtotal,
            MontoImpuesto = dto.MontoImpuesto,
            MontoTotal = dto.MontoTotal,
            Moneda = dto.Moneda,
            TipoCambio = dto.TipoCambio,
            FormaPago = dto.FormaPago,
            EstadoPago = dto.EstadoPago,
            MontoPagado = dto.MontoPagado,
            FechaPago = dto.FechaPago,
            Comprobante = dto.Comprobante,
            Aprobado = false, // Por defecto no aprobado
            UsuarioCreacionId = userId,
            FechaCreacion = FechaCostaRicaHelper.Ahora
        };

        return await _gastoRepository.AddAsync(gasto);
    }

    public async Task<Gasto> ActualizarGastoAsync(Guid id, GastoDTO dto, string userId)
    {
        var gasto = await _gastoRepository.GetAsync(id);
        if (gasto == null)
        {
            throw new InvalidOperationException("El gasto no existe.");
        }

        // No permitir modificar gastos aprobados
        if (gasto.Aprobado)
        {
            throw new InvalidOperationException("No se puede modificar un gasto ya aprobado.");
        }

        // Validar que la categoría exista y esté activa
        var categoria = await _categoriaGastoRepository.GetAsync(dto.CategoriaGastoId);
        if (categoria == null || !categoria.Activa)
        {
            throw new InvalidOperationException("La categoría de gasto no existe o está inactiva.");
        }

        // Validar proveedor si se especifica
        if (dto.ProveedorId.HasValue)
        {
            var proveedor = await _proveedorRepository.GetAsync(dto.ProveedorId.Value);
            if (proveedor == null || proveedor.EmpresaId != gasto.EmpresaId)
            {
                throw new InvalidOperationException("El proveedor no existe o no pertenece a esta empresa.");
            }
        }

        gasto.ProveedorId = dto.ProveedorId;
        gasto.CategoriaGastoId = dto.CategoriaGastoId;
        gasto.NumeroDocumento = dto.NumeroDocumento;
        gasto.FechaGasto = dto.FechaGasto;
        gasto.Descripcion = dto.Descripcion;
        gasto.MontoSubtotal = dto.MontoSubtotal;
        gasto.MontoImpuesto = dto.MontoImpuesto;
        gasto.MontoTotal = dto.MontoTotal;
        gasto.Moneda = dto.Moneda;
        gasto.TipoCambio = dto.TipoCambio;
        gasto.FormaPago = dto.FormaPago;
        gasto.MontoPagado = dto.MontoPagado;
        gasto.FechaPago = dto.FechaPago;
        gasto.Comprobante = dto.Comprobante;
        gasto.UsuarioModificacionId = userId;

        await _gastoRepository.UpdateAsync(gasto);
        return gasto;
    }

    public async Task<Gasto> RegistrarPagoAsync(RegistrarPagoDTO dto, string userId)
    {
        var gasto = await _gastoRepository.GetAsync(dto.GastoId);
        if (gasto == null)
        {
            throw new InvalidOperationException("El gasto no existe.");
        }

        if (gasto.EstadoPago == EstadoPago.Pagado)
        {
            throw new InvalidOperationException("Este gasto ya está completamente pagado.");
        }

        // Validar que el monto del pago no exceda el saldo pendiente
        if (dto.MontoPago > gasto.SaldoPendiente)
        {
            throw new InvalidOperationException($"El monto del pago ({dto.MontoPago:C2}) excede el saldo pendiente ({gasto.SaldoPendiente:C2}).");
        }

        gasto.MontoPagado += dto.MontoPago;
        gasto.FechaPago = dto.FechaPago;
        gasto.UsuarioModificacionId = userId;

        await _gastoRepository.UpdateAsync(gasto);

        // Generar asiento contable para el pago (si está habilitado)
        try
        {
            await _contabilidadIntegracion.GenerarAsientoPagoGastoAsync(gasto, dto.MontoPago, userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error generando asiento contable para pago de gasto {GastoId} - operación continúa", gasto.Id);
        }

        return gasto;
    }

    public async Task<Gasto> AprobarGastoAsync(AprobarGastoDTO dto, string userId)
    {
        var gasto = await _gastoRepository.GetAsync(dto.GastoId);
        if (gasto == null)
        {
            throw new InvalidOperationException("El gasto no existe.");
        }

        gasto.Aprobado = dto.Aprobado;
        gasto.UsuarioAprobacionId = userId;
        gasto.FechaAprobacion = FechaCostaRicaHelper.Ahora;
        gasto.NotasAprobacion = dto.Notas;
        gasto.UsuarioModificacionId = userId;

        await _gastoRepository.UpdateAsync(gasto);

        // Generar asiento contable para la compra aprobada (si está habilitado)
        if (dto.Aprobado)
        {
            try
            {
                await _contabilidadIntegracion.GenerarAsientoCompraAsync(gasto, userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error generando asiento contable para gasto aprobado {GastoId} - operación continúa", gasto.Id);
            }
        }

        return gasto;
    }

    public async Task<EstadisticasGastosDTO> GetEstadisticasAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin)
    {
        var gastos = await _gastoRepository.GetByFechaRangoAsync(empresaId, fechaInicio, fechaFin);
        var gastosList = gastos.ToList();

        var estadisticas = new EstadisticasGastosDTO
        {
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            TotalGastos = gastosList.Sum(g => g.MontoTotal),
            TotalPagado = gastosList.Sum(g => g.MontoPagado),
            TotalPendiente = gastosList.Sum(g => g.SaldoPendiente),
            CantidadGastos = gastosList.Count,
            GastosAprobados = gastosList.Count(g => g.Aprobado),
            GastosPendientesAprobacion = gastosList.Count(g => !g.Aprobado),
            GastosPorCategoria = (await _gastoRepository.GetTotalPorCategoriaAsync(empresaId, fechaInicio, fechaFin)).ToList(),
            GastosPorProveedor = (await _gastoRepository.GetTotalPorProveedorAsync(empresaId, fechaInicio, fechaFin)).ToList(),
            GastosPorMes = (await _gastoRepository.GetTotalPorMesAsync(empresaId, fechaInicio, fechaFin)).ToList()
        };

        return estadisticas;
    }

    public async Task<IEnumerable<GastoResumenDTO>> GetResumenGastosAsync(Guid empresaId, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        IEnumerable<Gasto> gastos;

        if (fechaInicio.HasValue && fechaFin.HasValue)
        {
            gastos = await _gastoRepository.GetByFechaRangoAsync(empresaId, fechaInicio.Value, fechaFin.Value);
        }
        else
        {
            gastos = await _gastoRepository.GetByEmpresaAsync(empresaId);
        }

        return gastos.Select(g => new GastoResumenDTO
        {
            Id = g.Id,
            EmpresaId = g.EmpresaId,
            NumeroDocumento = g.NumeroDocumento,
            FechaGasto = g.FechaGasto,
            Descripcion = g.Descripcion,
            MontoSubtotal = g.MontoSubtotal,
            MontoImpuesto = g.MontoImpuesto,
            MontoTotal = g.MontoTotal,
            Moneda = g.Moneda,
            TipoCambio = g.TipoCambio,
            FormaPago = g.FormaPago,
            EstadoPago = g.EstadoPago,
            MontoPagado = g.MontoPagado,
            SaldoPendiente = g.SaldoPendiente,
            FechaPago = g.FechaPago,
            Aprobado = g.Aprobado,
            FechaAprobacion = g.FechaAprobacion,
            NotasAprobacion = g.NotasAprobacion,
            ProveedorId = g.ProveedorId,
            ProveedorNombre = g.Proveedor?.Nombre,
            CategoriaGastoId = g.CategoriaGastoId,
            CategoriaGastoNombre = g.CategoriaGasto!.Nombre,
            UsuarioCreacionNombre = g.UsuarioCreacion?.FullName,
            UsuarioAprobacionNombre = g.UsuarioAprobacion?.FullName,
            FechaCreacion = g.FechaCreacion
        }).ToList();
    }
}
