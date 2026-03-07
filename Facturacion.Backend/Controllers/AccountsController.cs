using Facturacion.Backend.Helpers;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Facturacion.Backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace Facturacion.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IUserHelper _userHelper;
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountsController> _logger;
    private readonly DataContext _context;
    private readonly IMemoryCache _cache;

    public AccountsController(
        IUserHelper userHelper,
        UserManager<User> userManager,
        IConfiguration configuration,
        ILogger<AccountsController> logger,
        DataContext context,
        IMemoryCache cache)
    {
        _userHelper = userHelper;
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
        _context = context;
        _cache = cache;
    }

    [HttpPost("ValidateCredentials")]
    public async Task<IActionResult> ValidateCredentialsAsync([FromBody] LoginDto model)
    {
        try
        {
            _logger.LogInformation("Validating credentials for email: {Email}", model.Email);

            // Get user first to check lockout status
            var user = await _userHelper.GetUserByEmailAsync(model.Email);

            if (user == null)
            {
                _logger.LogWarning("Validation attempt with non-existent email: {Email}", model.Email);
                return Ok(new ValidateCredentialsResponseDto
                {
                    IsValid = false,
                    Message = "Email o contraseña incorrectos."
                });
            }

            // Check if user is locked out
            if (await _userHelper.IsLockedOutAsync(user))
            {
                var lockoutEnd = await _userHelper.GetLockoutEndDateAsync(user);
                var remainingMinutes = lockoutEnd.HasValue
                    ? Math.Ceiling((lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes)
                    : 0;

                _logger.LogWarning("Validation attempt for locked out user: {UserId} - {Email}. Lockout ends in {Minutes} minutes",
                    user.Id, model.Email, remainingMinutes);

                return Ok(new ValidateCredentialsResponseDto
                {
                    IsValid = false,
                    Message = $"Cuenta bloqueada temporalmente por múltiples intentos fallidos. Intente nuevamente en {remainingMinutes} minuto(s)."
                });
            }

            // Validate password without signing in
            var isValidPassword = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!isValidPassword)
            {
                // Increment failed attempts
                await _userManager.AccessFailedAsync(user);
                var failedCount = await _userHelper.GetAccessFailedCountAsync(user);
                var remainingAttempts = 5 - failedCount;

                _logger.LogWarning("Failed password validation for user: {UserId} - {Email}. Failed attempts: {FailedCount}/5",
                    user.Id, model.Email, failedCount);

                return Ok(new ValidateCredentialsResponseDto
                {
                    IsValid = false,
                    Message = remainingAttempts > 0
                        ? $"Email o contraseña incorrectos. Intentos restantes: {remainingAttempts}"
                        : "Email o contraseña incorrectos."
                });
            }

            // Password is valid - get user's empresas
            var empresas = await _context.UsuariosEmpresas
                .Include(ue => ue.Empresa)
                .Where(ue => ue.UserId == user.Id && !ue.Empresa!.IsDeleted)
                .OrderBy(ue => ue.Empresa!.NombreComercial)
                .Select(ue => new EmpresaLoginDto
                {
                    Id = ue.EmpresaId,
                    NombreComercial = ue.Empresa!.NombreComercial,
                    CedulaJuridica = ue.Empresa.NumeroIdentificacion
                })
                .ToListAsync();

            // Check user roles - SuperUser can see all empresas
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("SuperUser"))
            {
                _logger.LogDebug("SuperUser {UserId} validated credentials - loading all empresas", user.Id);
                empresas = await _context.Empresas
                    .Where(e => !e.IsDeleted)
                    .OrderBy(e => e.NombreComercial)
                    .Select(e => new EmpresaLoginDto
                    {
                        Id = e.Id,
                        NombreComercial = e.NombreComercial,
                        CedulaJuridica = e.NumeroIdentificacion
                    })
                    .ToListAsync();
            }

            _logger.LogInformation("Successfully validated credentials for user: {UserId} - {Email}. Available empresas: {EmpresaCount}",
                user.Id, model.Email, empresas.Count);

            // Generate pre-auth token to avoid re-hashing password on login
            var preAuthToken = Guid.NewGuid().ToString("N");
            _cache.Set($"preauth:{user.Id}", preAuthToken, TimeSpan.FromSeconds(60));

            return Ok(new ValidateCredentialsResponseDto
            {
                IsValid = true,
                Empresas = empresas,
                RequiresEmpresaSelection = empresas.Count > 1,
                PreAuthToken = preAuthToken,
                IsSuperUser = roles.Contains("SuperUser")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating credentials for email: {Email}", model.Email);
            return StatusCode(500, "Error interno del servidor al validar las credenciales.");
        }
    }

    [HttpPost("Login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginDto model)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", model.Email);

            // Get user first to check lockout status
            var user = await _userHelper.GetUserByEmailAsync(model.Email);

            if (user == null)
            {
                _logger.LogWarning("Login attempt with non-existent email: {Email}", model.Email);
                return BadRequest("Email o contraseña incorrectos.");
            }

            // Check if user is locked out
            if (await _userHelper.IsLockedOutAsync(user))
            {
                var lockoutEnd = await _userHelper.GetLockoutEndDateAsync(user);
                var remainingMinutes = lockoutEnd.HasValue
                    ? Math.Ceiling((lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes)
                    : 0;

                _logger.LogWarning("Login attempt for locked out user: {UserId} - {Email}. Lockout ends in {Minutes} minutes",
                    user.Id, model.Email, remainingMinutes);

                return BadRequest($"Cuenta bloqueada temporalmente por múltiples intentos fallidos. Intente nuevamente en {remainingMinutes} minuto(s).");
            }

            // Check if we have a valid pre-auth token (from ValidateCredentials)
            bool isAuthenticated = false;

            if (!string.IsNullOrEmpty(model.PreAuthToken) &&
                _cache.TryGetValue($"preauth:{user.Id}", out string? cachedToken) &&
                cachedToken == model.PreAuthToken)
            {
                // Pre-auth token valid — skip password re-hash
                isAuthenticated = true;
                _cache.Remove($"preauth:{user.Id}"); // one-time use

                _logger.LogInformation("Login via pre-auth token for user: {UserId} - {Email}",
                    user.Id, model.Email);
            }
            else
            {
                // No pre-auth token — full password check
                var result = await _userHelper.LoginAsync(model);

                if (result.Succeeded)
                {
                    isAuthenticated = true;
                }
                else if (result.IsLockedOut)
                {
                    _logger.LogWarning("Account locked out after failed login attempt: {UserId} - {Email}",
                        user.Id, model.Email);
                    return BadRequest("Cuenta bloqueada temporalmente por múltiples intentos fallidos. Intente nuevamente en 15 minutos.");
                }
                else
                {
                    var failedCount = await _userHelper.GetAccessFailedCountAsync(user);
                    var remainingAttempts = 5 - failedCount;
                    _logger.LogWarning("Failed login attempt for user: {UserId} - {Email}. Failed attempts: {FailedCount}/5",
                        user.Id, model.Email, failedCount);
                    return BadRequest(remainingAttempts > 0
                        ? $"Email o contraseña incorrectos. Intentos restantes: {remainingAttempts}"
                        : "Email o contraseña incorrectos.");
                }
            }

            if (isAuthenticated)
            {
                await _userHelper.ResetAccessFailedCountAsync(user);

                user.UltimaConexion = Helpers.FechaCostaRicaHelper.Ahora;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successful login for user: {UserId} - {Email} with empresa: {EmpresaId}",
                    user.Id, model.Email, model.EmpresaId);

                return Ok(await BuildLoginResponse(user, model.EmpresaId));
            }

            return BadRequest("Email o contraseña incorrectos.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login attempt for email: {Email}", model.Email);
            return StatusCode(500, "Error interno del servidor al procesar el inicio de sesión.");
        }
    }

    private async Task<LoginResponseDto> BuildLoginResponse(User user, Guid? selectedEmpresaId = null)
    {
        try
        {
            _logger.LogDebug("Building login response for user: {UserId} with empresa: {EmpresaId}",
                user.Id, selectedEmpresaId);

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Get empresa based on selection or default to first assigned
            Empresa? selectedEmpresa = null;

            if (selectedEmpresaId.HasValue)
            {
                // SuperUser can select any empresa
                if (roles.Contains("SuperUser"))
                {
                    selectedEmpresa = await _context.Empresas
                        .FirstOrDefaultAsync(e => e.Id == selectedEmpresaId.Value && !e.IsDeleted);
                }
                else
                {
                    // Regular users can only select empresas they're assigned to
                    var usuarioEmpresa = await _context.UsuariosEmpresas
                        .Include(ue => ue.Empresa)
                        .FirstOrDefaultAsync(ue => ue.UserId == user.Id && ue.EmpresaId == selectedEmpresaId.Value);

                    selectedEmpresa = usuarioEmpresa?.Empresa;
                }
            }

            // Fallback to first assigned empresa if no selection or invalid selection
            if (selectedEmpresa == null)
            {
                var defaultUsuarioEmpresa = await _context.UsuariosEmpresas
                    .Include(ue => ue.Empresa)
                    .Where(ue => ue.UserId == user.Id && !ue.Empresa!.IsDeleted)
                    .OrderBy(ue => ue.FechaAsignacion)
                    .FirstOrDefaultAsync();

                selectedEmpresa = defaultUsuarioEmpresa?.Empresa;

                if (selectedEmpresa != null)
                {
                    _logger.LogDebug("Using default empresa {EmpresaId} for user {UserId}",
                        selectedEmpresa.Id, user.Id);
                }
            }

            // Build claims including roles and empresa
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("FullName", user.FullName),
                new Claim("Document", user.Document)
            };

            // Add EmpresaId claim if user has an empresa
            if (selectedEmpresa != null)
            {
                claims.Add(new Claim("EmpresaId", selectedEmpresa.Id.ToString()));
                claims.Add(new Claim("EmpresaNombre", selectedEmpresa.NombreComercial));
            }

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Get expiration hours from configuration (default 8 hours)
            var expirationHours = _configuration.GetValue<int>("Jwt:ExpirationHours", 8);
            var expiration = DateTime.UtcNow.AddHours(expirationHours);

            // Build JWT token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: credentials);

            _logger.LogInformation("JWT token generated for user {UserId} with {RoleCount} roles and empresa {EmpresaId}. Token expires at {ExpiresAt}",
                user.Id, roles.Count, selectedEmpresa?.Id, expiration);

            return new LoginResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FullName = user.FullName,
                    Document = user.Document,
                    Roles = roles.ToList(),
                    EmpresaId = selectedEmpresa?.Id,
                    EmpresaNombre = selectedEmpresa?.NombreComercial
                },
                ExpiresAt = expiration
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building login response for user: {UserId}", user.Id);
            throw;
        }
    }

    [Authorize]
    [HttpGet("user-companies")]
    public async Task<IActionResult> GetUserCompaniesAsync()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Usuario no encontrado");
            }

            // Get user's empresas
            var empresas = await _context.UsuariosEmpresas
                .Include(ue => ue.Empresa)
                .Where(ue => ue.UserId == user.Id && !ue.Empresa!.IsDeleted)
                .OrderBy(ue => ue.Empresa!.NombreComercial)
                .Select(ue => new EmpresaLoginDto
                {
                    Id = ue.EmpresaId,
                    NombreComercial = ue.Empresa!.NombreComercial,
                    CedulaJuridica = ue.Empresa.NumeroIdentificacion
                })
                .ToListAsync();

            // Check user roles - SuperUser can see all empresas
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("SuperUser"))
            {
                empresas = await _context.Empresas
                    .Where(e => !e.IsDeleted)
                    .OrderBy(e => e.NombreComercial)
                    .Select(e => new EmpresaLoginDto
                    {
                        Id = e.Id,
                        NombreComercial = e.NombreComercial,
                        CedulaJuridica = e.NumeroIdentificacion
                    })
                    .ToListAsync();
            }

            return Ok(empresas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user companies");
            return StatusCode(500, "Error al obtener las empresas del usuario");
        }
    }

    [Authorize]
    [HttpPost("switch-company")]
    public async Task<IActionResult> SwitchCompanyAsync([FromBody] SwitchCompanyRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Usuario no encontrado");
            }

            if (!request.EmpresaId.HasValue)
            {
                return BadRequest("EmpresaId es requerido");
            }

            // Verify user has access to this empresa
            var roles = await _userManager.GetRolesAsync(user);
            var hasAccess = false;

            if (roles.Contains("SuperUser"))
            {
                // SuperUser can access any empresa
                hasAccess = await _context.Empresas.AnyAsync(e => e.Id == request.EmpresaId.Value && !e.IsDeleted);
            }
            else
            {
                // Regular users can only access empresas they're assigned to
                hasAccess = await _context.UsuariosEmpresas
                    .AnyAsync(ue => ue.UserId == user.Id && ue.EmpresaId == request.EmpresaId.Value);
            }

            if (!hasAccess)
            {
                return Forbid("No tiene acceso a esta empresa");
            }

            // Generate new token with the selected empresa
            var loginResponse = await BuildLoginResponse(user, request.EmpresaId.Value);

            _logger.LogInformation("Usuario {Email} cambió a empresa {EmpresaId}", user.Email, request.EmpresaId.Value);

            return Ok(loginResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error switching company");
            return StatusCode(500, "Error al cambiar de empresa");
        }
    }

    public class SwitchCompanyRequest
    {
        public Guid? EmpresaId { get; set; }
    }
}
