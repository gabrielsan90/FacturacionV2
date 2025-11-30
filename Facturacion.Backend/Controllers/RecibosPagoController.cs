using Facturacion.Backend.Services.Implementations;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

/// <summary>
/// Controlador para gestión de Recibos Electrónicos de Pago (REP)
/// NUEVO en Hacienda v4.4
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class RecibosPagoController : ControllerBase
{
    private readonly IReciboPagoService _reciboPagoService;
    private readonly ILogger<RecibosPagoController> _logger;

    public RecibosPagoController(
        IReciboPagoService reciboPagoService,
        ILogger<RecibosPagoController> logger)
    {
        _reciboPagoService = reciboPagoService;
        _logger = logger;
    }

    /// <summary>
    /// Genera un nuevo Recibo Electrónico de Pago (REP)
    /// </summary>
    /// <param name="dto">Datos del recibo de pago</param>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="terminalId">ID del terminal</param>
    /// <returns>Resultado del proceso</returns>
    [HttpPost("generar")]
    [ProducesResponseType(typeof(ResultadoREP), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResultadoREP>> GenerarREP(
        [FromBody] ReciboPagoDTO dto,
        [FromQuery] Guid empresaId,
        [FromQuery] Guid terminalId)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResultadoREP
                {
                    Exitoso = false,
                    Mensaje = "Datos inválidos",
                    Errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("Usuario no autenticado");

            _logger.LogInformation("Usuario {UserId} generando REP para documento {DocumentoId}", userId, dto.DocumentoOriginalId);

            var resultado = await _reciboPagoService.GenerarREPAsync(dto, empresaId, terminalId, userId);

            if (!resultado.Exitoso)
            {
                return BadRequest(resultado);
            }

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar REP");
            return StatusCode(500, new ResultadoREP
            {
                Exitoso = false,
                Mensaje = "Error interno del servidor",
                Errores = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Obtiene documentos pendientes de pago (cuentas por cobrar)
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="clienteId">Filtro opcional por cliente</param>
    /// <param name="soloVencidos">Si true, solo retorna documentos vencidos</param>
    /// <returns>Lista de documentos con saldo pendiente</returns>
    [HttpGet("pendientes")]
    [ProducesResponseType(typeof(List<DocumentoPendientePagoDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<DocumentoPendientePagoDTO>>> GetDocumentosPendientes(
        [FromQuery] Guid empresaId,
        [FromQuery] Guid? clienteId = null,
        [FromQuery] bool soloVencidos = false)
    {
        try
        {
            var documentos = await _reciboPagoService.GetDocumentosPendientesPagoAsync(empresaId, clienteId, soloVencidos);
            return Ok(documentos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener documentos pendientes");
            return StatusCode(500, new { mensaje = "Error al obtener documentos pendientes", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene el detalle de pagos de un documento específico
    /// </summary>
    /// <param name="documentoId">ID del documento original</param>
    /// <returns>Detalle con todos los pagos aplicados</returns>
    [HttpGet("documento/{documentoId}")]
    [ProducesResponseType(typeof(DetallePagosDocumentoDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DetallePagosDocumentoDTO>> GetDetallePagosDocumento(Guid documentoId)
    {
        try
        {
            var detalle = await _reciboPagoService.GetDetallePagosDocumentoAsync(documentoId);
            return Ok(detalle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle de pagos del documento {DocumentoId}", documentoId);
            return StatusCode(500, new { mensaje = "Error al obtener detalle de pagos", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene un recibo de pago específico
    /// </summary>
    /// <param name="reciboId">ID del recibo</param>
    /// <returns>Recibo de pago completo</returns>
    [HttpGet("{reciboId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> GetRecibo(Guid reciboId)
    {
        try
        {
            var recibo = await _reciboPagoService.GetReciboAsync(reciboId);

            if (recibo == null)
            {
                return NotFound(new { mensaje = "Recibo no encontrado" });
            }

            return Ok(recibo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener recibo {ReciboId}", reciboId);
            return StatusCode(500, new { mensaje = "Error al obtener recibo", error = ex.Message });
        }
    }

    /// <summary>
    /// Calcula el saldo pendiente de un documento
    /// </summary>
    /// <param name="documentoId">ID del documento</param>
    /// <returns>Saldo pendiente</returns>
    [HttpGet("saldo/{documentoId}")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<decimal>> GetSaldoPendiente(Guid documentoId)
    {
        try
        {
            var saldo = await _reciboPagoService.CalcularSaldoPendienteAsync(documentoId);
            return Ok(new { documentoId, saldoPendiente = saldo });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular saldo pendiente del documento {DocumentoId}", documentoId);
            return StatusCode(500, new { mensaje = "Error al calcular saldo", error = ex.Message });
        }
    }

    /// <summary>
    /// Valida si un pago puede ser aplicado a un documento
    /// </summary>
    /// <param name="request">Datos de validación</param>
    /// <returns>Resultado de la validación</returns>
    [HttpPost("validar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> ValidarPago([FromBody] ValidarPagoRequest request)
    {
        try
        {
            var (esValido, mensaje) = await _reciboPagoService.ValidarPagoAsync(request.DocumentoId, request.MontoPago);

            return Ok(new
            {
                esValido,
                mensaje,
                documentoId = request.DocumentoId,
                montoPago = request.MontoPago
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar pago");
            return StatusCode(500, new { mensaje = "Error al validar pago", error = ex.Message });
        }
    }

    /// <summary>
    /// Anula un REP (genera nota de crédito o marca como anulado)
    /// </summary>
    /// <param name="reciboId">ID del recibo a anular</param>
    /// <param name="request">Datos de anulación</param>
    /// <returns>Resultado de la anulación</returns>
    [HttpPost("{reciboId}/anular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> AnularREP(Guid reciboId, [FromBody] AnularREPRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("Usuario no autenticado");

            _logger.LogWarning("Usuario {UserId} anulando REP {ReciboId}. Razón: {Razon}", userId, reciboId, request.Razon);

            var (exitoso, mensaje) = await _reciboPagoService.AnularREPAsync(reciboId, userId, request.Razon);

            if (!exitoso)
            {
                return BadRequest(new { exitoso, mensaje });
            }

            return Ok(new { exitoso, mensaje });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al anular REP {ReciboId}", reciboId);
            return StatusCode(500, new { mensaje = "Error al anular REP", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene estadísticas de cuentas por cobrar de una empresa
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>Estadísticas de cobranza</returns>
    [HttpGet("estadisticas/{empresaId}")]
    [ProducesResponseType(typeof(EstadisticasCobranzaDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EstadisticasCobranzaDTO>> GetEstadisticasCobranza(Guid empresaId)
    {
        try
        {
            var estadisticas = await _reciboPagoService.GetEstadisticasCobranzaAsync(empresaId);
            return Ok(estadisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de cobranza para empresa {EmpresaId}", empresaId);
            return StatusCode(500, new { mensaje = "Error al obtener estadísticas", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene todos los REPs de un documento original
    /// </summary>
    /// <param name="documentoId">ID del documento original</param>
    /// <returns>Lista de recibos de pago</returns>
    [HttpGet("documento/{documentoId}/recibos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> GetRecibosPorDocumento(Guid documentoId)
    {
        try
        {
            var recibos = await _reciboPagoService.GetRecibosPorDocumentoAsync(documentoId);
            return Ok(recibos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener recibos del documento {DocumentoId}", documentoId);
            return StatusCode(500, new { mensaje = "Error al obtener recibos", error = ex.Message });
        }
    }
}

/// <summary>
/// Request para validar un pago
/// </summary>
public class ValidarPagoRequest
{
    public Guid DocumentoId { get; set; }
    public decimal MontoPago { get; set; }
}

/// <summary>
/// Request para anular un REP
/// </summary>
public class AnularREPRequest
{
    public string Razon { get; set; } = null!;
}
