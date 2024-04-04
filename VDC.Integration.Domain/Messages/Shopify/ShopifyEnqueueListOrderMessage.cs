using System.Collections.Generic;

namespace VDC.Integration.Domain.Messages.Shopify
{
    public class ShopifyEnqueueListOrderMessage
    {
        public List<long> OrderIds { get; set; }
    }
}