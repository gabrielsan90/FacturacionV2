using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Backend.Controllers;

/// <summary>
/// Controlador para consultar códigos CABYS (Códigos de Actividades, Bienes y Servicios)
/// API oficial de Hacienda: https://api.hacienda.go.cr/fe/cabys
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class CabysController : ControllerBase
{
    private readonly ICabysService _cabysService;
    private readonly ILogger<CabysController> _logger;

    public CabysController(ICabysService cabysService, ILogger<CabysController> logger)
    {
        _cabysService = cabysService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene información detallada de un código CABYS específico
    /// GET: api/Cabys/12345678901234
    /// </summary>
    /// <param name="codigo">Código CABYS (13 dígitos)</param>
    /// <returns>Información del código CABYS o NotFound si no existe</returns>
    [HttpGet("{codigo}")]
    public async Task<IActionResult> GetAsync(string codigo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return BadRequest("El código CABYS es obligatorio");
            }

            var cabys = await _cabysService.ObtenerPorCodigoAsync(codigo);

            if (cabys == null)
            {
                return NotFound($"No se encontró el código CABYS: {codigo}");
            }

            return Ok(cabys);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al consultar código CABYS: {Codigo}", codigo);
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar código CABYS: {Codigo}", codigo);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Valida que un código CABYS existe y es válido
    /// GET: api/Cabys/validar/12345678901234
    /// </summary>
    /// <param name="codigo">Código CABYS a validar (13 dígitos)</param>
    /// <returns>True si el código es válido, False en caso contrario</returns>
    [HttpGet("validar/{codigo}")]
    public async Task<IActionResult> ValidarAsync(string codigo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return BadRequest("El código CABYS es obligatorio");
            }

            var esValido = await _cabysService.ValidarCodigoAsync(codigo);

            return Ok(new { codigo, esValido });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar código CABYS: {Codigo}", codigo);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Busca códigos CABYS por descripción
    /// GET: api/Cabys/buscar/descripcion?q=cafe&top=10
    /// </summary>
    /// <param name="q">Texto a buscar en la descripción</param>
    /// <param name="top">Número máximo de resultados (default: 10)</param>
    /// <returns>Lista de códigos CABYS que coinciden con la búsqueda</returns>
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

            var resultado = await _cabysService.BuscarPorDescripcionAsync(q, top);

            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al buscar códigos CABYS");
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al buscar códigos CABYS");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Realiza una búsqueda general de códigos CABYS
    /// POST: api/Cabys/buscar
    /// </summary>
    /// <param name="parametros">Parámetros de búsqueda</param>
    /// <returns>Lista de códigos CABYS que coinciden con la búsqueda</returns>
    [HttpPost("buscar")]
    public async Task<IActionResult> BuscarAsync([FromBody] CabysBusquedaDTO parametros)
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

            var resultado = await _cabysService.BuscarAsync(parametros);

            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al buscar códigos CABYS");
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al buscar códigos CABYS");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene el porcentaje de impuesto para un código CABYS
    /// GET: api/Cabys/12345678901234/impuesto
    /// </summary>
    /// <param name="codigo">Código CABYS (13 dígitos)</param>
    /// <returns>Porcentaje de impuesto o NotFound si el código no existe</returns>
    [HttpGet("{codigo}/impuesto")]
    public async Task<IActionResult> ObtenerImpuestoAsync(string codigo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return BadRequest("El código CABYS es obligatorio");
            }

            var impuesto = await _cabysService.ObtenerPorcentajeImpuestoAsync(codigo);

            if (!impuesto.HasValue)
            {
                return NotFound($"No se encontró el código CABYS: {codigo}");
            }

            return Ok(new { codigo, impuesto = impuesto.Value });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al consultar impuesto de código CABYS: {Codigo}", codigo);
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar impuesto de código CABYS: {Codigo}", codigo);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Limpia la caché de códigos CABYS
    /// POST: api/Cabys/limpiar-cache
    /// </summary>
    /// <returns>Confirmación de limpieza de caché</returns>
    [HttpPost("limpiar-cache")]
    [Authorize(Roles = "SuperUser,Administrador")]
    public IActionResult LimpiarCacheAsync()
    {
        try
        {
            _cabysService.LimpiarCache();
            _logger.LogInformation("Caché de códigos CABYS limpiada");
            return Ok(new { message = "Caché limpiada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al limpiar caché de códigos CABYS");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
