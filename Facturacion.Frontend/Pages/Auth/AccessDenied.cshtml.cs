using Facturacion.Frontend.Services;
using Facturacion.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Facturacion.Frontend.Pages.Auth;

[AllowAnonymous]
public class AccessDeniedModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly ILogger<AccessDeniedModel> _logger;

    public AccessDeniedModel(IAuthService authService, ILogger<AccessDeniedModel> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public UserDto? CurrentUser { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            // Get current user information if authenticated
            CurrentUser = await _authService.GetCurrentUserAsync();

            if (CurrentUser != null)
            {
                _logger.LogWarning(
                    "Acceso denegado para el usuario {Email} con roles: {Roles}",
                    CurrentUser.Email,
                    string.Join(", ", CurrentUser.Roles)
                );
            }
            else
            {
                _logger.LogWarning("Acceso denegado para usuario no autenticado");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener información del usuario en AccessDenied");
        }
    }
}
