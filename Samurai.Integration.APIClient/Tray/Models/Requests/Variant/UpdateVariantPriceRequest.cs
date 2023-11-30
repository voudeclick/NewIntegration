using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.APIClient.Tray.Models.Requests.Variant
{
   public class UpdateVariantPriceRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("cost_price")]
        public double CostPrice { get; set; }

        [JsonProperty("stock")]
        public long? Stock { get; set; }
    }
}
