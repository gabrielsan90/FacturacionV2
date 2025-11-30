using Facturacion.Frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Facturacion.Frontend.Pages.Auth;

[Authorize]
public class LogoutModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(IAuthService authService, ILogger<LogoutModel> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var user = await _authService.GetCurrentUserAsync();
            var userEmail = user?.Email ?? "Unknown";

            await _authService.LogoutAsync();

            _logger.LogInformation("Usuario {Email} cerró sesión exitosamente", userEmail);

            TempData["SuccessMessage"] = "Ha cerrado sesión exitosamente.";

            return RedirectToPage("/Auth/Login");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el cierre de sesión");

            TempData["ErrorMessage"] = "Ocurrió un error al cerrar sesión.";

            return RedirectToPage("/Auth/Login");
        }
    }

    public async Task<IActionResult> OnPost()
    {
        return await OnGet();
    }
}
