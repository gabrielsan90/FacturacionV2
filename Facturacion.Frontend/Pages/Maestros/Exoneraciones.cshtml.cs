using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Maestros;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado,Contador,Facturador")]
public class ExoneracionesModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExoneracionesModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public void OnGet()
    {
        // Page load - no initial data needed since user searches manually
    }

    /// <summary>
    /// Handler to validate an exemption document
    /// POST: ?handler=Validar
    /// </summary>
    public async Task<IActionResult> OnPostValidarAsync([FromBody] ExoneracionValidacionRequest request)
    {
        if (request == null ||
            string.IsNullOrWhiteSpace(request.NumeroDocumento) ||
            string.IsNullOrWhiteSpace(request.NombreInstitucion))
        {
            return new JsonResult(new
            {
                success = false,
                message = "El número de documento y el nombre de la institución son obligatorios"
            });
        }

        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            // Get JWT token from user claims
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // Build request body
            var validacionData = new
            {
                numeroDocumento = request.NumeroDocumento,
                nombreInstitucion = request.NombreInstitucion,
                fechaValidacion = request.FechaValidacion
            };

            var json = JsonSerializer.Serialize(validacionData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/exoneraciones/validar", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var validacionResponse = JsonSerializer.Deserialize<object>(responseContent, _jsonOptions);

                return new JsonResult(new
                {
                    success = true,
                    data = validacionResponse
                });
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                var error = await response.Content.ReadAsStringAsync();
                return new JsonResult(new
                {
                    success = false,
                    message = "El servicio de Hacienda no está disponible en este momento. Por favor, intente más tarde."
                });
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return new JsonResult(new
                {
                    success = false,
                    message = $"Error al validar la exoneración: {error}"
                });
            }
        }
        catch (HttpRequestException ex)
        {
            return new JsonResult(new
            {
                success = false,
                message = "Error de conexión al consultar el servicio de exoneraciones. Verifique su conexión a internet."
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new
            {
                success = false,
                message = $"Error inesperado: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Handler to get exemptions by beneficiary identification
    /// GET: ?handler=PorBeneficiario&identificacion=xxx
    /// </summary>
    public async Task<IActionResult> OnGetPorBeneficiarioAsync(string identificacion)
    {
        if (string.IsNullOrWhiteSpace(identificacion))
        {
            return new JsonResult(new
            {
                success = false,
                message = "La identificación del beneficiario es obligatoria"
            });
        }

        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            // Get JWT token from user claims
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/exoneraciones/beneficiario/{Uri.EscapeDataString(identificacion)}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var exoneracionesResponse = JsonSerializer.Deserialize<object>(responseContent, _jsonOptions);

                return new JsonResult(new
                {
                    success = true,
                    data = exoneracionesResponse
                });
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "El servicio de Hacienda no está disponible en este momento. Por favor, intente más tarde."
                });
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new JsonResult(new
                {
                    success = true,
                    data = new
                    {
                        identificacion,
                        total = 0,
                        exoneraciones = new List<object>()
                    }
                });
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return new JsonResult(new
                {
                    success = false,
                    message = $"Error al consultar exoneraciones: {error}"
                });
            }
        }
        catch (HttpRequestException ex)
        {
            return new JsonResult(new
            {
                success = false,
                message = "Error de conexión al consultar el servicio de exoneraciones. Verifique su conexión a internet."
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new
            {
                success = false,
                message = $"Error inesperado: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Handler to clear the exemptions cache
    /// POST: ?handler=LimpiarCache
    /// </summary>
    [Authorize(Roles = "SuperUser,Administrador de Empresa")]
    public async Task<IActionResult> OnPostLimpiarCacheAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            // Get JWT token from user claims
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.PostAsync("/api/exoneraciones/limpiar-cache", null);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<object>(responseContent, _jsonOptions);

                return new JsonResult(new
                {
                    success = true,
                    message = "Caché de exoneraciones limpiada exitosamente"
                });
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return new JsonResult(new
                {
                    success = false,
                    message = $"Error al limpiar caché: {error}"
                });
            }
        }
        catch (Exception ex)
        {
            return new JsonResult(new
            {
                success = false,
                message = $"Error inesperado: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Request model for exemption validation
    /// </summary>
    public class ExoneracionValidacionRequest
    {
        public string NumeroDocumento { get; set; } = null!;
        public string NombreInstitucion { get; set; } = null!;
        public DateTime? FechaValidacion { get; set; }
    }
}
