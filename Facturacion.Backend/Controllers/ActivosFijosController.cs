using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class ActivosFijosController : ControllerBase
{
    private readonly IActivoFijoUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly ILogger<ActivosFijosController> _logger;
    private readonly IExcelImportService _excelImportService;

    public ActivosFijosController(
        IActivoFijoUnitOfWork unitOfWork,
        DataContext context,
        ILogger<ActivosFijosController> logger,
        IExcelImportService excelImportService)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
        _excelImportService = excelImportService;
    }

    /// <summary>
    /// Obtiene todos los activos fijos de una empresa con filtros opcionales
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(
        Guid empresaId,
        [FromQuery] string? estado = null,
        [FromQuery] Guid? categoriaId = null,
        [FromQuery] Guid? sucursalId = null,
        [FromQuery] string? search = null)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var query = _context.ActivosFijos
            .Where(a => a.EmpresaId == empresaId && !a.IsDeleted)
            .Include(a => a.CategoriaActivo)
            .Include(a => a.Sucursal)
            .Include(a => a.Responsable)
            .AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrEmpty(estado))
        {
            query = query.Where(a => a.Estado == estado);
        }

        if (categoriaId.HasValue)
        {
            query = query.Where(a => a.CategoriaActivoId == categoriaId.Value);
        }

        if (sucursalId.HasValue)
        {
            query = query.Where(a => a.SucursalId == sucursalId.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(a =>
                a.Codigo.Contains(search) ||
                a.Nombre.Contains(search) ||
                (a.Descripcion != null && a.Descripcion.Contains(search)) ||
                (a.NumeroSerie != null && a.NumeroSerie.Contains(search)) ||
                (a.Placa != null && a.Placa.Contains(search)));
        }

        var activos = await query
            .OrderBy(a => a.Codigo)
            .AsNoTracking()
            .ToListAsync();

        return Ok(activos);
    }

    /// <summary>
    /// Obtiene un activo fijo con su historial de depreciaciones
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var action = await _unitOfWork.ActivoFijoRepository.GetWithDepreciacionesAsync(id);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Activo no encontrado: {Id}", id);
                return NotFound(action.Message);
            }

            if (!await TieneAccesoEmpresaAsync(action.Result!.EmpresaId))
            {
                _logger.LogWarning("Usuario sin acceso a empresa del activo: {Id}", id);
                return Forbid();
            }

            return Ok(action.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener activo: {Id}", id);
            return StatusCode(500, $"Error al obtener el activo: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea un nuevo activo fijo
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] ActivoFijo activo)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!await TieneAccesoEmpresaAsync(activo.EmpresaId))
        {
            _logger.LogWarning("Usuario sin acceso a empresa: {EmpresaId}", activo.EmpresaId);
            return Forbid();
        }

        try
        {
            // Verificar código único
            var codigoAction = await _unitOfWork.ActivoFijoRepository.GetByCodigoAsync(activo.EmpresaId, activo.Codigo);
            if (codigoAction.WasSuccess && codigoAction.Result != null)
            {
                return BadRequest("Ya existe un activo con este código en la empresa.");
            }

            // Validar que la categoría existe y pertenece a la empresa
            var categoriaAction = await _unitOfWork.CategoriaActivoRepository.GetAsync(activo.CategoriaActivoId);
            if (!categoriaAction.WasSuccess || categoriaAction.Result == null || categoriaAction.Result.EmpresaId != activo.EmpresaId)
            {
                return BadRequest("La categoría especificada no existe o no pertenece a la empresa.");
            }

            // Establecer valores por defecto si no vienen
            if (activo.FechaInicioDepreciacion == null)
            {
                activo.FechaInicioDepreciacion = activo.FechaCompra;
            }

            activo.Id = Guid.NewGuid();
            activo.FechaCreacion = DateTime.Now;
            activo.UsuarioCreacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            activo.DepreciacionAcumulada = 0;
            activo.Estado = "ACT"; // Activo por defecto

            var action = await _unitOfWork.ActivoFijoRepository.AddAsync(activo);

            if (!action.WasSuccess)
            {
                _logger.LogError("Error al crear activo: {Message}", action.Message);
                return BadRequest(action.Message);
            }

            await _unitOfWork.SaveAsync();
            _logger.LogInformation("Activo creado exitosamente: {Id}", activo.Id);

            return Ok(action.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear activo");
            return StatusCode(500, $"Error al crear el activo: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza un activo fijo
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] ActivoFijo activo)
    {
        if (id != activo.Id)
        {
            return BadRequest("El ID no coincide.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var existenteAction = await _unitOfWork.ActivoFijoRepository.GetAsync(id);

            if (!existenteAction.WasSuccess || existenteAction.Result == null)
            {
                _logger.LogWarning("Activo no encontrado para actualización: {Id}", id);
                return NotFound("Activo no encontrado.");
            }

            var existente = existenteAction.Result;

            if (!await TieneAccesoEmpresaAsync(existente.EmpresaId))
            {
                _logger.LogWarning("Usuario sin acceso a empresa del activo: {Id}", id);
                return Forbid();
            }

            // Verificar código único (excluyendo el activo actual)
            var codigoAction = await _unitOfWork.ActivoFijoRepository.GetByCodigoAsync(activo.EmpresaId, activo.Codigo);
            if (codigoAction.WasSuccess && codigoAction.Result != null && codigoAction.Result.Id != id)
            {
                return BadRequest("Ya existe otro activo con este código en la empresa.");
            }

            // Validar que la categoría existe y pertenece a la empresa
            var categoriaAction = await _unitOfWork.CategoriaActivoRepository.GetAsync(activo.CategoriaActivoId);
            if (!categoriaAction.WasSuccess || categoriaAction.Result == null || categoriaAction.Result.EmpresaId != activo.EmpresaId)
            {
                return BadRequest("La categoría especificada no existe o no pertenece a la empresa.");
            }

            // Preservar campos de auditoría y estado calculado
            activo.FechaCreacion = existente.FechaCreacion;
            activo.UsuarioCreacionId = existente.UsuarioCreacionId;
            activo.FechaModificacion = DateTime.Now;
            activo.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            activo.DepreciacionAcumulada = existente.DepreciacionAcumulada; // No se modifica directamente
            activo.UltimaDepreciacion = existente.UltimaDepreciacion;

            var action = await _unitOfWork.ActivoFijoRepository.UpdateAsync(activo);

            if (!action.WasSuccess)
            {
                _logger.LogError("Error al actualizar activo: {Message}", action.Message);
                return BadRequest(action.Message);
            }

            await _unitOfWork.SaveAsync();
            _logger.LogInformation("Activo actualizado exitosamente: {Id}", id);

            return Ok(action.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar activo: {Id}", id);
            return StatusCode(500, $"Error al actualizar el activo: {ex.Message}");
        }
    }

    /// <summary>
    /// Calcula y registra la depreciación de un activo
    /// </summary>
    [HttpPost("{id:guid}/depreciar")]
    public async Task<IActionResult> DepreciarAsync(Guid id, [FromBody] DepreciacionRequest request)
    {
        try
        {
            var activoAction = await _unitOfWork.ActivoFijoRepository.GetAsync(id);

            if (!activoAction.WasSuccess || activoAction.Result == null)
            {
                _logger.LogWarning("Activo no encontrado para depreciación: {Id}", id);
                return NotFound("Activo no encontrado.");
            }

            var activo = activoAction.Result;

            if (!await TieneAccesoEmpresaAsync(activo.EmpresaId))
            {
                _logger.LogWarning("Usuario sin acceso a empresa del activo: {Id}", id);
                return Forbid();
            }

            // Validar que el activo está activo
            if (activo.Estado != "ACT")
            {
                return BadRequest("No se puede depreciar un activo que no está activo.");
            }

            // Validar que no esté totalmente depreciado
            if (activo.EstaDepreciadoTotal)
            {
                return BadRequest("El activo ya está totalmente depreciado.");
            }

            // Usar la fecha proporcionada o la actual
            var fechaDepreciacion = request.Fecha ?? DateTime.Now;

            // Validar que no se deprecie antes de la última depreciación
            if (activo.UltimaDepreciacion.HasValue && fechaDepreciacion <= activo.UltimaDepreciacion.Value)
            {
                return BadRequest("La fecha de depreciación debe ser posterior a la última depreciación.");
            }

            // Validar que no se deprecie antes de la fecha de inicio
            if (activo.FechaInicioDepreciacion.HasValue && fechaDepreciacion < activo.FechaInicioDepreciacion.Value)
            {
                return BadRequest("La fecha de depreciación debe ser posterior a la fecha de inicio de depreciación.");
            }

            // Calcular depreciación según el método
            decimal montoDepreciacion = 0;

            switch (activo.MetodoDepreciacion)
            {
                case "LR": // Línea Recta
                    if (activo.FrecuenciaDepreciacion == "MEN")
                    {
                        montoDepreciacion = activo.DepreciacionMensual;
                    }
                    else // ANU
                    {
                        montoDepreciacion = activo.ValorDepreciable / activo.VidaUtilAnios;
                    }
                    break;

                case "SAD": // Suma de Años Dígitos
                    // Implementación simplificada - puede mejorarse
                    montoDepreciacion = activo.DepreciacionMensual;
                    break;

                case "SDD": // Saldo Doble Decreciente
                    // Implementación simplificada - puede mejorarse
                    var tasaDoble = (2.0m / activo.VidaUtilAnios);
                    montoDepreciacion = activo.ValorActual * tasaDoble;
                    if (activo.FrecuenciaDepreciacion == "MEN")
                    {
                        montoDepreciacion /= 12;
                    }
                    break;

                default:
                    montoDepreciacion = activo.DepreciacionMensual;
                    break;
            }

            // Asegurar que no se deprecie más allá del valor depreciable
            var nuevaDepreciacionAcumulada = activo.DepreciacionAcumulada + montoDepreciacion;
            if (nuevaDepreciacionAcumulada > activo.ValorDepreciable)
            {
                montoDepreciacion = activo.ValorDepreciable - activo.DepreciacionAcumulada;
                nuevaDepreciacionAcumulada = activo.ValorDepreciable;
            }

            // Crear registro de depreciación
            var depreciacion = new DepreciacionActivo
            {
                Id = Guid.NewGuid(),
                ActivoFijoId = activo.Id,
                Fecha = fechaDepreciacion,
                Anio = fechaDepreciacion.Year,
                Mes = fechaDepreciacion.Month,
                MontoDepreciacion = montoDepreciacion,
                DepreciacionAcumuladaAnterior = activo.DepreciacionAcumulada,
                DepreciacionAcumuladaNueva = nuevaDepreciacionAcumulada,
                ValorLibrosAnterior = activo.ValorActual,
                ValorLibrosNuevo = activo.ValorOriginal - nuevaDepreciacionAcumulada,
                Estado = "APL", // Aplicada
                Observaciones = request.Observaciones,
                FechaCreacion = DateTime.Now,
                UsuarioCreacionId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            // Actualizar activo
            activo.DepreciacionAcumulada = nuevaDepreciacionAcumulada;
            activo.UltimaDepreciacion = fechaDepreciacion;
            activo.FechaModificacion = DateTime.Now;
            activo.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Si está totalmente depreciado, cambiar estado
            if (activo.EstaDepreciadoTotal)
            {
                activo.Estado = "DEP";
            }

            // Usar repositorios para guardar
            var depreciacionAction = await _unitOfWork.DepreciacionActivoRepository.AddAsync(depreciacion);
            if (!depreciacionAction.WasSuccess)
            {
                _logger.LogError("Error al crear registro de depreciación: {Message}", depreciacionAction.Message);
                return BadRequest(depreciacionAction.Message);
            }

            var updateAction = await _unitOfWork.ActivoFijoRepository.UpdateAsync(activo);
            if (!updateAction.WasSuccess)
            {
                _logger.LogError("Error al actualizar activo con depreciación: {Message}", updateAction.Message);
                return BadRequest(updateAction.Message);
            }

            await _unitOfWork.SaveAsync();
            _logger.LogInformation("Depreciación registrada exitosamente para activo: {Id}", id);

            return Ok(new ActionResponse<DepreciacionActivo>
            {
                WasSuccess = true,
                Message = "Depreciación registrada exitosamente.",
                Result = depreciacionAction.Result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar depreciación para activo: {Id}", id);
            return StatusCode(500, $"Error al registrar la depreciación: {ex.Message}");
        }
    }

    /// <summary>
    /// Da de baja un activo fijo (venta, robo, pérdida, etc.)
    /// </summary>
    [HttpPost("{id:guid}/dar-de-baja")]
    public async Task<IActionResult> DarDeBajaAsync(Guid id, [FromBody] BajaActivoRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioId))
            {
                _logger.LogWarning("Usuario no autenticado intentando dar de baja activo");
                return Unauthorized();
            }

            // Validar motivo de baja
            if (string.IsNullOrWhiteSpace(request.Motivo))
            {
                return BadRequest("Debe especificar el motivo de la baja.");
            }

            // Validar estados permitidos
            var estadosPermitidos = new[] { "BAJ", "VEN", "ROB" };
            if (!estadosPermitidos.Contains(request.NuevoEstado))
            {
                return BadRequest("El estado de baja especificado no es válido. Use BAJ, VEN o ROB.");
            }

            var activoAction = await _unitOfWork.ActivoFijoRepository.GetAsync(id);

            if (!activoAction.WasSuccess || activoAction.Result == null)
            {
                _logger.LogWarning("Activo no encontrado para dar de baja: {Id}", id);
                return NotFound("Activo no encontrado.");
            }

            var activo = activoAction.Result;

            if (!await TieneAccesoEmpresaAsync(activo.EmpresaId))
            {
                _logger.LogWarning("Usuario sin acceso a empresa del activo: {Id}", id);
                return Forbid();
            }

            // Validar que el activo está activo
            if (activo.Estado != "ACT")
            {
                return BadRequest("El activo ya no está activo.");
            }

            var fechaBaja = request.FechaBaja ?? DateTime.Now;
            var action = await _unitOfWork.ActivoFijoRepository.DarDeBajaAsync(
                id,
                usuarioId,
                request.Motivo,
                fechaBaja
            );

            if (!action.WasSuccess)
            {
                _logger.LogError("Error al dar de baja activo: {Message}", action.Message);
                return BadRequest(action.Message);
            }

            // Actualizar campos adicionales que no están en el método del repositorio
            activo.Estado = request.NuevoEstado;
            activo.FechaBaja = fechaBaja;
            activo.MotivoBaja = request.Motivo;
            activo.ValorVenta = request.ValorVenta;
            activo.FechaModificacion = DateTime.Now;
            activo.UsuarioModificacionId = usuarioId;

            var updateAction = await _unitOfWork.ActivoFijoRepository.UpdateAsync(activo);

            if (!updateAction.WasSuccess)
            {
                _logger.LogError("Error al actualizar activo dado de baja: {Message}", updateAction.Message);
                return BadRequest(updateAction.Message);
            }

            await _unitOfWork.SaveAsync();
            _logger.LogInformation("Activo dado de baja exitosamente: {Id}", id);

            return Ok(new ActionResponse<ActivoFijo>
            {
                WasSuccess = true,
                Message = "Activo dado de baja exitosamente.",
                Result = updateAction.Result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al dar de baja activo: {Id}", id);
            return StatusCode(500, $"Error al dar de baja el activo: {ex.Message}");
        }
    }

    /// <summary>
    /// Elimina (soft delete) un activo fijo
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioId))
            {
                _logger.LogWarning("Usuario no autenticado intentando eliminar activo");
                return Unauthorized();
            }

            var activoAction = await _unitOfWork.ActivoFijoRepository.GetWithDepreciacionesAsync(id);

            if (!activoAction.WasSuccess || activoAction.Result == null)
            {
                _logger.LogWarning("Activo no encontrado para eliminar: {Id}", id);
                return NotFound("Activo no encontrado.");
            }

            var activo = activoAction.Result;

            if (!await TieneAccesoEmpresaAsync(activo.EmpresaId))
            {
                _logger.LogWarning("Usuario sin acceso a empresa del activo: {Id}", id);
                return Forbid();
            }

            // No permitir eliminar si tiene depreciaciones aplicadas
            if (activo.Depreciaciones?.Any(d => d.Estado == "APL") == true)
            {
                return BadRequest("No se puede eliminar un activo que tiene depreciaciones aplicadas. Debe dar de baja el activo.");
            }

            var action = await _unitOfWork.ActivoFijoRepository.DeleteAsync(id, usuarioId);

            if (!action.WasSuccess)
            {
                _logger.LogError("Error al eliminar activo: {Message}", action.Message);
                return BadRequest(action.Message);
            }

            await _unitOfWork.SaveAsync();
            _logger.LogInformation("Activo eliminado exitosamente: {Id}", id);

            return Ok(new ActionResponse<bool>
            {
                WasSuccess = true,
                Message = "Activo eliminado exitosamente.",
                Result = action.Result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar activo: {Id}", id);
            return StatusCode(500, $"Error al eliminar el activo: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene el historial de depreciaciones de un activo
    /// </summary>
    [HttpGet("{id:guid}/depreciaciones")]
    public async Task<IActionResult> GetDepreciacionesAsync(Guid id)
    {
        try
        {
            var activoAction = await _unitOfWork.ActivoFijoRepository.GetAsync(id);

            if (!activoAction.WasSuccess || activoAction.Result == null)
            {
                _logger.LogWarning("Activo no encontrado para obtener depreciaciones: {Id}", id);
                return NotFound("Activo no encontrado.");
            }

            if (!await TieneAccesoEmpresaAsync(activoAction.Result.EmpresaId))
            {
                _logger.LogWarning("Usuario sin acceso a empresa del activo: {Id}", id);
                return Forbid();
            }

            var depreciacionesAction = await _unitOfWork.DepreciacionActivoRepository.GetByActivoAsync(id);

            if (!depreciacionesAction.WasSuccess)
            {
                _logger.LogError("Error al obtener depreciaciones del activo: {Message}", depreciacionesAction.Message);
                return BadRequest(depreciacionesAction.Message);
            }

            return Ok(depreciacionesAction.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener depreciaciones del activo: {Id}", id);
            return StatusCode(500, $"Error al obtener las depreciaciones: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene todas las depreciaciones de una empresa con filtros opcionales
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}/depreciaciones")]
    public async Task<IActionResult> GetDepreciacionesByEmpresaAsync(
        Guid empresaId,
        [FromQuery] Guid? activoId,
        [FromQuery] int? anio,
        [FromQuery] int? mes,
        [FromQuery] string? estado)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
                return Forbid();

            var query = _context.DepreciacionesActivo
                .Include(d => d.ActivoFijo)
                .Where(d => d.ActivoFijo != null && d.ActivoFijo.EmpresaId == empresaId && !d.ActivoFijo.IsDeleted)
                .AsNoTracking();

            if (activoId.HasValue && activoId != Guid.Empty)
                query = query.Where(d => d.ActivoFijoId == activoId);

            if (anio.HasValue)
                query = query.Where(d => d.Anio == anio);

            if (mes.HasValue)
                query = query.Where(d => d.Mes == mes);

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(d => d.Estado == estado);

            var depreciaciones = await query
                .OrderByDescending(d => d.Fecha)
                .ToListAsync();

            return Ok(depreciaciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener depreciaciones para empresa: {EmpresaId}", empresaId);
            return StatusCode(500, $"Error al obtener depreciaciones: {ex.Message}");
        }
    }

    /// <summary>
    /// Genera depreciaciones masivas para todos los activos activos de una empresa
    /// </summary>
    [HttpPost("empresa/{empresaId:guid}/generar-depreciacion")]
    public async Task<IActionResult> GenerarDepreciacionMasivaAsync(Guid empresaId, [FromBody] GenerarDepreciacionMasivaRequest request)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
                return Forbid();

            var result = await _unitOfWork.DepreciacionActivoRepository
                .GenerarDepreciacionesMasivasAsync(empresaId, request.Anio, request.Mes);

            if (!result.WasSuccess)
            {
                _logger.LogError("Error al generar depreciaciones masivas: {Message}", result.Message);
                return BadRequest(result.Message);
            }

            await _unitOfWork.SaveAsync();

            var depreciaciones = result.Result?.ToList() ?? new List<DepreciacionActivo>();
            _logger.LogInformation("Generadas {Count} depreciaciones para empresa {EmpresaId}", depreciaciones.Count, empresaId);

            return Ok(new
            {
                activosProcesados = depreciaciones.Select(d => d.ActivoFijoId).Distinct().Count(),
                depreciacionesGeneradas = depreciaciones.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar depreciaciones masivas para empresa: {EmpresaId}", empresaId);
            return StatusCode(500, $"Error al generar depreciaciones: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene resumen de activos por categoría para una empresa
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}/resumen-por-categoria")]
    public async Task<IActionResult> GetResumenPorCategoriaAsync(Guid empresaId)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var resumen = await _context.ActivosFijos
            .Where(a => a.EmpresaId == empresaId && !a.IsDeleted)
            .Include(a => a.CategoriaActivo)
            .GroupBy(a => new { a.CategoriaActivoId, a.CategoriaActivo!.Nombre })
            .Select(g => new
            {
                CategoriaId = g.Key.CategoriaActivoId,
                CategoriaNombre = g.Key.Nombre,
                Cantidad = g.Count(),
                ValorOriginalTotal = g.Sum(a => a.ValorOriginal),
                DepreciacionAcumuladaTotal = g.Sum(a => a.DepreciacionAcumulada),
                ValorActualTotal = g.Sum(a => a.ValorActual)
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(resumen);
    }

    /// <summary>
    /// Obtiene activos próximos a depreciar totalmente
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}/proximos-a-depreciar")]
    public async Task<IActionResult> GetProximosADepreciarAsync(Guid empresaId, [FromQuery] decimal porcentajeMinimo = 80)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var activos = await _context.ActivosFijos
            .Where(a => a.EmpresaId == empresaId &&
                       !a.IsDeleted &&
                       a.Estado == "ACT" &&
                       a.ValorDepreciable > 0)
            .Include(a => a.CategoriaActivo)
            .Include(a => a.Sucursal)
            .AsNoTracking()
            .ToListAsync();

        // Filtrar en memoria por propiedad calculada
        var proximosADeprecias = activos
            .Where(a => a.PorcentajeDepreciado >= porcentajeMinimo && !a.EstaDepreciadoTotal)
            .OrderByDescending(a => a.PorcentajeDepreciado)
            .ToList();

        return Ok(proximosADeprecias);
    }

    /// <summary>
    /// Importa activos fijos desde un archivo Excel
    /// </summary>
    [HttpPost("empresa/{empresaId:guid}/importar")]
    public async Task<IActionResult> ImportarAsync(Guid empresaId, IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest("Debe proporcionar un archivo Excel.");

        if (!await TieneAccesoEmpresaAsync(empresaId))
            return Forbid();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        using var stream = archivo.OpenReadStream();
        var result = await _excelImportService.ImportarActivosFijosAsync(stream, empresaId, userId!);
        return Ok(result);
    }

    /// <summary>
    /// Descarga una plantilla de Excel para importar activos fijos
    /// </summary>
    [HttpGet("plantilla")]
    public IActionResult DescargarPlantilla()
    {
        var fileBytes = _excelImportService.GenerarPlantillaActivosFijos();
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Plantilla_ActivosFijos.xlsx");
    }

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

/// <summary>
/// Modelo de request para depreciar un activo
/// </summary>
public class DepreciacionRequest
{
    public DateTime? Fecha { get; set; }
    public string? Observaciones { get; set; }
}

/// <summary>
/// Modelo de request para dar de baja un activo
/// </summary>
public class BajaActivoRequest
{
    [Required(ErrorMessage = "El motivo de baja es obligatorio.")]
    public string Motivo { get; set; } = null!;

    [Required(ErrorMessage = "El nuevo estado es obligatorio.")]
    public string NuevoEstado { get; set; } = null!; // BAJ, VEN, ROB

    public DateTime? FechaBaja { get; set; }
    public decimal? ValorVenta { get; set; }
}

/// <summary>
/// Modelo de request para generar depreciaciones masivas
/// </summary>
public class GenerarDepreciacionMasivaRequest
{
    public int Anio { get; set; }
    public int Mes { get; set; }
    public string? Observaciones { get; set; }
}
