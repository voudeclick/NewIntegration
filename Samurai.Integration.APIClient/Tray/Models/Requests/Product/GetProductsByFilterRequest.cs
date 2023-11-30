using Akka.Routing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Tray.Models.Requests.Product
{
    public class GetProductsByFilterRequest
    {
        public long Id { get; set; }
        public string Name { get; set; }

        [JsonProperty("category_id")]
        public string CategoryId { get; set; }

        public string Ean { get; set; }
        public string Brand { get; set; }
        public string Reference { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public int Limit { get => 50; }
        public int Page { get => 1; }
    }
}
