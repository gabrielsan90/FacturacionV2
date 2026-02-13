using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Facturacion.Frontend.Pages.Presupuestos;

[Authorize]
public class EjecucionPresupuestalModel : PageModel
{
    public void OnGet()
    {
    }
}
