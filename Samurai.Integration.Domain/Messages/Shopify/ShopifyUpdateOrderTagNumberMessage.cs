using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Messages.Shopify;
using System;
using System.Collections.Generic;

namespace Samurai.Integration.Domain.Messages.Shopify
{
    public class ShopifyUpdateOrderTagNumberMessage
    {
        public long ShopifyId { get; set; }
        public string OrderExternalId { get; set; }
        public string OrderNumber { get; set; }
        public OrderStatus? IntegrationStatus { get; set; }
        public IList<string> CustomTags { get; set; } = new List<string>();
        public Guid? ShopifyListOrderProcessId { get; set; }

    }
}
