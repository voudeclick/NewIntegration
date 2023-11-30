using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.APIClient.Tray.Models.Requests.Product
{
    public class UpdateProductPriceRequest
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("price")]
        public double Price { get; set; }

        [JsonPropertyName("cost_price")]
        public double CostPrice { get; set; }

        [JsonPropertyName("availability")]
        public string Availability { get; set; }

        [JsonPropertyName("availability_days")]
        public int AvailabilityDays { get; set; }

        [JsonPropertyName("stock")]
        public long? Stock { get; set; }

        [JsonPropertyName("available")]
        public int? Available { get; set; }
    }
}
