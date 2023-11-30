using Newtonsoft.Json;
using System.Collections.Generic;

namespace Samurai.Integration.APIClient.Tray.Models.Response.Customer
{
    public class GetCustomerAddressByFilterResponse
    {
        public List<CustomerAddressItem> CustomerAddresses { get; set; }

        public class CustomerAddressItem
        {
            [JsonProperty("CustomerAddress")]
            public CustomerAddressDetail CustomerAddress { get; set; }
        }
        public class CustomerAddressDetail
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("customer_id")]
            public string CustomerId { get; set; }

            [JsonProperty("address")]
            public string Address { get; set; }

            [JsonProperty("number")]
            public string Number { get; set; }

            [JsonProperty("complement")]
            public string Complement { get; set; }

            [JsonProperty("neighborhood")]
            public string Neighborhood { get; set; }

            [JsonProperty("city")]
            public string City { get; set; }

            [JsonProperty("state")]
            public string State { get; set; }

            [JsonProperty("zip_code")]
            public string ZipCode { get; set; }

            [JsonProperty("country")]
            public string Country { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("active")]
            public string Active { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("recipient")]
            public string Recipient { get; set; }

            [JsonProperty("type_delivery")]
            public string TypeDelivery { get; set; }

            [JsonProperty("not_list")]
            public string NotList { get; set; }

            [JsonIgnore]
            public string FullAddress => $"{Address} {Number}";
        }

      

      

    }
}
