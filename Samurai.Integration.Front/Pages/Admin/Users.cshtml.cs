using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Samurai.Integration.Identity.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Samurai.Integration.Front.Pages.Admin
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
