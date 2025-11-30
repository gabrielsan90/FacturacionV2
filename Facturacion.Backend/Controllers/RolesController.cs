using Facturacion.Backend.Data;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperUser,Administrador de Empresa")]
public class RolesController : ControllerBase
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly DataContext _context;
    private readonly ILogger<RolesController> _logger;

    public RolesController(
        RoleManager<IdentityRole> roleManager,
        DataContext context,
        ILogger<RolesController> logger)
    {
        _roleManager = roleManager;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los roles disponibles con información de privilegios
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        try
        {
            var roles = await _context.Roles
                .Select(r => new RolDto
                {
                    Id = r.Id,
                    Nombre = r.Nombre,
                    Descripcion = r.Descripcion,
                    EsSistema = r.EsSistema,
                    Activo = r.Activo,
                    CantidadPrivilegios = r.RolesPrivilegios!.Count,
                    FechaCreacion = r.FechaCreacion
                })
                .OrderBy(r => r.Nombre)
                .ToListAsync();

            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los roles");
            return StatusCode(500, "Error interno del servidor al obtener los roles.");
        }
    }

    /// <summary>
    /// Obtiene un rol por su ID con sus privilegios
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(string id)
    {
        try
        {
            var rol = await _context.Roles
                .Where(r => r.Id == id)
                .Select(r => new RolDto
                {
                    Id = r.Id,
                    Nombre = r.Nombre,
                    Descripcion = r.Descripcion,
                    EsSistema = r.EsSistema,
                    Activo = r.Activo,
                    CantidadPrivilegios = r.RolesPrivilegios!.Count,
                    FechaCreacion = r.FechaCreacion
                })
                .FirstOrDefaultAsync();

            if (rol == null)
            {
                return NotFound("Rol no encontrado.");
            }

            return Ok(rol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el rol {RolId}", id);
            return StatusCode(500, "Error interno del servidor al obtener el rol.");
        }
    }

    /// <summary>
    /// Crea un nuevo rol personalizado
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] CreateRolDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar si el nombre ya existe
            var existingRol = await _context.Roles
                .FirstOrDefaultAsync(r => r.Nombre.ToLower() == model.Nombre.ToLower());

            if (existingRol != null)
            {
                return BadRequest("Ya existe un rol con este nombre.");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var rol = new Rol
            {
                Id = Guid.NewGuid().ToString(),
                Nombre = model.Nombre,
                NormalizedName = model.Nombre.ToUpper(),
                Descripcion = model.Descripcion,
                EsSistema = false, // Los roles creados manualmente nunca son del sistema
                Activo = model.Activo,
                FechaCreacion = DateTime.UtcNow,
                UsuarioCreacionId = currentUserId,
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            _context.Roles.Add(rol);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Rol creado: {RolId} - {RolNombre} por usuario {UserId}",
                rol.Id, rol.Nombre, currentUserId);

            return CreatedAtAction(nameof(GetAsync), new { id = rol.Id }, new { id = rol.Id, nombre = rol.Nombre });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el rol");
            return StatusCode(500, "Error interno del servidor al crear el rol.");
        }
    }

    /// <summary>
    /// Actualiza un rol existente (solo roles personalizados)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(string id, [FromBody] UpdateRolDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != model.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID del rol.");
            }

            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
            {
                return NotFound("Rol no encontrado.");
            }

            // Verificar si es un rol del sistema
            if (rol.EsSistema)
            {
                return BadRequest("No se pueden editar los roles del sistema.");
            }

            // Verificar nombre único (excepto el actual)
            var existingRol = await _context.Roles
                .FirstOrDefaultAsync(r => r.Nombre.ToLower() == model.Nombre.ToLower() && r.Id != id);

            if (existingRol != null)
            {
                return BadRequest("Ya existe un rol con este nombre.");
            }

            // Actualizar propiedades
            rol.Nombre = model.Nombre;
            rol.NormalizedName = model.Nombre.ToUpper();
            rol.Descripcion = model.Descripcion;
            rol.Activo = model.Activo;

            _context.Roles.Update(rol);
            await _context.SaveChangesAsync();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Rol actualizado: {RolId} - {RolNombre} por usuario {UserId}",
                rol.Id, rol.Nombre, currentUserId);

            return Ok(new { id = rol.Id, nombre = rol.Nombre });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el rol {RolId}", id);
            return StatusCode(500, "Error interno del servidor al actualizar el rol.");
        }
    }

    /// <summary>
    /// Elimina un rol (solo roles personalizados)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        try
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
            {
                return NotFound("Rol no encontrado.");
            }

            // Verificar si es un rol del sistema
            if (rol.EsSistema)
            {
                return BadRequest("No se pueden eliminar los roles del sistema.");
            }

            // Verificar si hay usuarios asignados a este rol
            var usersInRole = await _context.UserRoles
                .Where(ur => ur.RoleId == id)
                .CountAsync();

            if (usersInRole > 0)
            {
                return BadRequest($"No se puede eliminar el rol porque tiene {usersInRole} usuario(s) asignado(s).");
            }

            // Eliminar privilegios asociados
            var rolePrivilegios = await _context.RolesPrivilegios
                .Where(rp => rp.RolId == id)
                .ToListAsync();

            if (rolePrivilegios.Any())
            {
                _context.RolesPrivilegios.RemoveRange(rolePrivilegios);
            }

            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Rol eliminado: {RolId} - {RolNombre} por usuario {UserId}",
                id, rol.Nombre, currentUserId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el rol {RolId}", id);
            return StatusCode(500, "Error interno del servidor al eliminar el rol.");
        }
    }

    /// <summary>
    /// Obtiene los privilegios asignados a un rol
    /// </summary>
    [HttpGet("{id}/privilegios")]
    public async Task<IActionResult> GetPrivilegiosAsync(string id)
    {
        try
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
            {
                return NotFound("Rol no encontrado.");
            }

            var privilegioIds = await _context.RolesPrivilegios
                .Where(rp => rp.RolId == id)
                .Select(rp => rp.PrivilegioId)
                .ToListAsync();

            return Ok(privilegioIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener privilegios del rol {RolId}", id);
            return StatusCode(500, "Error interno del servidor al obtener los privilegios del rol.");
        }
    }

    /// <summary>
    /// Actualiza los privilegios de un rol
    /// </summary>
    [HttpPut("{id}/privilegios")]
    public async Task<IActionResult> UpdatePrivilegiosAsync(string id, [FromBody] RolPrivilegiosDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != model.RolId)
            {
                return BadRequest("El ID de la URL no coincide con el ID del rol.");
            }

            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
            {
                return NotFound("Rol no encontrado.");
            }

            // Obtener privilegios actuales
            var currentPrivilegios = await _context.RolesPrivilegios
                .Where(rp => rp.RolId == id)
                .ToListAsync();

            // Eliminar todos los privilegios actuales
            if (currentPrivilegios.Any())
            {
                _context.RolesPrivilegios.RemoveRange(currentPrivilegios);
            }

            // Agregar nuevos privilegios
            if (model.PrivilegioIds != null && model.PrivilegioIds.Any())
            {
                // Validar que todos los privilegios existen
                var validPrivilegios = await _context.Privilegios
                    .Where(p => model.PrivilegioIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync();

                if (validPrivilegios.Count != model.PrivilegioIds.Count)
                {
                    return BadRequest("Algunos privilegios especificados no existen.");
                }

                foreach (var privilegioId in model.PrivilegioIds)
                {
                    var rolPrivilegio = new RolPrivilegio
                    {
                        RolId = id,
                        PrivilegioId = privilegioId
                    };
                    _context.RolesPrivilegios.Add(rolPrivilegio);
                }
            }

            await _context.SaveChangesAsync();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Privilegios actualizados para rol {RolId} por usuario {UserId}. Total privilegios: {Count}",
                id, currentUserId, model.PrivilegioIds?.Count ?? 0);

            return Ok(new { message = "Privilegios actualizados exitosamente.", cantidadPrivilegios = model.PrivilegioIds?.Count ?? 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar privilegios del rol {RolId}", id);
            return StatusCode(500, "Error interno del servidor al actualizar los privilegios del rol.");
        }
    }
}
