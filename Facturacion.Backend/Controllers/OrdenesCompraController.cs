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
/// Controlador para gestión de órdenes de compra a proveedores.
/// Incluye creación, modificación, aprobación y seguimiento de estado.
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class OrdenesCompraController : ControllerBase
{
    private readonly IOrdenCompraUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly ILogger<OrdenesCompraController> _logger;

    public OrdenesCompraController(
        IOrdenCompraUnitOfWork unitOfWork,
        DataContext context,
        ILogger<OrdenesCompraController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene las órdenes de compra de una empresa con filtros opcionales.
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="estado">Estado opcional (BOR, PEN, APR, PAR, COM, ANU)</param>
    /// <param name="proveedorId">ID del proveedor opcional</param>
    /// <param name="fechaDesde">Fecha desde opcional</param>
    /// <param name="fechaHasta">Fecha hasta opcional</param>
    /// <returns>Lista de órdenes de compra</returns>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(
        Guid empresaId,
        [FromQuery] string? estado = null,
        [FromQuery] Guid? proveedorId = null,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        // Obtener las órdenes de la empresa
        ActionResponse<IEnumerable<OrdenCompra>> response;

        if (!string.IsNullOrWhiteSpace(estado))
        {
            response = await _unitOfWork.GetByEstadoAsync(empresaId, estado);
        }
        else if (proveedorId.HasValue)
        {
            response = await _unitOfWork.GetByProveedorAsync(proveedorId.Value);
        }
        else
        {
            response = await _unitOfWork.GetByEmpresaAsync(empresaId);
        }

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        var ordenes = response.Result;

        // Aplicar filtros adicionales en memoria (fechas)
        if (fechaDesde.HasValue)
        {
            ordenes = ordenes!.Where(o => o.Fecha >= fechaDesde.Value);
        }

        if (fechaHasta.HasValue)
        {
            ordenes = ordenes!.Where(o => o.Fecha <= fechaHasta.Value);
        }

        // Ordenar resultados
        ordenes = ordenes!.OrderByDescending(o => o.Fecha)
                         .ThenByDescending(o => o.FechaCreacion);

        return Ok(ordenes);
    }

    /// <summary>
    /// Obtiene una orden de compra por su ID con todos sus detalles.
    /// </summary>
    /// <param name="id">ID de la orden de compra</param>
    /// <returns>Orden de compra completa con detalles</returns>
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
    /// Crea una nueva orden de compra con sus detalles.
    /// </summary>
    /// <param name="orden">Datos de la orden de compra incluyendo detalles</param>
    /// <returns>Orden de compra creada</returns>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] OrdenCompra orden)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!await TieneAccesoEmpresaAsync(orden.EmpresaId))
        {
            return Forbid();
        }

        // Establecer valores de auditoría
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        orden.Id = Guid.NewGuid();
        orden.CreadoPorId = userId;
        orden.Estado = string.IsNullOrWhiteSpace(orden.Estado) ? "BOR" : orden.Estado;

        // Asignar IDs a los detalles
        if (orden.Detalles != null)
        {
            int numeroLinea = 1;
            foreach (var detalle in orden.Detalles)
            {
                detalle.Id = Guid.NewGuid();
                detalle.OrdenCompraId = orden.Id;
                detalle.NumeroLinea = numeroLinea++;
                detalle.CantidadRecibida = 0;
            }
        }

        var response = await _unitOfWork.AddAsync(orden);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Actualiza una orden de compra existente.
    /// Solo permite editar órdenes en estado Borrador (BOR) o Pendiente (PEN).
    /// </summary>
    /// <param name="id">ID de la orden</param>
    /// <param name="orden">Datos actualizados de la orden</param>
    /// <returns>Orden actualizada</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] OrdenCompra orden)
    {
        if (id != orden.Id)
        {
            return BadRequest("El ID no coincide.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verificar que la orden existe y obtener EmpresaId para validación de acceso
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
        orden.ModificadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var response = await _unitOfWork.UpdateAsync(orden);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Aprueba una orden de compra.
    /// Cambia el estado de Pendiente (PEN) o Borrador (BOR) a Aprobada (APR).
    /// </summary>
    /// <param name="id">ID de la orden</param>
    /// <returns>Orden aprobada</returns>
    [HttpPost("{id:guid}/aprobar")]
    public async Task<IActionResult> AprobarAsync(Guid id)
    {
        // Verificar acceso primero
        var ordenTemp = await _unitOfWork.GetAsync(id);
        if (!ordenTemp.WasSuccess)
        {
            return NotFound(ordenTemp.Message);
        }

        if (!await TieneAccesoEmpresaAsync(ordenTemp.Result!.EmpresaId))
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
    /// Anula una orden de compra.
    /// Cambia el estado a Anulada (ANU).
    /// </summary>
    /// <param name="id">ID de la orden</param>
    /// <param name="motivo">Motivo de la anulación</param>
    /// <returns>Orden anulada</returns>
    [HttpPost("{id:guid}/anular")]
    public async Task<IActionResult> AnularAsync(Guid id, [FromBody] string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            return BadRequest("Debe proporcionar un motivo para la anulación.");
        }

        // Verificar acceso primero
        var ordenTemp = await _unitOfWork.GetAsync(id);
        if (!ordenTemp.WasSuccess)
        {
            return NotFound(ordenTemp.Message);
        }

        if (!await TieneAccesoEmpresaAsync(ordenTemp.Result!.EmpresaId))
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
    /// Elimina (soft delete) una orden de compra.
    /// Solo permite eliminar órdenes en estado Borrador (BOR).
    /// </summary>
    /// <param name="id">ID de la orden</param>
    /// <returns>Resultado de la operación</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        // Verificar acceso primero
        var ordenTemp = await _unitOfWork.GetAsync(id);
        if (!ordenTemp.WasSuccess)
        {
            return NotFound(ordenTemp.Message);
        }

        if (!await TieneAccesoEmpresaAsync(ordenTemp.Result!.EmpresaId))
        {
            return Forbid();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var response = await _unitOfWork.DeleteAsync(id, userId!);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(new { message = "Orden de compra eliminada correctamente." });
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
