using Facturacion.Backend.Helpers;
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
            _logger.LogInformation("Getting roles list. RequestedBy: {UserId}",
                User.FindFirstValue(ClaimTypes.NameIdentifier));

            var roles = await _context.Roles
                .Select(r => new RolDto
                {
                    Id = r.Id,
                    Nombre = r.Nombre ?? r.Name ?? "",
                    Descripcion = r.Descripcion,
                    EsSistema = r.EsSistema,
                    Activo = r.Activo,
                    CantidadPrivilegios = r.RolesPrivilegios!.Count,
                    FechaCreacion = r.FechaCreacion
                })
                .OrderBy(r => r.Nombre)
                .ToListAsync();

            _logger.LogInformation("Successfully retrieved {RoleCount} roles", roles.Count);

            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles list. RequestedBy: {UserId}",
                User.FindFirstValue(ClaimTypes.NameIdentifier));
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
            _logger.LogInformation("Getting role details. RoleId: {RoleId}, RequestedBy: {UserId}",
                id, User.FindFirstValue(ClaimTypes.NameIdentifier));

            var rol = await _context.Roles
                .Where(r => r.Id == id)
                .Select(r => new RolDto
                {
                    Id = r.Id,
                    Nombre = r.Nombre ?? r.Name ?? "",
                    Descripcion = r.Descripcion,
                    EsSistema = r.EsSistema,
                    Activo = r.Activo,
                    CantidadPrivilegios = r.RolesPrivilegios!.Count,
                    FechaCreacion = r.FechaCreacion
                })
                .FirstOrDefaultAsync();

            if (rol == null)
            {
                _logger.LogWarning("Role not found. RoleId: {RoleId}", id);
                return NotFound("Rol no encontrado.");
            }

            _logger.LogInformation("Successfully retrieved role. RoleId: {RoleId}, Name: {RoleName}, Privileges: {PrivilegeCount}",
                id, rol.Nombre, rol.CantidadPrivilegios);

            return Ok(rol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role details. RoleId: {RoleId}, RequestedBy: {UserId}",
                id, User.FindFirstValue(ClaimTypes.NameIdentifier));
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
            _logger.LogInformation("Creating new role. Name: {RoleName}, CreatedBy: {UserId}",
                model.Nombre, User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when creating role. Name: {RoleName}", model.Nombre);
                return BadRequest(ModelState);
            }

            // Verificar si el nombre ya existe
            var existingRol = await _context.Roles
                .FirstOrDefaultAsync(r => r.Nombre.ToLower() == model.Nombre.ToLower());

            if (existingRol != null)
            {
                _logger.LogWarning("Attempted to create role with existing name. Name: {RoleName}", model.Nombre);
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
                FechaCreacion = FechaCostaRicaHelper.Ahora,
                UsuarioCreacionId = currentUserId,
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            _context.Roles.Add(rol);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Role created successfully. RoleId: {RolId}, Name: {RolNombre}, Active: {Activo}, CreatedBy: {UserId}",
                rol.Id, rol.Nombre, rol.Activo, currentUserId);

            return Ok(new { id = rol.Id, nombre = rol.Nombre });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role. Name: {RoleName}, CreatedBy: {UserId}",
                model.Nombre, User.FindFirstValue(ClaimTypes.NameIdentifier));
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
            _logger.LogInformation("Updating role. RoleId: {RoleId}, UpdatedBy: {UserId}",
                id, User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when updating role. RoleId: {RoleId}", id);
                return BadRequest(ModelState);
            }

            if (id != model.Id)
            {
                _logger.LogWarning("ID mismatch when updating role. URL ID: {UrlId}, Model ID: {ModelId}", id, model.Id);
                return BadRequest("El ID de la URL no coincide con el ID del rol.");
            }

            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
            {
                _logger.LogWarning("Role not found for update. RoleId: {RoleId}", id);
                return NotFound("Rol no encontrado.");
            }

            // Verificar si es un rol del sistema
            if (rol.EsSistema)
            {
                _logger.LogWarning("Attempted to edit system role. RoleId: {RoleId}, Name: {RoleName}", id, rol.Nombre);
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
            _logger.LogInformation("Role updated successfully. RoleId: {RolId}, Name: {RolNombre}, Active: {Activo}, UpdatedBy: {UserId}",
                rol.Id, rol.Nombre, rol.Activo, currentUserId);

            return Ok(new { id = rol.Id, nombre = rol.Nombre });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role. RoleId: {RolId}, UpdatedBy: {UserId}",
                id, User.FindFirstValue(ClaimTypes.NameIdentifier));
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
            _logger.LogInformation("Deleting role. RoleId: {RoleId}, DeletedBy: {UserId}",
                id, User.FindFirstValue(ClaimTypes.NameIdentifier));

            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
            {
                _logger.LogWarning("Role not found for deletion. RoleId: {RoleId}", id);
                return NotFound("Rol no encontrado.");
            }

            // Verificar si es un rol del sistema
            if (rol.EsSistema)
            {
                _logger.LogWarning("Attempted to delete system role. RoleId: {RoleId}, Name: {RoleName}", id, rol.Nombre);
                return BadRequest("No se pueden eliminar los roles del sistema.");
            }

            // Verificar si hay usuarios asignados a este rol
            var usersInRole = await _context.UserRoles
                .Where(ur => ur.RoleId == id)
                .CountAsync();

            if (usersInRole > 0)
            {
                _logger.LogWarning("Cannot delete role with assigned users. RoleId: {RoleId}, UserCount: {UserCount}",
                    id, usersInRole);
                return BadRequest($"No se puede eliminar el rol porque tiene {usersInRole} usuario(s) asignado(s).");
            }

            // Eliminar privilegios asociados
            var rolePrivilegios = await _context.RolesPrivilegios
                .Where(rp => rp.RolId == id)
                .ToListAsync();

            if (rolePrivilegios.Any())
            {
                _context.RolesPrivilegios.RemoveRange(rolePrivilegios);
                _logger.LogDebug("Removed {PrivilegeCount} privileges associated with role {RoleId}",
                    rolePrivilegios.Count, id);
            }

            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Role deleted successfully. RoleId: {RolId}, Name: {RolNombre}, DeletedBy: {UserId}",
                id, rol.Nombre, currentUserId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role. RoleId: {RolId}, DeletedBy: {UserId}",
                id, User.FindFirstValue(ClaimTypes.NameIdentifier));
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
            _logger.LogInformation("Updating role privileges. RoleId: {RoleId}, NewPrivilegeCount: {PrivilegeCount}, UpdatedBy: {UserId}",
                id, model.PrivilegioIds?.Count ?? 0, User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when updating role privileges. RoleId: {RoleId}", id);
                return BadRequest(ModelState);
            }

            if (id != model.RolId)
            {
                _logger.LogWarning("ID mismatch when updating role privileges. URL ID: {UrlId}, Model ID: {ModelId}", id, model.RolId);
                return BadRequest("El ID de la URL no coincide con el ID del rol.");
            }

            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
            {
                _logger.LogWarning("Role not found when updating privileges. RoleId: {RoleId}", id);
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
            _logger.LogInformation("Role privileges updated successfully. RoleId: {RolId}, NewPrivilegeCount: {Count}, UpdatedBy: {UserId}",
                id, model.PrivilegioIds?.Count ?? 0, currentUserId);

            return Ok(new { message = "Privilegios actualizados exitosamente.", cantidadPrivilegios = model.PrivilegioIds?.Count ?? 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role privileges. RoleId: {RolId}, UpdatedBy: {UserId}",
                id, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return StatusCode(500, "Error interno del servidor al actualizar los privilegios del rol.");
        }
    }
}
