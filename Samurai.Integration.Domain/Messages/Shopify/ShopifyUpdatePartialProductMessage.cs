using Samurai.Integration.Domain.Models;
using System;

namespace Samurai.Integration.Domain.Messages.Shopify
{
    public class ShopifyUpdatePartialProductMessage : BaseProduct
    {
        public override Product.Info ProductInfo { get; set; }
        public Guid? IntegrationId { get; set; }
    }
}
