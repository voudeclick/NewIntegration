using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Samurai.Integration.Front
{
    [Authorize(Roles = "Administrador,Suporte,Viewer")]
    public class LogsModel : PageModel
    {
        public void OnGet()
        {

        }
    }
}