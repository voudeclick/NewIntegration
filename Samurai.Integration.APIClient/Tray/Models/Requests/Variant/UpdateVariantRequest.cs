using Newtonsoft.Json;
using Samurai.Integration.APIClient.Tray.Models.Requests.Inputs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.APIClient.Tray.Models.Requests.Variation
{
    public class UpdateVariantRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public ProductVariation ProductVariation { get; set; }
    }
}
