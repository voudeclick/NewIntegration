using Samurai.Integration.APIClient.Converters;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Samurai.Integration.WebApi.ViewModels.Shopify
{
    public class ShopifyLocation
    {
        public List<Item> locations { get; set; }

        public class Item 
        {
            [JsonConverter(typeof(StringConverter))]
            public string id { get; set; }
            public string name { get; set; }
            public string address1 { get; set; }
        }

    }

}
