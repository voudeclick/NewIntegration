using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Models.Nexaas
{
    public class NexaasOrder
    {
        public long id { get; set; }
        public long organization_id { get; set; }
        public string organization_name { get; set; }
        public Data data { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string status { get; set; }
        public string payment_status { get; set; }
        public int sale_channel_id { get; set; }
        public List<Nfe> nfes { get; set; }
        public Stock stock { get; set; }
        public ShipingService shipping_service { get; set; }

        public class Data
        {
            public string code { get; set; }
            public string comments { get; set; }
            public Customer customer { get; set; }
            public decimal discount { get; set; }
            public List<Payment> payments { get; set; }
            public decimal shipping { get; set; }
            public DateTime placed_at { get; set; }
            public bool pre_order { get; set; }
            public decimal total_value { get; set; }
            public Address billing_address { get; set; }
            public Address shipping_address { get; set; }
            public List<Item> items { get; set; }
            public decimal interest { get; set; }
            public DateTime? updated_at { get; set; }
            public decimal shipping_cost { get; set; }
            public decimal total_ordered { get; set; }
        }

        public class Customer
        {
            public string name { get; set; }
            public string email { get; set; }
            public string document { get; set; }
            public List<string> phones { get; set; }
        }

        public class Address
        {
            public string street { get; set; }
            public string number { get; set; }
            public string detail { get; set; }
            public string zipcode { get; set; }
            public string neighborhood { get; set; }
            public string city_code { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string country { get; set; }
        }

        public class Payment
        {
            public string method { get; set; }
            public int? installments { get; set; }
            public decimal value { get; set; }
        }

        public class Item
        {
            public int quantity { get; set; }
            public decimal item_value { get; set; }
            public decimal unit_price { get; set; }
            public Stock_Sku stock_sku { get; set; }
            public Product_Sku product_sku { get; set; }
        }

        public class Stock_Sku
        {
            public long id { get; set; }
            public long stock_id { get; set; }
            public long product_sku_id { get; set; }
            public long in_stock_quantity { get; set; }
            public long reserved_quantity { get; set; }
            public long available_quantity { get; set; }
            public long? in_transit_quantity { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public string replacement_cost { get; set; }
            public bool negative_stock { get; set; }
            public string batch { get; set; }
            public string batch_expiration_date { get; set; }
        }

        public class Product_Sku
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public string code { get; set; }
            public string ean { get; set; }
            public string name { get; set; }
            public bool active { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
        }

        public class Stock
        {
            public long id { get; set; }
            public long? organization_id { get; set; }
            public string name { get; set; }
            public string document { get; set; }
            public bool active { get; set; }
            public string zip_code { get; set; }
            public long emites_id { get; set; }
            public string serie_nfe { get; set; }
            public string street { get; set; }
            public string number { get; set; }
            public string complement { get; set; }
            public string city { get; set; }
            public string neighborhood { get; set; }
            public string state { get; set; }
            public string latitude { get; set; }
            public string longitude { get; set; }
            public DateTime created_at { get; set; }
            public List<Sale_Channels> sale_channels { get; set; }
        }

        public class Sale_Channels
        {
            public int id { get; set; }
            public string name { get; set; }
            public long? organization_id { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
        }

        public class Nfe
        {
            public string type { get; set; }
            public string description { get; set; }
            public string danfe_url { get; set; }
            public string xml_url { get; set; }
            public string number { get; set; }
            public string serie { get; set; }
        }

        public class ShipingService
        {
            public string name { get; set; }
            public string carrier { get; set; }
            public string tracking_code { get; set; }
            public string tracking_url { get; set; }
            public string estimated_due_date { get; set; }
        }
    }
}
