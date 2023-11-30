using Samurai.Integration.Domain.Models;
using System.Collections.Generic;

namespace Samurai.Integration.Domain.Messages.Shopify
{
    public class ShopifyEnqueueUpdatePartialProductMessage
    {
        public List<Product.Info> ProductInfos { get; set; }
    }
}
