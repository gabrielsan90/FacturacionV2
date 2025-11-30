using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Facturacion.Frontend.Pages;

[Authorize]
public class EditarDocumentoModel : PageModel
{
    private readonly IConfiguration _configuration;

    public EditarDocumentoModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string ApiUrl => _configuration["ApiUrl"] ?? "";
    public string DocumentoId { get; set; } = "";

    public void OnGet(string id)
    {
        DocumentoId = id ?? "";
    }
}
