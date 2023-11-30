using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.AliExpress.Models.Request
{
    public class AliExpressListProductRequest
    {
        [JsonProperty("product_id")]
        public long ProductId { get; set; }

        [JsonProperty("local_country")]
        public string LocalCountry { get; set; }

        [JsonProperty("local_language")]
        public string LocalLanguage { get; set; }
    }
}
