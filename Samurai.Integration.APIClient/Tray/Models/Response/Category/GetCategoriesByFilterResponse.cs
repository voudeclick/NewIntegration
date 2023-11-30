using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Tray.Models.Response.Category
{
    public class GetCategoriesByFilterResponse : BaseResponse
    {
        public List<Samurai.Integration.APIClient.Tray.Models.Response.Inputs.Category> Categories { get; set; }
    }
}
