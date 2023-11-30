using System;

namespace Samurai.Integration.Domain.Messages.Shopify
{
    public class ShopifyListRecentOrdersMessage
    {
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
