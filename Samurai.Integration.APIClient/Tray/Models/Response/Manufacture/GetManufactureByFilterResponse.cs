using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Tray.Models.Response.Manufacture
{
    public class GetManufactureByFilterResponse : BaseResponse
    {
        public List<Samurai.Integration.APIClient.Tray.Models.Response.Inputs.Manufacture> Brands { get; set; }
    }
}
