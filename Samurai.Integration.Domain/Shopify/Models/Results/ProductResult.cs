using System.Collections.Generic;

namespace Samurai.Integration.Domain.Shopify.Models.Results
{
    public class ProductResult
    {
        public string id { get; set; }
        public string legacyResourceId { get; set; }
        public string handle { get; set; }
        public string title { get; set; }
        public List<string> tags { get; set; }
        public string onlineStoreUrl { get; set; }
        public List<OptionResult> options { get; set; }
        public Connection<MetafieldResult> metafields { get; set; }
        public Connection<VariantResult> variants { get; set; }
    }
}
