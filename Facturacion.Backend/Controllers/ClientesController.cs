using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly IExcelImportService _excelImportService;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(
        IClienteUnitOfWork unitOfWork,
        DataContext context,
        IExcelImportService excelImportService,
        ILogger<ClientesController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _excelImportService = excelImportService;
        _logger = logger;
    }

    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            // Verificar que el usuario tiene acceso a esta empresa
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó acceder a clientes de empresa {EmpresaId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
                return Forbid();
            }

            var clientes = await _unitOfWork.ClienteRepository.GetByEmpresaAsync(empresaId);
            _logger.LogInformation("Se obtuvieron {Count} clientes para empresa {EmpresaId}", clientes.Count(), empresaId);
            return Ok(clientes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener clientes de empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error interno al obtener clientes.");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var cliente = await _unitOfWork.ClienteRepository.GetAsync(id);

            if (cliente == null)
            {
                _logger.LogWarning("Cliente {ClienteId} no encontrado", id);
                return NotFound();
            }

            // Verificar que el usuario tiene acceso a la empresa del cliente
            if (!await TieneAccesoEmpresaAsync(cliente.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó acceder a cliente {ClienteId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), id);
                return Forbid();
            }

            return Ok(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cliente {ClienteId}", id);
            return StatusCode(500, "Error interno al obtener cliente.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Cliente cliente)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Intento de crear cliente con datos inválidos");
                return BadRequest(ModelState);
            }

            // Verificar que el usuario tiene acceso a esta empresa
            if (!await TieneAccesoEmpresaAsync(cliente.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó crear cliente en empresa {EmpresaId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), cliente.EmpresaId);
                return Forbid();
            }

            // Verificar que no exista otro cliente con la misma identificación en esta empresa
            var existente = await _unitOfWork.ClienteRepository.GetByIdentificationAsync(
                cliente.EmpresaId,
                cliente.NumeroIdentificacion);

            if (existente != null)
            {
                _logger.LogWarning("Intento de crear cliente duplicado con identificación {Identificacion} en empresa {EmpresaId}",
                    cliente.NumeroIdentificacion, cliente.EmpresaId);
                return BadRequest("Ya existe un cliente con esta identificación en la empresa.");
            }

            // Establecer usuario de creación
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            cliente.UsuarioCreacionId = userId;

            var nuevoCliente = await _unitOfWork.ClienteRepository.AddAsync(cliente);

            _logger.LogInformation("Cliente {ClienteId} creado exitosamente para empresa {EmpresaId} por usuario {UserId}",
                nuevoCliente.Id, cliente.EmpresaId, userId);

            return Ok(nuevoCliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cliente para empresa {EmpresaId}", cliente.EmpresaId);
            return StatusCode(500, "Error interno al crear cliente.");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Cliente cliente)
    {
        try
        {
            if (id != cliente.Id)
            {
                _logger.LogWarning("Intento de actualizar cliente con ID inconsistente. URL: {UrlId}, Body: {BodyId}", id, cliente.Id);
                return BadRequest("El ID de la URL no coincide con el ID del cliente.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Intento de actualizar cliente {ClienteId} con datos inválidos", id);
                return BadRequest(ModelState);
            }

            // Verificar que el cliente existe
            var clienteExistente = await _unitOfWork.ClienteRepository.GetAsync(id);
            if (clienteExistente == null)
            {
                _logger.LogWarning("Intento de actualizar cliente inexistente {ClienteId}", id);
                return NotFound();
            }

            // Verificar que el usuario tiene acceso a esta empresa
            if (!await TieneAccesoEmpresaAsync(clienteExistente.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó actualizar cliente {ClienteId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), id);
                return Forbid();
            }

            // Verificar que no exista otro cliente con la misma identificación en esta empresa
            var duplicado = await _unitOfWork.ClienteRepository.GetByIdentificationAsync(
                cliente.EmpresaId,
                cliente.NumeroIdentificacion);

            if (duplicado != null && duplicado.Id != id)
            {
                _logger.LogWarning("Intento de actualizar cliente {ClienteId} con identificación duplicada {Identificacion}",
                    id, cliente.NumeroIdentificacion);
                return BadRequest("Ya existe otro cliente con esta identificación en la empresa.");
            }

            // Preservar campos de auditoría
            cliente.FechaCreacion = clienteExistente.FechaCreacion;
            cliente.UsuarioCreacionId = clienteExistente.UsuarioCreacionId;

            // Establecer usuario de modificación
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            cliente.UsuarioModificacionId = userId;

            await _unitOfWork.ClienteRepository.UpdateAsync(cliente);

            _logger.LogInformation("Cliente {ClienteId} actualizado exitosamente por usuario {UserId}", id, userId);

            return Ok(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cliente {ClienteId}", id);
            return StatusCode(500, "Error interno al actualizar cliente.");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var cliente = await _unitOfWork.ClienteRepository.GetAsync(id);

            if (cliente == null)
            {
                _logger.LogWarning("Intento de eliminar cliente inexistente {ClienteId}", id);
                return NotFound();
            }

            // Verificar que el usuario tiene acceso a esta empresa
            if (!await TieneAccesoEmpresaAsync(cliente.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó eliminar cliente {ClienteId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), id);
                return Forbid();
            }

            // TODO: Verificar que el cliente no tenga documentos asociados
            // Por ahora solo hacemos soft delete

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _unitOfWork.ClienteRepository.DeleteAsync(id, userId!);

            _logger.LogInformation("Cliente {ClienteId} eliminado exitosamente por usuario {UserId}", id, userId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar cliente {ClienteId}", id);
            return StatusCode(500, "Error interno al eliminar cliente.");
        }
    }

    /// <summary>
    /// Importa clientes desde un archivo Excel
    /// </summary>
    [HttpPost("empresa/{empresaId:guid}/importar")]
    public async Task<IActionResult> ImportarAsync(Guid empresaId, IFormFile archivo)
    {
        try
        {
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest("No se proporcionó un archivo válido");
            }

            // Validar extensión
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
            {
                return BadRequest("El archivo debe ser un documento Excel (.xlsx o .xls)");
            }

            // Verificar acceso a la empresa
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó importar clientes a empresa {EmpresaId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
                return Forbid();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            using var stream = archivo.OpenReadStream();
            var resultado = await _excelImportService.ImportarClientesAsync(stream, empresaId, userId);

            _logger.LogInformation("Importación de clientes para empresa {EmpresaId}: {Success} exitosos, {Errors} errores",
                empresaId, resultado.SuccessCount, resultado.ErrorCount);

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar clientes para empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error interno al importar clientes");
        }
    }

    /// <summary>
    /// Descarga la plantilla Excel para importación de clientes
    /// </summary>
    [HttpGet("plantilla")]
    public IActionResult DescargarPlantilla()
    {
        try
        {
            var plantilla = _excelImportService.GenerarPlantillaClientes();
            return File(plantilla, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PlantillaClientes.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar plantilla de clientes");
            return StatusCode(500, "Error interno al generar plantilla");
        }
    }

    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar acceso a empresa {EmpresaId}", empresaId);
            return false;
        }
    }
}
