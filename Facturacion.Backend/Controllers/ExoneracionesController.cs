using Facturacion.Backend.Helpers;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Backend.Controllers;

/// <summary>
/// Controlador para validar documentos de exoneración
/// API oficial de Hacienda: https://api.hacienda.go.cr/fe/ex
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class ExoneracionesController : ControllerBase
{
    private readonly IExoneracionService _exoneracionService;
    private readonly ILogger<ExoneracionesController> _logger;

    public ExoneracionesController(
        IExoneracionService exoneracionService,
        ILogger<ExoneracionesController> logger)
    {
        _exoneracionService = exoneracionService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene información detallada de un documento de exoneración
    /// GET: api/Exoneraciones?numeroDocumento=EX-2025-001&nombreInstitucion=Ministerio de Hacienda
    /// </summary>
    /// <param name="numeroDocumento">Número del documento de autorización</param>
    /// <param name="nombreInstitucion">Nombre de la institución emisora</param>
    /// <returns>Información de la exoneración o NotFound si no existe</returns>
    [HttpGet]
    public async Task<IActionResult> GetAsync(
        [FromQuery] string numeroDocumento,
        [FromQuery] string nombreInstitucion)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(numeroDocumento))
            {
                return BadRequest("El número de documento es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(nombreInstitucion))
            {
                return BadRequest("El nombre de la institución es obligatorio");
            }

            var exoneracion = await _exoneracionService.ObtenerExoneracionAsync(
                numeroDocumento,
                nombreInstitucion);

            if (exoneracion == null)
            {
                return NotFound($"No se encontró el documento de exoneración: {numeroDocumento}");
            }

            return Ok(exoneracion);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al consultar exoneración: {NumeroDocumento}", numeroDocumento);
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar exoneración: {NumeroDocumento}", numeroDocumento);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Valida que un documento de exoneración existe y está vigente
    /// POST: api/Exoneraciones/validar
    /// </summary>
    /// <param name="validacion">Datos de validación</param>
    /// <returns>Resultado de la validación con información detallada</returns>
    [HttpPost("validar")]
    public async Task<IActionResult> ValidarAsync([FromBody] ExoneracionValidacionDTO validacion)
    {
        try
        {
            if (validacion == null)
            {
                return BadRequest("Los datos de validación son obligatorios");
            }

            if (string.IsNullOrWhiteSpace(validacion.NumeroDocumento))
            {
                return BadRequest("El número de documento es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(validacion.NombreInstitucion))
            {
                return BadRequest("El nombre de la institución es obligatorio");
            }

            var resultado = await _exoneracionService.ValidarExoneracionAsync(
                validacion.NumeroDocumento,
                validacion.NombreInstitucion,
                validacion.FechaValidacion);

            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al validar exoneración");
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al validar exoneración");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Verifica si una exoneración está vigente en una fecha específica
    /// GET: api/Exoneraciones/vigente?numeroDocumento=EX-2025-001&nombreInstitucion=Ministerio de Hacienda&fecha=2025-12-01
    /// </summary>
    /// <param name="numeroDocumento">Número del documento de autorización</param>
    /// <param name="nombreInstitucion">Nombre de la institución emisora</param>
    /// <param name="fecha">Fecha a validar (default: fecha actual)</param>
    /// <returns>True si está vigente, False en caso contrario</returns>
    [HttpGet("vigente")]
    public async Task<IActionResult> EstaVigenteAsync(
        [FromQuery] string numeroDocumento,
        [FromQuery] string nombreInstitucion,
        [FromQuery] DateTime? fecha = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(numeroDocumento))
            {
                return BadRequest("El número de documento es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(nombreInstitucion))
            {
                return BadRequest("El nombre de la institución es obligatorio");
            }

            var esVigente = await _exoneracionService.EstaVigenteAsync(
                numeroDocumento,
                nombreInstitucion,
                fecha);

            return Ok(new
            {
                numeroDocumento,
                nombreInstitucion,
                fecha = (fecha ?? FechaCostaRicaHelper.Ahora).ToString("yyyy-MM-dd"),
                vigente = esVigente
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar vigencia de exoneración: {NumeroDocumento}", numeroDocumento);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene todas las exoneraciones vigentes de un beneficiario
    /// GET: api/Exoneraciones/beneficiario/304560789
    /// </summary>
    /// <param name="identificacion">Identificación del beneficiario</param>
    /// <returns>Lista de exoneraciones vigentes del beneficiario</returns>
    [HttpGet("beneficiario/{identificacion}")]
    public async Task<IActionResult> ObtenerPorBeneficiarioAsync(string identificacion)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(identificacion))
            {
                return BadRequest("La identificación del beneficiario es obligatoria");
            }

            var exoneraciones = await _exoneracionService.ObtenerExoneracionesPorBeneficiarioAsync(identificacion);

            return Ok(new
            {
                identificacion,
                total = exoneraciones.Count,
                exoneraciones
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al consultar exoneraciones por beneficiario: {Identificacion}", identificacion);
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar exoneraciones por beneficiario: {Identificacion}", identificacion);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Valida múltiples exoneraciones en una sola llamada
    /// POST: api/Exoneraciones/validar-multiple
    /// </summary>
    /// <param name="validaciones">Lista de exoneraciones a validar</param>
    /// <returns>Lista de resultados de validación</returns>
    [HttpPost("validar-multiple")]
    public async Task<IActionResult> ValidarMultipleAsync([FromBody] List<ExoneracionValidacionDTO> validaciones)
    {
        try
        {
            if (validaciones == null || validaciones.Count == 0)
            {
                return BadRequest("Debe proporcionar al menos una exoneración para validar");
            }

            if (validaciones.Count > 20)
            {
                return BadRequest("No se pueden validar más de 20 exoneraciones a la vez");
            }

            var resultados = new List<ExoneracionValidacionRespuestaDTO>();

            foreach (var validacion in validaciones)
            {
                try
                {
                    var resultado = await _exoneracionService.ValidarExoneracionAsync(
                        validacion.NumeroDocumento,
                        validacion.NombreInstitucion,
                        validacion.FechaValidacion);

                    resultados.Add(resultado);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al validar exoneración: {NumeroDocumento}",
                        validacion.NumeroDocumento);

                    resultados.Add(new ExoneracionValidacionRespuestaDTO
                    {
                        EsValida = false,
                        Mensaje = $"Error al validar: {ex.Message}"
                    });
                }
            }

            return Ok(new
            {
                total = resultados.Count,
                validas = resultados.Count(r => r.EsValida),
                invalidas = resultados.Count(r => !r.EsValida),
                resultados
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al validar múltiples exoneraciones");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Limpia la caché de exoneraciones
    /// POST: api/Exoneraciones/limpiar-cache
    /// </summary>
    /// <returns>Confirmación de limpieza de caché</returns>
    [HttpPost("limpiar-cache")]
    [Authorize(Roles = "SuperUser,Administrador")]
    public IActionResult LimpiarCacheAsync()
    {
        try
        {
            _exoneracionService.LimpiarCache();
            _logger.LogInformation("Caché de exoneraciones limpiada");
            return Ok(new { message = "Caché limpiada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al limpiar caché de exoneraciones");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
