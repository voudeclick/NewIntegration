using System;

namespace VDC.Integration.Domain.Messages.Shopify
{
    public class ShopifyListRecentOrdersMessage
    {
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
