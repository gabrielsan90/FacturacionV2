using Facturacion.Backend.Data;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class GastosController : ControllerBase
{
    private readonly IGastoUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly ILogger<GastosController> _logger;

    public GastosController(IGastoUnitOfWork unitOfWork, DataContext context, ILogger<GastosController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los gastos de una empresa
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó acceder a gastos de empresa {EmpresaId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
                return Forbid();
            }

            var gastos = await _unitOfWork.GastoRepository.GetByEmpresaAsync(empresaId);
            _logger.LogInformation("Se obtuvieron {Count} gastos para empresa {EmpresaId}", gastos.Count(), empresaId);
            return Ok(gastos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener gastos de empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error interno al obtener gastos.");
        }
    }

    /// <summary>
    /// Obtiene el resumen de gastos de una empresa con filtros opcionales
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}/resumen")]
    public async Task<IActionResult> GetResumenAsync(
        Guid empresaId,
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó acceder a resumen de gastos de empresa {EmpresaId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
                return Forbid();
            }

            var resumen = await _unitOfWork.GastoService.GetResumenGastosAsync(empresaId, fechaInicio, fechaFin);
            _logger.LogInformation("Resumen de gastos generado para empresa {EmpresaId} (Período: {FechaInicio} - {FechaFin})",
                empresaId, fechaInicio, fechaFin);
            return Ok(resumen);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener resumen de gastos de empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error interno al obtener resumen de gastos.");
        }
    }

    /// <summary>
    /// Obtiene un gasto por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var gasto = await _unitOfWork.GastoRepository.GetAsync(id);

            if (gasto == null)
            {
                _logger.LogWarning("Gasto {GastoId} no encontrado", id);
                return NotFound("El gasto no existe.");
            }

            if (!await TieneAccesoEmpresaAsync(gasto.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó acceder a gasto {GastoId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), id);
                return Forbid();
            }

            return Ok(gasto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener gasto {GastoId}", id);
            return StatusCode(500, "Error interno al obtener gasto.");
        }
    }

    /// <summary>
    /// Obtiene los gastos con pagos pendientes
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}/pendientes")]
    public async Task<IActionResult> GetPendientesAsync(Guid empresaId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó acceder a gastos pendientes de empresa {EmpresaId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
                return Forbid();
            }

            var pendientes = await _unitOfWork.GastoRepository.GetPendientesPagoAsync(empresaId);
            _logger.LogInformation("Se obtuvieron {Count} gastos pendientes para empresa {EmpresaId}",
                pendientes.Count(), empresaId);
            return Ok(pendientes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener gastos pendientes de empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error interno al obtener gastos pendientes.");
        }
    }

    /// <summary>
    /// Obtiene gastos por proveedor
    /// </summary>
    [HttpGet("proveedor/{proveedorId:guid}")]
    public async Task<IActionResult> GetByProveedorAsync(Guid proveedorId)
    {
        try
        {
            var gastos = await _unitOfWork.GastoRepository.GetByProveedorAsync(proveedorId);

            // Verificar acceso a la empresa del primer gasto
            if (gastos.Any() && !await TieneAccesoEmpresaAsync(gastos.First().EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó acceder a gastos de proveedor {ProveedorId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), proveedorId);
                return Forbid();
            }

            _logger.LogInformation("Se obtuvieron {Count} gastos para proveedor {ProveedorId}",
                gastos.Count(), proveedorId);
            return Ok(gastos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener gastos de proveedor {ProveedorId}", proveedorId);
            return StatusCode(500, "Error interno al obtener gastos por proveedor.");
        }
    }

    /// <summary>
    /// Obtiene gastos por categoría
    /// </summary>
    [HttpGet("categoria/{categoriaId:int}")]
    public async Task<IActionResult> GetByCategoriaAsync(int categoriaId)
    {
        try
        {
            var gastos = await _unitOfWork.GastoRepository.GetByCategoriaAsync(categoriaId);

            // Verificar acceso a la empresa del primer gasto
            if (gastos.Any() && !await TieneAccesoEmpresaAsync(gastos.First().EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó acceder a gastos de categoría {CategoriaId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), categoriaId);
                return Forbid();
            }

            _logger.LogInformation("Se obtuvieron {Count} gastos para categoría {CategoriaId}",
                gastos.Count(), categoriaId);
            return Ok(gastos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener gastos de categoría {CategoriaId}", categoriaId);
            return StatusCode(500, "Error interno al obtener gastos por categoría.");
        }
    }

    /// <summary>
    /// Obtiene estadísticas de gastos por periodo
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}/estadisticas")]
    public async Task<IActionResult> GetEstadisticasAsync(
        Guid empresaId,
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó acceder a estadísticas de gastos de empresa {EmpresaId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
                return Forbid();
            }

            var estadisticas = await _unitOfWork.GastoService.GetEstadisticasAsync(empresaId, fechaInicio, fechaFin);
            _logger.LogInformation("Estadísticas de gastos generadas para empresa {EmpresaId} (Período: {FechaInicio} - {FechaFin})",
                empresaId, fechaInicio, fechaFin);
            return Ok(estadisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de gastos de empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error interno al obtener estadísticas.");
        }
    }

    /// <summary>
    /// Crea un nuevo gasto
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] GastoDTO dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Intento de crear gasto con datos inválidos");
            return BadRequest(ModelState);
        }

        if (!await TieneAccesoEmpresaAsync(dto.EmpresaId))
        {
            _logger.LogWarning("Usuario {UserId} intentó crear gasto en empresa {EmpresaId} sin autorización",
                User.FindFirstValue(ClaimTypes.NameIdentifier), dto.EmpresaId);
            return Forbid();
        }

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var gasto = await _unitOfWork.GastoService.CrearGastoAsync(dto, dto.EmpresaId, userId!);

            _logger.LogInformation("Gasto {GastoId} creado exitosamente para empresa {EmpresaId} por usuario {UserId}",
                gasto.Id, dto.EmpresaId, userId);

            return Ok(gasto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error de validación al crear gasto para empresa {EmpresaId}", dto.EmpresaId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear gasto para empresa {EmpresaId}", dto.EmpresaId);
            return StatusCode(500, "Error interno al crear el gasto.");
        }
    }

    /// <summary>
    /// Actualiza un gasto existente
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] GastoDTO dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Intento de actualizar gasto {GastoId} con datos inválidos", id);
            return BadRequest(ModelState);
        }

        var gastoExistente = await _unitOfWork.GastoRepository.GetAsync(id);
        if (gastoExistente == null)
        {
            _logger.LogWarning("Intento de actualizar gasto inexistente {GastoId}", id);
            return NotFound("El gasto no existe.");
        }

        if (!await TieneAccesoEmpresaAsync(gastoExistente.EmpresaId))
        {
            _logger.LogWarning("Usuario {UserId} intentó actualizar gasto {GastoId} sin autorización",
                User.FindFirstValue(ClaimTypes.NameIdentifier), id);
            return Forbid();
        }

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var gasto = await _unitOfWork.GastoService.ActualizarGastoAsync(id, dto, userId!);

            _logger.LogInformation("Gasto {GastoId} actualizado exitosamente por usuario {UserId}", id, userId);

            return Ok(gasto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error de validación al actualizar gasto {GastoId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar gasto {GastoId}", id);
            return StatusCode(500, "Error interno al actualizar el gasto.");
        }
    }

    /// <summary>
    /// Elimina (soft delete) un gasto
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var gasto = await _unitOfWork.GastoRepository.GetAsync(id);
            if (gasto == null)
            {
                _logger.LogWarning("Intento de eliminar gasto inexistente {GastoId}", id);
                return NotFound("El gasto no existe.");
            }

            if (!await TieneAccesoEmpresaAsync(gasto.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó eliminar gasto {GastoId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), id);
                return Forbid();
            }

            // No permitir eliminar gastos aprobados
            if (gasto.Aprobado)
            {
                _logger.LogWarning("Intento de eliminar gasto aprobado {GastoId}", id);
                return BadRequest("No se puede eliminar un gasto aprobado.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _unitOfWork.GastoRepository.DeleteAsync(id, userId!);

            _logger.LogInformation("Gasto {GastoId} eliminado exitosamente por usuario {UserId}", id, userId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar gasto {GastoId}", id);
            return StatusCode(500, "Error interno al eliminar el gasto.");
        }
    }

    /// <summary>
    /// Registra un pago parcial o total
    /// </summary>
    [HttpPost("pagar")]
    public async Task<IActionResult> RegistrarPagoAsync([FromBody] RegistrarPagoDTO dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Intento de registrar pago con datos inválidos");
            return BadRequest(ModelState);
        }

        try
        {
            var gasto = await _unitOfWork.GastoRepository.GetAsync(dto.GastoId);
            if (gasto == null)
            {
                _logger.LogWarning("Intento de registrar pago para gasto inexistente {GastoId}", dto.GastoId);
                return NotFound("El gasto no existe.");
            }

            if (!await TieneAccesoEmpresaAsync(gasto.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó registrar pago para gasto {GastoId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), dto.GastoId);
                return Forbid();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var gastoActualizado = await _unitOfWork.GastoService.RegistrarPagoAsync(dto, userId!);

            _logger.LogInformation("Pago de {MontoPago} registrado exitosamente para gasto {GastoId} por usuario {UserId}",
                dto.MontoPago, dto.GastoId, userId);

            return Ok(gastoActualizado);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error de validación al registrar pago para gasto {GastoId}", dto.GastoId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar pago para gasto {GastoId}", dto.GastoId);
            return StatusCode(500, "Error interno al registrar el pago.");
        }
    }

    /// <summary>
    /// Aprueba o rechaza un gasto
    /// </summary>
    [HttpPost("aprobar")]
    public async Task<IActionResult> AprobarAsync([FromBody] AprobarGastoDTO dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Intento de aprobar/rechazar gasto con datos inválidos");
            return BadRequest(ModelState);
        }

        try
        {
            var gasto = await _unitOfWork.GastoRepository.GetAsync(dto.GastoId);
            if (gasto == null)
            {
                _logger.LogWarning("Intento de aprobar/rechazar gasto inexistente {GastoId}", dto.GastoId);
                return NotFound("El gasto no existe.");
            }

            if (!await TieneAccesoEmpresaAsync(gasto.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó aprobar/rechazar gasto {GastoId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), dto.GastoId);
                return Forbid();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var gastoActualizado = await _unitOfWork.GastoService.AprobarGastoAsync(dto, userId!);

            _logger.LogInformation("Gasto {GastoId} {Accion} exitosamente por usuario {UserId}",
                dto.GastoId, dto.Aprobado ? "aprobado" : "rechazado", userId);

            return Ok(gastoActualizado);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error de validación al aprobar/rechazar gasto {GastoId}", dto.GastoId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aprobar/rechazar gasto {GastoId}", dto.GastoId);
            return StatusCode(500, "Error interno al aprobar/rechazar el gasto.");
        }
    }

    /// <summary>
    /// Verifica si el usuario tiene acceso a la empresa
    /// </summary>
    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            // Verificar si el usuario es SuperUser (tiene acceso a todo)
            if (User.IsInRole("SuperUser"))
            {
                return true;
            }

            // Verificar si el usuario tiene acceso a esta empresa
            var tieneAcceso = await _context.UsuariosEmpresas
                .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);

            return tieneAcceso;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar acceso a empresa {EmpresaId}", empresaId);
            return false;
        }
    }
}
