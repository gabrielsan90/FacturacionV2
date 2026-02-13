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
/// Controlador para gestión de cotizaciones de proveedores.
/// Permite administrar cotizaciones recibidas de proveedores para requisiciones de compra.
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class CotizacionesProveedorController : ControllerBase
{
    private readonly ICotizacionProveedorUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly ILogger<CotizacionesProveedorController> _logger;

    public CotizacionesProveedorController(
        ICotizacionProveedorUnitOfWork unitOfWork,
        DataContext context,
        ILogger<CotizacionesProveedorController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene las cotizaciones relacionadas a una requisición específica.
    /// </summary>
    /// <param name="requisicionId">ID de la requisición</param>
    /// <returns>Lista de cotizaciones de la requisición</returns>
    [HttpGet("requisicion/{requisicionId:guid}")]
    public async Task<IActionResult> GetByRequisicionAsync(Guid requisicionId)
    {
        var response = await _unitOfWork.GetByRequisicionAsync(requisicionId);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        // Validar acceso a la empresa de al menos una cotización
        if (response.Result != null && response.Result.Any())
        {
            var primeraCotizacion = response.Result.First();
            if (!await TieneAccesoEmpresaAsync(primeraCotizacion.EmpresaId))
            {
                return Forbid();
            }
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Obtiene las cotizaciones de un proveedor específico.
    /// </summary>
    /// <param name="proveedorId">ID del proveedor</param>
    /// <returns>Lista de cotizaciones del proveedor</returns>
    [HttpGet("proveedor/{proveedorId:guid}")]
    public async Task<IActionResult> GetByProveedorAsync(Guid proveedorId)
    {
        var response = await _unitOfWork.GetByProveedorAsync(proveedorId);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        // Validar acceso a la empresa de al menos una cotización
        if (response.Result != null && response.Result.Any())
        {
            var primeraCotizacion = response.Result.First();
            if (!await TieneAccesoEmpresaAsync(primeraCotizacion.EmpresaId))
            {
                return Forbid();
            }
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Obtiene las cotizaciones de una empresa filtradas por estado.
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="estado">Estado de la cotización (PEN, SEL, REC)</param>
    /// <returns>Lista de cotizaciones por estado</returns>
    [HttpGet("empresa/{empresaId:guid}/estado/{estado}")]
    public async Task<IActionResult> GetByEstadoAsync(Guid empresaId, string estado)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var response = await _unitOfWork.GetByEstadoAsync(empresaId, estado);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Obtiene las cotizaciones vencidas (fecha de validez expirada) de una empresa.
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>Lista de cotizaciones vencidas</returns>
    [HttpGet("empresa/{empresaId:guid}/vencidas")]
    public async Task<IActionResult> GetVencidasAsync(Guid empresaId)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var response = await _unitOfWork.GetVencidasAsync(empresaId);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Obtiene una cotización por su ID con todos sus detalles.
    /// </summary>
    /// <param name="id">ID de la cotización</param>
    /// <returns>Cotización completa con detalles</returns>
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
    /// Crea una nueva cotización de proveedor con sus detalles.
    /// </summary>
    /// <param name="cotizacion">Datos de la cotización incluyendo detalles</param>
    /// <returns>Cotización creada</returns>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] CotizacionProveedor cotizacion)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!await TieneAccesoEmpresaAsync(cotizacion.EmpresaId))
        {
            return Forbid();
        }

        // Establecer valores de auditoría
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        cotizacion.Id = Guid.NewGuid();
        cotizacion.RegistradoPorId = userId;
        cotizacion.FechaCreacion = DateTime.Now;
        cotizacion.Estado = string.IsNullOrWhiteSpace(cotizacion.Estado) ? "ENV" : cotizacion.Estado;

        // Asignar IDs a los detalles
        if (cotizacion.Detalles != null)
        {
            int numeroLinea = 1;
            foreach (var detalle in cotizacion.Detalles)
            {
                detalle.Id = Guid.NewGuid();
                detalle.CotizacionProveedorId = cotizacion.Id;
                detalle.NumeroLinea = numeroLinea++;
            }
        }

        var response = await _unitOfWork.AddAsync(cotizacion);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Actualiza una cotización existente.
    /// Solo permite editar cotizaciones en estado Pendiente (PEN).
    /// </summary>
    /// <param name="id">ID de la cotización</param>
    /// <param name="cotizacion">Datos actualizados de la cotización</param>
    /// <returns>Cotización actualizada</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] CotizacionProveedor cotizacion)
    {
        if (id != cotizacion.Id)
        {
            return BadRequest("El ID no coincide.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verificar que la cotización existe y obtener EmpresaId para validación de acceso
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
        cotizacion.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        cotizacion.FechaModificacion = DateTime.Now;

        var response = await _unitOfWork.UpdateAsync(cotizacion);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Selecciona una cotización como ganadora.
    /// Cambia el estado a Seleccionada (SEL).
    /// </summary>
    /// <param name="id">ID de la cotización</param>
    /// <returns>Cotización seleccionada</returns>
    [HttpPost("{id:guid}/seleccionar")]
    public async Task<IActionResult> SeleccionarAsync(Guid id)
    {
        // Verificar acceso primero
        var cotizacionTemp = await _unitOfWork.GetAsync(id);
        if (!cotizacionTemp.WasSuccess)
        {
            return NotFound(cotizacionTemp.Message);
        }

        if (!await TieneAccesoEmpresaAsync(cotizacionTemp.Result!.EmpresaId))
        {
            return Forbid();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var response = await _unitOfWork.SeleccionarAsync(id, userId!);

        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(response.Result);
    }

    /// <summary>
    /// Rechaza una cotización de proveedor.
    /// Cambia el estado a Rechazada (REC).
    /// </summary>
    /// <param name="id">ID de la cotización</param>
    /// <param name="motivo">Motivo del rechazo</param>
    /// <returns>Cotización rechazada</returns>
    [HttpPost("{id:guid}/rechazar")]
    public async Task<IActionResult> RechazarAsync(Guid id, [FromBody] string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            return BadRequest("Debe proporcionar un motivo para el rechazo.");
        }

        // Verificar acceso primero
        var cotizacionTemp = await _unitOfWork.GetAsync(id);
        if (!cotizacionTemp.WasSuccess)
        {
            return NotFound(cotizacionTemp.Message);
        }

        if (!await TieneAccesoEmpresaAsync(cotizacionTemp.Result!.EmpresaId))
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
