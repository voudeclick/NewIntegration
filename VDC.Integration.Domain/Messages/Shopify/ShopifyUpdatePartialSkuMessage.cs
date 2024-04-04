using VDC.Integration.Domain.Models;

namespace VDC.Integration.Domain.Messages.Shopify
{
    public class ShopifyUpdatePartialSkuMessage
    {
        public string ExternalProductId { get; set; }
        public Product.SkuInfo SkuInfo { get; set; }
    }
}
