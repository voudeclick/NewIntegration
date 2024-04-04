using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VDC.Integration.Front.Pages.Admin
{
    [Authorize(Roles = "Administrador,Suporte,Viewer")]
    public class TenantModel : PageModel
    {
        public long? ID { get; set; }
        public void OnGet(long? id)
        {
            ID = id;
        }
    }
}