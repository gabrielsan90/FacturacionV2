using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    private readonly IHaciendaApiService _haciendaApiService;
    private readonly ILogger<MensajesReceptorController> _logger;
    private readonly DataContext _context;

    public MensajesReceptorController(
        IMensajeReceptorService mensajeReceptorService,
        IHaciendaApiService haciendaApiService,
        ILogger<MensajesReceptorController> logger,
        DataContext context)
    {
        _mensajeReceptorService = mensajeReceptorService;
        _haciendaApiService = haciendaApiService;
        _logger = logger;
        _context = context;
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
    /// Verifica el estado de un MR en Procesando consultando a Hacienda
    /// POST /api/mensajesreceptor/{id}/verificar-estado
    /// </summary>
    [HttpPost("{id}/verificar-estado")]
    public async Task<IActionResult> VerificarEstado(Guid id)
    {
        try
        {
            var mensaje = await _context.DocumentoReceptorMensajes
                .Include(m => m.DocumentoOriginal)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (mensaje == null)
                return NotFound(new { mensaje = "Mensaje receptor no encontrado" });

            if (mensaje.Estado != "Procesando")
                return Ok(new { mensaje = $"El mensaje ya tiene estado final: {mensaje.Estado}", estado = mensaje.Estado });

            if (string.IsNullOrEmpty(mensaje.ClaveMensaje))
                return BadRequest(new { mensaje = "El mensaje no tiene clave asignada" });

            var empresaId = mensaje.DocumentoOriginal?.EmpresaId ?? Guid.Empty;
            if (empresaId == Guid.Empty)
                return BadRequest(new { mensaje = "No se pudo determinar la empresa del documento" });

            var respuesta = await _haciendaApiService.ConsultarEstadoConTokenAsync(
                mensaje.ClaveMensaje, empresaId, "stag");

            if (respuesta != null && !string.IsNullOrEmpty(respuesta.IndEstado))
            {
                var indEstado = respuesta.IndEstado.ToLower().Trim();
                if (indEstado == "aceptado")
                {
                    mensaje.Estado = "Aceptado";
                    mensaje.MensajeHacienda = respuesta.RespuestaXml ?? "Aceptado por Hacienda";
                    mensaje.FechaRespuestaHacienda = Helpers.FechaCostaRicaHelper.Ahora;
                }
                else if (indEstado == "rechazado")
                {
                    mensaje.Estado = "Rechazado";
                    mensaje.MensajeHacienda = respuesta.RespuestaXml ?? "Rechazado por Hacienda";
                    mensaje.FechaRespuestaHacienda = Helpers.FechaCostaRicaHelper.Ahora;
                }

                await _context.SaveChangesAsync();
                return Ok(new { mensaje = $"Estado actualizado: {mensaje.Estado}", estado = mensaje.Estado });
            }

            return Ok(new { mensaje = "Hacienda aún no tiene respuesta. El mensaje sigue en procesamiento.", estado = "Procesando" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar estado de Mensaje Receptor {Id}", id);
            return StatusCode(500, new { mensaje = "Error al consultar Hacienda", error = ex.Message });
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

    /// <summary>
    /// Obtiene todos los mensajes receptor con filtros opcionales
    /// GET /api/mensajesreceptor
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMensajes(
        [FromQuery] Guid? empresaId,
        [FromQuery] string? fechaInicio,
        [FromQuery] string? fechaFin,
        [FromQuery] int? tipoMensaje,
        [FromQuery] string? estado)
    {
        try
        {
            var query = _context.Set<Shared.Entities.DocumentoReceptorMensaje>()
                .Include(m => m.DocumentoOriginal)
                .Where(m => !m.IsDeleted);

            // Apply filters
            if (empresaId.HasValue)
            {
                query = query.Where(m => m.DocumentoOriginal!.EmpresaId == empresaId.Value);
            }

            if (!string.IsNullOrEmpty(fechaInicio) && DateTime.TryParse(fechaInicio, out var fInicio))
            {
                query = query.Where(m => m.FechaEmision >= fInicio);
            }

            if (!string.IsNullOrEmpty(fechaFin) && DateTime.TryParse(fechaFin, out var fFin))
            {
                fFin = fFin.AddDays(1); // Include entire day
                query = query.Where(m => m.FechaEmision < fFin);
            }

            if (tipoMensaje.HasValue)
            {
                query = query.Where(m => m.TipoMensaje == tipoMensaje.Value);
            }

            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(m => m.Estado == estado);
            }

            var mensajes = await query
                .OrderByDescending(m => m.FechaEmision)
                .Select(m => new
                {
                    m.Id,
                    m.ClaveMensaje,
                    m.NumeroConsecutivo,
                    m.TipoMensaje,
                    m.CodigoMensaje,
                    m.DetalleMensaje,
                    m.FechaEmision,
                    m.FechaEnvioHacienda,
                    m.FechaRespuestaHacienda,
                    m.MensajeHacienda,
                    m.Estado,
                    m.MontoTotalAceptado,
                    m.MontoTotalImpuestoAceptado,
                    m.DocumentoOriginalId,
                    DocumentoOriginal = m.DocumentoOriginal == null ? null : new
                    {
                        m.DocumentoOriginal.Id,
                        m.DocumentoOriginal.Clave,
                        m.DocumentoOriginal.NumeroConsecutivo,
                        m.DocumentoOriginal.TipoDocumento
                    }
                })
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} mensajes receptor", mensajes.Count);

            return Ok(mensajes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener mensajes receptor");
            return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene un mensaje receptor por ID
    /// GET /api/mensajesreceptor/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMensaje(Guid id)
    {
        try
        {
            var mensaje = await _context.Set<Shared.Entities.DocumentoReceptorMensaje>()
                .Include(m => m.DocumentoOriginal)
                .Where(m => m.Id == id && !m.IsDeleted)
                .Select(m => new
                {
                    m.Id,
                    m.ClaveMensaje,
                    m.NumeroConsecutivo,
                    m.TipoMensaje,
                    m.CodigoMensaje,
                    m.DetalleMensaje,
                    m.FechaEmision,
                    m.FechaEnvioHacienda,
                    m.FechaRespuestaHacienda,
                    m.MensajeHacienda,
                    m.Estado,
                    m.MontoTotalAceptado,
                    m.MontoTotalImpuestoAceptado,
                    m.DocumentoOriginalId,
                    m.XmlGenerado,
                    m.XmlFirmado,
                    DocumentoOriginal = m.DocumentoOriginal == null ? null : new
                    {
                        m.DocumentoOriginal.Id,
                        m.DocumentoOriginal.Clave,
                        m.DocumentoOriginal.NumeroConsecutivo,
                        m.DocumentoOriginal.TipoDocumento
                    }
                })
                .FirstOrDefaultAsync();

            if (mensaje == null)
            {
                return NotFound(new { mensaje = "Mensaje receptor no encontrado" });
            }

            return Ok(mensaje);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener mensaje receptor");
            return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Descarga el XML firmado de un mensaje receptor
    /// GET /api/mensajesreceptor/{id}/descargar-xml
    /// </summary>
    [HttpGet("{id}/descargar-xml")]
    public async Task<IActionResult> DescargarXml(Guid id)
    {
        try
        {
            var mensaje = await _context.Set<Shared.Entities.DocumentoReceptorMensaje>()
                .Where(m => m.Id == id && !m.IsDeleted)
                .FirstOrDefaultAsync();

            if (mensaje == null)
            {
                return NotFound(new { mensaje = "Mensaje receptor no encontrado" });
            }

            if (string.IsNullOrEmpty(mensaje.XmlFirmado))
            {
                return BadRequest(new { mensaje = "XML no disponible para este mensaje" });
            }

            var xmlBytes = System.Text.Encoding.UTF8.GetBytes(mensaje.XmlFirmado);
            var fileName = $"MR_{mensaje.ClaveMensaje}.xml";

            return File(xmlBytes, "application/xml", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar XML");
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
