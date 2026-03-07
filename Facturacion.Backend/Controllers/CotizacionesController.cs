using Facturacion.Backend.Data;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Facturacion.Shared.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class CotizacionesController : ControllerBase
{
    private readonly ICuentaPorCobrarUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly ILogger<CotizacionesController> _logger;

    public CotizacionesController(
        ICuentaPorCobrarUnitOfWork unitOfWork,
        DataContext context,
        ILogger<CotizacionesController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all quotations for a specific company (multi-tenant filtering)
    /// </summary>
    [HttpGet("empresa/{empresaId}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            // Verificar acceso a la empresa
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("Acceso denegado a empresa {EmpresaId}", empresaId);
                return Forbid();
            }

            var action = await _unitOfWork.CotizacionRepository.GetByEmpresaAsync(empresaId);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Error al obtener cotizaciones de empresa {EmpresaId}: {Message}", empresaId, action.Message);
            }

            return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener cotizaciones de empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Get a single quotation by ID with all details
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var action = await _unitOfWork.CotizacionRepository.GetWithDetallesAsync(id);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Cotización no encontrada: {Id}", id);
                return NotFound(action.Message);
            }

            // Verificar acceso a la empresa
            if (!await TieneAccesoEmpresaAsync(action.Result!.EmpresaId))
            {
                _logger.LogWarning("Acceso denegado a cotización {Id}, empresa {EmpresaId}", id, action.Result.EmpresaId);
                return Forbid();
            }

            return Ok(action.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener cotización {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Create a new quotation
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Cotizacion cotizacion)
    {
        try
        {
            // Verificar acceso a la empresa
            if (!await TieneAccesoEmpresaAsync(cotizacion.EmpresaId))
            {
                _logger.LogWarning("Acceso denegado a empresa {EmpresaId}", cotizacion.EmpresaId);
                return Forbid();
            }

            // Set initial state
            cotizacion.Estado = EstadoCotizacion.Borrador;
            cotizacion.IsDeleted = false;

            // Establecer usuario de creación
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            cotizacion.UsuarioCreacionId = userId;

            // Generate quotation number before validation ([ApiController] auto-validates [Required])
            if (string.IsNullOrWhiteSpace(cotizacion.Numero) || cotizacion.Numero == "PENDIENTE" || cotizacion.Numero == "AUTO")
            {
                cotizacion.Numero = await GenerateQuotationNumberAsync(cotizacion.EmpresaId);
            }

            ModelState.Remove("Numero");
            if (!TryValidateModel(cotizacion))
            {
                return ValidationProblem(ModelState);
            }

            // Copy client data for historical record
            var cliente = await _context.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == cotizacion.ClienteId);

            if (cliente != null)
            {
                cotizacion.ClienteTipoIdentificacion = cliente.TipoIdentificacion;
                cotizacion.ClienteNumeroIdentificacion = cliente.NumeroIdentificacion;
                cotizacion.ClienteNombre = cliente.Nombre;
                cotizacion.ClienteEmail = cliente.EmailPrincipal;
                cotizacion.ClienteTelefono = cliente.TelefonoPrincipal;
            }

            var action = await _unitOfWork.CotizacionRepository.AddAsync(cotizacion);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Error al crear cotización: {Message}", action.Message);
                return BadRequest(action.Message);
            }

            _logger.LogInformation("Cotización creada exitosamente: {Id}", action.Result!.Id);
            return Ok(action.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear cotización");
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            return StatusCode(500, $"Error al crear la cotización: {innerMsg}");
        }
    }

    /// <summary>
    /// Update an existing quotation
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Cotizacion cotizacion)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (id != cotizacion.Id)
            {
                return BadRequest("El ID de la cotización no coincide");
            }

            // Verificar que la cotización existe
            var existente = await _unitOfWork.CotizacionRepository.GetAsync(id);
            if (!existente.WasSuccess)
            {
                _logger.LogWarning("Cotización no encontrada: {Id}", id);
                return NotFound(existente.Message);
            }

            // Verificar acceso a la empresa
            if (!await TieneAccesoEmpresaAsync(existente.Result!.EmpresaId))
            {
                _logger.LogWarning("Acceso denegado a cotización {Id}, empresa {EmpresaId}", id, existente.Result.EmpresaId);
                return Forbid();
            }

            // Establecer usuario de modificación
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            cotizacion.UsuarioModificacionId = userId;

            // Update client data for historical record
            var cliente = await _context.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == cotizacion.ClienteId);

            if (cliente != null)
            {
                cotizacion.ClienteTipoIdentificacion = cliente.TipoIdentificacion;
                cotizacion.ClienteNumeroIdentificacion = cliente.NumeroIdentificacion;
                cotizacion.ClienteNombre = cliente.Nombre;
                cotizacion.ClienteEmail = cliente.EmailPrincipal;
                cotizacion.ClienteTelefono = cliente.TelefonoPrincipal;
            }

            var action = await _unitOfWork.CotizacionRepository.UpdateAsync(cotizacion);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Error al actualizar cotización {Id}: {Message}", id, action.Message);
                return BadRequest(action.Message);
            }

            _logger.LogInformation("Cotización actualizada exitosamente: {Id}", id);
            return Ok(action.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar cotización {Id}", id);
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            return StatusCode(500, $"Error al actualizar la cotización: {innerMsg}");
        }
    }

    /// <summary>
    /// Send quotation to client (mark as Enviada)
    /// </summary>
    [HttpPost("{id}/enviar")]
    public async Task<IActionResult> EnviarAsync(Guid id)
    {
        try
        {
            // Obtener la cotización para verificar acceso
            var cotizacionAction = await _unitOfWork.CotizacionRepository.GetAsync(id);
            if (!cotizacionAction.WasSuccess)
            {
                _logger.LogWarning("Cotización no encontrada: {Id}", id);
                return NotFound(cotizacionAction.Message);
            }

            // Verificar acceso a la empresa
            if (!await TieneAccesoEmpresaAsync(cotizacionAction.Result!.EmpresaId))
            {
                _logger.LogWarning("Acceso denegado a cotización {Id}, empresa {EmpresaId}", id, cotizacionAction.Result.EmpresaId);
                return Forbid();
            }

            // Usar DataContext para esta operación de cambio de estado
            // (No está en el repositorio como método específico)
            var cotizacion = await _context.Cotizaciones
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (cotizacion == null)
            {
                return NotFound("Cotización no encontrada");
            }

            if (cotizacion.Estado != EstadoCotizacion.Borrador)
            {
                return BadRequest("Solo se pueden enviar cotizaciones en estado Borrador");
            }

            cotizacion.Estado = EstadoCotizacion.Enviada;
            cotizacion.FechaEnvio = DateTime.UtcNow;
            cotizacion.FechaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cotización enviada exitosamente: {Id}", id);
            return Ok(new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Cotización enviada exitosamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al enviar cotización {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Mark quotation as approved by client
    /// </summary>
    [HttpPost("{id}/aprobar")]
    public async Task<IActionResult> AprobarAsync(Guid id)
    {
        try
        {
            // Obtener la cotización para verificar acceso
            var cotizacionAction = await _unitOfWork.CotizacionRepository.GetAsync(id);
            if (!cotizacionAction.WasSuccess)
            {
                _logger.LogWarning("Cotización no encontrada: {Id}", id);
                return NotFound(cotizacionAction.Message);
            }

            // Verificar acceso a la empresa
            if (!await TieneAccesoEmpresaAsync(cotizacionAction.Result!.EmpresaId))
            {
                _logger.LogWarning("Acceso denegado a cotización {Id}, empresa {EmpresaId}", id, cotizacionAction.Result.EmpresaId);
                return Forbid();
            }

            // Obtener usuario actual
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var action = await _unitOfWork.CotizacionRepository.AprobarAsync(id, userId!);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Error al aprobar cotización {Id}: {Message}", id, action.Message);
                return BadRequest(action.Message);
            }

            _logger.LogInformation("Cotización aprobada exitosamente: {Id}", id);
            return Ok(new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Cotización aprobada exitosamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al aprobar cotización {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Mark quotation as rejected by client
    /// </summary>
    [HttpPost("{id}/rechazar")]
    public async Task<IActionResult> RechazarAsync(Guid id)
    {
        try
        {
            // Obtener la cotización para verificar acceso
            var cotizacionAction = await _unitOfWork.CotizacionRepository.GetAsync(id);
            if (!cotizacionAction.WasSuccess)
            {
                _logger.LogWarning("Cotización no encontrada: {Id}", id);
                return NotFound(cotizacionAction.Message);
            }

            // Verificar acceso a la empresa
            if (!await TieneAccesoEmpresaAsync(cotizacionAction.Result!.EmpresaId))
            {
                _logger.LogWarning("Acceso denegado a cotización {Id}, empresa {EmpresaId}", id, cotizacionAction.Result.EmpresaId);
                return Forbid();
            }

            // Usar DataContext para esta operación de cambio de estado
            // (No está en el repositorio como método específico)
            var cotizacion = await _context.Cotizaciones
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (cotizacion == null)
            {
                return NotFound("Cotización no encontrada");
            }

            if (cotizacion.Estado != EstadoCotizacion.Enviada && cotizacion.Estado != EstadoCotizacion.Aprobada)
            {
                return BadRequest("Solo se pueden rechazar cotizaciones en estado Enviada o Aprobada");
            }

            cotizacion.Estado = EstadoCotizacion.Rechazada;
            cotizacion.FechaRechazo = DateTime.UtcNow;
            cotizacion.FechaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cotización rechazada: {Id}", id);
            return Ok(new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Cotización rechazada"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al rechazar cotización {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Convert quotation to Documento (invoice)
    /// </summary>
    [HttpPost("{id}/convertir")]
    public async Task<IActionResult> ConvertirAsync(Guid id)
    {
        try
        {
            // Obtener la cotización con detalles para verificar acceso y validar
            var cotizacionAction = await _unitOfWork.CotizacionRepository.GetWithDetallesAsync(id);
            if (!cotizacionAction.WasSuccess)
            {
                _logger.LogWarning("Cotización no encontrada: {Id}", id);
                return NotFound(cotizacionAction.Message);
            }

            var cotizacion = cotizacionAction.Result!;

            // Verificar acceso a la empresa
            if (!await TieneAccesoEmpresaAsync(cotizacion.EmpresaId))
            {
                _logger.LogWarning("Acceso denegado a cotización {Id}, empresa {EmpresaId}", id, cotizacion.EmpresaId);
                return Forbid();
            }

            if (cotizacion.Estado != EstadoCotizacion.Aprobada)
            {
                return BadRequest("Solo se pueden convertir cotizaciones en estado Aprobada");
            }

            if (cotizacion.DocumentoGeneradoId.HasValue)
            {
                return BadRequest("Esta cotización ya fue convertida a documento");
            }

            // Create new Documento from Cotizacion
            var documento = new Documento
            {
                Id = Guid.NewGuid(),
                EmpresaId = cotizacion.EmpresaId,
                SucursalId = cotizacion.SucursalId,
                TerminalId = cotizacion.TerminalId,
                TipoDocumento = DocumentoTipo.FacturaElectronica,
                Estado = EstadoDocumento.Borrador,
                Ambiente = Ambiente.Produccion, // Set based on company configuration
                FechaEmision = DateTime.Now,
                ClienteId = cotizacion.ClienteId,
                ReceptorTipoIdentificacion = cotizacion.ClienteTipoIdentificacion,
                ReceptorNumeroIdentificacion = cotizacion.ClienteNumeroIdentificacion,
                ReceptorNombre = cotizacion.ClienteNombre,
                ReceptorEmails = cotizacion.ClienteEmail,
                ReceptorTelefono = cotizacion.ClienteTelefono,
                CondicionVenta = cotizacion.CondicionVenta,
                PlazoCreditoDias = cotizacion.PlazoCreditoDias,
                MedioPago = cotizacion.MedioPago,
                Moneda = cotizacion.Moneda,
                TipoCambio = cotizacion.TipoCambio,
                Subtotal = cotizacion.Subtotal,
                TotalDescuentos = cotizacion.TotalDescuentos,
                TotalImpuestos = cotizacion.TotalImpuestos,
                TotalVenta = cotizacion.Total,
                Observaciones = cotizacion.Notas,
                FechaCreacion = DateTime.UtcNow,
                UsuarioCreacionId = cotizacion.UsuarioCreacionId,
                IsDeleted = false
            };

            // Note: Clave, NumeroConsecutivo, and ActividadEconomica must be generated
            // This should be done by a DocumentoService that handles the business logic
            // For now, we'll set placeholder values that should be updated before sending to Hacienda
            documento.Clave = "PENDING_GENERATION"; // Will be generated by DocumentoService
            documento.NumeroConsecutivo = "PENDING_GENERATION"; // Will be generated by ConsecutivoService
            documento.ActividadEconomica = "000000"; // Should be set from Empresa configuration

            // Copy details from Cotizacion to Documento
            int lineNumber = 1;
            foreach (var cotizacionDetalle in cotizacion.Detalles)
            {
                var documentoDetalle = new DocumentoDetalle
                {
                    Id = Guid.NewGuid(),
                    DocumentoId = documento.Id,
                    NumeroLinea = lineNumber++,
                    ProductoId = cotizacionDetalle.ProductoId,
                    Codigo = cotizacionDetalle.Codigo,
                    CodigoCabys = null, // Should be populated from Producto
                    Descripcion = cotizacionDetalle.Descripcion,
                    Cantidad = cotizacionDetalle.Cantidad,
                    UnidadMedidaId = 1, // Default, should be populated from Producto
                    UnidadMedidaComercial = cotizacionDetalle.UnidadMedida,
                    PrecioUnitario = cotizacionDetalle.PrecioUnitario,
                    MontoTotal = cotizacionDetalle.MontoTotal,
                    MontoDescuento = cotizacionDetalle.MontoDescuento,
                    Subtotal = cotizacionDetalle.SubTotal,
                    BaseImponible = cotizacionDetalle.SubTotal,
                    MontoTotalLinea = cotizacionDetalle.TotalLinea
                };

                documento.Detalles.Add(documentoDetalle);

                // Add impuesto if applicable
                if (cotizacionDetalle.MontoIVA > 0 && !string.IsNullOrEmpty(cotizacionDetalle.CodigoTarifaIVA))
                {
                    var impuesto = new DocumentoDetalleImpuesto
                    {
                        Id = Guid.NewGuid(),
                        DocumentoDetalleId = documentoDetalle.Id,
                        CodigoImpuesto = "01", // IVA
                        CodigoTarifa = cotizacionDetalle.CodigoTarifaIVA,
                        Tarifa = cotizacionDetalle.TarifaIVA ?? 0,
                        MontoBase = cotizacionDetalle.SubTotal,
                        MontoImpuesto = cotizacionDetalle.MontoIVA
                    };
                    documentoDetalle.Impuestos.Add(impuesto);
                }
            }

            // Save documento using DataContext (complex document creation logic)
            _context.Documentos.Add(documento);
            await _context.SaveChangesAsync();

            // Obtener usuario actual
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Update cotizacion status using repository
            var action = await _unitOfWork.CotizacionRepository.ConvertirAFacturaAsync(id, documento.Id, userId!);

            if (!action.WasSuccess)
            {
                // Rollback document creation if conversion fails
                _context.Documentos.Remove(documento);
                await _context.SaveChangesAsync();

                _logger.LogWarning("Error al convertir cotización {Id}: {Message}", id, action.Message);
                return BadRequest(action.Message);
            }

            _logger.LogInformation("Cotización convertida a documento exitosamente: {CotizacionId} -> {DocumentoId}", id, documento.Id);
            return Ok(new ActionResponse<Guid>
            {
                WasSuccess = true,
                Result = documento.Id,
                Message = "Cotización convertida a documento exitosamente. IMPORTANTE: El documento requiere completar Clave, Consecutivo y Actividad Económica antes de enviar a Hacienda."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al convertir cotización {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Delete quotation (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            // Obtener la cotización para verificar acceso
            var cotizacionAction = await _unitOfWork.CotizacionRepository.GetAsync(id);
            if (!cotizacionAction.WasSuccess)
            {
                _logger.LogWarning("Cotización no encontrada: {Id}", id);
                return NotFound(cotizacionAction.Message);
            }

            // Verificar acceso a la empresa
            if (!await TieneAccesoEmpresaAsync(cotizacionAction.Result!.EmpresaId))
            {
                _logger.LogWarning("Acceso denegado a cotización {Id}, empresa {EmpresaId}", id, cotizacionAction.Result.EmpresaId);
                return Forbid();
            }

            // Obtener usuario actual
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var action = await _unitOfWork.CotizacionRepository.DeleteAsync(id, userId!);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Error al eliminar cotización {Id}: {Message}", id, action.Message);
                return BadRequest(action.Message);
            }

            _logger.LogInformation("Cotización eliminada exitosamente: {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al eliminar cotización {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Genera y descarga el PDF de una cotización
    /// </summary>
    [HttpGet("{id:guid}/descargar-pdf")]
    public async Task<IActionResult> DescargarPdfAsync(Guid id)
    {
        try
        {
            var cotizacion = await _context.Cotizaciones
                .Include(c => c.Empresa)
                    .ThenInclude(e => e!.Emails)
                .Include(c => c.Empresa)
                    .ThenInclude(e => e!.Telefonos)
                .Include(c => c.Cliente)
                .Include(c => c.Detalles.OrderBy(d => d.NumeroLinea))
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (cotizacion == null)
                return NotFound("Cotización no encontrada.");

            if (!await TieneAccesoEmpresaAsync(cotizacion.EmpresaId))
                return Forbid();

            var pdfDocument = new Facturacion.Backend.Services.Implementations.PdfDocuments.CotizacionPdfDocument(cotizacion);
            var pdfBytes = pdfDocument.GeneratePdf();

            var fileName = $"Cotizacion_{cotizacion.Numero}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar PDF de cotización {Id}", id);
            return StatusCode(500, "Error al generar el PDF de la cotización.");
        }
    }

    /// <summary>
    /// Generate a unique quotation number
    /// Format: COT-YYYY-MM-NNNN
    /// </summary>
    private async Task<string> GenerateQuotationNumberAsync(Guid empresaId)
    {
        var year = DateTime.Now.Year;
        var month = DateTime.Now.Month;
        var prefix = $"COT-{year:D4}-{month:D2}-";

        // IgnoreQueryFilters to include soft-deleted records and avoid
        // duplicate number collisions with the unique IX_Cotizacion_Numero index
        var lastQuotation = await _context.Cotizaciones
            .IgnoreQueryFilters()
            .Where(c => c.EmpresaId == empresaId && c.Numero.StartsWith(prefix))
            .OrderByDescending(c => c.Numero)
            .Select(c => c.Numero)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastQuotation != null)
        {
            var lastNumberPart = lastQuotation.Substring(prefix.Length);
            if (int.TryParse(lastNumberPart, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}";
    }

    /// <summary>
    /// Verifica si el usuario tiene acceso a una empresa específica
    /// SuperUser tiene acceso a todas las empresas
    /// Otros usuarios solo tienen acceso a sus empresas asignadas
    /// </summary>
    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        // SuperUser tiene acceso a todas las empresas
        if (userRoles.Contains("SuperUser"))
        {
            return true;
        }

        // Otros usuarios solo tienen acceso a sus empresas asignadas
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }
}
