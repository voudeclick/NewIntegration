using System.Collections.Generic;
using VDC.Integration.Domain.Models;

namespace VDC.Integration.Domain.Messages.Shopify
{
    public class ShopifyEnqueueUpdatePartialProductMessage
    {
        public List<Product.Info> ProductInfos { get; set; }
    }
}
