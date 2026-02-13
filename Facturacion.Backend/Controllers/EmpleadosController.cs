using Facturacion.Backend.Data;
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
public class EmpleadosController : ControllerBase
{
    private readonly IEmpleadoUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly ILogger<EmpleadosController> _logger;

    public EmpleadosController(
        IEmpleadoUnitOfWork unitOfWork,
        DataContext context,
        ILogger<EmpleadosController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los empleados de una empresa con filtros opcionales.
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId, [FromQuery] string? filtro = null)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            var empleados = await _unitOfWork.EmpleadoRepository.GetByEmpresaAsync(empresaId);

            // Aplicar filtro si se proporciona
            if (!string.IsNullOrWhiteSpace(filtro))
            {
                filtro = filtro.ToLower();
                empleados = empleados.Where(e =>
                    e.Codigo.ToLower().Contains(filtro) ||
                    e.Nombre.ToLower().Contains(filtro) ||
                    e.PrimerApellido.ToLower().Contains(filtro) ||
                    (e.SegundoApellido != null && e.SegundoApellido.ToLower().Contains(filtro)) ||
                    e.Identificacion.ToLower().Contains(filtro) ||
                    (e.Email != null && e.Email.ToLower().Contains(filtro)) ||
                    (e.EmailCorporativo != null && e.EmailCorporativo.ToLower().Contains(filtro))
                ).ToList();
            }

            return Ok(empleados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo empleados de empresa {EmpresaId}", empresaId);
            return BadRequest("Error al obtener los empleados.");
        }
    }

    /// <summary>
    /// Obtiene solo los empleados activos de una empresa.
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}/activos")]
    public async Task<IActionResult> GetActivosByEmpresaAsync(Guid empresaId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            var empleados = await _unitOfWork.EmpleadoRepository.GetActivosAsync(empresaId);

            var resultado = empleados.Select(e => new
            {
                e.Id,
                e.Codigo,
                e.Nombre,
                e.PrimerApellido,
                e.SegundoApellido,
                e.NombreCompleto,
                e.Identificacion,
                e.DepartamentoId,
                DepartamentoNombre = e.Departamento?.Nombre,
                e.PuestoId,
                PuestoNombre = e.Puesto?.Nombre ?? e.PuestoNombre,
                e.Email,
                e.EmailCorporativo,
                e.Estado
            });

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo empleados activos de empresa {EmpresaId}", empresaId);
            return BadRequest("Error al obtener los empleados activos.");
        }
    }

    /// <summary>
    /// Obtiene un empleado específico con sus relaciones (departamento, puesto, jefe directo).
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var empleado = await _unitOfWork.EmpleadoRepository.GetAsync(id);

            if (empleado == null)
            {
                return NotFound("Empleado no encontrado.");
            }

            if (!await TieneAccesoEmpresaAsync(empleado.EmpresaId))
            {
                return Forbid();
            }

            return Ok(empleado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo empleado {Id}", id);
            return BadRequest("Error al obtener el empleado.");
        }
    }

    /// <summary>
    /// Crea un nuevo empleado.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Empleado empleado)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await TieneAccesoEmpresaAsync(empleado.EmpresaId))
            {
                return Forbid();
            }

            // Verificar código único por empresa
            var existenteCodigo = await _unitOfWork.EmpleadoRepository.GetByCodigoAsync(empleado.EmpresaId, empleado.Codigo);
            if (existenteCodigo != null)
            {
                return BadRequest("Ya existe un empleado con este código en la empresa.");
            }

            // Verificar identificación única por empresa
            var existenteIdentificacion = await _unitOfWork.EmpleadoRepository.GetByIdentificacionAsync(
                empleado.EmpresaId, empleado.Identificacion);
            if (existenteIdentificacion != null)
            {
                return BadRequest("Ya existe un empleado con este número de identificación en la empresa.");
            }

            // Validar que el departamento exista y pertenezca a la empresa
            var departamento = await _unitOfWork.DepartamentoRepository.GetAsync(empleado.DepartamentoId);
            if (departamento == null || departamento.EmpresaId != empleado.EmpresaId)
            {
                return BadRequest("El departamento no existe o no pertenece a la empresa.");
            }

            // Validar puesto si se proporciona
            if (empleado.PuestoId.HasValue)
            {
                var puesto = await _unitOfWork.PuestoRepository.GetAsync(empleado.PuestoId.Value);
                if (puesto == null || puesto.EmpresaId != empleado.EmpresaId)
                {
                    return BadRequest("El puesto no existe o no pertenece a la empresa.");
                }
            }

            // Validar jefe directo si se proporciona
            if (empleado.JefeDirectoId.HasValue)
            {
                var jefe = await _unitOfWork.EmpleadoRepository.GetAsync(empleado.JefeDirectoId.Value);
                if (jefe == null || jefe.EmpresaId != empleado.EmpresaId)
                {
                    return BadRequest("El jefe directo no existe o no pertenece a la empresa.");
                }
            }

            // Validar banco si se proporciona (usar _context directamente, no hay BancoRepository en el UoW)
            if (empleado.BancoId.HasValue)
            {
                var banco = await _context.Bancos
                    .FirstOrDefaultAsync(b => b.Id == empleado.BancoId.Value && !b.IsDeleted);
                if (banco == null)
                {
                    return BadRequest("El banco especificado no existe.");
                }
            }

            empleado.Id = Guid.NewGuid();
            empleado.UsuarioCreacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var nuevoEmpleado = await _unitOfWork.EmpleadoRepository.AddAsync(empleado);
            return Ok(nuevoEmpleado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando empleado {Codigo}", empleado.Codigo);
            return BadRequest("Error al crear el empleado.");
        }
    }

    /// <summary>
    /// Actualiza un empleado existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Empleado empleado)
    {
        try
        {
            if (id != empleado.Id)
            {
                return BadRequest("El ID no coincide.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existente = await _unitOfWork.EmpleadoRepository.GetAsync(id);
            if (existente == null)
            {
                return NotFound("Empleado no encontrado.");
            }

            if (!await TieneAccesoEmpresaAsync(existente.EmpresaId))
            {
                return Forbid();
            }

            // Verificar código único (excluyendo el empleado actual)
            var duplicadoCodigo = await _unitOfWork.EmpleadoRepository.GetByCodigoAsync(empleado.EmpresaId, empleado.Codigo);
            if (duplicadoCodigo != null && duplicadoCodigo.Id != id)
            {
                return BadRequest("Ya existe otro empleado con este código en la empresa.");
            }

            // Verificar identificación única (excluyendo el empleado actual)
            var duplicadoIdentificacion = await _unitOfWork.EmpleadoRepository.GetByIdentificacionAsync(
                empleado.EmpresaId, empleado.Identificacion);
            if (duplicadoIdentificacion != null && duplicadoIdentificacion.Id != id)
            {
                return BadRequest("Ya existe otro empleado con este número de identificación en la empresa.");
            }

            // Validar que el departamento exista y pertenezca a la empresa
            var departamento = await _unitOfWork.DepartamentoRepository.GetAsync(empleado.DepartamentoId);
            if (departamento == null || departamento.EmpresaId != empleado.EmpresaId)
            {
                return BadRequest("El departamento no existe o no pertenece a la empresa.");
            }

            // Validar puesto si se proporciona
            if (empleado.PuestoId.HasValue)
            {
                var puesto = await _unitOfWork.PuestoRepository.GetAsync(empleado.PuestoId.Value);
                if (puesto == null || puesto.EmpresaId != empleado.EmpresaId)
                {
                    return BadRequest("El puesto no existe o no pertenece a la empresa.");
                }
            }

            // Validar jefe directo si se proporciona
            if (empleado.JefeDirectoId.HasValue)
            {
                if (empleado.JefeDirectoId.Value == empleado.Id)
                {
                    return BadRequest("Un empleado no puede ser jefe directo de sí mismo.");
                }

                var jefe = await _unitOfWork.EmpleadoRepository.GetAsync(empleado.JefeDirectoId.Value);
                if (jefe == null || jefe.EmpresaId != empleado.EmpresaId)
                {
                    return BadRequest("El jefe directo no existe o no pertenece a la empresa.");
                }
            }

            // Validar banco si se proporciona
            if (empleado.BancoId.HasValue)
            {
                var banco = await _context.Bancos
                    .FirstOrDefaultAsync(b => b.Id == empleado.BancoId.Value && !b.IsDeleted);
                if (banco == null)
                {
                    return BadRequest("El banco especificado no existe.");
                }
            }

            // Preservar campos de auditoría
            empleado.FechaCreacion = existente.FechaCreacion;
            empleado.UsuarioCreacionId = existente.UsuarioCreacionId;
            empleado.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _unitOfWork.EmpleadoRepository.UpdateAsync(empleado);
            return Ok(empleado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando empleado {Id}", id);
            return BadRequest("Error al actualizar el empleado.");
        }
    }

    /// <summary>
    /// Elimina un empleado (soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var empleado = await _context.Empleados
                .Include(e => e.Subordinados)
                .Include(e => e.DetallesPlanilla)
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            if (empleado == null)
            {
                return NotFound("Empleado no encontrado.");
            }

            if (!await TieneAccesoEmpresaAsync(empleado.EmpresaId))
            {
                return Forbid();
            }

            // No permitir eliminar si tiene subordinados activos
            if (empleado.Subordinados?.Any(s => !s.IsDeleted) == true)
            {
                return BadRequest("No se puede eliminar un empleado que tiene subordinados activos. Reasigne los subordinados primero.");
            }

            // No permitir eliminar si tiene detalles de planilla
            if (empleado.DetallesPlanilla?.Any() == true)
            {
                return BadRequest("No se puede eliminar un empleado que tiene registros en planillas. Considere cambiar su estado a Inactivo en su lugar.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _unitOfWork.EmpleadoRepository.DeleteAsync(id, userId!);

            return Ok(new { Message = "Empleado eliminado exitosamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando empleado {Id}", id);
            return BadRequest("Error al eliminar el empleado.");
        }
    }

    /// <summary>
    /// Verifica si el usuario actual tiene acceso a la empresa especificada.
    /// </summary>
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
