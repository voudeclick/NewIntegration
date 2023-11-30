using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.APIClient.Tray.Models.Response.Inputs
{
    public class Manufacture
    {
        public ManufactureModel Brand { get; set; }
    }

    public class ManufactureModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("brand")]
        public string Brand { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }
    }
}
