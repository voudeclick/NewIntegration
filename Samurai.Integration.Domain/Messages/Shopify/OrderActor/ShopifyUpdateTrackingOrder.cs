using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.Shopify.OrderActor
{
    public class ShopifyUpdateTrackingOrder
    {
        public string OrderExternalId { get; set; }
        public long? ShopifyId { get; set; }

        public ShippingStatus Shipping { get; set; }

        public class ShippingStatus
        {
            public bool IsShipped { get; set; }
            public bool IsDelivered { get; set; }
            public string TrackingCode { get; set; }
            public string TrackingUrl { get; set; }
        }

    }
}
