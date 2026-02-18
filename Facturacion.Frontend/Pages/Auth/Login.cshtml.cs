using Facturacion.Frontend.Services;
using Facturacion.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Auth;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LoginModel> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public LoginModel(IAuthService authService, IHttpClientFactory httpClientFactory, ILogger<LoginModel> logger)
    {
        _authService = authService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos {2} caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Empresa")]
        public Guid? EmpresaId { get; set; }

        public string? PreAuthToken { get; set; }
    }

    public void OnGet(string? returnUrl = null)
    {
        // If user is already authenticated, redirect to home
        if (_authService.IsAuthenticated())
        {
            Response.Redirect("/Index");
            return;
        }

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostValidateCredentialsAsync([FromBody] LoginDto model)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var response = await client.PostAsJsonAsync("/api/accounts/ValidateCredentials", model);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ValidateCredentialsResponseDto>(_jsonOptions);
                return new JsonResult(result);
            }

            return new JsonResult(new ValidateCredentialsResponseDto
            {
                IsValid = false,
                Message = "Error al validar las credenciales"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating credentials for {Email}", model.Email);
            return new JsonResult(new ValidateCredentialsResponseDto
            {
                IsValid = false,
                Message = "Error al conectar con el servidor"
            });
        }
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        // If user is already authenticated, redirect to home
        if (_authService.IsAuthenticated())
        {
            return LocalRedirect(returnUrl);
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var loginDto = new LoginDto
            {
                Email = Input.Email.Trim(),
                Password = Input.Password,
                EmpresaId = Input.EmpresaId,
                PreAuthToken = Input.PreAuthToken
            };

            var result = await _authService.LoginAsync(loginDto);

            if (result.WasSuccess)
            {
                _logger.LogInformation("Usuario {Email} inició sesión exitosamente", Input.Email);

                // Redirect to return URL or home page
                return LocalRedirect(returnUrl);
            }
            else
            {
                _logger.LogWarning("Intento de inicio de sesión fallido para {Email}: {Message}", Input.Email, result.Message);

                // Show actual error message from backend
                TempData["ErrorMessage"] = result.Message ?? "Correo electrónico o contraseña incorrectos. " + result.Message;

                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el proceso de inicio de sesión para {Email}", Input.Email);

            TempData["ErrorMessage"] = "Ocurrió un error al procesar su solicitud. Por favor, intente nuevamente más tarde.";

            return Page();
        }
    }
}
