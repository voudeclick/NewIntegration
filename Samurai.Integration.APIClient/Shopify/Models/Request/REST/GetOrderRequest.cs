using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Shopify.Models.Request.REST
{
    public class GetOrderRequest
    {
        public long OrderId { get; set; }
    }
    public class GetOrderFulfillmentRequest
    {
        public long OrderId { get; set; }
    }
}
