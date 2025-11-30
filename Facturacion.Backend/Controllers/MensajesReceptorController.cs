using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Backend.Controllers;

/// <summary>
/// Controller para gestionar Mensajes Receptor (MR)
/// Permite aceptar, aceptar parcialmente o rechazar documentos recibidos
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class MensajesReceptorController : ControllerBase
{
    private readonly IMensajeReceptorService _mensajeReceptorService;
    private readonly ILogger<MensajesReceptorController> _logger;

    public MensajesReceptorController(
        IMensajeReceptorService mensajeReceptorService,
        ILogger<MensajesReceptorController> logger)
    {
        _mensajeReceptorService = mensajeReceptorService;
        _logger = logger;
    }

    /// <summary>
    /// Genera y envía un Mensaje Receptor a Hacienda
    /// POST /api/mensajesreceptor/generar
    /// </summary>
    [HttpPost("generar")]
    public async Task<IActionResult> GenerarMensajeReceptor([FromBody] GenerarMRRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var resultado = await _mensajeReceptorService.GenerarYEnviarMensajeAsync(
                request.DocumentoOriginalId,
                request.TipoMensaje,
                request.CodigoMensaje,
                request.Detalle,
                request.MontoAceptado,
                request.MontoImpuestoAceptado);

            if (resultado.Exitoso)
            {
                _logger.LogInformation(
                    "Mensaje Receptor generado exitosamente. Clave: {Clave}, Tipo: {Tipo}",
                    resultado.ClaveMensaje,
                    resultado.TipoMensaje);

                return Ok(resultado);
            }
            else
            {
                _logger.LogWarning(
                    "Error al generar Mensaje Receptor: {Mensaje}",
                    resultado.Mensaje);

                return BadRequest(resultado);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar Mensaje Receptor");
            return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene documentos pendientes de enviar Mensaje Receptor
    /// GET /api/mensajesreceptor/pendientes/{empresaId}
    /// </summary>
    [HttpGet("pendientes/{empresaId}")]
    public async Task<IActionResult> GetDocumentosPendientesMR(Guid empresaId)
    {
        try
        {
            var documentos = await _mensajeReceptorService.ObtenerDocumentosPendientesMRAsync(empresaId);

            _logger.LogInformation(
                "Obtenidos {Count} documentos pendientes de MR para empresa {EmpresaId}",
                documentos.Count,
                empresaId);

            return Ok(documentos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener documentos pendientes de MR");
            return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene los mensajes receptor de un documento
    /// GET /api/mensajesreceptor/documento/{documentoId}
    /// </summary>
    [HttpGet("documento/{documentoId}")]
    public async Task<IActionResult> GetMensajesPorDocumento(Guid documentoId)
    {
        try
        {
            var mensajes = await _mensajeReceptorService.ObtenerMensajesPorDocumentoAsync(documentoId);

            return Ok(mensajes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener mensajes del documento");
            return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Reenvía un Mensaje Receptor a Hacienda
    /// POST /api/mensajesreceptor/{id}/reenviar
    /// </summary>
    [HttpPost("{id}/reenviar")]
    public async Task<IActionResult> ReenviarMensaje(Guid id)
    {
        try
        {
            var resultado = await _mensajeReceptorService.ReenviarMensajeAsync(id);

            if (resultado.Exitoso)
            {
                _logger.LogInformation(
                    "Mensaje Receptor reenviado exitosamente. Clave: {Clave}",
                    resultado.ClaveMensaje);

                return Ok(resultado);
            }
            else
            {
                _logger.LogWarning(
                    "Error al reenviar Mensaje Receptor: {Mensaje}",
                    resultado.Mensaje);

                return BadRequest(resultado);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al reenviar Mensaje Receptor");
            return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Valida si se puede enviar un MR para un documento
    /// GET /api/mensajesreceptor/validar/{documentoId}
    /// </summary>
    [HttpGet("validar/{documentoId}")]
    public async Task<IActionResult> ValidarMensajeReceptor(Guid documentoId)
    {
        try
        {
            var (puedeEnviar, razon) = await _mensajeReceptorService.ValidarMensajeReceptorAsync(documentoId);

            return Ok(new
            {
                puedeEnviar,
                razon = razon ?? "Se puede enviar el mensaje receptor"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar Mensaje Receptor");
            return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
        }
    }
}

/// <summary>
/// Request para generar un Mensaje Receptor
/// </summary>
public class GenerarMRRequest
{
    public Guid DocumentoOriginalId { get; set; }
    public TipoMensajeReceptor TipoMensaje { get; set; }
    public CodigoMensajeReceptor CodigoMensaje { get; set; }
    public string? Detalle { get; set; }
    public decimal? MontoAceptado { get; set; }
    public decimal? MontoImpuestoAceptado { get; set; }
}
