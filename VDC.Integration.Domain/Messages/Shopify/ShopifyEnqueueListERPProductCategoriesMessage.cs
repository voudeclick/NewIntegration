using System.Collections.Generic;

namespace VDC.Integration.Domain.Messages.Shopify
{
    public class ShopifyEnqueueListERPProductCategoriesMessage
    {
        public List<string> ExternalIds { get; set; }
    }
}
