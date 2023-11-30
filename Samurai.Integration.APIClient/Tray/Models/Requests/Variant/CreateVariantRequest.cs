using Newtonsoft.Json;
using Samurai.Integration.APIClient.Tray.Models.Requests.Inputs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Tray.Models.Requests.Variation
{
    public class CreateVariantRequest
    {
        public ProductVariation ProductVariation { get; set; }
    }
}
