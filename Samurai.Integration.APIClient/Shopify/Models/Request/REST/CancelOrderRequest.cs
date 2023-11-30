using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Shopify.Models.Request.REST
{
    public class CancelOrderRequest
    {
        public string OrderId { get; set; }
        public bool SendEmail { get; set; }
    }
}
