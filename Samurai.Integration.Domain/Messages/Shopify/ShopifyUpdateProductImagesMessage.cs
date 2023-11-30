using Samurai.Integration.Domain.Models;
using System;

namespace Samurai.Integration.Domain.Messages.Shopify
{
    public class ShopifyUpdateProductImagesMessage
    {
        public long? ShopifyProductId { get; set; }
        public string ExternalProductId { get; set; }
        public ProductImages Images { get; set; }
        public Guid? ReferenceIntegrationId { get; set; }
    }
}
