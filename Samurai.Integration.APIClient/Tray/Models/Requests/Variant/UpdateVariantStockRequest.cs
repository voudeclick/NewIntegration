using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.APIClient.Tray.Models.Requests.Variant
{
    public class UpdateVariantStockRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("stock")]
        public long Stock { get; set; }
    }
}
