using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VDC.Integration.Front.Pages.Admin
{
    [Authorize(Roles = "Administrador,Suporte,Viewer")]
    public class IntegrationErrorsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
