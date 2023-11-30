using Samurai.Integration.Domain.Models;
using System;

namespace Samurai.Integration.Domain.Messages.Shopify
{
    public class ShopifyUpdatePartialProductKitMessage : BaseProduct
    {
        public override Product.Info ProductInfo { get; set; }
        public Guid? IntegrationId { get; set; }
    }
}
