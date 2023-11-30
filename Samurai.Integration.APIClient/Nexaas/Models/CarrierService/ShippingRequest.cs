using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Nexaas.Models.CarrierService
{

    public class ShippingRequest
    {
        public ShippingRequestRate rate { get; set; }

        public ShippingRequest()
        {
        }

        public class ShippingRequestRate
        {
            public Address origin { get; set; }
            public Address destination { get; set; }
            public List<ShippingResquestItem> items { get; set; }
            public string currency { get; set; }
        }

        public class Address
        {
            public string country { get; set; }
            public string postal_code { get; set; }
            public string province { get; set; }
            public string city { get; set; }
            public string name { get; set; }
            public string address1 { get; set; }
            public string address2 { get; set; }
            public string address3 { get; set; }
            public string phone { get; set; }
            public string fax { get; set; }
            public string address_type { get; set; }
            public string company_name { get; set; }
        }

        public class ShippingResquestItem
        {
            public string name { get; set; }
            public string sku { get; set; }
            public long quantity { get; set; }
            public long grams { get; set; }
            public long price { get; set; }
            public string vendor { get; set; }
            public bool requires_shipping { get; set; }
            public bool taxable { get; set; }
            public string fulfillment_service { get; set; }
            public long product_id { get; set; }
        }
    }
}
