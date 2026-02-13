using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class CategoriasActivoController : ControllerBase
{
    private readonly ICategoriaActivoRepository _repository;
    private readonly DataContext _context;
    private readonly ILogger<CategoriasActivoController> _logger;
    private readonly IExcelImportService _excelImportService;

    public CategoriasActivoController(
        ICategoriaActivoRepository repository,
        DataContext context,
        ILogger<CategoriasActivoController> logger,
        IExcelImportService excelImportService)
    {
        _repository = repository;
        _context = context;
        _logger = logger;
        _excelImportService = excelImportService;
    }

    /// <summary>
    /// Obtiene todas las categorías de activos de una empresa
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId, [FromQuery] bool? activo = null)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó acceder a empresa {EmpresaId} sin permisos",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
                return Forbid();
            }

            ActionResponse<IEnumerable<CategoriaActivo>> action;

            if (activo.HasValue)
            {
                // Si se solicita filtrado por estado activo, usar repository específico o filtrar después
                if (activo.Value)
                {
                    action = await _repository.GetActivasAsync(empresaId);
                }
                else
                {
                    // Para inactivas, obtener todas y filtrar
                    var todasAction = await _repository.GetByEmpresaAsync(empresaId);
                    if (todasAction.WasSuccess)
                    {
                        var inactivas = todasAction.Result!.Where(c => !c.Activo).ToList();
                        action = new ActionResponse<IEnumerable<CategoriaActivo>>
                        {
                            WasSuccess = true,
                            Result = inactivas
                        };
                    }
                    else
                    {
                        action = todasAction;
                    }
                }
            }
            else
            {
                action = await _repository.GetByEmpresaAsync(empresaId);
            }

            if (!action.WasSuccess)
            {
                _logger.LogError("Error al obtener categorías de activo para empresa {EmpresaId}: {Message}",
                    empresaId, action.Message);
                return BadRequest(action.Message);
            }

            return Ok(action.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener categorías de activo para empresa {EmpresaId}", empresaId);
            return StatusCode(500, $"Error al obtener las categorías: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene una categoría de activo por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var action = await _repository.GetAsync(id);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Categoría de activo {CategoriaId} no encontrada", id);
                return NotFound(action.Message);
            }

            if (!await TieneAccesoEmpresaAsync(action.Result!.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó acceder a categoría {CategoriaId} sin permisos",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), id);
                return Forbid();
            }

            return Ok(action.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener categoría de activo {CategoriaId}", id);
            return StatusCode(500, $"Error al obtener la categoría: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea una nueva categoría de activo
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] CategoriaActivo categoria)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await TieneAccesoEmpresaAsync(categoria.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó crear categoría en empresa {EmpresaId} sin permisos",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), categoria.EmpresaId);
                return Forbid();
            }

            // Validar valores por defecto
            if (categoria.VidaUtilAnios <= 0)
            {
                categoria.VidaUtilAnios = 5;
            }

            if (categoria.PorcentajeDepreciacionAnual <= 0)
            {
                categoria.PorcentajeDepreciacionAnual = 100m / categoria.VidaUtilAnios;
            }

            // Configurar auditoría
            categoria.Id = Guid.NewGuid();
            categoria.UsuarioCreacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var action = await _repository.AddAsync(categoria);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Error al crear categoría de activo: {Message}", action.Message);
                return BadRequest(action.Message);
            }

            _logger.LogInformation("Categoría de activo {CategoriaId} creada exitosamente por usuario {UserId}",
                action.Result!.Id, User.FindFirstValue(ClaimTypes.NameIdentifier));

            return Ok(action.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear categoría de activo");
            return StatusCode(500, $"Error al crear la categoría: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza una categoría de activo
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] CategoriaActivo categoria)
    {
        try
        {
            if (id != categoria.Id)
            {
                return BadRequest("El ID no coincide.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validar porcentaje de depreciación
            if (categoria.VidaUtilAnios <= 0)
            {
                return BadRequest("La vida útil debe ser mayor a 0 años.");
            }

            if (categoria.PorcentajeDepreciacionAnual <= 0 || categoria.PorcentajeDepreciacionAnual > 100)
            {
                return BadRequest("El porcentaje de depreciación anual debe estar entre 0 y 100.");
            }

            // Verificar que la categoría existe y obtener datos de auditoría
            var existenteAction = await _repository.GetAsync(id);
            if (!existenteAction.WasSuccess)
            {
                _logger.LogWarning("Categoría de activo {CategoriaId} no encontrada para actualizar", id);
                return NotFound(existenteAction.Message);
            }

            if (!await TieneAccesoEmpresaAsync(existenteAction.Result!.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó actualizar categoría {CategoriaId} sin permisos",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), id);
                return Forbid();
            }

            // Preservar campos de auditoría
            categoria.FechaCreacion = existenteAction.Result.FechaCreacion;
            categoria.UsuarioCreacionId = existenteAction.Result.UsuarioCreacionId;
            categoria.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var action = await _repository.UpdateAsync(categoria);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Error al actualizar categoría de activo {CategoriaId}: {Message}",
                    id, action.Message);
                return BadRequest(action.Message);
            }

            _logger.LogInformation("Categoría de activo {CategoriaId} actualizada exitosamente por usuario {UserId}",
                id, User.FindFirstValue(ClaimTypes.NameIdentifier));

            return Ok(action.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar categoría de activo {CategoriaId}", id);
            return StatusCode(500, $"Error al actualizar la categoría: {ex.Message}");
        }
    }

    /// <summary>
    /// Elimina (soft delete) una categoría de activo
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            // Verificar que la categoría existe
            var existenteAction = await _repository.GetAsync(id);
            if (!existenteAction.WasSuccess)
            {
                _logger.LogWarning("Categoría de activo {CategoriaId} no encontrada para eliminar", id);
                return NotFound(existenteAction.Message);
            }

            if (!await TieneAccesoEmpresaAsync(existenteAction.Result!.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó eliminar categoría {CategoriaId} sin permisos",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), id);
                return Forbid();
            }

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var action = await _repository.DeleteAsync(id, usuarioId!);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Error al eliminar categoría de activo {CategoriaId}: {Message}",
                    id, action.Message);
                return BadRequest(action.Message);
            }

            _logger.LogInformation("Categoría de activo {CategoriaId} eliminada exitosamente por usuario {UserId}",
                id, usuarioId);

            return Ok(new ActionResponse<bool>
            {
                WasSuccess = true,
                Message = "Categoría eliminada exitosamente."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al eliminar categoría de activo {CategoriaId}", id);
            return StatusCode(500, $"Error al eliminar la categoría: {ex.Message}");
        }
    }

    /// <summary>
    /// Activa o desactiva una categoría
    /// </summary>
    [HttpPatch("{id:guid}/toggle-activo")]
    public async Task<IActionResult> ToggleActivoAsync(Guid id)
    {
        try
        {
            // Obtener la categoría actual
            var existenteAction = await _repository.GetAsync(id);
            if (!existenteAction.WasSuccess)
            {
                _logger.LogWarning("Categoría de activo {CategoriaId} no encontrada para toggle", id);
                return NotFound(existenteAction.Message);
            }

            var categoria = existenteAction.Result!;

            if (!await TieneAccesoEmpresaAsync(categoria.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó cambiar estado de categoría {CategoriaId} sin permisos",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), id);
                return Forbid();
            }

            // Toggle del estado
            categoria.Activo = !categoria.Activo;
            categoria.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var action = await _repository.UpdateAsync(categoria);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Error al cambiar estado de categoría de activo {CategoriaId}: {Message}",
                    id, action.Message);
                return BadRequest(action.Message);
            }

            _logger.LogInformation("Categoría de activo {CategoriaId} {Estado} por usuario {UserId}",
                id, categoria.Activo ? "activada" : "desactivada", User.FindFirstValue(ClaimTypes.NameIdentifier));

            return Ok(new ActionResponse<CategoriaActivo>
            {
                WasSuccess = true,
                Message = $"Categoría {(categoria.Activo ? "activada" : "desactivada")} exitosamente.",
                Result = action.Result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al cambiar estado de categoría de activo {CategoriaId}", id);
            return StatusCode(500, $"Error al cambiar el estado de la categoría: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene estadísticas de activos por categoría
    /// </summary>
    /// <remarks>
    /// Este método usa DataContext directamente debido a la complejidad de las agregaciones.
    /// </remarks>
    [HttpGet("{id:guid}/estadisticas")]
    public async Task<IActionResult> GetEstadisticasAsync(Guid id)
    {
        try
        {
            var categoria = await _context.CategoriasActivo
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (categoria == null)
            {
                _logger.LogWarning("Categoría de activo {CategoriaId} no encontrada para estadísticas", id);
                return NotFound("Categoría de activo no encontrada");
            }

            if (!await TieneAccesoEmpresaAsync(categoria.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó acceder a estadísticas de categoría {CategoriaId} sin permisos",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), id);
                return Forbid();
            }

            var activos = await _context.ActivosFijos
                .Where(a => a.CategoriaActivoId == id && !a.IsDeleted)
                .AsNoTracking()
                .ToListAsync();

            var estadisticas = new
            {
                CantidadTotal = activos.Count,
                CantidadActivos = activos.Count(a => a.Estado == "ACT"),
                CantidadBaja = activos.Count(a => a.Estado == "BAJ"),
                CantidadVendidos = activos.Count(a => a.Estado == "VEN"),
                CantidadRobo = activos.Count(a => a.Estado == "ROB"),
                CantidadDepreciadoTotal = activos.Count(a => a.Estado == "DEP"),
                ValorOriginalTotal = activos.Sum(a => a.ValorOriginal),
                DepreciacionAcumuladaTotal = activos.Sum(a => a.DepreciacionAcumulada),
                ValorActualTotal = activos.Sum(a => a.ValorActual),
                PorcentajeDepreciadoPromedio = activos.Any() ? activos.Average(a => a.PorcentajeDepreciado) : 0
            };

            _logger.LogInformation("Estadísticas obtenidas para categoría {CategoriaId}: {CantidadTotal} activos",
                id, estadisticas.CantidadTotal);

            return Ok(estadisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener estadísticas de categoría {CategoriaId}", id);
            return StatusCode(500, $"Error al obtener estadísticas: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene categorías activas para selectores
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}/activas")]
    public async Task<IActionResult> GetActivasAsync(Guid empresaId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó acceder a categorías activas de empresa {EmpresaId} sin permisos",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
                return Forbid();
            }

            var action = await _repository.GetActivasAsync(empresaId);

            if (!action.WasSuccess)
            {
                _logger.LogError("Error al obtener categorías activas para empresa {EmpresaId}: {Message}",
                    empresaId, action.Message);
                return BadRequest(action.Message);
            }

            // Proyectar a formato para selectores
            var categorias = action.Result!.Select(c => new
            {
                c.Id,
                c.Codigo,
                c.Nombre,
                c.VidaUtilAnios,
                c.PorcentajeDepreciacionAnual,
                CodigoNombre = $"{c.Codigo} - {c.Nombre}"
            }).ToList();

            return Ok(categorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener categorías activas para empresa {EmpresaId}", empresaId);
            return StatusCode(500, $"Error al obtener categorías activas: {ex.Message}");
        }
    }

    /// <summary>
    /// Importa categorías de activo desde un archivo Excel
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
        var result = await _excelImportService.ImportarCategoriasActivoAsync(stream, empresaId, userId!);
        return Ok(result);
    }

    /// <summary>
    /// Descarga una plantilla de Excel para importar categorías de activo
    /// </summary>
    [HttpGet("plantilla")]
    public IActionResult DescargarPlantilla()
    {
        var fileBytes = _excelImportService.GenerarPlantillaCategoriasActivo();
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Plantilla_CategoriasActivo.xlsx");
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
