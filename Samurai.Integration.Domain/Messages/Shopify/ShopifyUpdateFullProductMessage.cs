using Samurai.Integration.Domain.Models;
using System;

namespace Samurai.Integration.Domain.Messages.Shopify
{
    public class ShopifyUpdateFullProductMessage
    {
        public Product.Info ProductInfo { get; set; }
        public Guid? IntegrationId { get; set; }
    }
}
