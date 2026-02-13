using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Facturacion.Frontend.Pages.Aprobaciones;

[Authorize]
public class MisSolicitudesModel : PageModel
{
    public void OnGet()
    {
    }
}
