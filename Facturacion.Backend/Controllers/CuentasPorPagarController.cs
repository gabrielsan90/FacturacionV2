using Facturacion.Backend.Data;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

/// <summary>
/// Controlador para gestión de cuentas por pagar a proveedores.
/// Incluye registro de facturas, abonos, reportes de antigüedad y flujo de caja.
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class CuentasPorPagarController : ControllerBase
{
    private readonly ICuentaPorPagarUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly ILogger<CuentasPorPagarController> _logger;

    public CuentasPorPagarController(
        ICuentaPorPagarUnitOfWork unitOfWork,
        DataContext context,
        ILogger<CuentasPorPagarController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las cuentas por pagar de una empresa.
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>Lista de cuentas por pagar</returns>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var action = await _unitOfWork.CuentaPorPagarRepository.GetByEmpresaAsync(empresaId);
        if (!action.WasSuccess)
        {
            _logger.LogWarning("Error al obtener cuentas por pagar de empresa {EmpresaId}: {Message}",
                empresaId, action.Message);
            return BadRequest(action.Message);
        }

        var cuentas = action.Result!.Select(c => new CuentaPorPagarResumenDTO
        {
            Id = c.Id,
            NumeroFactura = c.NumeroFactura,
            FechaFactura = c.FechaFactura,
            FechaVencimiento = c.FechaVencimiento,
            MontoOriginal = c.MontoOriginal,
            MontoSaldo = c.MontoSaldo,
            Estado = c.Estado,
            EstadoDescripcion = c.EstadoDescripcion,
            DiasVencido = c.DiasVencido,
            MontoAbonado = c.MontoAbonado
        }).ToList();

        return Ok(cuentas);
    }

    /// <summary>
    /// Obtiene las cuentas por pagar pendientes (estado Pendiente o Parcial).
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>Lista de cuentas pendientes</returns>
    [HttpGet("empresa/{empresaId:guid}/pendientes")]
    public async Task<IActionResult> GetPendientesAsync(Guid empresaId)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var action = await _unitOfWork.CuentaPorPagarRepository.GetPendientesAsync(empresaId);
        if (!action.WasSuccess)
        {
            _logger.LogWarning("Error al obtener cuentas por pagar pendientes de empresa {EmpresaId}: {Message}",
                empresaId, action.Message);
            return BadRequest(action.Message);
        }

        var cuentas = action.Result!.Select(c => new CuentaPorPagarResumenDTO
        {
            Id = c.Id,
            NumeroFactura = c.NumeroFactura,
            FechaFactura = c.FechaFactura,
            FechaVencimiento = c.FechaVencimiento,
            MontoOriginal = c.MontoOriginal,
            MontoSaldo = c.MontoSaldo,
            Estado = c.Estado,
            EstadoDescripcion = c.EstadoDescripcion,
            DiasVencido = c.DiasVencido,
            MontoAbonado = c.MontoAbonado
        }).ToList();

        return Ok(cuentas);
    }

    /// <summary>
    /// Obtiene las cuentas por pagar vencidas.
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>Lista de cuentas vencidas</returns>
    [HttpGet("empresa/{empresaId:guid}/vencidas")]
    public async Task<IActionResult> GetVencidasAsync(Guid empresaId)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var action = await _unitOfWork.CuentaPorPagarRepository.GetVencidasAsync(empresaId);
        if (!action.WasSuccess)
        {
            _logger.LogWarning("Error al obtener cuentas por pagar vencidas de empresa {EmpresaId}: {Message}",
                empresaId, action.Message);
            return BadRequest(action.Message);
        }

        var cuentas = action.Result!.Select(c => new CuentaPorPagarResumenDTO
        {
            Id = c.Id,
            NumeroFactura = c.NumeroFactura,
            FechaFactura = c.FechaFactura,
            FechaVencimiento = c.FechaVencimiento,
            MontoOriginal = c.MontoOriginal,
            MontoSaldo = c.MontoSaldo,
            Estado = c.Estado,
            EstadoDescripcion = c.EstadoDescripcion,
            DiasVencido = c.DiasVencido,
            MontoAbonado = c.MontoAbonado
        }).ToList();

        return Ok(cuentas);
    }

    /// <summary>
    /// Obtiene una cuenta por pagar específica con todos sus detalles y abonos.
    /// </summary>
    /// <param name="id">ID de la cuenta por pagar</param>
    /// <returns>Cuenta por pagar con detalle completo</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var action = await _unitOfWork.CuentaPorPagarRepository.GetAsync(id);
        if (!action.WasSuccess)
        {
            _logger.LogWarning("Error al obtener cuenta por pagar {Id}: {Message}", id, action.Message);
            return NotFound(action.Message);
        }

        var cuenta = action.Result!;

        if (!await TieneAccesoEmpresaAsync(cuenta.EmpresaId))
        {
            return Forbid();
        }

        // Get abonos with full details
        var abonosAction = await _unitOfWork.AbonoPagoRepository.GetByCuentaPorPagarAsync(id);
        var abonos = abonosAction.WasSuccess ? abonosAction.Result : Enumerable.Empty<AbonoPago>();

        var dto = new CuentaPorPagarDetalleDTO
        {
            Id = cuenta.Id,
            NumeroFactura = cuenta.NumeroFactura,
            FechaFactura = cuenta.FechaFactura,
            FechaVencimiento = cuenta.FechaVencimiento,
            MontoOriginal = cuenta.MontoOriginal,
            MontoSaldo = cuenta.MontoSaldo,
            MontoAbonado = cuenta.MontoAbonado,
            Estado = cuenta.Estado,
            EstadoDescripcion = cuenta.EstadoDescripcion,
            DiasVencido = cuenta.DiasVencido,
            EstaVencida = cuenta.EstaVencida,
            Observaciones = cuenta.Observaciones,
            Moneda = cuenta.Moneda,
            TipoCambio = cuenta.TipoCambio,
            OrdenCompraId = cuenta.OrdenCompraId,
            OrdenCompraNumero = cuenta.OrdenCompra?.Numero,
            Abonos = abonos.Select(a => new AbonoPagoDTO
            {
                Id = a.Id,
                FechaPago = a.FechaPago,
                Monto = a.Monto,
                MetodoPago = a.MetodoPago,
                MetodoPagoDescripcion = a.MetodoPagoDescripcion,
                NumeroReferencia = a.NumeroReferencia,
                CuentaBancariaNombre = a.CuentaBancaria?.Nombre,
                Notas = a.Notas,
                RegistradoPorNombre = a.RegistradoPor?.FullName
            }).OrderByDescending(a => a.FechaPago).ToList()
        };

        return Ok(dto);
    }

    /// <summary>
    /// Crea una nueva cuenta por pagar.
    /// </summary>
    /// <param name="cuenta">Datos de la cuenta por pagar</param>
    /// <returns>Cuenta por pagar creada</returns>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] CuentaPorPagar cuenta)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!await TieneAccesoEmpresaAsync(cuenta.EmpresaId))
        {
            return Forbid();
        }

        // Validaciones de negocio
        var validationError = await ValidarCuentaPorPagarAsync(cuenta);
        if (validationError != null)
        {
            return BadRequest(validationError);
        }

        // Establecer valores de auditoría
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        cuenta.Id = Guid.NewGuid();
        cuenta.CreadoPorId = userId;

        var action = await _unitOfWork.CuentaPorPagarRepository.AddAsync(cuenta);
        if (!action.WasSuccess)
        {
            _logger.LogError("Error al crear cuenta por pagar {NumeroFactura}: {Message}",
                cuenta.NumeroFactura, action.Message);
            return BadRequest(action.Message);
        }

        _logger.LogInformation("Cuenta por pagar {NumeroFactura} creada exitosamente por usuario {UserId}",
            cuenta.NumeroFactura, userId);

        return Ok(action.Result);
    }

    /// <summary>
    /// Actualiza una cuenta por pagar existente.
    /// </summary>
    /// <param name="id">ID de la cuenta</param>
    /// <param name="cuenta">Datos actualizados</param>
    /// <returns>Cuenta actualizada</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] CuentaPorPagar cuenta)
    {
        if (id != cuenta.Id)
        {
            return BadRequest("El ID no coincide.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verificar que la cuenta existe
        var existenteAction = await _unitOfWork.CuentaPorPagarRepository.GetAsync(id);
        if (!existenteAction.WasSuccess)
        {
            return NotFound("Cuenta por pagar no encontrada.");
        }

        var existente = existenteAction.Result!;

        if (!await TieneAccesoEmpresaAsync(existente.EmpresaId))
        {
            return Forbid();
        }

        // Verificar si tiene abonos usando el repositorio
        var abonosAction = await _unitOfWork.AbonoPagoRepository.GetByCuentaPorPagarAsync(id);
        if (abonosAction.WasSuccess && abonosAction.Result!.Any())
        {
            return BadRequest("No se puede editar una cuenta por pagar que tiene abonos registrados.");
        }

        // Validaciones de negocio
        var validationError = await ValidarCuentaPorPagarAsync(cuenta);
        if (validationError != null)
        {
            return BadRequest(validationError);
        }

        // Verificar número de factura único por proveedor (excluyendo la cuenta actual)
        var duplicado = await _context.CuentasPorPagar
            .FirstOrDefaultAsync(c => c.EmpresaId == cuenta.EmpresaId &&
                                     c.ProveedorId == cuenta.ProveedorId &&
                                     c.NumeroFactura == cuenta.NumeroFactura &&
                                     c.Id != id &&
                                     !c.IsDeleted);

        if (duplicado != null)
        {
            return BadRequest("Ya existe otra factura con este número para el proveedor seleccionado.");
        }

        // Establecer valores de auditoría
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        cuenta.ModificadoPorId = userId;

        var action = await _unitOfWork.CuentaPorPagarRepository.UpdateAsync(cuenta);
        if (!action.WasSuccess)
        {
            _logger.LogError("Error al actualizar cuenta por pagar {Id}: {Message}", id, action.Message);
            return BadRequest(action.Message);
        }

        _logger.LogInformation("Cuenta por pagar {Id} actualizada exitosamente por usuario {UserId}", id, userId);

        return Ok(action.Result);
    }

    /// <summary>
    /// Registra un pago (abono) a una cuenta por pagar.
    /// </summary>
    /// <param name="id">ID de la cuenta por pagar</param>
    /// <param name="pagoDto">Datos del pago</param>
    /// <returns>Cuenta por pagar actualizada</returns>
    [HttpPost("{id:guid}/pagar")]
    public async Task<IActionResult> RegistrarPagoAsync(Guid id, [FromBody] RegistrarPagoCuentaDTO pagoDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != pagoDto.CuentaPorPagarId)
        {
            return BadRequest("El ID no coincide.");
        }

        // Verificar que la cuenta existe
        var cuentaAction = await _unitOfWork.CuentaPorPagarRepository.GetAsync(id);
        if (!cuentaAction.WasSuccess)
        {
            return NotFound("Cuenta por pagar no encontrada.");
        }

        var cuenta = cuentaAction.Result!;

        if (!await TieneAccesoEmpresaAsync(cuenta.EmpresaId))
        {
            return Forbid();
        }

        // Validar que no esté pagada
        if (cuenta.Estado == EstadoCuentaPorPagar.Pagada)
        {
            return BadRequest("La cuenta por pagar ya está completamente pagada.");
        }

        // Validar que el monto no exceda el saldo
        if (pagoDto.Monto <= 0)
        {
            return BadRequest("El monto del pago debe ser mayor a cero.");
        }

        if (pagoDto.Monto > cuenta.MontoSaldo)
        {
            return BadRequest($"El monto del pago ({pagoDto.Monto:C2}) excede el saldo pendiente ({cuenta.MontoSaldo:C2}).");
        }

        // Validar cuenta bancaria si se proporcionó (mantener DataContext para validación)
        if (pagoDto.CuentaBancariaId.HasValue)
        {
            var cuentaBancaria = await _context.CuentasBancarias
                .FirstOrDefaultAsync(cb => cb.Id == pagoDto.CuentaBancariaId.Value &&
                                          cb.EmpresaId == cuenta.EmpresaId &&
                                          !cb.IsDeleted);

            if (cuentaBancaria == null)
            {
                return BadRequest("La cuenta bancaria no existe o no pertenece a la empresa.");
            }
        }

        // Crear el abono
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var abono = new AbonoPago
        {
            Id = Guid.NewGuid(),
            CuentaPorPagarId = cuenta.Id,
            FechaPago = pagoDto.FechaPago,
            Monto = pagoDto.Monto,
            MetodoPago = pagoDto.MetodoPago,
            NumeroReferencia = pagoDto.NumeroReferencia,
            CuentaBancariaId = pagoDto.CuentaBancariaId,
            Notas = pagoDto.Notas,
            RegistradoPorId = userId
        };

        var abonoAction = await _unitOfWork.AbonoPagoRepository.AddAsync(abono);
        if (!abonoAction.WasSuccess)
        {
            _logger.LogError("Error al crear abono para cuenta {CuentaId}: {Message}", id, abonoAction.Message);
            return BadRequest(abonoAction.Message);
        }

        // Actualizar saldo de la cuenta
        cuenta.MontoSaldo -= pagoDto.Monto;
        cuenta.ModificadoPorId = userId;

        // Actualizar estado según el nuevo saldo
        if (cuenta.MontoSaldo <= 0)
        {
            cuenta.Estado = EstadoCuentaPorPagar.Pagada;
            cuenta.MontoSaldo = 0; // Asegurar que quede en cero
        }
        else
        {
            cuenta.Estado = EstadoCuentaPorPagar.Parcial;
        }

        var updateAction = await _unitOfWork.CuentaPorPagarRepository.UpdateAsync(cuenta);
        if (!updateAction.WasSuccess)
        {
            _logger.LogError("Error al actualizar cuenta después de registrar pago {CuentaId}: {Message}",
                id, updateAction.Message);
            return BadRequest(updateAction.Message);
        }

        _logger.LogInformation("Pago de {Monto} registrado en cuenta {CuentaId} por usuario {UserId}",
            pagoDto.Monto, id, userId);

        // Retornar la cuenta actualizada
        var cuentaActualizadaAction = await _unitOfWork.CuentaPorPagarRepository.GetAsync(id);
        return Ok(cuentaActualizadaAction.Result);
    }

    /// <summary>
    /// Obtiene el estado de cuenta de un proveedor específico.
    /// Incluye todas las facturas y su historial de pagos.
    /// </summary>
    /// <param name="proveedorId">ID del proveedor</param>
    /// <returns>Estado de cuenta del proveedor</returns>
    [HttpGet("proveedor/{proveedorId:guid}/estado")]
    public async Task<IActionResult> GetEstadoCuentaProveedorAsync(Guid proveedorId)
    {
        var proveedor = await _context.Proveedores
            .FirstOrDefaultAsync(p => p.Id == proveedorId && !p.IsDeleted);

        if (proveedor == null)
        {
            return NotFound("Proveedor no encontrado.");
        }

        if (!await TieneAccesoEmpresaAsync(proveedor.EmpresaId))
        {
            return Forbid();
        }

        var cuentas = await _context.CuentasPorPagar
            .Where(c => c.ProveedorId == proveedorId && !c.IsDeleted)
            .Include(c => c.OrdenCompra)
            .Include(c => c.Abonos!)
                .ThenInclude(a => a.CuentaBancaria)
            .Include(c => c.Abonos!)
                .ThenInclude(a => a.RegistradoPor)
            .OrderByDescending(c => c.FechaFactura)
            .ToListAsync();

        var estadoCuenta = new EstadoCuentaProveedorDTO
        {
            ProveedorId = proveedor.Id,
            ProveedorNombre = proveedor.Nombre,
            ProveedorIdentificacion = proveedor.NumeroIdentificacion,
            EmailPrincipal = proveedor.EmailPrincipal,
            TelefonoPrincipal = proveedor.TelefonoPrincipal,
            TotalFacturas = cuentas.Sum(c => c.MontoOriginal),
            TotalPagado = cuentas.Sum(c => c.MontoAbonado),
            TotalPendiente = cuentas.Sum(c => c.MontoSaldo),
            CantidadFacturasTotal = cuentas.Count,
            CantidadFacturasPendientes = cuentas.Count(c => c.Estado == EstadoCuentaPorPagar.Pendiente ||
                                                           c.Estado == EstadoCuentaPorPagar.Parcial),
            CantidadFacturasVencidas = cuentas.Count(c => c.EstaVencida),
            CuentasPorPagar = cuentas.Select(c => new CuentaPorPagarDetalleDTO
            {
                Id = c.Id,
                NumeroFactura = c.NumeroFactura,
                FechaFactura = c.FechaFactura,
                FechaVencimiento = c.FechaVencimiento,
                MontoOriginal = c.MontoOriginal,
                MontoSaldo = c.MontoSaldo,
                MontoAbonado = c.MontoAbonado,
                Estado = c.Estado,
                EstadoDescripcion = c.EstadoDescripcion,
                DiasVencido = c.DiasVencido,
                EstaVencida = c.EstaVencida,
                Observaciones = c.Observaciones,
                Moneda = c.Moneda,
                TipoCambio = c.TipoCambio,
                OrdenCompraId = c.OrdenCompraId,
                OrdenCompraNumero = c.OrdenCompra?.Numero,
                Abonos = c.Abonos?.Select(a => new AbonoPagoDTO
                {
                    Id = a.Id,
                    FechaPago = a.FechaPago,
                    Monto = a.Monto,
                    MetodoPago = a.MetodoPago,
                    MetodoPagoDescripcion = a.MetodoPagoDescripcion,
                    NumeroReferencia = a.NumeroReferencia,
                    CuentaBancariaNombre = a.CuentaBancaria?.Nombre,
                    Notas = a.Notas,
                    RegistradoPorNombre = a.RegistradoPor?.FullName
                }).ToList() ?? new List<AbonoPagoDTO>()
            }).ToList()
        };

        return Ok(estadoCuenta);
    }

    /// <summary>
    /// Genera el reporte de antigüedad de saldos (aging report).
    /// Agrupa las cuentas por pagar por rangos de días vencidos.
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>Reporte de antigüedad de saldos</returns>
    [HttpGet("empresa/{empresaId:guid}/antiguedad")]
    public async Task<IActionResult> GetAntiguedadSaldosAsync(Guid empresaId)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var hoy = DateTime.Now.Date;
        var cuentas = await _context.CuentasPorPagar
            .Where(c => c.EmpresaId == empresaId &&
                       !c.IsDeleted &&
                       c.Estado != EstadoCuentaPorPagar.Pagada)
            .Include(c => c.Proveedor)
            .ToListAsync();

        var reportePorProveedor = cuentas
            .GroupBy(c => new
            {
                c.ProveedorId,
                ProveedorNombre = c.Proveedor!.Nombre,
                ProveedorIdentificacion = c.Proveedor!.NumeroIdentificacion
            })
            .Select(g => new AntiguedadSaldosDTO
            {
                ProveedorId = g.Key.ProveedorId,
                ProveedorNombre = g.Key.ProveedorNombre,
                ProveedorIdentificacion = g.Key.ProveedorIdentificacion,
                Corriente = g.Where(c => c.FechaVencimiento >= hoy || (hoy - c.FechaVencimiento).Days <= 30)
                            .Sum(c => c.MontoSaldo),
                Dias31a60 = g.Where(c => (hoy - c.FechaVencimiento).Days > 30 &&
                                        (hoy - c.FechaVencimiento).Days <= 60)
                            .Sum(c => c.MontoSaldo),
                Dias61a90 = g.Where(c => (hoy - c.FechaVencimiento).Days > 60 &&
                                        (hoy - c.FechaVencimiento).Days <= 90)
                            .Sum(c => c.MontoSaldo),
                Dias91a120 = g.Where(c => (hoy - c.FechaVencimiento).Days > 90 &&
                                         (hoy - c.FechaVencimiento).Days <= 120)
                             .Sum(c => c.MontoSaldo),
                MasDe120Dias = g.Where(c => (hoy - c.FechaVencimiento).Days > 120)
                              .Sum(c => c.MontoSaldo),
                TotalSaldo = g.Sum(c => c.MontoSaldo),
                CantidadFacturas = g.Count(),
                Facturas = g.Select(c => new CuentaPorPagarResumenDTO
                {
                    Id = c.Id,
                    NumeroFactura = c.NumeroFactura,
                    FechaFactura = c.FechaFactura,
                    FechaVencimiento = c.FechaVencimiento,
                    MontoOriginal = c.MontoOriginal,
                    MontoSaldo = c.MontoSaldo,
                    Estado = c.Estado,
                    EstadoDescripcion = c.EstadoDescripcion,
                    DiasVencido = c.DiasVencido,
                    MontoAbonado = c.MontoAbonado
                }).OrderBy(c => c.FechaVencimiento).ToList()
            })
            .OrderByDescending(r => r.TotalSaldo)
            .ToList();

        return Ok(reportePorProveedor);
    }

    /// <summary>
    /// Genera la proyección de flujo de caja de cuentas por pagar.
    /// Muestra los pagos programados por períodos.
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="dias">Número de días a proyectar (default: 90)</param>
    /// <returns>Proyección de flujo de caja</returns>
    [HttpGet("empresa/{empresaId:guid}/flujo-caja")]
    public async Task<IActionResult> GetFlujoCajaAsync(Guid empresaId, [FromQuery] int dias = 90)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var hoy = DateTime.Now.Date;
        var fechaFin = hoy.AddDays(dias);

        var cuentas = await _context.CuentasPorPagar
            .Where(c => c.EmpresaId == empresaId &&
                       !c.IsDeleted &&
                       c.Estado != EstadoCuentaPorPagar.Pagada)
            .Include(c => c.Proveedor)
            .ToListAsync();

        var vencidas = cuentas.Where(c => c.FechaVencimiento < hoy).ToList();
        var estaSemana = cuentas.Where(c => c.FechaVencimiento >= hoy &&
                                           c.FechaVencimiento <= hoy.AddDays(7)).ToList();
        var proximos30 = cuentas.Where(c => c.FechaVencimiento > hoy.AddDays(7) &&
                                           c.FechaVencimiento <= hoy.AddDays(30)).ToList();
        var dias31a60 = cuentas.Where(c => c.FechaVencimiento > hoy.AddDays(30) &&
                                          c.FechaVencimiento <= hoy.AddDays(60)).ToList();
        var dias61a90 = cuentas.Where(c => c.FechaVencimiento > hoy.AddDays(60) &&
                                          c.FechaVencimiento <= hoy.AddDays(90)).ToList();

        var flujo = new FlujoCajaCuentasPorPagarDTO
        {
            FechaInicio = hoy,
            FechaFin = fechaFin,
            TotalPorPagar = cuentas.Sum(c => c.MontoSaldo),
            TotalVencido = vencidas.Sum(c => c.MontoSaldo),
            TotalProximoVencer = estaSemana.Sum(c => c.MontoSaldo),
            Periodos = new List<PeriodoFlujoCajaDTO>
            {
                new PeriodoFlujoCajaDTO
                {
                    Periodo = "Vencido",
                    FechaInicio = DateTime.MinValue,
                    FechaFin = hoy.AddDays(-1),
                    Monto = vencidas.Sum(c => c.MontoSaldo),
                    CantidadFacturas = vencidas.Count
                },
                new PeriodoFlujoCajaDTO
                {
                    Periodo = "Esta Semana",
                    FechaInicio = hoy,
                    FechaFin = hoy.AddDays(7),
                    Monto = estaSemana.Sum(c => c.MontoSaldo),
                    CantidadFacturas = estaSemana.Count
                },
                new PeriodoFlujoCajaDTO
                {
                    Periodo = "Próximos 30 días",
                    FechaInicio = hoy.AddDays(8),
                    FechaFin = hoy.AddDays(30),
                    Monto = proximos30.Sum(c => c.MontoSaldo),
                    CantidadFacturas = proximos30.Count
                },
                new PeriodoFlujoCajaDTO
                {
                    Periodo = "31-60 días",
                    FechaInicio = hoy.AddDays(31),
                    FechaFin = hoy.AddDays(60),
                    Monto = dias31a60.Sum(c => c.MontoSaldo),
                    CantidadFacturas = dias31a60.Count
                },
                new PeriodoFlujoCajaDTO
                {
                    Periodo = "61-90 días",
                    FechaInicio = hoy.AddDays(61),
                    FechaFin = hoy.AddDays(90),
                    Monto = dias61a90.Sum(c => c.MontoSaldo),
                    CantidadFacturas = dias61a90.Count
                }
            },
            Proveedores = cuentas
                .GroupBy(c => new { c.ProveedorId, ProveedorNombre = c.Proveedor!.Nombre })
                .Select(g => new ProveedorFlujoCajaDTO
                {
                    ProveedorId = g.Key.ProveedorId,
                    ProveedorNombre = g.Key.ProveedorNombre,
                    TotalPendiente = g.Sum(c => c.MontoSaldo),
                    TotalVencido = g.Where(c => c.FechaVencimiento < hoy).Sum(c => c.MontoSaldo),
                    ProximoVencimiento = g.Where(c => c.FechaVencimiento >= hoy)
                                         .OrderBy(c => c.FechaVencimiento)
                                         .Select(c => c.FechaVencimiento)
                                         .FirstOrDefault(),
                    MontoProximoVencimiento = g.Where(c => c.FechaVencimiento >= hoy)
                                              .OrderBy(c => c.FechaVencimiento)
                                              .Select(c => c.MontoSaldo)
                                              .FirstOrDefault()
                })
                .OrderByDescending(p => p.TotalPendiente)
                .ToList()
        };

        return Ok(flujo);
    }

    /// <summary>
    /// Elimina (soft delete) una cuenta por pagar.
    /// Solo permite eliminar cuentas sin abonos.
    /// </summary>
    /// <param name="id">ID de la cuenta</param>
    /// <returns>Resultado de la operación</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        // Verificar que la cuenta existe
        var cuentaAction = await _unitOfWork.CuentaPorPagarRepository.GetAsync(id);
        if (!cuentaAction.WasSuccess)
        {
            return NotFound("Cuenta por pagar no encontrada.");
        }

        var cuenta = cuentaAction.Result!;

        if (!await TieneAccesoEmpresaAsync(cuenta.EmpresaId))
        {
            return Forbid();
        }

        // Verificar si tiene abonos
        var abonosAction = await _unitOfWork.AbonoPagoRepository.GetByCuentaPorPagarAsync(id);
        if (abonosAction.WasSuccess && abonosAction.Result!.Any())
        {
            return BadRequest("No se puede eliminar una cuenta por pagar que tiene abonos registrados.");
        }

        // Anular usando el repositorio
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var action = await _unitOfWork.CuentaPorPagarRepository.AnularAsync(id, userId!, "Eliminada por el usuario");

        if (!action.WasSuccess)
        {
            _logger.LogError("Error al eliminar cuenta por pagar {Id}: {Message}", id, action.Message);
            return BadRequest(action.Message);
        }

        _logger.LogInformation("Cuenta por pagar {Id} eliminada por usuario {UserId}", id, userId);

        return Ok(new { message = "Cuenta por pagar eliminada correctamente." });
    }

    /// <summary>
    /// Verifica si el usuario actual tiene acceso a la empresa especificada.
    /// </summary>
    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        // SuperUser tiene acceso a todas las empresas
        if (User.IsInRole("SuperUser"))
        {
            return true;
        }

        // Verificar si el usuario tiene acceso a la empresa
        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }

    /// <summary>
    /// Valida las reglas de negocio de una cuenta por pagar.
    /// </summary>
    private async Task<string?> ValidarCuentaPorPagarAsync(CuentaPorPagar cuenta)
    {
        // Validar que el proveedor exista y pertenezca a la empresa
        var proveedor = await _context.Proveedores
            .FirstOrDefaultAsync(p => p.Id == cuenta.ProveedorId &&
                                     p.EmpresaId == cuenta.EmpresaId &&
                                     !p.IsDeleted);

        if (proveedor == null)
        {
            return "El proveedor no existe o no pertenece a la empresa.";
        }

        // Validar que el monto original sea positivo
        if (cuenta.MontoOriginal <= 0)
        {
            return "El monto original debe ser mayor a cero.";
        }

        // Validar que la fecha de vencimiento sea posterior a la fecha de factura
        if (cuenta.FechaVencimiento < cuenta.FechaFactura)
        {
            return "La fecha de vencimiento no puede ser anterior a la fecha de factura.";
        }

        // Validar orden de compra si se proporcionó
        if (cuenta.OrdenCompraId.HasValue)
        {
            var ordenCompra = await _context.OrdenesCompra
                .FirstOrDefaultAsync(o => o.Id == cuenta.OrdenCompraId.Value &&
                                         o.EmpresaId == cuenta.EmpresaId &&
                                         !o.IsDeleted);

            if (ordenCompra == null)
            {
                return "La orden de compra no existe o no pertenece a la empresa.";
            }
        }

        return null;
    }
}
