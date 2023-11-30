using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.APIClient.Tray.Models.Response
{
    public class BaseResponse
    {
        [JsonProperty("paging")]
        public Paging Paging { get; set; }

        [JsonProperty("sort")]
        public List<Sort> Sort { get; set; }

    }
    public class Paging
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; }

        [JsonProperty("maxlimit")]
        public int MaxLimit { get; set; }
    }
    public class Sort
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

}
