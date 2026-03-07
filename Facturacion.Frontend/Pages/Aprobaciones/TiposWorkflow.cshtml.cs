using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Aprobaciones;

[Authorize]
public class TiposWorkflowModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions;

    public TiposWorkflowModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetDataAsync()
    {
        try
        {
            var client = CreateApiClient();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var response = await client.GetAsync($"api/tiposworkflow/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                // Unwrap ActionResponse if needed
                string dataJson;
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    dataJson = content;
                }
                else if (doc.RootElement.TryGetProperty("result", out var resultProp))
                {
                    dataJson = resultProp.GetRawText();
                }
                else
                {
                    dataJson = "[]";
                }

                return new ContentResult
                {
                    Content = $"{{\"data\":{dataJson}}}",
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }

            return new JsonResult(new { data = Array.Empty<object>() });
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnPostSaveAsync([FromBody] TipoWorkflowDto tipo)
    {
        try
        {
            var client = CreateApiClient();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var tipoEntity = new TipoWorkflow
            {
                Id = tipo.Id == Guid.Empty ? Guid.NewGuid() : tipo.Id,
                EmpresaId = empresaId.Value,
                Codigo = tipo.Codigo,
                Nombre = tipo.Nombre,
                Descripcion = tipo.Descripcion,
                RequiereAprobacion = tipo.RequiereAprobacion,
                MontoMinimoAprobacion = tipo.MontoMinimoAprobacion,
                MontoMaximoSinAprobacion = tipo.MontoMaximoSinAprobacion,
                NotificarEmail = tipo.NotificarEmail,
                NotificarSistema = tipo.NotificarSistema,
                DiasEscalamiento = tipo.DiasEscalamiento,
                EscalamientoAutomatico = tipo.EscalamientoAutomatico,
                Activo = tipo.Activo
            };

            var json = JsonSerializer.Serialize(tipoEntity, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            if (tipo.Id == Guid.Empty)
            {
                response = await client.PostAsync("api/tiposworkflow", content);
            }
            else
            {
                response = await client.PutAsync($"api/tiposworkflow/{tipo.Id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true });
            }

            var error = await response.Content.ReadAsStringAsync();
            return BadRequest(error);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        try
        {
            var client = CreateApiClient();

            var response = await client.DeleteAsync($"api/tiposworkflow/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true });
            }

            var error = await response.Content.ReadAsStringAsync();
            return BadRequest(error);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    private HttpClient CreateApiClient()
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    private Guid? GetEmpresaId()
    {
        var empresaIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
        if (string.IsNullOrEmpty(empresaIdClaim)) return null;
        return Guid.Parse(empresaIdClaim);
    }

    public class TipoWorkflowDto
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool RequiereAprobacion { get; set; }
        public decimal? MontoMinimoAprobacion { get; set; }
        public decimal? MontoMaximoSinAprobacion { get; set; }
        public bool NotificarEmail { get; set; }
        public bool NotificarSistema { get; set; }
        public int? DiasEscalamiento { get; set; }
        public bool EscalamientoAutomatico { get; set; }
        public bool Activo { get; set; }
    }
}
