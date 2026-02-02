using Facturacion.Backend.Helpers;
using Facturacion.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Backend.Controllers;

/// <summary>
/// Controlador para consultar el tipo de cambio oficial del BCCR (Banco Central de Costa Rica)
/// Recomendación 9 de Hacienda v4.4
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class TipoCambioController : ControllerBase
{
    private readonly ITipoCambioBCCRService _tipoCambioService;
    private readonly ILogger<TipoCambioController> _logger;

    public TipoCambioController(
        ITipoCambioBCCRService tipoCambioService,
        ILogger<TipoCambioController> logger)
    {
        _tipoCambioService = tipoCambioService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el tipo de cambio de compra del dólar para una fecha específica
    /// GET: api/TipoCambio/compra?fecha=2025-12-01
    /// </summary>
    /// <param name="fecha">Fecha para la cual obtener el tipo de cambio (default: fecha actual)</param>
    /// <returns>Tipo de cambio de compra</returns>
    [HttpGet("compra")]
    public async Task<IActionResult> ObtenerCompraAsync([FromQuery] DateTime? fecha = null)
    {
        try
        {
            fecha ??= FechaCostaRicaHelper.Ahora;

            var tipoCambio = await _tipoCambioService.ObtenerTipoCambioCompraAsync(fecha.Value);

            return Ok(new
            {
                fecha = fecha.Value.ToString("yyyy-MM-dd"),
                tipo = "compra",
                valor = tipoCambio,
                moneda = "USD"
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al consultar tipo de cambio de compra");
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar tipo de cambio de compra");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene el tipo de cambio de venta del dólar para una fecha específica
    /// GET: api/TipoCambio/venta?fecha=2025-12-01
    /// </summary>
    /// <param name="fecha">Fecha para la cual obtener el tipo de cambio (default: fecha actual)</param>
    /// <returns>Tipo de cambio de venta</returns>
    [HttpGet("venta")]
    public async Task<IActionResult> ObtenerVentaAsync([FromQuery] DateTime? fecha = null)
    {
        try
        {
            fecha ??= FechaCostaRicaHelper.Ahora;

            var tipoCambio = await _tipoCambioService.ObtenerTipoCambioVentaAsync(fecha.Value);

            return Ok(new
            {
                fecha = fecha.Value.ToString("yyyy-MM-dd"),
                tipo = "venta",
                valor = tipoCambio,
                moneda = "USD"
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al consultar tipo de cambio de venta");
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar tipo de cambio de venta");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene ambos tipos de cambio (compra y venta) para una fecha específica
    /// GET: api/TipoCambio?fecha=2025-12-01
    /// </summary>
    /// <param name="fecha">Fecha para la cual obtener el tipo de cambio (default: fecha actual)</param>
    /// <returns>Tipos de cambio de compra y venta</returns>
    [HttpGet]
    public async Task<IActionResult> ObtenerAsync([FromQuery] DateTime? fecha = null)
    {
        try
        {
            fecha ??= FechaCostaRicaHelper.Ahora;

            var (compra, venta) = await _tipoCambioService.ObtenerTiposCambioAsync(fecha.Value);

            return Ok(new
            {
                fecha = fecha.Value.ToString("yyyy-MM-dd"),
                compra = compra,
                venta = venta,
                moneda = "USD"
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al consultar tipos de cambio");
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar tipos de cambio");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene el tipo de cambio de venta para una moneda específica
    /// GET: api/TipoCambio/moneda/USD?fecha=2025-12-01
    /// </summary>
    /// <param name="codigoMoneda">Código de moneda (USD, EUR, etc.)</param>
    /// <param name="fecha">Fecha para la cual obtener el tipo de cambio (default: fecha actual)</param>
    /// <returns>Tipo de cambio de venta para la moneda especificada</returns>
    [HttpGet("moneda/{codigoMoneda}")]
    public async Task<IActionResult> ObtenerPorMonedaAsync(string codigoMoneda, [FromQuery] DateTime? fecha = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codigoMoneda))
            {
                return BadRequest("El código de moneda es obligatorio");
            }

            fecha ??= FechaCostaRicaHelper.Ahora;

            var tipoCambio = await _tipoCambioService.ObtenerTipoCambioMonedaAsync(codigoMoneda, fecha.Value);

            return Ok(new
            {
                fecha = fecha.Value.ToString("yyyy-MM-dd"),
                moneda = codigoMoneda.ToUpperInvariant(),
                tipo = "venta",
                valor = tipoCambio
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Código de moneda inválido: {CodigoMoneda}", codigoMoneda);
            return BadRequest(new { message = ex.Message });
        }
        catch (NotSupportedException ex)
        {
            _logger.LogWarning(ex, "Moneda no soportada: {CodigoMoneda}", codigoMoneda);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al consultar tipo de cambio para moneda: {CodigoMoneda}", codigoMoneda);
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar tipo de cambio para moneda: {CodigoMoneda}", codigoMoneda);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene los tipos de cambio para múltiples fechas (rango)
    /// POST: api/TipoCambio/rango
    /// Body: { "fechaInicio": "2025-12-01", "fechaFin": "2025-12-31" }
    /// </summary>
    /// <param name="request">Rango de fechas</param>
    /// <returns>Lista de tipos de cambio para cada fecha en el rango</returns>
    [HttpPost("rango")]
    public async Task<IActionResult> ObtenerRangoAsync([FromBody] TipoCambioRangoRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest("Los parámetros de búsqueda son obligatorios");
            }

            if (request.FechaInicio > request.FechaFin)
            {
                return BadRequest("La fecha de inicio debe ser anterior o igual a la fecha de fin");
            }

            var diferenciaDias = (request.FechaFin - request.FechaInicio).TotalDays;
            if (diferenciaDias > 365)
            {
                return BadRequest("El rango de fechas no puede ser mayor a 365 días");
            }

            var resultados = new List<object>();

            for (var fecha = request.FechaInicio; fecha <= request.FechaFin; fecha = fecha.AddDays(1))
            {
                try
                {
                    var (compra, venta) = await _tipoCambioService.ObtenerTiposCambioAsync(fecha);

                    resultados.Add(new
                    {
                        fecha = fecha.ToString("yyyy-MM-dd"),
                        compra = compra,
                        venta = venta,
                        moneda = "USD"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al consultar tipo de cambio para fecha: {Fecha}", fecha);
                    // Continuar con las siguientes fechas
                }
            }

            return Ok(resultados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar tipos de cambio por rango");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// DTO para solicitud de rango de tipos de cambio
    /// </summary>
    public class TipoCambioRangoRequest
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}
