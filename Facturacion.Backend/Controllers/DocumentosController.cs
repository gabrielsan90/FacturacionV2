using Facturacion.Backend.Services.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class DocumentosController : ControllerBase
{
    private readonly IDocumentoUnitOfWork _unitOfWork;
    private readonly IDocumentoHaciendaService _haciendaService;
    private readonly IDocumentoService _documentoService;
    private readonly ILogger<DocumentosController> _logger;

    public DocumentosController(
        IDocumentoUnitOfWork unitOfWork,
        IDocumentoHaciendaService haciendaService,
        IDocumentoService documentoService,
        ILogger<DocumentosController> logger)
    {
        _unitOfWork = unitOfWork;
        _haciendaService = haciendaService;
        _documentoService = documentoService;
        _logger = logger;
    }

    /// <summary>
    /// Get all documents with optional filters (paginated)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAsync(
        [FromQuery] Guid? empresaId,
        [FromQuery] Guid? sucursalId,
        [FromQuery] Guid? terminalId,
        [FromQuery] EstadoDocumento? estado,
        [FromQuery] DocumentoTipo? tipoDocumento,
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        [FromQuery] Ambiente? ambiente)
    {
        try
        {
            if (!empresaId.HasValue)
            {
                return BadRequest("Debe proporcionar el parámetro empresaId.");
            }

            // Obtener todos los documentos de la empresa
            var documentos = await _unitOfWork.DocumentoRepository.GetByEmpresaAsync(empresaId.Value);

            // Aplicar filtros adicionales
            if (sucursalId.HasValue)
            {
                documentos = documentos.Where(d => d.SucursalId == sucursalId.Value);
            }

            if (terminalId.HasValue)
            {
                documentos = documentos.Where(d => d.TerminalId == terminalId.Value);
            }

            if (estado.HasValue)
            {
                documentos = documentos.Where(d => d.Estado == estado.Value);
            }

            if (tipoDocumento.HasValue)
            {
                documentos = documentos.Where(d => d.TipoDocumento == tipoDocumento.Value);
            }

            if (fechaInicio.HasValue)
            {
                documentos = documentos.Where(d => d.FechaEmision >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                // Agregar un día para incluir todo el día final
                var fechaFinInclusive = fechaFin.Value.AddDays(1);
                documentos = documentos.Where(d => d.FechaEmision < fechaFinInclusive);
            }

            if (ambiente.HasValue)
            {
                documentos = documentos.Where(d => d.Ambiente == ambiente.Value);
            }

            // Ordenar por fecha de emisión descendente
            documentos = documentos.OrderByDescending(d => d.FechaEmision);

            return Ok(documentos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Get document by ID with all details
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var documento = await _unitOfWork.DocumentoRepository.GetWithDetallesAsync(id);

            if (documento == null)
            {
                return NotFound($"Documento con ID {id} no encontrado.");
            }

            return Ok(documento);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Get document by Clave (50-digit key)
    /// </summary>
    [HttpGet("clave/{clave}")]
    public async Task<IActionResult> GetByClaveAsync(string clave)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(clave) || clave.Length != 50)
            {
                return BadRequest("La clave debe tener exactamente 50 caracteres.");
            }

            var documento = await _unitOfWork.DocumentoRepository.GetByClaveAsync(clave);

            if (documento == null)
            {
                return NotFound($"Documento con clave {clave} no encontrado.");
            }

            return Ok(documento);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Get document by consecutive number
    /// </summary>
    [HttpGet("consecutivo/{empresaId:guid}/{consecutivo}")]
    public async Task<IActionResult> GetByConsecutivoAsync(Guid empresaId, string consecutivo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(consecutivo))
            {
                return BadRequest("El número consecutivo es requerido.");
            }

            var documento = await _unitOfWork.DocumentoRepository.GetByConsecutivoAsync(empresaId, consecutivo);

            if (documento == null)
            {
                return NotFound($"Documento con consecutivo {consecutivo} no encontrado.");
            }

            return Ok(documento);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Get documents by empresa
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var documentos = await _unitOfWork.DocumentoRepository.GetByEmpresaAsync(empresaId);
            return Ok(documentos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Get documents by sucursal
    /// </summary>
    [HttpGet("sucursal/{sucursalId:guid}")]
    public async Task<IActionResult> GetBySucursalAsync(Guid sucursalId)
    {
        try
        {
            var documentos = await _unitOfWork.DocumentoRepository.GetBySucursalAsync(sucursalId);
            return Ok(documentos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Get documents by terminal
    /// </summary>
    [HttpGet("terminal/{terminalId:guid}")]
    public async Task<IActionResult> GetByTerminalAsync(Guid terminalId)
    {
        try
        {
            var documentos = await _unitOfWork.DocumentoRepository.GetByTerminalAsync(terminalId);
            return Ok(documentos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Get documents by cliente
    /// </summary>
    [HttpGet("cliente/{clienteId:guid}")]
    public async Task<IActionResult> GetByClienteAsync(Guid clienteId)
    {
        try
        {
            var documentos = await _unitOfWork.DocumentoRepository.GetByClienteAsync(clienteId);
            return Ok(documentos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Get pending documents for sending to Hacienda
    /// </summary>
    [HttpGet("pendientes/{empresaId:guid}")]
    public async Task<IActionResult> GetPendientesAsync(Guid empresaId)
    {
        try
        {
            var documentos = await _unitOfWork.DocumentoRepository.GetPendientesEnvioAsync(empresaId);
            return Ok(documentos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Create new document using DTO (automatically generates consecutive number and calculates totals)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] CreateDocumentoDTO dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get current user ID from JWT claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuario no autenticado.");
            }

            // Create document from DTO (includes consecutive number generation and totals calculation)
            var documento = await _documentoService.CrearDocumentoDesdeDTO(dto, userId);

            // Validate document
            var erroresValidacion = await _documentoService.ValidarDocumentoAsync(documento);
            if (erroresValidacion.Any())
            {
                return BadRequest(new
                {
                    Mensaje = "El documento tiene errores de validación",
                    Errores = erroresValidacion
                });
            }

            // Save to database
            var nuevoDocumento = await _unitOfWork.DocumentoRepository.AddAsync(documento);

            _logger.LogInformation(
                "Documento creado: ID={DocumentoId}, Tipo={TipoDocumento}, Consecutivo={Consecutivo}, Usuario={UserId}",
                nuevoDocumento.Id, nuevoDocumento.TipoDocumento, nuevoDocumento.NumeroConsecutivo, userId);

            return CreatedAtAction(nameof(GetAsync), new { id = nuevoDocumento.Id }, nuevoDocumento);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error de validación al crear documento");
            return BadRequest(new { Mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el documento");
            return StatusCode(500, new { Mensaje = "Error al crear el documento", Detalle = ex.Message });
        }
    }

    /// <summary>
    /// Update document (only if in Borrador state)
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Documento documento)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != documento.Id)
            {
                return BadRequest("El ID del documento no coincide con el ID de la URL.");
            }

            // Get existing document
            var documentoExistente = await _unitOfWork.DocumentoRepository.GetAsync(id);
            if (documentoExistente == null)
            {
                return NotFound($"Documento con ID {id} no encontrado.");
            }

            // Only allow modifications if document is in Borrador state
            if (documentoExistente.Estado != EstadoDocumento.Borrador)
            {
                return BadRequest($"No se puede modificar un documento en estado {documentoExistente.Estado}. Solo se permiten modificaciones en estado Borrador.");
            }

            // Get current user ID from JWT claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuario no autenticado.");
            }

            // Set audit fields
            documento.UsuarioModificacionId = userId;
            documento.FechaModificacion = DateTime.UtcNow;

            await _unitOfWork.DocumentoRepository.UpdateAsync(documento);

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar el documento: {ex.Message}");
        }
    }

    /// <summary>
    /// Soft delete document (only if in Borrador state)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            // Get existing document
            var documento = await _unitOfWork.DocumentoRepository.GetAsync(id);
            if (documento == null)
            {
                return NotFound($"Documento con ID {id} no encontrado.");
            }

            // Only allow deletion if document is in Borrador state
            if (documento.Estado != EstadoDocumento.Borrador)
            {
                return BadRequest($"No se puede eliminar un documento en estado {documento.Estado}. Solo se permiten eliminaciones en estado Borrador.");
            }

            // Get current user ID from JWT claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuario no autenticado.");
            }

            await _unitOfWork.DocumentoRepository.DeleteAsync(id, userId);

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al eliminar el documento: {ex.Message}");
        }
    }

    // ========================================
    // HACIENDA ENDPOINTS
    // ========================================

    /// <summary>
    /// Process and send document to Hacienda
    /// Generates Clave, XML, signs, and sends to Hacienda API
    /// </summary>
    [HttpPost("{id:guid}/procesar")]
    public async Task<IActionResult> ProcesarYEnviarAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Procesando y enviando documento {DocumentoId} a Hacienda", id);

            var resultado = await _haciendaService.ProcesarYEnviarAsync(id);

            if (resultado.Exitoso)
            {
                return Ok(resultado);
            }
            else
            {
                return BadRequest(resultado);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar documento {DocumentoId}", id);
            return StatusCode(500, new
            {
                Exitoso = false,
                Mensaje = "Error interno al procesar el documento",
                Detalle = ex.Message
            });
        }
    }

    /// <summary>
    /// Check document status in Hacienda
    /// </summary>
    [HttpGet("{id:guid}/consultar")]
    public async Task<IActionResult> ConsultarEstadoAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Consultando estado de documento {DocumentoId} en Hacienda", id);

            var resultado = await _haciendaService.ConsultarEstadoAsync(id);

            if (resultado.Exitoso)
            {
                return Ok(resultado);
            }
            else
            {
                return BadRequest(resultado);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar estado de documento {DocumentoId}", id);
            return StatusCode(500, new
            {
                Exitoso = false,
                Mensaje = "Error interno al consultar el estado",
                Detalle = ex.Message
            });
        }
    }

    /// <summary>
    /// Resend rejected document to Hacienda
    /// </summary>
    [HttpPost("{id:guid}/reenviar")]
    public async Task<IActionResult> ReenviarAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Reenviando documento {DocumentoId} a Hacienda", id);

            var exitoso = await _haciendaService.ReenviarAsync(id);

            if (exitoso)
            {
                return Ok(new
                {
                    Exitoso = true,
                    Mensaje = "Documento reenviado exitosamente"
                });
            }
            else
            {
                return BadRequest(new
                {
                    Exitoso = false,
                    Mensaje = "No se pudo reenviar el documento. Verifique que esté en estado Rechazado o Contingencia."
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al reenviar documento {DocumentoId}", id);
            return StatusCode(500, new
            {
                Exitoso = false,
                Mensaje = "Error interno al reenviar el documento",
                Detalle = ex.Message
            });
        }
    }

    /// <summary>
    /// Generate XML without sending (for preview/testing)
    /// </summary>
    [HttpGet("{id:guid}/xml")]
    public async Task<IActionResult> GenerarXmlAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Generando XML para documento {DocumentoId}", id);

            var xml = await _haciendaService.GenerarXmlAsync(id);

            return Content(xml, "application/xml");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar XML de documento {DocumentoId}", id);
            return StatusCode(500, new
            {
                Mensaje = "Error al generar el XML",
                Detalle = ex.Message
            });
        }
    }

    /// <summary>
    /// Validate document before sending
    /// </summary>
    [HttpGet("{id:guid}/validar")]
    public async Task<IActionResult> ValidarDocumentoAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Validando documento {DocumentoId}", id);

            var errores = await _haciendaService.ValidarDocumentoAsync(id);

            if (errores.Any())
            {
                return Ok(new
                {
                    Valido = false,
                    Errores = errores
                });
            }
            else
            {
                return Ok(new
                {
                    Valido = true,
                    Mensaje = "El documento es válido y puede ser enviado a Hacienda"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar documento {DocumentoId}", id);
            return StatusCode(500, new
            {
                Mensaje = "Error al validar el documento",
                Detalle = ex.Message
            });
        }
    }

    // ========================================
    // ADDITIONAL BUSINESS ENDPOINTS
    // ========================================

    /// <summary>
    /// Get next consecutive number for a terminal and document type
    /// </summary>
    [HttpGet("consecutivo/{terminalId:guid}/siguiente")]
    public async Task<IActionResult> GetSiguienteConsecutivoAsync(
        Guid terminalId,
        [FromQuery] string tipoDocumento = "01")
    {
        try
        {
            var consecutivo = await _documentoService.ObtenerSiguienteConsecutivoAsync(terminalId, tipoDocumento);

            return Ok(new
            {
                TerminalId = terminalId,
                TipoDocumento = tipoDocumento,
                SiguienteConsecutivo = consecutivo
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener siguiente consecutivo para terminal {TerminalId}", terminalId);
            return StatusCode(500, new { Mensaje = "Error al obtener el siguiente consecutivo", Detalle = ex.Message });
        }
    }

    /// <summary>
    /// Download signed XML of a document
    /// </summary>
    [HttpGet("{id:guid}/descargar-xml")]
    public async Task<IActionResult> DescargarXmlAsync(Guid id)
    {
        try
        {
            var documento = await _unitOfWork.DocumentoRepository.GetAsync(id);

            if (documento == null)
            {
                return NotFound($"Documento con ID {id} no encontrado.");
            }

            if (string.IsNullOrWhiteSpace(documento.XmlFirmado))
            {
                return BadRequest("El documento no tiene XML firmado disponible. Debe procesarse primero.");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(documento.XmlFirmado);
            var fileName = $"{documento.NumeroConsecutivo.Replace("-", "")}.xml";

            return File(bytes, "application/xml", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar XML del documento {DocumentoId}", id);
            return StatusCode(500, new { Mensaje = "Error al descargar el XML", Detalle = ex.Message });
        }
    }

    /// <summary>
    /// Download PDF of a document (if generated)
    /// </summary>
    [HttpGet("{id:guid}/descargar-pdf")]
    public async Task<IActionResult> DescargarPdfAsync(Guid id)
    {
        try
        {
            var documento = await _unitOfWork.DocumentoRepository.GetAsync(id);

            if (documento == null)
            {
                return NotFound($"Documento con ID {id} no encontrado.");
            }

            if (string.IsNullOrWhiteSpace(documento.PDF))
            {
                return BadRequest("El documento no tiene PDF generado.");
            }

            // TODO: Implement PDF download from file path or storage
            return BadRequest("Funcionalidad de PDF aún no implementada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar PDF del documento {DocumentoId}", id);
            return StatusCode(500, new { Mensaje = "Error al descargar el PDF", Detalle = ex.Message });
        }
    }

    /// <summary>
    /// Generate PDF for a document
    /// </summary>
    [HttpPost("{id:guid}/generar-pdf")]
    public async Task<IActionResult> GenerarPdfAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Generando PDF para documento {DocumentoId}", id);

            // TODO: Implement PDF generation service
            return BadRequest(new
            {
                Mensaje = "La generación de PDF aún no está implementada",
                DocumentoId = id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar PDF del documento {DocumentoId}", id);
            return StatusCode(500, new { Mensaje = "Error al generar el PDF", Detalle = ex.Message });
        }
    }

    /// <summary>
    /// Cancel a document (generates Mensaje Receptor if needed)
    /// </summary>
    [HttpPost("{id:guid}/anular")]
    public async Task<IActionResult> AnularAsync(Guid id, [FromBody] AnularDocumentoDTO? dto = null)
    {
        try
        {
            var documento = await _unitOfWork.DocumentoRepository.GetAsync(id);

            if (documento == null)
            {
                return NotFound($"Documento con ID {id} no encontrado.");
            }

            // Only accepted documents can be cancelled
            if (documento.Estado != EstadoDocumento.Aceptado)
            {
                return BadRequest($"Solo se pueden anular documentos en estado Aceptado. Estado actual: {documento.Estado}");
            }

            // Get current user ID from JWT claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuario no autenticado.");
            }

            documento.Estado = EstadoDocumento.Anulado;
            documento.FechaModificacion = DateTime.UtcNow;
            documento.UsuarioModificacionId = userId;

            if (dto != null && !string.IsNullOrWhiteSpace(dto.Motivo))
            {
                documento.Observaciones = $"ANULADO: {dto.Motivo}. {documento.Observaciones}";
            }

            await _unitOfWork.DocumentoRepository.UpdateAsync(documento);

            _logger.LogInformation("Documento {DocumentoId} anulado por usuario {UserId}", id, userId);

            // TODO: Generate and send Mensaje Receptor (type 05) to Hacienda

            return Ok(new
            {
                Mensaje = "Documento anulado exitosamente",
                DocumentoId = id,
                Estado = documento.Estado
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al anular documento {DocumentoId}", id);
            return StatusCode(500, new { Mensaje = "Error al anular el documento", Detalle = ex.Message });
        }
    }

    /// <summary>
    /// Get Hacienda response XML for a document
    /// </summary>
    [HttpGet("{id:guid}/respuesta-hacienda")]
    public async Task<IActionResult> GetRespuestaHaciendaAsync(Guid id)
    {
        try
        {
            var documento = await _unitOfWork.DocumentoRepository.GetAsync(id);

            if (documento == null)
            {
                return NotFound($"Documento con ID {id} no encontrado.");
            }

            if (string.IsNullOrWhiteSpace(documento.XmlRespuestaHacienda))
            {
                return BadRequest("El documento no tiene respuesta de Hacienda disponible.");
            }

            return Content(documento.XmlRespuestaHacienda, "application/xml");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener respuesta de Hacienda del documento {DocumentoId}", id);
            return StatusCode(500, new { Mensaje = "Error al obtener la respuesta de Hacienda", Detalle = ex.Message });
        }
    }

    /// <summary>
    /// Download ZIP file containing XML firmado, respuesta Hacienda, and PDF
    /// </summary>
    [HttpGet("{id:guid}/download-zip")]
    public async Task<IActionResult> DownloadZipAsync(Guid id)
    {
        try
        {
            var documento = await _unitOfWork.DocumentoRepository.GetAsync(id);

            if (documento == null)
            {
                return NotFound($"Documento con ID {id} no encontrado.");
            }

            if (string.IsNullOrWhiteSpace(documento.XmlFirmado))
            {
                return BadRequest("El documento no ha sido enviado a Hacienda aún.");
            }

            using var memoryStream = new System.IO.MemoryStream();
            using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
            {
                // Add XML firmado
                if (!string.IsNullOrWhiteSpace(documento.XmlFirmado))
                {
                    var xmlEntry = archive.CreateEntry($"{documento.NumeroConsecutivo.Replace("-", "")}.xml");
                    using var xmlStream = xmlEntry.Open();
                    var xmlBytes = System.Text.Encoding.UTF8.GetBytes(documento.XmlFirmado);
                    xmlStream.Write(xmlBytes, 0, xmlBytes.Length);
                }

                // Add respuesta Hacienda
                if (!string.IsNullOrWhiteSpace(documento.XmlRespuestaHacienda))
                {
                    var respuestaEntry = archive.CreateEntry("respuestahacienda.xml");
                    using var respuestaStream = respuestaEntry.Open();
                    var respuestaBytes = System.Text.Encoding.UTF8.GetBytes(documento.XmlRespuestaHacienda);
                    respuestaStream.Write(respuestaBytes, 0, respuestaBytes.Length);
                }

                // TODO: Add PDF when PDF generation is implemented
                // if (!string.IsNullOrWhiteSpace(documento.PDF))
                // {
                //     var pdfEntry = archive.CreateEntry("factura.pdf");
                //     // Add PDF bytes
                // }
            }

            memoryStream.Position = 0;
            var fileName = $"{documento.NumeroConsecutivo.Replace("-", "")}.zip";

            return File(memoryStream.ToArray(), "application/zip", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar ZIP del documento {DocumentoId}", id);
            return StatusCode(500, new { Mensaje = "Error al generar el archivo ZIP", Detalle = ex.Message });
        }
    }

    /// <summary>
    /// Send document by email
    /// </summary>
    [HttpPost("enviar-correo")]
    public async Task<IActionResult> EnviarCorreoAsync([FromBody] EnviarCorreoDTO dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // TODO: Implement email service integration
            // For now, return success to indicate the endpoint is working
            _logger.LogInformation("Solicitud de envío de correo para documento {DocumentoId}", dto.DocumentoId);

            return Ok(new
            {
                Exitoso = true,
                Mensaje = "La funcionalidad de envío de correo se implementará en un próximo sprint."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo del documento");
            return StatusCode(500, new { Mensaje = "Error al enviar el correo", Detalle = ex.Message });
        }
    }
}

/// <summary>
/// DTO para anular un documento
/// </summary>
public class AnularDocumentoDTO
{
    public string? Motivo { get; set; }
}

/// <summary>
/// DTO para enviar documento por correo
/// </summary>
public class EnviarCorreoDTO
{
    public Guid DocumentoId { get; set; }
    public string Para { get; set; } = null!;
    public string? CC { get; set; }
    public string? CCO { get; set; }
    public string Asunto { get; set; } = null!;
    public string Mensaje { get; set; } = null!;
}
