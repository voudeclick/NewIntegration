using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Nexaas.Models.Requests
{
    public class NexaasApiPickupPointSearchRequest
    {
        public Search search { get; set; }

        public class Search
        {
            public string zip_code { get; set; }
            public List<Stock_Skus> stock_skus { get; set; }
        }

        public class Stock_Skus
        {
            public string sku_code { get; set; }
            public long quantity { get; set; }
        }
    }
}
