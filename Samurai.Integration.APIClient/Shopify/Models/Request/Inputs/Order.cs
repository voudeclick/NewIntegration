using System.Collections.Generic;

namespace Samurai.Integration.APIClient.Shopify.Models.Request.Inputs
{
    public class Order
    {
        public string id { get; set; }
        public List<string> tags { get; set; }
    }
}
