using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class MovimientosBancariosController : ControllerBase
{
    private readonly IMovimientoBancarioUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly IContabilidadIntegracionService _contabilidadIntegracion;
    private readonly ILogger<MovimientosBancariosController> _logger;

    public MovimientosBancariosController(
        IMovimientoBancarioUnitOfWork unitOfWork,
        DataContext context,
        IContabilidadIntegracionService contabilidadIntegracion,
        ILogger<MovimientosBancariosController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _contabilidadIntegracion = contabilidadIntegracion;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los movimientos bancarios de una cuenta bancaria específica.
    /// </summary>
    [HttpGet("cuenta/{cuentaBancariaId:guid}")]
    public async Task<IActionResult> GetByCuentaBancariaAsync(Guid cuentaBancariaId)
    {
        var cuentaResponse = await _unitOfWork.CuentaBancariaRepository.GetAsync(cuentaBancariaId);

        if (!cuentaResponse.WasSuccess || cuentaResponse.Result == null)
        {
            return NotFound("Cuenta bancaria no encontrada.");
        }

        if (!await TieneAccesoEmpresaAsync(cuentaResponse.Result.EmpresaId))
        {
            return Forbid();
        }

        var movimientosResponse = await _unitOfWork.MovimientoBancarioRepository.GetByCuentaBancariaAsync(cuentaBancariaId);

        if (!movimientosResponse.WasSuccess)
        {
            return BadRequest(movimientosResponse.Message);
        }

        return Ok(movimientosResponse.Result);
    }

    /// <summary>
    /// Obtiene todos los movimientos bancarios de todas las cuentas de una empresa.
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var movimientosResponse = await _unitOfWork.MovimientoBancarioRepository.GetByEmpresaAsync(empresaId);

        if (!movimientosResponse.WasSuccess)
        {
            return BadRequest(movimientosResponse.Message);
        }

        return Ok(movimientosResponse.Result);
    }

    /// <summary>
    /// Obtiene un movimiento bancario por su ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var movimientoResponse = await _unitOfWork.MovimientoBancarioRepository.GetAsync(id);

        if (!movimientoResponse.WasSuccess || movimientoResponse.Result == null)
        {
            return NotFound("Movimiento bancario no encontrado.");
        }

        if (!await TieneAccesoEmpresaAsync(movimientoResponse.Result.EmpresaId))
        {
            return Forbid();
        }

        return Ok(movimientoResponse.Result);
    }

    /// <summary>
    /// Crea un nuevo movimiento bancario y actualiza el saldo de la cuenta.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] MovimientoBancario movimiento)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!await TieneAccesoEmpresaAsync(movimiento.EmpresaId))
        {
            return Forbid();
        }

        // Verificar que la cuenta bancaria existe y pertenece a la empresa
        var cuentaResponse = await _unitOfWork.CuentaBancariaRepository.GetAsync(movimiento.CuentaBancariaId);

        if (!cuentaResponse.WasSuccess || cuentaResponse.Result == null)
        {
            return BadRequest("La cuenta bancaria no existe o no pertenece a la empresa.");
        }

        var cuenta = cuentaResponse.Result;

        if (cuenta.EmpresaId != movimiento.EmpresaId || cuenta.IsDeleted)
        {
            return BadRequest("La cuenta bancaria no existe o no pertenece a la empresa.");
        }

        if (!cuenta.Activo)
        {
            return BadRequest("La cuenta bancaria no está activa.");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Establecer valores del nuevo movimiento
            movimiento.Id = Guid.NewGuid();
            movimiento.FechaCreacion = DateTime.Now;
            movimiento.CreadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            movimiento.Estado = EstadosMovimientoBancario.Registrado;
            movimiento.Conciliado = false;
            movimiento.SaldoAnterior = cuenta.SaldoActual;

            // Calcular nuevo saldo según naturaleza del movimiento
            if (movimiento.Naturaleza == NaturalezaMovimiento.Credito)
            {
                // Crédito: entrada de dinero (aumenta el saldo)
                movimiento.SaldoNuevo = movimiento.SaldoAnterior + movimiento.Monto;
            }
            else if (movimiento.Naturaleza == NaturalezaMovimiento.Debito)
            {
                // Débito: salida de dinero (disminuye el saldo)
                movimiento.SaldoNuevo = movimiento.SaldoAnterior - movimiento.Monto;
            }
            else
            {
                await transaction.RollbackAsync();
                return BadRequest("Naturaleza de movimiento inválida. Debe ser CRE (Crédito) o DEB (Débito).");
            }

            // Actualizar saldo de la cuenta bancaria
            var nuevoSaldo = movimiento.SaldoNuevo;
            var saldoResponse = await _unitOfWork.CuentaBancariaRepository.UpdateSaldoAsync(cuenta.Id, nuevoSaldo);

            if (!saldoResponse.WasSuccess)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Error al actualizar saldo: {saldoResponse.Message}");
            }

            // Agregar movimiento
            var movimientoResponse = await _unitOfWork.MovimientoBancarioRepository.AddAsync(movimiento);

            if (!movimientoResponse.WasSuccess)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Error al crear movimiento: {movimientoResponse.Message}");
            }

            await transaction.CommitAsync();

            // Generar asiento contable (si está habilitado)
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _contabilidadIntegracion.GenerarAsientoMovimientoBancarioAsync(movimiento, userId!);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error generando asiento contable para movimiento bancario {MovimientoId} - operación continúa", movimiento.Id);
            }

            return Ok(movimientoResponse.Result);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al crear movimiento bancario");
            return StatusCode(500, $"Error al crear el movimiento bancario: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza un movimiento bancario existente y recalcula el saldo de la cuenta.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] MovimientoBancario movimiento)
    {
        if (id != movimiento.Id)
        {
            return BadRequest("El ID no coincide.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existenteResponse = await _unitOfWork.MovimientoBancarioRepository.GetAsync(id);

        if (!existenteResponse.WasSuccess || existenteResponse.Result == null)
        {
            return NotFound("Movimiento bancario no encontrado.");
        }

        var existente = existenteResponse.Result;

        if (!await TieneAccesoEmpresaAsync(existente.EmpresaId))
        {
            return Forbid();
        }

        // No permitir modificar movimientos conciliados
        if (existente.Conciliado)
        {
            return BadRequest("No se puede modificar un movimiento que ya está conciliado.");
        }

        // No permitir modificar movimientos anulados
        if (existente.Estado == EstadosMovimientoBancario.Anulado)
        {
            return BadRequest("No se puede modificar un movimiento anulado.");
        }

        // Verificar que la cuenta bancaria existe
        var cuentaResponse = await _unitOfWork.CuentaBancariaRepository.GetAsync(movimiento.CuentaBancariaId);

        if (!cuentaResponse.WasSuccess || cuentaResponse.Result == null || cuentaResponse.Result.IsDeleted)
        {
            return BadRequest("La cuenta bancaria no existe.");
        }

        var cuenta = cuentaResponse.Result;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Revertir el saldo anterior del movimiento existente
            decimal saldoRevertido = cuenta.SaldoActual;
            if (existente.Naturaleza == NaturalezaMovimiento.Credito)
            {
                saldoRevertido -= existente.Monto;
            }
            else if (existente.Naturaleza == NaturalezaMovimiento.Debito)
            {
                saldoRevertido += existente.Monto;
            }

            // Recalcular el nuevo saldo con los nuevos valores
            movimiento.SaldoAnterior = saldoRevertido;

            if (movimiento.Naturaleza == NaturalezaMovimiento.Credito)
            {
                movimiento.SaldoNuevo = movimiento.SaldoAnterior + movimiento.Monto;
            }
            else if (movimiento.Naturaleza == NaturalezaMovimiento.Debito)
            {
                movimiento.SaldoNuevo = movimiento.SaldoAnterior - movimiento.Monto;
            }
            else
            {
                await transaction.RollbackAsync();
                return BadRequest("Naturaleza de movimiento inválida. Debe ser CRE (Crédito) o DEB (Débito).");
            }

            // Preservar campos de auditoría
            movimiento.FechaCreacion = existente.FechaCreacion;
            movimiento.CreadoPorId = existente.CreadoPorId;
            movimiento.FechaModificacion = DateTime.Now;
            movimiento.ModificadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Actualizar movimiento
            var movimientoResponse = await _unitOfWork.MovimientoBancarioRepository.UpdateAsync(movimiento);

            if (!movimientoResponse.WasSuccess)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Error al actualizar movimiento: {movimientoResponse.Message}");
            }

            // Actualizar saldo de cuenta
            var saldoResponse = await _unitOfWork.CuentaBancariaRepository.UpdateSaldoAsync(cuenta.Id, movimiento.SaldoNuevo);

            if (!saldoResponse.WasSuccess)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Error al actualizar saldo: {saldoResponse.Message}");
            }

            await transaction.CommitAsync();

            return Ok(movimientoResponse.Result);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al actualizar movimiento bancario {Id}", id);
            return StatusCode(500, $"Error al actualizar el movimiento bancario: {ex.Message}");
        }
    }

    /// <summary>
    /// Elimina (anula) un movimiento bancario y revierte el saldo de la cuenta.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var movimientoResponse = await _unitOfWork.MovimientoBancarioRepository.GetAsync(id);

        if (!movimientoResponse.WasSuccess || movimientoResponse.Result == null)
        {
            return NotFound("Movimiento bancario no encontrado.");
        }

        var movimiento = movimientoResponse.Result;

        if (!await TieneAccesoEmpresaAsync(movimiento.EmpresaId))
        {
            return Forbid();
        }

        // No permitir eliminar movimientos conciliados
        if (movimiento.Conciliado)
        {
            return BadRequest("No se puede eliminar un movimiento que ya está conciliado.");
        }

        // No permitir eliminar movimientos ya anulados
        if (movimiento.Estado == EstadosMovimientoBancario.Anulado)
        {
            return BadRequest("El movimiento ya está anulado.");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var cuentaResponse = await _unitOfWork.CuentaBancariaRepository.GetAsync(movimiento.CuentaBancariaId);

            if (!cuentaResponse.WasSuccess || cuentaResponse.Result == null)
            {
                await transaction.RollbackAsync();
                return BadRequest("No se encontró la cuenta bancaria asociada.");
            }

            var cuenta = cuentaResponse.Result;

            // Calcular saldo revertido
            decimal nuevoSaldo = cuenta.SaldoActual;
            if (movimiento.Naturaleza == NaturalezaMovimiento.Credito)
            {
                nuevoSaldo -= movimiento.Monto;
            }
            else if (movimiento.Naturaleza == NaturalezaMovimiento.Debito)
            {
                nuevoSaldo += movimiento.Monto;
            }

            // Actualizar saldo de cuenta
            var saldoResponse = await _unitOfWork.CuentaBancariaRepository.UpdateSaldoAsync(cuenta.Id, nuevoSaldo);

            if (!saldoResponse.WasSuccess)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Error al actualizar saldo: {saldoResponse.Message}");
            }

            // Anular movimiento
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var deleteResponse = await _unitOfWork.MovimientoBancarioRepository.DeleteAsync(id, userId!);

            if (!deleteResponse.WasSuccess)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Error al anular movimiento: {deleteResponse.Message}");
            }

            await transaction.CommitAsync();

            return Ok(new { message = "Movimiento bancario anulado exitosamente." });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al anular movimiento bancario {Id}", id);
            return StatusCode(500, $"Error al anular el movimiento bancario: {ex.Message}");
        }
    }

    /// <summary>
    /// Marca un movimiento bancario como conciliado.
    /// </summary>
    [HttpPost("{id:guid}/conciliar")]
    public async Task<IActionResult> ConciliarAsync(Guid id)
    {
        var movimientoResponse = await _unitOfWork.MovimientoBancarioRepository.GetAsync(id);

        if (!movimientoResponse.WasSuccess || movimientoResponse.Result == null)
        {
            return NotFound("Movimiento bancario no encontrado.");
        }

        var movimiento = movimientoResponse.Result;

        if (!await TieneAccesoEmpresaAsync(movimiento.EmpresaId))
        {
            return Forbid();
        }

        if (movimiento.Estado == EstadosMovimientoBancario.Anulado)
        {
            return BadRequest("No se puede conciliar un movimiento anulado.");
        }

        if (movimiento.Conciliado)
        {
            return BadRequest("El movimiento ya está conciliado.");
        }

        try
        {
            // Crear una conciliación temporal (Guid vacío indica conciliación manual)
            var conciliadoResponse = await _unitOfWork.MovimientoBancarioRepository.MarcarComoConciliadoAsync(id, Guid.Empty);

            if (!conciliadoResponse.WasSuccess)
            {
                return BadRequest($"Error al conciliar movimiento: {conciliadoResponse.Message}");
            }

            return Ok(conciliadoResponse.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al conciliar movimiento bancario {Id}", id);
            return StatusCode(500, $"Error al conciliar el movimiento bancario: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene el saldo actual de una cuenta bancaria basado en sus movimientos.
    /// </summary>
    [HttpGet("cuenta/{cuentaBancariaId:guid}/saldo")]
    public async Task<IActionResult> GetSaldoCuentaAsync(Guid cuentaBancariaId)
    {
        var cuentaResponse = await _unitOfWork.CuentaBancariaRepository.GetAsync(cuentaBancariaId);

        if (!cuentaResponse.WasSuccess || cuentaResponse.Result == null)
        {
            return NotFound("Cuenta bancaria no encontrada.");
        }

        var cuenta = cuentaResponse.Result;

        if (!await TieneAccesoEmpresaAsync(cuenta.EmpresaId))
        {
            return Forbid();
        }

        // Calcular saldo usando el repositorio
        var saldoResponse = await _unitOfWork.MovimientoBancarioRepository.GetSaldoCuentaAsync(cuentaBancariaId);

        if (!saldoResponse.WasSuccess)
        {
            return BadRequest($"Error al calcular saldo: {saldoResponse.Message}");
        }

        var saldoCalculado = saldoResponse.Result;

        // Obtener movimientos para estadísticas
        var movimientosResponse = await _unitOfWork.MovimientoBancarioRepository.GetByCuentaBancariaAsync(cuentaBancariaId);

        if (!movimientosResponse.WasSuccess)
        {
            return BadRequest(movimientosResponse.Message);
        }

        var movimientos = movimientosResponse.Result?.ToList() ?? new List<MovimientoBancario>();

        var totalCreditos = movimientos
            .Where(m => m.Naturaleza == NaturalezaMovimiento.Credito)
            .Sum(m => m.Monto);

        var totalDebitos = movimientos
            .Where(m => m.Naturaleza == NaturalezaMovimiento.Debito)
            .Sum(m => m.Monto);

        var ultimoMovimiento = movimientos
            .OrderByDescending(m => m.Fecha)
            .ThenByDescending(m => m.FechaCreacion)
            .FirstOrDefault();

        return Ok(new
        {
            cuentaBancariaId = cuenta.Id,
            nombreCuenta = cuenta.Nombre,
            numeroCuenta = cuenta.NumeroCuenta,
            moneda = cuenta.Moneda,
            saldoInicial = cuenta.SaldoInicial,
            saldoActual = cuenta.SaldoActual,
            saldoCalculado = saldoCalculado,
            diferencia = cuenta.SaldoActual - saldoCalculado,
            totalCreditos = totalCreditos,
            totalDebitos = totalDebitos,
            cantidadMovimientos = movimientos.Count,
            fechaUltimoMovimiento = ultimoMovimiento?.Fecha,
            ultimoMovimientoNumero = ultimoMovimiento?.Numero
        });
    }

    /// <summary>
    /// Verifica si el usuario tiene acceso a la empresa especificada.
    /// </summary>
    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        // Verificar si es SuperUser
        if (User.IsInRole("SuperUser"))
        {
            return true;
        }

        // Verificar si el usuario tiene acceso a la empresa
        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }
}
