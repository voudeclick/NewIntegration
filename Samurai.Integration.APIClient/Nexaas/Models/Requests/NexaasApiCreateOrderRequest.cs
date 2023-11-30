using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.APIClient.Nexaas.Models.Requests
{
    public class NexaasApiCreateOrderRequest
    {
        public Order order { get; set; }

        public class Order
        {
            public long sale_channel_id { get; set; }
            public long organization_id { get; set; }
            public long stock_id { get; set; }
            public string code { get; set; }
            public DateTime placed_at { get; set; }
            public decimal discount { get; set; }
            public bool pre_order { get; set; }
            public string payment_status { get; set; }
            public decimal shipping { get; set; }
            public decimal total_value { get; set; }
            public bool pickup_on_store { get; set; }
            public List<Item> items { get; set; }
            public Customer customer { get; set; }
            public Address billing_address { get; set; }
            public Address shipping_address { get; set; }
            public List<Payment> payments { get; set; }
            public ShippingService shipping_service { get; set; }
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
            public string city { get; set; }
            public string state { get; set; }
            public string country { get; set; }
        }

        public class Item
        {
            public long product_sku_id { get; set; }
            public int quantity { get; set; }
            public decimal unit_price { get; set; }
            public decimal item_value { get; set; }
        }

        public class Payment
        {
            public string method { get; set; }
            public string card_brand { get; set; }
            public int installments { get; set; }
            public decimal value { get; set; }
            public decimal taxes { get; set; }
            public decimal total { get; set; }
        }

        public static class PaymentMethods
        {
            public static string Dinheiro = "01";
            public static string Cheque = "02";
            public static string CartaaoDeCredito = "03";
            public static string CartaaoDeDebito = "04";
            public static string CreditoLoja = "05";
            public static string ValeAlimentaccao = "10";
            public static string ValeRefeicao = "11";
            public static string ValePresente = "12";
            public static string ValeCombustivel = "13";
            public static string DuplicadaMercantil = "14";
            public static string BoletoBancario = "15";
            public static string SemPagamento = "90";
            public static string Outros = "99";
        }

        public class ShippingService
        {
            public string name { get; set; }
            public string carrier { get; set; }
            public string tracking_code { get; set; }
            public string tracking_url { get; set; }
            public string estimated_due_date { get; set; }
        }
    }
}
