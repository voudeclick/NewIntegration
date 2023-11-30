using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Shopify.Models.Results.REST
{
    public class CarrierServiceResult
    {
        public List<CarrierService> carrier_services { get; set; }

        public class CarrierService
        {
            public long id { get; set; }
            public string name { get; set; }
            public bool active { get; set; }
            public bool service_discovery { get; set; }
            public string carrier_service_type { get; set; }
            public string admin_graphql_api_id { get; set; }
            public string format { get; set; }
            public string callback_url { get; set; }
        }
    }
}
