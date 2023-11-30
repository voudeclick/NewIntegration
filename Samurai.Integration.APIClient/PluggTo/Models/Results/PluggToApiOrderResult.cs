using Samurai.Integration.APIClient.PluggTo.Models.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.PluggTo.Models.Results
{
    public class PluggToApiOrderResult
    {
        public OrderResult Order { get; set; }

    }

    public class OrderResult
    {
        public string id { get; set; }
        public string original_id { get; set; }
        public long order_id { get; set; }
        public string user_id { get; set; }
        public string status { get; set; }
        public string external { get; set; }
        public List<Shipment> shipments { get; set; }
    }
}