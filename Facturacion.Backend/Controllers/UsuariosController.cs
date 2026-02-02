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
public class UsuariosController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly DataContext _context;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        DataContext context,
        ILogger<UsuariosController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los usuarios (filtrados por empresa para Administradores)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        try
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var isSuperUser = userRoles.Contains("SuperUser");

            IQueryable<User> query = _context.Users;

            // Si no es SuperUser, filtrar por empresas asignadas
            if (!isSuperUser)
            {
                // Obtener las empresas del usuario actual
                var empresasUsuario = await _context.UsuariosEmpresas
                    .Where(ue => ue.UserId == currentUserId)
                    .Select(ue => ue.EmpresaId)
                    .ToListAsync();

                // Filtrar usuarios que están en las mismas empresas
                query = query.Where(u => _context.UsuariosEmpresas
                    .Any(ue => ue.UserId == u.Id && empresasUsuario.Contains(ue.EmpresaId)));
            }

            var users = await query.ToListAsync();

            var userList = new List<UserListDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var empresas = await _context.UsuariosEmpresas
                    .Where(ue => ue.UserId == user.Id)
                    .Include(ue => ue.Empresa)
                    .Select(ue => new EmpresaBasicDto
                    {
                        Id = ue.EmpresaId,
                        NombreComercial = ue.Empresa!.NombreComercial
                    })
                    .ToListAsync();

                userList.Add(new UserListDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FullName = user.FullName,
                    Document = user.Document,
                    PhoneNumber = user.PhoneNumber,
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = roles.ToList(),
                    Empresas = empresas
                });
            }

            return Ok(userList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los usuarios");
            return StatusCode(500, "Error interno del servidor al obtener los usuarios.");
        }
    }

    /// <summary>
    /// Obtiene un usuario por su ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            // Obtener ID del usuario actual
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verificar permisos si no es SuperUser
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var isSuperUser = userRoles.Contains("SuperUser");

            if (!isSuperUser)
            {
                var empresasUsuario = await _context.UsuariosEmpresas
                    .Where(ue => ue.UserId == currentUserId)
                    .Select(ue => ue.EmpresaId)
                    .ToListAsync();

                var hasAccess = await _context.UsuariosEmpresas
                    .AnyAsync(ue => ue.UserId == id && empresasUsuario.Contains(ue.EmpresaId));

                if (!hasAccess)
                {
                    return Forbid("No tiene permisos para ver este usuario.");
                }
            }

            var roles = await _userManager.GetRolesAsync(user);
            var empresaIds = await _context.UsuariosEmpresas
                .Where(ue => ue.UserId == user.Id)
                .Select(ue => ue.EmpresaId)
                .ToListAsync();

            var userDto = new UserManagementDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Document = user.Document,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles.ToList(),
                EmpresaIds = empresaIds
            };

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el usuario {UserId}", id);
            return StatusCode(500, "Error interno del servidor al obtener el usuario.");
        }
    }

    /// <summary>
    /// Crea un nuevo usuario
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] UserManagementDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validar contraseña para creación
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                return BadRequest("La contraseña es obligatoria al crear un usuario.");
            }

            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest("La contraseña y la confirmación no coinciden.");
            }

            // Verificar si el email ya existe
            var existingEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null)
            {
                return BadRequest("Ya existe un usuario con este correo electrónico.");
            }

            // Verificar si el documento ya existe
            var existingDocument = await _context.Users
                .FirstOrDefaultAsync(u => u.Document == model.Document);
            if (existingDocument != null)
            {
                return BadRequest("Ya existe un usuario con este número de documento.");
            }

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Document = model.Document,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = model.EmailConfirmed
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest($"Error al crear el usuario: {errors}");
            }

            // Asignar roles
            if (model.Roles != null && model.Roles.Any())
            {
                foreach (var roleName in model.Roles)
                {
                    if (await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _userManager.AddToRoleAsync(user, roleName);
                    }
                }
            }

            // Asignar empresas
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (model.EmpresaIds != null && model.EmpresaIds.Any())
            {
                foreach (var empresaId in model.EmpresaIds)
                {
                    var usuarioEmpresa = new UsuarioEmpresa
                    {
                        UserId = user.Id,
                        EmpresaId = empresaId,
                        FechaAsignacion = FechaCostaRicaHelper.Ahora,
                        AsignadoPorId = currentUserId
                    };
                    _context.UsuariosEmpresas.Add(usuarioEmpresa);
                }
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Usuario creado: {UserId} - {Email} por usuario {CreatedBy}",
                user.Id, user.Email, currentUserId);

            return CreatedAtAction(nameof(GetAsync), new { id = user.Id }, new { id = user.Id, email = user.Email });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el usuario");
            return StatusCode(500, "Error interno del servidor al crear el usuario.");
        }
    }

    /// <summary>
    /// Actualiza un usuario existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(string id, [FromBody] UserManagementDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != model.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID del usuario.");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            // Obtener ID del usuario actual
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verificar permisos si no es SuperUser
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var isSuperUser = userRoles.Contains("SuperUser");

            if (!isSuperUser)
            {
                var empresasUsuario = await _context.UsuariosEmpresas
                    .Where(ue => ue.UserId == currentUserId)
                    .Select(ue => ue.EmpresaId)
                    .ToListAsync();

                var hasAccess = await _context.UsuariosEmpresas
                    .AnyAsync(ue => ue.UserId == id && empresasUsuario.Contains(ue.EmpresaId));

                if (!hasAccess)
                {
                    return Forbid("No tiene permisos para modificar este usuario.");
                }
            }

            // Verificar email único (excepto el actual)
            if (user.Email != model.Email)
            {
                var existingEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingEmail != null)
                {
                    return BadRequest("Ya existe un usuario con este correo electrónico.");
                }
            }

            // Verificar documento único (excepto el actual)
            if (user.Document != model.Document)
            {
                var existingDocument = await _context.Users
                    .FirstOrDefaultAsync(u => u.Document == model.Document && u.Id != id);
                if (existingDocument != null)
                {
                    return BadRequest("Ya existe un usuario con este número de documento.");
                }
            }

            // Actualizar propiedades
            user.Email = model.Email;
            user.UserName = model.Email;
            user.FullName = model.FullName;
            user.Document = model.Document;
            user.PhoneNumber = model.PhoneNumber;
            user.EmailConfirmed = model.EmailConfirmed;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                return BadRequest($"Error al actualizar el usuario: {errors}");
            }

            // Actualizar contraseña si se proporcionó
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (model.Password != model.ConfirmPassword)
                {
                    return BadRequest("La contraseña y la confirmación no coinciden.");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.Password);

                if (!passwordResult.Succeeded)
                {
                    var errors = string.Join(", ", passwordResult.Errors.Select(e => e.Description));
                    return BadRequest($"Error al actualizar la contraseña: {errors}");
                }
            }

            // Actualizar roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.Except(model.Roles ?? new List<string>()).ToList();
            var rolesToAdd = (model.Roles ?? new List<string>()).Except(currentRoles).ToList();

            if (rolesToRemove.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }

            if (rolesToAdd.Any())
            {
                foreach (var roleName in rolesToAdd)
                {
                    if (await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _userManager.AddToRoleAsync(user, roleName);
                    }
                }
            }

            // Actualizar empresas
            var currentEmpresas = await _context.UsuariosEmpresas
                .Where(ue => ue.UserId == id)
                .ToListAsync();

            var empresasToRemove = currentEmpresas
                .Where(ue => !model.EmpresaIds.Contains(ue.EmpresaId))
                .ToList();

            var empresasToAdd = model.EmpresaIds
                .Except(currentEmpresas.Select(ue => ue.EmpresaId))
                .ToList();

            if (empresasToRemove.Any())
            {
                _context.UsuariosEmpresas.RemoveRange(empresasToRemove);
            }

            foreach (var empresaId in empresasToAdd)
            {
                var usuarioEmpresa = new UsuarioEmpresa
                {
                    UserId = user.Id,
                    EmpresaId = empresaId,
                    FechaAsignacion = FechaCostaRicaHelper.Ahora,
                    AsignadoPorId = currentUserId
                };
                _context.UsuariosEmpresas.Add(usuarioEmpresa);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuario actualizado: {UserId} - {Email} por usuario {UpdatedBy}",
                user.Id, user.Email, currentUserId);

            return Ok(new { id = user.Id, email = user.Email });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el usuario {UserId}", id);
            return StatusCode(500, "Error interno del servidor al actualizar el usuario.");
        }
    }

    /// <summary>
    /// Elimina un usuario
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            // Obtener ID del usuario actual
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verificar permisos si no es SuperUser
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var isSuperUser = userRoles.Contains("SuperUser");

            if (!isSuperUser)
            {
                var empresasUsuario = await _context.UsuariosEmpresas
                    .Where(ue => ue.UserId == currentUserId)
                    .Select(ue => ue.EmpresaId)
                    .ToListAsync();

                var hasAccess = await _context.UsuariosEmpresas
                    .AnyAsync(ue => ue.UserId == id && empresasUsuario.Contains(ue.EmpresaId));

                if (!hasAccess)
                {
                    return Forbid("No tiene permisos para eliminar este usuario.");
                }
            }

            // No permitir que un usuario se elimine a sí mismo
            var currentUserIdForCheck = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == currentUserIdForCheck)
            {
                return BadRequest("No puede eliminar su propio usuario.");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest($"Error al eliminar el usuario: {errors}");
            }

            _logger.LogInformation("Usuario eliminado: {UserId} - {Email} por usuario {DeletedBy}",
                id, user.Email, currentUserIdForCheck);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el usuario {UserId}", id);
            return StatusCode(500, "Error interno del servidor al eliminar el usuario.");
        }
    }

    /// <summary>
    /// Asigna un usuario a una empresa
    /// </summary>
    [HttpPost("{userId}/empresas/{empresaId}")]
    public async Task<IActionResult> AssignToEmpresaAsync(string userId, Guid empresaId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            var empresa = await _context.Empresas.FindAsync(empresaId);
            if (empresa == null)
            {
                return NotFound("Empresa no encontrada.");
            }

            // Verificar si ya existe la asignación
            var existing = await _context.UsuariosEmpresas
                .FirstOrDefaultAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);

            if (existing != null)
            {
                return BadRequest("El usuario ya está asignado a esta empresa.");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuarioEmpresa = new UsuarioEmpresa
            {
                UserId = userId,
                EmpresaId = empresaId,
                FechaAsignacion = FechaCostaRicaHelper.Ahora,
                AsignadoPorId = currentUserId
            };

            _context.UsuariosEmpresas.Add(usuarioEmpresa);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuario {UserId} asignado a empresa {EmpresaId} por usuario {AssignedBy}",
                userId, empresaId, currentUserId);

            return Ok(new { message = "Usuario asignado a la empresa exitosamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al asignar usuario {UserId} a empresa {EmpresaId}", userId, empresaId);
            return StatusCode(500, "Error interno del servidor al asignar el usuario a la empresa.");
        }
    }

    /// <summary>
    /// Remueve un usuario de una empresa
    /// </summary>
    [HttpDelete("{userId}/empresas/{empresaId}")]
    public async Task<IActionResult> RemoveFromEmpresaAsync(string userId, Guid empresaId)
    {
        try
        {
            var usuarioEmpresa = await _context.UsuariosEmpresas
                .FirstOrDefaultAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);

            if (usuarioEmpresa == null)
            {
                return NotFound("La asignación no existe.");
            }

            _context.UsuariosEmpresas.Remove(usuarioEmpresa);
            await _context.SaveChangesAsync();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Usuario {UserId} removido de empresa {EmpresaId} por usuario {RemovedBy}",
                userId, empresaId, currentUserId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al remover usuario {UserId} de empresa {EmpresaId}", userId, empresaId);
            return StatusCode(500, "Error interno del servidor al remover el usuario de la empresa.");
        }
    }

    /// <summary>
    /// Actualiza los roles de un usuario
    /// </summary>
    [HttpPut("{userId}/roles")]
    public async Task<IActionResult> UpdateRolesAsync(string userId, [FromBody] UpdateUserRolesDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.Except(model.Roles).ToList();
            var rolesToAdd = model.Roles.Except(currentRoles).ToList();

            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                    return BadRequest($"Error al remover roles: {errors}");
                }
            }

            if (rolesToAdd.Any())
            {
                foreach (var roleName in rolesToAdd)
                {
                    if (await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _userManager.AddToRoleAsync(user, roleName);
                    }
                    else
                    {
                        return BadRequest($"El rol '{roleName}' no existe.");
                    }
                }
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Roles actualizados para usuario {UserId} por usuario {UpdatedBy}",
                userId, currentUserId);

            return Ok(new { message = "Roles actualizados exitosamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar roles del usuario {UserId}", userId);
            return StatusCode(500, "Error interno del servidor al actualizar los roles.");
        }
    }
}
