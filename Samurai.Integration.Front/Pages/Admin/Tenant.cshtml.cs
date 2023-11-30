using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Samurai.Integration.Front
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