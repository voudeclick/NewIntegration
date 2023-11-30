using System.Collections.Generic;

namespace Samurai.Integration.Domain.Shopify.Models.Results
{
    public class VariantResult
    {
        public string id { get; set; }
        public string legacyResourceId { get; set; }
        public string sku { get; set; }
        public decimal? price { get; set; }
        public decimal? compareAtPrice { get; set; }
        public List<OptionResult> selectedOptions { get; set; }
        public Connection<MetafieldResult> metafields { get; set; }
        public InventoryItemResult inventoryItem { get; set; }
        public ProductResult product { get; set; }
    }
}
