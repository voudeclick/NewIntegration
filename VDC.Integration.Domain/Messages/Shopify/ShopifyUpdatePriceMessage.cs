using System;
using VDC.Integration.Domain.Models;

namespace VDC.Integration.Domain.Messages.Shopify
{
    public class ShopifyUpdatePriceMessage
    {
        public string ExternalProductId { get; set; }
        public Guid? IntegrationId { get; set; }
        public Product.SkuPrice Value { get; set; }
    }
}
