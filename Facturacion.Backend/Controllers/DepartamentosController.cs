using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
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
public class DepartamentosController : ControllerBase
{
    private readonly IDepartamentoRepository _departamentoRepository;
    private readonly DataContext _context;
    private readonly ILogger<DepartamentosController> _logger;

    public DepartamentosController(
        IDepartamentoRepository departamentoRepository,
        DataContext context,
        ILogger<DepartamentosController> logger)
    {
        _departamentoRepository = departamentoRepository;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los departamentos de una empresa.
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            var departamentos = await _departamentoRepository.GetByEmpresaAsync(empresaId);
            return Ok(departamentos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo departamentos de empresa {EmpresaId}", empresaId);
            return BadRequest("Error al obtener los departamentos.");
        }
    }

    /// <summary>
    /// Obtiene solo los departamentos activos de una empresa.
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

            var departamentos = await _departamentoRepository.GetActivosAsync(empresaId);
            return Ok(departamentos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo departamentos activos de empresa {EmpresaId}", empresaId);
            return BadRequest("Error al obtener los departamentos activos.");
        }
    }

    /// <summary>
    /// Obtiene un departamento específico.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var departamento = await _departamentoRepository.GetAsync(id);

            if (departamento == null)
            {
                return NotFound("Departamento no encontrado.");
            }

            if (!await TieneAccesoEmpresaAsync(departamento.EmpresaId))
            {
                return Forbid();
            }

            return Ok(departamento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo departamento {Id}", id);
            return BadRequest("Error al obtener el departamento.");
        }
    }

    /// <summary>
    /// Crea un nuevo departamento.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Departamento departamento)
    {
        try
        {
            // Validar solo campos requeridos del negocio
            if (departamento.EmpresaId == Guid.Empty)
            {
                return BadRequest("La empresa es obligatoria.");
            }
            if (string.IsNullOrWhiteSpace(departamento.Codigo))
            {
                return BadRequest("El código es obligatorio.");
            }
            if (string.IsNullOrWhiteSpace(departamento.Nombre))
            {
                return BadRequest("El nombre es obligatorio.");
            }

            if (!await TieneAccesoEmpresaAsync(departamento.EmpresaId))
            {
                return Forbid();
            }

            // Verificar código único por empresa
            var existenteCodigo = await _departamentoRepository.GetByCodigoAsync(departamento.EmpresaId, departamento.Codigo);
            if (existenteCodigo != null)
            {
                return BadRequest("Ya existe un departamento con este código en la empresa.");
            }

            // Validar departamento padre si se proporciona
            if (departamento.DepartamentoPadreId.HasValue)
            {
                var padre = await _departamentoRepository.GetAsync(departamento.DepartamentoPadreId.Value);
                if (padre == null || padre.EmpresaId != departamento.EmpresaId)
                {
                    return BadRequest("El departamento padre no existe o no pertenece a la empresa.");
                }
            }

            // Validar jefe si se proporciona
            if (departamento.JefeId.HasValue)
            {
                var jefe = await _context.Empleados
                    .FirstOrDefaultAsync(e => e.Id == departamento.JefeId.Value &&
                                             e.EmpresaId == departamento.EmpresaId &&
                                             !e.IsDeleted);
                if (jefe == null)
                {
                    return BadRequest("El jefe especificado no existe o no pertenece a la empresa.");
                }
            }

            departamento.Id = Guid.NewGuid();
            departamento.UsuarioCreacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var nuevoDepartamento = await _departamentoRepository.AddAsync(departamento);
            return Ok(nuevoDepartamento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando departamento {Codigo}", departamento.Codigo);
            return BadRequest("Error al crear el departamento.");
        }
    }

    /// <summary>
    /// Actualiza un departamento existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Departamento departamento)
    {
        try
        {
            if (id != departamento.Id)
            {
                return BadRequest("El ID no coincide.");
            }

            // Validar solo campos requeridos del negocio, no de auditoría
            if (string.IsNullOrWhiteSpace(departamento.Codigo))
            {
                return BadRequest("El código es obligatorio.");
            }
            if (string.IsNullOrWhiteSpace(departamento.Nombre))
            {
                return BadRequest("El nombre es obligatorio.");
            }

            var existente = await _departamentoRepository.GetAsync(id);
            if (existente == null)
            {
                return NotFound("Departamento no encontrado.");
            }

            if (!await TieneAccesoEmpresaAsync(existente.EmpresaId))
            {
                return Forbid();
            }

            // Verificar código único (excluyendo el departamento actual)
            var duplicadoCodigo = await _departamentoRepository.GetByCodigoAsync(departamento.EmpresaId, departamento.Codigo);
            if (duplicadoCodigo != null && duplicadoCodigo.Id != id)
            {
                return BadRequest("Ya existe otro departamento con este código en la empresa.");
            }

            // Validar departamento padre si se proporciona
            if (departamento.DepartamentoPadreId.HasValue)
            {
                if (departamento.DepartamentoPadreId.Value == departamento.Id)
                {
                    return BadRequest("Un departamento no puede ser padre de sí mismo.");
                }

                var padre = await _departamentoRepository.GetAsync(departamento.DepartamentoPadreId.Value);
                if (padre == null || padre.EmpresaId != departamento.EmpresaId)
                {
                    return BadRequest("El departamento padre no existe o no pertenece a la empresa.");
                }
            }

            // Validar jefe si se proporciona
            if (departamento.JefeId.HasValue)
            {
                var jefe = await _context.Empleados
                    .FirstOrDefaultAsync(e => e.Id == departamento.JefeId.Value &&
                                             e.EmpresaId == departamento.EmpresaId &&
                                             !e.IsDeleted);
                if (jefe == null)
                {
                    return BadRequest("El jefe especificado no existe o no pertenece a la empresa.");
                }
            }

            // Preservar campos de auditoría
            departamento.FechaCreacion = existente.FechaCreacion;
            departamento.UsuarioCreacionId = existente.UsuarioCreacionId;
            departamento.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _departamentoRepository.UpdateAsync(departamento);
            return Ok(departamento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando departamento {Id}", id);
            return BadRequest("Error al actualizar el departamento.");
        }
    }

    /// <summary>
    /// Elimina un departamento (soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var departamento = await _context.Departamentos
                .Include(d => d.Empleados)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

            if (departamento == null)
            {
                return NotFound("Departamento no encontrado.");
            }

            if (!await TieneAccesoEmpresaAsync(departamento.EmpresaId))
            {
                return Forbid();
            }

            // No permitir eliminar si tiene subdepartamentos activos
            var tieneSubdepartamentos = await _context.Departamentos
                .AnyAsync(d => d.DepartamentoPadreId == id && !d.IsDeleted);
            if (tieneSubdepartamentos)
            {
                return BadRequest("No se puede eliminar un departamento que tiene subdepartamentos activos.");
            }

            // No permitir eliminar si tiene empleados activos
            if (departamento.Empleados?.Any(e => !e.IsDeleted) == true)
            {
                return BadRequest("No se puede eliminar un departamento que tiene empleados asignados. Reasigne los empleados primero.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _departamentoRepository.DeleteAsync(id, userId!);

            return Ok(new { Message = "Departamento eliminado exitosamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando departamento {Id}", id);
            return BadRequest("Error al eliminar el departamento.");
        }
    }

    /// <summary>
    /// Genera un catálogo predefinido de departamentos para la empresa.
    /// </summary>
    [HttpPost("empresa/{empresaId:guid}/generar-catalogo")]
    public async Task<IActionResult> GenerarCatalogoAsync(Guid empresaId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verificar si ya existen departamentos
            var existentes = await _context.Departamentos
                .Where(d => d.EmpresaId == empresaId && !d.IsDeleted)
                .CountAsync();

            if (existentes > 0)
            {
                return BadRequest("Ya existen departamentos en esta empresa. El catálogo predefinido solo se puede generar cuando no hay departamentos.");
            }

            // Catálogo predefinido de departamentos
            var departamentosPredefinidos = new List<(string Codigo, string Nombre, string Descripcion)>
            {
                ("GG", "Gerencia General", "Dirección estratégica y toma de decisiones ejecutivas"),
                ("ADM", "Administración", "Gestión administrativa y coordinación general"),
                ("FIN", "Finanzas", "Contabilidad, tesorería y gestión financiera"),
                ("RRHH", "Recursos Humanos", "Gestión del talento humano y desarrollo organizacional"),
                ("VEN", "Ventas", "Comercialización y atención al cliente"),
                ("MKT", "Mercadeo", "Estrategias de marketing y publicidad"),
                ("OPE", "Operaciones", "Producción y logística operativa"),
                ("TI", "Tecnología", "Sistemas de información y soporte técnico"),
                ("LEG", "Legal", "Asesoría jurídica y cumplimiento normativo"),
                ("CAL", "Control de Calidad", "Aseguramiento y control de calidad"),
                ("COM", "Compras", "Adquisiciones y gestión de proveedores"),
                ("BOD", "Bodega", "Almacenamiento y control de inventarios"),
                ("MAN", "Mantenimiento", "Mantenimiento de equipos e instalaciones"),
                ("SEG", "Seguridad", "Seguridad física y ocupacional"),
                ("ATC", "Atención al Cliente", "Servicio y soporte al cliente")
            };

            var departamentosCreados = new List<Departamento>();

            foreach (var (codigo, nombre, descripcion) in departamentosPredefinidos)
            {
                var departamento = new Departamento
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = empresaId,
                    Codigo = codigo,
                    Nombre = nombre,
                    Descripcion = descripcion,
                    Activo = true,
                    UsuarioCreacionId = userId
                };
                departamentosCreados.Add(departamento);
            }

            // Guardar todos los departamentos
            foreach (var dept in departamentosCreados)
            {
                await _departamentoRepository.AddAsync(dept);
            }

            _logger.LogInformation("Catálogo de {Count} departamentos generado para empresa {EmpresaId}",
                departamentosCreados.Count, empresaId);

            return Ok(new
            {
                success = true,
                message = $"Se generaron {departamentosCreados.Count} departamentos exitosamente.",
                count = departamentosCreados.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando catálogo de departamentos para empresa {EmpresaId}", empresaId);
            return BadRequest("Error al generar el catálogo de departamentos.");
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
