using System;
using VDC.Integration.Domain.Models;

namespace VDC.Integration.Domain.Messages.Shopify
{
    public class ShopifyUpdatePartialProductMessage : BaseProduct
    {
        public override Product.Info ProductInfo { get; set; }
        public Guid? IntegrationId { get; set; }
    }
}
