﻿using Samurai.Integration.Domain.Models;

namespace Samurai.Integration.Domain.Messages.Shopify
{
    public class ShopifyUpdatePartialSkuMessage
    {
        public string ExternalProductId { get; set; }
        public Product.SkuInfo SkuInfo { get; set; }
    }
}
