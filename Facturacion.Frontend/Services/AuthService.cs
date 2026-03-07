using Facturacion.Frontend.Helpers;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Responses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Facturacion.Frontend.Services;

public class AuthService : IAuthService
{
    private readonly IApiService _apiService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IApiService apiService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthService> logger)
    {
        _apiService = apiService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<ActionResponse<bool>> LoginAsync(LoginDto model)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                _logger.LogError("HttpContext is null in LoginAsync");
                return new ActionResponse<bool> { WasSuccess = false, Message = "HttpContext is null" };
            }

            // Log the login attempt details
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LOGIN ATTEMPT - Email: {model.Email}, EmpresaId: {model.EmpresaId}, PasswordLength: {model.Password?.Length ?? 0}";
            Console.WriteLine(logMessage);
            try { File.AppendAllText("login_debug.log", logMessage + Environment.NewLine); } catch { }

            // Call backend API to login
            var response = await _apiService.PostAsync<LoginDto, LoginResponseDto>("/api/accounts/login", model);

            var responseLog = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LOGIN RESPONSE - WasSuccess: {response.WasSuccess}, Message: {response.Message}, Result: {(response.Result != null ? "OK" : "NULL")}";
            Console.WriteLine(responseLog);
            try { File.AppendAllText("login_debug.log", responseLog + Environment.NewLine); } catch { }

            if (!response.WasSuccess || response.Result == null)
            {
                _logger.LogWarning("Login failed for user {Email}: {Message}", model.Email, response.Message);
                var failLog = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LOGIN FAILED - {response.Message}";
                Console.WriteLine(failLog);
                try { File.AppendAllText("login_debug.log", failLog + Environment.NewLine); } catch { }
                return new ActionResponse<bool> { WasSuccess = false, Message = response.Message ?? "Login failed" };
            }

            var loginResponse = response.Result;

            // Create claims for cookie authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, loginResponse.User.Id),
                new Claim(ClaimTypes.Name, loginResponse.User.Email),
                new Claim(ClaimTypes.Email, loginResponse.User.Email),
                new Claim("FullName", loginResponse.User.FullName),
                new Claim("Document", loginResponse.User.Document),
                new Claim("Token", loginResponse.Token),
                new Claim("TokenExpiration", loginResponse.ExpiresAt.ToString("o"))
            };

            // Add EmpresaId claim if user has an empresa
            if (loginResponse.User.EmpresaId.HasValue)
            {
                claims.Add(new Claim("EmpresaId", loginResponse.User.EmpresaId.Value.ToString()));
            }

            if (!string.IsNullOrWhiteSpace(loginResponse.User.EmpresaNombre))
            {
                claims.Add(new Claim("EmpresaNombre", loginResponse.User.EmpresaNombre));
            }

            // Add role claims
            foreach (var role in loginResponse.User.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = loginResponse.ExpiresAt,
                AllowRefresh = false
            };

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Set authToken cookie for JavaScript access (e.g., DataTables Ajax calls)
            httpContext.Response.Cookies.Append("authToken", loginResponse.Token, new CookieOptions
            {
                HttpOnly = true, // Prevent XSS token theft
                Secure = true, // Only send over HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = loginResponse.ExpiresAt
            });

            _logger.LogInformation("User {Email} logged in successfully", model.Email);

            return new ActionResponse<bool> { WasSuccess = true, Result = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Email}", model.Email);
            var exLog = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LOGIN EXCEPTION - Type: {ex.GetType().Name}, Message: {ex.Message}, Inner: {ex.InnerException?.Message ?? "none"}";
            Console.WriteLine(exLog);
            try { File.AppendAllText("login_debug.log", exLog + Environment.NewLine); } catch { }
            return new ActionResponse<bool> { WasSuccess = false, Message = $"Exception: {ex.Message}" };
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                _logger.LogError("HttpContext is null in LogoutAsync");
                return;
            }

            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Remove authToken cookie
            httpContext.Response.Cookies.Delete("authToken");

            _logger.LogInformation("User logged out successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
        }
    }

    public Task<UserDto?> GetCurrentUserAsync()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.User?.Identity?.IsAuthenticated != true)
            {
                return Task.FromResult<UserDto?>(null);
            }

            var user = httpContext.User;

            var empresaIdClaim = user.FindFirst("EmpresaId")?.Value;
            Guid? empresaId = null;
            if (!string.IsNullOrWhiteSpace(empresaIdClaim) && Guid.TryParse(empresaIdClaim, out var parsedEmpresaId))
            {
                empresaId = parsedEmpresaId;
            }

            var userDto = new UserDto
            {
                Id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
                Email = user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
                FullName = user.FindFirst("FullName")?.Value ?? string.Empty,
                Document = user.FindFirst("Document")?.Value ?? string.Empty,
                Roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
                EmpresaId = empresaId,
                EmpresaNombre = user.FindFirst("EmpresaNombre")?.Value
            };

            return Task.FromResult<UserDto?>(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return Task.FromResult<UserDto?>(null);
        }
    }

    public bool IsAuthenticated()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.User?.Identity?.IsAuthenticated ?? false;
    }

    public async Task UpdateAuthenticationAsync(LoginResponseDto loginResponse)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                _logger.LogError("HttpContext is null in UpdateAuthenticationAsync");
                return;
            }

            // Create claims for cookie authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, loginResponse.User.Id),
                new Claim(ClaimTypes.Name, loginResponse.User.Email),
                new Claim(ClaimTypes.Email, loginResponse.User.Email),
                new Claim("FullName", loginResponse.User.FullName),
                new Claim("Document", loginResponse.User.Document),
                new Claim("Token", loginResponse.Token),
                new Claim("TokenExpiration", loginResponse.ExpiresAt.ToString("o"))
            };

            // Add EmpresaId claim if user has an empresa
            if (loginResponse.User.EmpresaId.HasValue)
            {
                claims.Add(new Claim("EmpresaId", loginResponse.User.EmpresaId.Value.ToString()));
            }

            if (!string.IsNullOrWhiteSpace(loginResponse.User.EmpresaNombre))
            {
                claims.Add(new Claim("EmpresaNombre", loginResponse.User.EmpresaNombre));
            }

            // Add role claims
            foreach (var role in loginResponse.User.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = loginResponse.ExpiresAt,
                AllowRefresh = false
            };

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Update authToken cookie
            httpContext.Response.Cookies.Append("authToken", loginResponse.Token, new CookieOptions
            {
                HttpOnly = true, // Prevent XSS token theft
                Secure = true, // Only send over HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = loginResponse.ExpiresAt
            });

            _logger.LogInformation("Authentication updated successfully for user {Email}", loginResponse.User.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating authentication");
        }
    }
}
