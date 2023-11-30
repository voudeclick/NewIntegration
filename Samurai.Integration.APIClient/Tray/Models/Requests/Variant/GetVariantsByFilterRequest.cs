using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Tray.Models.Requests.Variation
{
    public class GetVariantsByFilterRequest
    {
        public int Id { get; set; }

        [JsonProperty("product_id")]
        public long ProductId { get; set; }

        public string Ean { get; set; }
        public string Reference { get; set; }
        public int Limit { get => 50; }
        public int Page { get; set; } = 1;
    }
}
