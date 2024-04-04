using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VDC.Integration.Front.Pages.Admin
{
    [Authorize(Roles = "Administrador")]
    public class UsersModel : PageModel
    {
        public IActionResult OnGetAsync()
        {
            return Page();
        }
    }
}
