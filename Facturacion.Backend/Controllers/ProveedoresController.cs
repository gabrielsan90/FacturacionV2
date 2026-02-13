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
public class ProveedoresController : ControllerBase
{
    private readonly IProveedorUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly IExcelImportService _excelImportService;
    private readonly ILogger<ProveedoresController> _logger;

    public ProveedoresController(
        IProveedorUnitOfWork unitOfWork,
        DataContext context,
        IExcelImportService excelImportService,
        ILogger<ProveedoresController> logger)
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
                _logger.LogWarning("Usuario {UserId} intentó acceder a proveedores de empresa {EmpresaId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
                return Forbid();
            }

            var proveedores = await _unitOfWork.ProveedorRepository.GetByEmpresaAsync(empresaId);
            _logger.LogInformation("Se obtuvieron {Count} proveedores para empresa {EmpresaId}", proveedores.Count(), empresaId);
            return Ok(proveedores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener proveedores de empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error interno al obtener proveedores.");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var proveedor = await _unitOfWork.ProveedorRepository.GetAsync(id);

            if (proveedor == null)
            {
                _logger.LogWarning("Proveedor {ProveedorId} no encontrado", id);
                return NotFound();
            }

            // Verificar que el usuario tiene acceso a la empresa del proveedor
            if (!await TieneAccesoEmpresaAsync(proveedor.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó acceder a proveedor {ProveedorId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), id);
                return Forbid();
            }

            return Ok(proveedor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener proveedor {ProveedorId}", id);
            return StatusCode(500, "Error interno al obtener proveedor.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Proveedor proveedor)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Intento de crear proveedor con datos inválidos");
                return BadRequest(ModelState);
            }

            // Verificar que el usuario tiene acceso a esta empresa
            if (!await TieneAccesoEmpresaAsync(proveedor.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó crear proveedor en empresa {EmpresaId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), proveedor.EmpresaId);
                return Forbid();
            }

            // Verificar que no exista otro proveedor con la misma identificación en esta empresa
            var existente = await _unitOfWork.ProveedorRepository.GetByIdentificationAsync(
                proveedor.EmpresaId,
                proveedor.NumeroIdentificacion);

            if (existente != null)
            {
                _logger.LogWarning("Intento de crear proveedor duplicado con identificación {Identificacion} en empresa {EmpresaId}",
                    proveedor.NumeroIdentificacion, proveedor.EmpresaId);
                return BadRequest("Ya existe un proveedor con esta identificación en la empresa.");
            }

            // Establecer usuario de creación
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            proveedor.UsuarioCreacionId = userId;

            var nuevoProveedor = await _unitOfWork.ProveedorRepository.AddAsync(proveedor);

            _logger.LogInformation("Proveedor {ProveedorId} creado exitosamente para empresa {EmpresaId} por usuario {UserId}",
                nuevoProveedor.Id, proveedor.EmpresaId, userId);

            return Ok(nuevoProveedor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear proveedor para empresa {EmpresaId}", proveedor.EmpresaId);
            return StatusCode(500, "Error interno al crear proveedor.");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Proveedor proveedor)
    {
        try
        {
            if (id != proveedor.Id)
            {
                _logger.LogWarning("Intento de actualizar proveedor con ID inconsistente. URL: {UrlId}, Body: {BodyId}", id, proveedor.Id);
                return BadRequest("El ID de la URL no coincide con el ID del proveedor.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Intento de actualizar proveedor {ProveedorId} con datos inválidos", id);
                return BadRequest(ModelState);
            }

            // Verificar que el proveedor existe
            var proveedorExistente = await _unitOfWork.ProveedorRepository.GetAsync(id);
            if (proveedorExistente == null)
            {
                _logger.LogWarning("Intento de actualizar proveedor inexistente {ProveedorId}", id);
                return NotFound();
            }

            // Verificar que el usuario tiene acceso a esta empresa
            if (!await TieneAccesoEmpresaAsync(proveedorExistente.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó actualizar proveedor {ProveedorId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), id);
                return Forbid();
            }

            // Verificar que no exista otro proveedor con la misma identificación en esta empresa
            var duplicado = await _unitOfWork.ProveedorRepository.GetByIdentificationAsync(
                proveedor.EmpresaId,
                proveedor.NumeroIdentificacion);

            if (duplicado != null && duplicado.Id != id)
            {
                _logger.LogWarning("Intento de actualizar proveedor {ProveedorId} con identificación duplicada {Identificacion}",
                    id, proveedor.NumeroIdentificacion);
                return BadRequest("Ya existe otro proveedor con esta identificación en la empresa.");
            }

            // Preservar campos de auditoría
            proveedor.FechaCreacion = proveedorExistente.FechaCreacion;
            proveedor.UsuarioCreacionId = proveedorExistente.UsuarioCreacionId;

            // Establecer usuario de modificación
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            proveedor.UsuarioModificacionId = userId;

            await _unitOfWork.ProveedorRepository.UpdateAsync(proveedor);

            _logger.LogInformation("Proveedor {ProveedorId} actualizado exitosamente por usuario {UserId}", id, userId);

            return Ok(proveedor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar proveedor {ProveedorId}", id);
            return StatusCode(500, "Error interno al actualizar proveedor.");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var proveedor = await _unitOfWork.ProveedorRepository.GetAsync(id);

            if (proveedor == null)
            {
                _logger.LogWarning("Intento de eliminar proveedor inexistente {ProveedorId}", id);
                return NotFound();
            }

            // Verificar que el usuario tiene acceso a esta empresa
            if (!await TieneAccesoEmpresaAsync(proveedor.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó eliminar proveedor {ProveedorId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), id);
                return Forbid();
            }

            // TODO: Verificar que el proveedor no tenga documentos asociados
            // Por ahora solo hacemos soft delete

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _unitOfWork.ProveedorRepository.DeleteAsync(id, userId!);

            _logger.LogInformation("Proveedor {ProveedorId} eliminado exitosamente por usuario {UserId}", id, userId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar proveedor {ProveedorId}", id);
            return StatusCode(500, "Error interno al eliminar proveedor.");
        }
    }

    /// <summary>
    /// Importa proveedores desde un archivo Excel
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
                _logger.LogWarning("Usuario {UserId} intentó importar proveedores a empresa {EmpresaId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
                return Forbid();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            using var stream = archivo.OpenReadStream();
            var resultado = await _excelImportService.ImportarProveedoresAsync(stream, empresaId, userId);

            _logger.LogInformation("Importación de proveedores para empresa {EmpresaId}: {Success} exitosos, {Errors} errores",
                empresaId, resultado.SuccessCount, resultado.ErrorCount);

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar proveedores para empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error interno al importar proveedores");
        }
    }

    /// <summary>
    /// Descarga la plantilla Excel para importación de proveedores
    /// </summary>
    [HttpGet("plantilla")]
    public IActionResult DescargarPlantilla()
    {
        try
        {
            var plantilla = _excelImportService.GenerarPlantillaProveedores();
            return File(plantilla, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PlantillaProveedores.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar plantilla de proveedores");
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
