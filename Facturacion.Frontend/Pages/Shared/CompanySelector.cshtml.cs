using Facturacion.Frontend.Services;
using Facturacion.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Shared;

[Authorize]
public class CompanySelectorModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthService _authService;
    private readonly ILogger<CompanySelectorModel> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public CompanySelectorModel(
        IHttpClientFactory httpClientFactory,
        IAuthService authService,
        ILogger<CompanySelectorModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Get user's available companies
    /// </summary>
    public async Task<IActionResult> OnGetCompaniesAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            // Get JWT from claims
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/accounts/user-companies");

            if (response.IsSuccessStatusCode)
            {
                var companies = await response.Content.ReadFromJsonAsync<List<EmpresaLoginDto>>(_jsonOptions);

                // Get current empresa from claims
                var currentEmpresaId = User.FindFirst("EmpresaId")?.Value;

                return new JsonResult(new
                {
                    companies = companies ?? new List<EmpresaLoginDto>(),
                    currentEmpresaId = currentEmpresaId
                });
            }

            _logger.LogWarning("User companies API returned status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { companies = new List<EmpresaLoginDto>(), currentEmpresaId = string.Empty });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user companies");
            return new JsonResult(new { companies = new List<EmpresaLoginDto>(), currentEmpresaId = string.Empty });
        }
    }

    /// <summary>
    /// Switch to a different company
    /// </summary>
    public async Task<IActionResult> OnPostSwitchCompanyAsync([FromBody] SwitchCompanyRequest request)
    {
        try
        {
            if (!request.EmpresaId.HasValue)
            {
                return new JsonResult(new { success = false, message = "EmpresaId es requerido" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.PostAsJsonAsync("/api/accounts/switch-company", new { empresaId = request.EmpresaId });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>(_jsonOptions);

                if (result != null)
                {
                    // Logout current session
                    await _authService.LogoutAsync();

                    // Re-login with new token (simulating a login with the new empresa)
                    var loginDto = new LoginDto
                    {
                        Email = result.User.Email,
                        Password = string.Empty, // Not needed for token-based switch
                        EmpresaId = request.EmpresaId
                    };

                    // Manually set the authentication using the new token
                    await _authService.UpdateAuthenticationAsync(result);

                    _logger.LogInformation("Usuario cambió a empresa {EmpresaId}", request.EmpresaId);

                    return new JsonResult(new
                    {
                        success = true,
                        message = "Empresa cambiada exitosamente",
                        empresaNombre = result.User.EmpresaNombre
                    });
                }
            }

            _logger.LogWarning("Switch company API returned status code: {StatusCode}", response.StatusCode);
            var errorMessage = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = $"Error al cambiar de empresa: {errorMessage}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error switching company");
            return new JsonResult(new { success = false, message = "Error al cambiar de empresa" });
        }
    }

    public class SwitchCompanyRequest
    {
        public Guid? EmpresaId { get; set; }
    }
}
