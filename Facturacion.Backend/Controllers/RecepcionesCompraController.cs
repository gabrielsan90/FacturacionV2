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
/// Controlador para gestión de recepciones de compra.
/// Permite registrar la recepción física de mercancía relacionada a órdenes de compra.
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class RecepcionesCompraController : ControllerBase
{
    private readonly IRecepcionCompraUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly ILogger<RecepcionesCompraController> _logger;

    public RecepcionesCompraController(
        IRecepcionCompraUnitOfWork unitOfWork,
        DataContext context,
        ILogger<RecepcionesCompraController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene las recepciones de compra de una empresa.
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>Lista de recepciones de compra</returns>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var response = await _unitOfWork.GetByEmpresaAsync(empresaId);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Obtiene las recepciones pendientes de aplicar de una empresa.
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>Lista de recepciones pendientes</returns>
    [HttpGet("empresa/{empresaId:guid}/pendientes")]
    public async Task<IActionResult> GetPendientesAsync(Guid empresaId)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var response = await _unitOfWork.GetPendientesAsync(empresaId);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Obtiene las recepciones relacionadas a una orden de compra específica.
    /// </summary>
    /// <param name="ordenCompraId">ID de la orden de compra</param>
    /// <returns>Lista de recepciones de la orden</returns>
    [HttpGet("orden-compra/{ordenCompraId:guid}")]
    public async Task<IActionResult> GetByOrdenCompraAsync(Guid ordenCompraId)
    {
        var response = await _unitOfWork.GetByOrdenCompraAsync(ordenCompraId);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        // Validar acceso a la empresa de al menos una recepción
        if (response.Result != null && response.Result.Any())
        {
            var primeraRecepcion = response.Result.First();
            if (!await TieneAccesoEmpresaAsync(primeraRecepcion.EmpresaId))
            {
                return Forbid();
            }
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Obtiene una recepción de compra por su ID con todos sus detalles.
    /// </summary>
    /// <param name="id">ID de la recepción de compra</param>
    /// <returns>Recepción de compra completa con detalles</returns>
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
    /// Crea una nueva recepción de compra con sus detalles.
    /// </summary>
    /// <param name="recepcion">Datos de la recepción de compra incluyendo detalles</param>
    /// <returns>Recepción de compra creada</returns>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] RecepcionCompra recepcion)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!await TieneAccesoEmpresaAsync(recepcion.EmpresaId))
        {
            return Forbid();
        }

        // Validar cantidades antes de crear
        if (recepcion.Detalles != null && recepcion.Detalles.Any())
        {
            var validacionResponse = await _unitOfWork.ValidarCantidadesAsync(
                recepcion.OrdenCompraId,
                recepcion.Detalles.ToList());

            if (!validacionResponse.WasSuccess)
            {
                return BadRequest(validacionResponse.Message);
            }
        }

        // Establecer valores de auditoría
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        recepcion.Id = Guid.NewGuid();
        recepcion.CreadoPorId = userId;
        recepcion.Estado = string.IsNullOrWhiteSpace(recepcion.Estado) ? "PEN" : recepcion.Estado;

        // Asignar IDs a los detalles
        if (recepcion.Detalles != null)
        {
            int numeroLinea = 1;
            foreach (var detalle in recepcion.Detalles)
            {
                detalle.Id = Guid.NewGuid();
                detalle.RecepcionCompraId = recepcion.Id;
                detalle.NumeroLinea = numeroLinea++;
            }
        }

        var response = await _unitOfWork.AddAsync(recepcion);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Actualiza una recepción de compra existente.
    /// Solo permite editar recepciones en estado Pendiente (PEN).
    /// </summary>
    /// <param name="id">ID de la recepción</param>
    /// <param name="recepcion">Datos actualizados de la recepción</param>
    /// <returns>Recepción actualizada</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] RecepcionCompra recepcion)
    {
        if (id != recepcion.Id)
        {
            return BadRequest("El ID no coincide.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verificar que la recepción existe y obtener EmpresaId para validación de acceso
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
        recepcion.ModificadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var response = await _unitOfWork.UpdateAsync(recepcion);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Anula una recepción de compra.
    /// Cambia el estado a Anulada (ANU) y revierte los movimientos de inventario.
    /// </summary>
    /// <param name="id">ID de la recepción</param>
    /// <param name="motivo">Motivo de la anulación</param>
    /// <returns>Recepción anulada</returns>
    [HttpPost("{id:guid}/anular")]
    public async Task<IActionResult> AnularAsync(Guid id, [FromBody] string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            return BadRequest("Debe proporcionar un motivo para la anulación.");
        }

        // Verificar acceso primero
        var recepcionTemp = await _unitOfWork.GetAsync(id);
        if (!recepcionTemp.WasSuccess)
        {
            return NotFound(recepcionTemp.Message);
        }

        if (!await TieneAccesoEmpresaAsync(recepcionTemp.Result!.EmpresaId))
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
    /// Elimina (soft delete) una recepción de compra.
    /// Solo permite eliminar recepciones en estado Pendiente (PEN).
    /// </summary>
    /// <param name="id">ID de la recepción</param>
    /// <returns>Resultado de la operación</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        // Verificar acceso primero
        var recepcionTemp = await _unitOfWork.GetAsync(id);
        if (!recepcionTemp.WasSuccess)
        {
            return NotFound(recepcionTemp.Message);
        }

        if (!await TieneAccesoEmpresaAsync(recepcionTemp.Result!.EmpresaId))
        {
            return Forbid();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var response = await _unitOfWork.DeleteAsync(id, userId!);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(new { message = "Recepción de compra eliminada correctamente." });
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
