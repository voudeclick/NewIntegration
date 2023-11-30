using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.APIClient.Nexaas.Models.Requests
{
    public class NexaasApiUpdateOrderRequest
    {
        public Order order { get; set; }

        public class Order
        {
            public long id { get; set; }
            public string payment_status { get; set; }
        }
    }
}
