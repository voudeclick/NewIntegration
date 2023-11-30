using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Tray.Models.Response.Variant
{
    public class GetVariantsByFilterResponse : BaseResponse
    {
        public List<Samurai.Integration.APIClient.Tray.Models.Response.Inputs.Variant> Variants { get; set; }
    }
}
