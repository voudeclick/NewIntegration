using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.Shopify
{
    public class ShopifyEnqueueListOrderMessage
    {
        public List<long> OrderIds { get; set; }
    }
}