using System;
using VDC.Integration.Domain.Models;

namespace VDC.Integration.Domain.Messages.Shopify
{
    public class ShopifyUpdateFullProductMessage
    {
        public Product.Info ProductInfo { get; set; }
        public Guid? IntegrationId { get; set; }
    }
}
