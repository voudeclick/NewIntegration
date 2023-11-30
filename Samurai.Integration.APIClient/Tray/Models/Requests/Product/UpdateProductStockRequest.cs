using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.APIClient.Tray.Models.Requests.Product
{
    public class UpdateProductStockRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("stock")]
        public long Stock { get; set; }

        [JsonPropertyName("available")]
        public int? Available { get; set; }
    }
}
