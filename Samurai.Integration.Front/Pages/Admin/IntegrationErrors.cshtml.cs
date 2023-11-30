using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Samurai.Integration.Front.Pages.Admin
{
    [Authorize(Roles = "Administrador,Suporte,Viewer")]
    public class IntegrationErrorsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
