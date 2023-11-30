using System;
using System.Collections.Generic;

namespace Samurai.Integration.Domain.Shopify.Models.Results
{
    public class FulfillmentResult
    {
        public string id { get; set; }
        public string legacyResourceId { get; set; }
        public DateTime? deliveredAt { get; set; }
        public List <TrackingInfo> trackingInfo { get; set; }
    }
}
