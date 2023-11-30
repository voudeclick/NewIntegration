using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static Samurai.Integration.APIClient.Tray.Models.Response.Customer.GetCustomerAddressByFilterResponse;

namespace Samurai.Integration.APIClient.Tray.Models.Response.Customer
{
    public class GetCustomerByFilterResponse
    {
        [JsonProperty("Customers")]
        public List<CustomerItem> Customers { get; set; }

        public class CustomerItem
        {
            [JsonProperty("Customer")]
            public CustomerItemDetail Customer { get; set; }
        }
        public class CustomerItemDetail
        {
            [JsonProperty("newsletter")]
            public string Newsletter { get; set; }

            [JsonProperty("cnpj")]
            public string Cnpj { get; set; }

            [JsonProperty("created")]
            public string Created { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("registration_date")]
            public string RegistrationDate { get; set; }

            [JsonProperty("cpf")]
            public string Cpf { get; set; }

            [JsonProperty("birth_date")]
            public string BirthDate { get; set; }

            [JsonProperty("gender")]
            public string Gender { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }

            [JsonProperty("nickname")]
            public string Nickname { get; set; }

            [JsonProperty("discount")]
            public string Discount { get; set; }

            [JsonProperty("profile_customer_id")]
            public string ProfileCustomerId { get; set; }

            [JsonProperty("last_visit")]
            public string LastVisit { get; set; }

            [JsonProperty("city")]
            public string City { get; set; }

            [JsonProperty("state")]
            public string State { get; set; }

            [JsonProperty("modified")]
            public string Modified { get; set; }

            [JsonProperty("CustomerAddress")]
            public List<CustomerAddress> CustomerAddress { get; set; }
            
            [JsonIgnore]
            public CustomerAddressDetail Address { get; set; }


            [JsonProperty("Extensions")]
            public Extensions Extensions { get; set; }
        }

        public class CustomerAddress
        {
            [JsonProperty("id")]
            public string Id { get; set; }
        }

        public class Profile
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class Extensions
        {
            [JsonProperty("Profile")]
            public Profile Profile { get; set; }
        }

        
        



    }
}
