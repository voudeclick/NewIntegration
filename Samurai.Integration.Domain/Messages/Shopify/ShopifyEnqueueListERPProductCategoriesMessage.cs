using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.Shopify
{
    public class ShopifyEnqueueListERPProductCategoriesMessage
    {
        public List<string> ExternalIds { get; set; }
    }
}
