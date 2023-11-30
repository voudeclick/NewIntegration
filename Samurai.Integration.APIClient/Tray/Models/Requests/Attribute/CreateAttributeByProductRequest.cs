using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Tray.Models.Requests.Attribute
{
    public class CreateAttributeByProductRequest
    {
        public int ProductId { get; set; }
        [JsonProperty("property_id")]
        public string PropertyId { get; set; }

        public string Value { get; set; }
    }
}
