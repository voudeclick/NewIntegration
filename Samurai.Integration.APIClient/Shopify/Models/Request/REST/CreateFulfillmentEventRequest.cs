﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Shopify.Models.Request.REST
{
    public class CreateFulfillmentEventRequest
    {
        public string OrderId { get; set; }
        public string FulfillmentId { get; set; }
        public string Status { get; set; }
    }
}
