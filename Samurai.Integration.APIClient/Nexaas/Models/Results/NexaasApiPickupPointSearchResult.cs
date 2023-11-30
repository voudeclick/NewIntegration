using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Nexaas.Models.Results
{
    public class NexaasApiPickupPointSearchResult
    {
        public List<Pickup_Points> pickup_points { get; set; }
    }

    public class Pickup_Points
    {
        public int stock_id { get; set; }
        public Organization organization { get; set; }
    }

    public class Organization
    {
        public int id { get; set; }
        public string name { get; set; }
        public int average_withdrawal_term { get; set; }
        public decimal average_withdrawal_price { get; set; }
        public Address address { get; set; }
    }

    public class Address
    {
        public string street { get; set; }
        public string street_number { get; set; }
        public string complement { get; set; }
        public string neighborhood { get; set; }
        public string zip_code { get; set; }
        public string city_name { get; set; }
        public string state_acronym { get; set; }
        public Location location { get; set; }
    }

    public class Location
    {
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }
    }

}
