using Facturacion.Backend.Data;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

/// <summary>
/// Controlador para gestión de requisiciones de compra.
/// Las requisiciones son solicitudes internas de compra que requieren aprobación.
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class RequisicionesController : ControllerBase
{
    private readonly IRequisicionUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly ILogger<RequisicionesController> _logger;

    public RequisicionesController(
        IRequisicionUnitOfWork unitOfWork,
        DataContext context,
        ILogger<RequisicionesController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene las requisiciones de una empresa con filtros opcionales.
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="estado">Estado opcional (BOR, PEN, APR, REC, ANU)</param>
    /// <returns>Lista de requisiciones</returns>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(
        Guid empresaId,
        [FromQuery] string? estado = null)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        ActionResponse<IEnumerable<Requisicion>> response;

        if (!string.IsNullOrWhiteSpace(estado))
        {
            response = await _unitOfWork.GetByEstadoAsync(empresaId, estado);
        }
        else
        {
            response = await _unitOfWork.GetByEmpresaAsync(empresaId);
        }

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Obtiene las requisiciones pendientes de aprobación de una empresa.
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>Lista de requisiciones pendientes de aprobación</returns>
    [HttpGet("empresa/{empresaId:guid}/pendientes-aprobacion")]
    public async Task<IActionResult> GetPendientesAprobacionAsync(Guid empresaId)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var response = await _unitOfWork.GetPendientesAprobacionAsync(empresaId);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Obtiene las requisiciones aprobadas sin orden de compra asociada.
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>Lista de requisiciones aprobadas sin OC</returns>
    [HttpGet("empresa/{empresaId:guid}/aprobadas-sin-oc")]
    public async Task<IActionResult> GetAprobadasSinOCAsync(Guid empresaId)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var response = await _unitOfWork.GetAprobadasSinOCAsync(empresaId);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Obtiene las requisiciones de un solicitante específico.
    /// </summary>
    /// <param name="solicitanteId">ID del usuario solicitante</param>
    /// <returns>Lista de requisiciones del solicitante</returns>
    [HttpGet("solicitante/{solicitanteId}")]
    public async Task<IActionResult> GetBySolicitanteAsync(string solicitanteId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Solo el propio usuario o un SuperUser puede ver sus requisiciones
        if (userId != solicitanteId && !User.IsInRole("SuperUser"))
        {
            return Forbid();
        }

        var response = await _unitOfWork.GetBySolicitanteAsync(solicitanteId);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Obtiene una requisición por su ID con todos sus detalles.
    /// </summary>
    /// <param name="id">ID de la requisición</param>
    /// <returns>Requisición completa con detalles</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var response = await _unitOfWork.GetWithDetallesAsync(id);

        if (!response.WasSuccess)
        {
            return NotFound(response.Message);
        }

        if (!await TieneAccesoEmpresaAsync(response.Result!.EmpresaId))
        {
            return Forbid();
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Crea una nueva requisición de compra con sus detalles.
    /// </summary>
    /// <param name="requisicion">Datos de la requisición incluyendo detalles</param>
    /// <returns>Requisición creada</returns>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Requisicion requisicion)
    {
        // Set server-generated fields BEFORE model validation
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        requisicion.Id = Guid.NewGuid();
        requisicion.SolicitanteId = userId!;
        requisicion.UsuarioCreacionId = userId;
        requisicion.FechaCreacion = DateTime.Now;
        requisicion.Estado = string.IsNullOrWhiteSpace(requisicion.Estado) ? "BOR" : requisicion.Estado;

        if (string.IsNullOrWhiteSpace(requisicion.Numero) || requisicion.Numero == "AUTO")
        {
            requisicion.Numero = await GenerarNumeroRequisicionAsync(requisicion.EmpresaId);
        }

        // Clear model state errors for server-generated fields and re-validate
        ModelState.Remove("Numero");
        ModelState.Remove("SolicitanteId");
        if (!TryValidateModel(requisicion))
        {
            return BadRequest(ModelState);
        }

        if (!await TieneAccesoEmpresaAsync(requisicion.EmpresaId))
        {
            return Forbid();
        }

        // Asignar IDs a los detalles
        if (requisicion.Detalles != null)
        {
            int numeroLinea = 1;
            foreach (var detalle in requisicion.Detalles)
            {
                detalle.Id = Guid.NewGuid();
                detalle.RequisicionId = requisicion.Id;
                detalle.NumeroLinea = numeroLinea++;
            }
        }

        var response = await _unitOfWork.AddAsync(requisicion);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Actualiza una requisición existente.
    /// Solo permite editar requisiciones en estado Borrador (BOR).
    /// </summary>
    /// <param name="id">ID de la requisición</param>
    /// <param name="requisicion">Datos actualizados de la requisición</param>
    /// <returns>Requisición actualizada</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Requisicion requisicion)
    {
        if (id != requisicion.Id)
        {
            return BadRequest("El ID no coincide.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verificar que la requisición existe y obtener EmpresaId para validación de acceso
        var existenteResponse = await _unitOfWork.GetAsync(id);
        if (!existenteResponse.WasSuccess)
        {
            return NotFound(existenteResponse.Message);
        }

        if (!await TieneAccesoEmpresaAsync(existenteResponse.Result!.EmpresaId))
        {
            return Forbid();
        }

        // Establecer usuario de modificación
        requisicion.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        requisicion.FechaModificacion = DateTime.Now;

        var response = await _unitOfWork.UpdateAsync(requisicion);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Envía una requisición para aprobación.
    /// Cambia el estado de Borrador (BOR) a Pendiente (PEN).
    /// </summary>
    /// <param name="id">ID de la requisición</param>
    /// <returns>Requisición enviada</returns>
    [HttpPost("{id:guid}/enviar")]
    public async Task<IActionResult> EnviarAsync(Guid id)
    {
        var requisicionTemp = await _unitOfWork.GetAsync(id);
        if (!requisicionTemp.WasSuccess)
        {
            return NotFound(requisicionTemp.Message);
        }

        if (!await TieneAccesoEmpresaAsync(requisicionTemp.Result!.EmpresaId))
        {
            return Forbid();
        }

        var requisicion = requisicionTemp.Result;

        if (requisicion.Estado != "BOR" && requisicion.Estado != "PEN")
        {
            return BadRequest("Solo se pueden enviar requisiciones en estado Borrador o Pendiente.");
        }

        requisicion.Estado = "PEN";
        requisicion.FechaModificacion = DateTime.UtcNow;
        requisicion.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var response = await _unitOfWork.UpdateAsync(requisicion);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Aprueba una requisición de compra.
    /// Cambia el estado de Pendiente (PEN) a Aprobada (APR).
    /// </summary>
    /// <param name="id">ID de la requisición</param>
    /// <returns>Requisición aprobada</returns>
    [HttpPost("{id:guid}/aprobar")]
    public async Task<IActionResult> AprobarAsync(Guid id)
    {
        // Verificar acceso primero
        var requisicionTemp = await _unitOfWork.GetAsync(id);
        if (!requisicionTemp.WasSuccess)
        {
            return NotFound(requisicionTemp.Message);
        }

        if (!await TieneAccesoEmpresaAsync(requisicionTemp.Result!.EmpresaId))
        {
            return Forbid();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var response = await _unitOfWork.AprobarAsync(id, userId!);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Rechaza una requisición de compra.
    /// Cambia el estado a Rechazada (REC).
    /// </summary>
    /// <param name="id">ID de la requisición</param>
    /// <param name="motivo">Motivo del rechazo</param>
    /// <returns>Requisición rechazada</returns>
    [HttpPost("{id:guid}/rechazar")]
    public async Task<IActionResult> RechazarAsync(Guid id, [FromBody] string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            return BadRequest("Debe proporcionar un motivo para el rechazo.");
        }

        // Verificar acceso primero
        var requisicionTemp = await _unitOfWork.GetAsync(id);
        if (!requisicionTemp.WasSuccess)
        {
            return NotFound(requisicionTemp.Message);
        }

        if (!await TieneAccesoEmpresaAsync(requisicionTemp.Result!.EmpresaId))
        {
            return Forbid();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var response = await _unitOfWork.RechazarAsync(id, userId!, motivo);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Anula una requisición de compra.
    /// Cambia el estado a Anulada (ANU).
    /// </summary>
    /// <param name="id">ID de la requisición</param>
    /// <param name="motivo">Motivo de la anulación</param>
    /// <returns>Requisición anulada</returns>
    [HttpPost("{id:guid}/anular")]
    public async Task<IActionResult> AnularAsync(Guid id, [FromBody] string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            return BadRequest("Debe proporcionar un motivo para la anulación.");
        }

        // Verificar acceso primero
        var requisicionTemp = await _unitOfWork.GetAsync(id);
        if (!requisicionTemp.WasSuccess)
        {
            return NotFound(requisicionTemp.Message);
        }

        if (!await TieneAccesoEmpresaAsync(requisicionTemp.Result!.EmpresaId))
        {
            return Forbid();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var response = await _unitOfWork.AnularAsync(id, userId!, motivo);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Elimina (soft delete) una requisición de compra.
    /// Solo permite eliminar requisiciones en estado Borrador (BOR).
    /// </summary>
    /// <param name="id">ID de la requisición</param>
    /// <returns>Resultado de la operación</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        // Verificar acceso primero
        var requisicionTemp = await _unitOfWork.GetAsync(id);
        if (!requisicionTemp.WasSuccess)
        {
            return NotFound(requisicionTemp.Message);
        }

        if (!await TieneAccesoEmpresaAsync(requisicionTemp.Result!.EmpresaId))
        {
            return Forbid();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var response = await _unitOfWork.DeleteAsync(id, userId!);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(new { message = "Requisición eliminada correctamente." });
    }

    private async Task<string> GenerarNumeroRequisicionAsync(Guid empresaId)
    {
        var year = DateTime.Now.Year;
        var month = DateTime.Now.Month;
        var prefix = $"REQ-{year:D4}-{month:D2}-";

        var lastNumero = await _context.Requisiciones
            .IgnoreQueryFilters()
            .Where(r => r.EmpresaId == empresaId && r.Numero.StartsWith(prefix))
            .OrderByDescending(r => r.Numero)
            .Select(r => r.Numero)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastNumero != null)
        {
            var lastPart = lastNumero.Substring(prefix.Length);
            if (int.TryParse(lastPart, out int last))
                nextNumber = last + 1;
        }

        return $"{prefix}{nextNumber:D4}";
    }

    /// <summary>
    /// Verifica si el usuario actual tiene acceso a la empresa especificada.
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>True si tiene acceso, False en caso contrario</returns>
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
