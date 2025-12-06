using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Backend.Controllers;

/// <summary>
/// Controlador para consultar actividades económicas (CIIU)
/// API oficial de Hacienda: https://api.hacienda.go.cr/fe/ae
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class ActividadesEconomicasController : ControllerBase
{
    private readonly IActividadEconomicaService _actividadService;
    private readonly ILogger<ActividadesEconomicasController> _logger;

    public ActividadesEconomicasController(
        IActividadEconomicaService actividadService,
        ILogger<ActividadesEconomicasController> logger)
    {
        _actividadService = actividadService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene información detallada de una actividad económica específica
    /// GET: api/ActividadesEconomicas/620101
    /// </summary>
    /// <param name="codigo">Código de actividad económica</param>
    /// <returns>Información de la actividad económica o NotFound si no existe</returns>
    [HttpGet("{codigo}")]
    public async Task<IActionResult> GetAsync(string codigo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return BadRequest("El código de actividad económica es obligatorio");
            }

            var actividad = await _actividadService.ObtenerPorCodigoAsync(codigo);

            if (actividad == null)
            {
                return NotFound($"No se encontró la actividad económica: {codigo}");
            }

            return Ok(actividad);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al consultar actividad económica: {Codigo}", codigo);
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar actividad económica: {Codigo}", codigo);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Valida que un código de actividad económica existe y está activo
    /// GET: api/ActividadesEconomicas/validar/620101
    /// </summary>
    /// <param name="codigo">Código de actividad económica a validar</param>
    /// <returns>True si el código es válido y está activo, False en caso contrario</returns>
    [HttpGet("validar/{codigo}")]
    public async Task<IActionResult> ValidarAsync(string codigo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return BadRequest("El código de actividad económica es obligatorio");
            }

            var esValido = await _actividadService.ValidarCodigoAsync(codigo);

            return Ok(new { codigo, esValido });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar actividad económica: {Codigo}", codigo);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Busca actividades económicas por descripción
    /// GET: api/ActividadesEconomicas/buscar/descripcion?q=programacion&top=10
    /// </summary>
    /// <param name="q">Texto a buscar en la descripción</param>
    /// <param name="top">Número máximo de resultados (default: 10)</param>
    /// <returns>Lista de actividades económicas que coinciden con la búsqueda</returns>
    [HttpGet("buscar/descripcion")]
    public async Task<IActionResult> BuscarPorDescripcionAsync([FromQuery] string q, [FromQuery] int top = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest("El parámetro de búsqueda 'q' es obligatorio");
            }

            if (top <= 0 || top > 100)
            {
                return BadRequest("El parámetro 'top' debe estar entre 1 y 100");
            }

            var resultado = await _actividadService.BuscarPorDescripcionAsync(q, top);

            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al buscar actividades económicas");
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al buscar actividades económicas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Realiza una búsqueda general de actividades económicas
    /// POST: api/ActividadesEconomicas/buscar
    /// </summary>
    /// <param name="parametros">Parámetros de búsqueda</param>
    /// <returns>Lista de actividades económicas que coinciden con la búsqueda</returns>
    [HttpPost("buscar")]
    public async Task<IActionResult> BuscarAsync([FromBody] ActividadEconomicaBusquedaDTO parametros)
    {
        try
        {
            if (parametros == null)
            {
                return BadRequest("Los parámetros de búsqueda son obligatorios");
            }

            if (parametros.Top <= 0 || parametros.Top > 100)
            {
                parametros.Top = 10;
            }

            var resultado = await _actividadService.BuscarAsync(parametros);

            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al buscar actividades económicas");
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al buscar actividades económicas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene todas las actividades económicas activas
    /// GET: api/ActividadesEconomicas/todas
    /// </summary>
    /// <returns>Lista de todas las actividades económicas activas</returns>
    [HttpGet("todas")]
    public async Task<IActionResult> ObtenerTodasActivasAsync()
    {
        try
        {
            var actividades = await _actividadService.ObtenerTodasActivasAsync();

            return Ok(actividades);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al obtener actividades económicas");
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener actividades económicas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Limpia la caché de actividades económicas
    /// POST: api/ActividadesEconomicas/limpiar-cache
    /// </summary>
    /// <returns>Confirmación de limpieza de caché</returns>
    [HttpPost("limpiar-cache")]
    [Authorize(Roles = "SuperUser,Administrador")]
    public IActionResult LimpiarCacheAsync()
    {
        try
        {
            _actividadService.LimpiarCache();
            _logger.LogInformation("Caché de actividades económicas limpiada");
            return Ok(new { message = "Caché limpiada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al limpiar caché de actividades económicas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
