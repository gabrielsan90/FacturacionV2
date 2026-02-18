using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Presupuestos;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Contador")]
public class EjecucionPresupuestalModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EjecucionPresupuestalModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public void OnGet()
    {
    }

    // Lista de presupuestos para el combo selector
    public async Task<IActionResult> OnGetPresupuestosAsync()
    {
        try
        {
            var client = CreateApiClient();
            var empresaId = GetEmpresaId();
            if (empresaId == null)
            {
                return new JsonResult(new List<object>());
            }

            var response = await client.GetAsync($"/api/presupuestos/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return new ContentResult { Content = content, ContentType = "application/json", StatusCode = 200 };
            }

            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            return new JsonResult(new List<object>());
        }
    }

    // Detalle completo de un presupuesto con líneas para el dashboard
    public async Task<IActionResult> OnGetEjecucionAsync(Guid id)
    {
        try
        {
            var client = CreateApiClient();
            var response = await client.GetAsync($"/api/presupuestos/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return new ContentResult { Content = content, ContentType = "application/json", StatusCode = 200 };
            }

            return new JsonResult(new { success = false, message = "Presupuesto no encontrado" });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = "Error al obtener la ejecución presupuestal" });
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
        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrEmpty(empresaId) || !Guid.TryParse(empresaId, out var guid))
            return null;
        return guid;
    }
}
