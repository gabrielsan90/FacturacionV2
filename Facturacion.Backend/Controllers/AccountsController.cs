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

    public AccountsController(
        IUserHelper userHelper,
        UserManager<User> userManager,
        IConfiguration configuration,
        ILogger<AccountsController> logger,
        DataContext context)
    {
        _userHelper = userHelper;
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
        _context = context;
    }

    [HttpPost("ValidateCredentials")]
    public async Task<IActionResult> ValidateCredentialsAsync([FromBody] LoginDto model)
    {
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

            _logger.LogWarning("Failed password validation for user: {Email}. Failed attempts: {FailedCount}/5",
                model.Email, failedCount);

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

        return Ok(new ValidateCredentialsResponseDto
        {
            IsValid = true,
            Empresas = empresas,
            RequiresEmpresaSelection = empresas.Count > 1
        });
    }

    [HttpPost("Login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginDto model)
    {
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

            _logger.LogWarning("Login attempt for locked out user: {Email}. Lockout ends in {Minutes} minutes",
                model.Email, remainingMinutes);

            return BadRequest($"Cuenta bloqueada temporalmente por múltiples intentos fallidos. Intente nuevamente en {remainingMinutes} minuto(s).");
        }

        // Attempt login
        var result = await _userHelper.LoginAsync(model);

        if (result.Succeeded)
        {
            // Login successful - reset failed attempts counter
            await _userHelper.ResetAccessFailedCountAsync(user);

            _logger.LogInformation("Successful login for user: {Email}", model.Email);

            // Use selected empresa if provided, otherwise use default
            return Ok(await BuildLoginResponse(user, model.EmpresaId));
        }

        if (result.IsLockedOut)
        {
            // Account just got locked out
            _logger.LogWarning("Account locked out after failed login attempt: {Email}", model.Email);

            return BadRequest("Cuenta bloqueada temporalmente por múltiples intentos fallidos. Intente nuevamente en 15 minutos.");
        }

        // Failed login - log the attempt
        var failedCount = await _userHelper.GetAccessFailedCountAsync(user);
        var remainingAttempts = 5 - failedCount;

        _logger.LogWarning("Failed login attempt for user: {Email}. Failed attempts: {FailedCount}/5",
            model.Email, failedCount);

        if (remainingAttempts > 0)
        {
            return BadRequest($"Email o contraseña incorrectos. Intentos restantes: {remainingAttempts}");
        }

        return BadRequest("Email o contraseña incorrectos.");
    }

    private async Task<LoginResponseDto> BuildLoginResponse(User user, Guid? selectedEmpresaId = null)
    {
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
