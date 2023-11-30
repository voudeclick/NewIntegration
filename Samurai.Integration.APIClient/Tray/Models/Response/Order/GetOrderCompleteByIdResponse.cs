using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Tray.Models.Response.Order
{
    public class GetOrderCompleteByIdResponse
    {
        public OrderModel Order { get; set; }
        public class OrderModel
        {
            [JsonProperty("id")]
            public string Id { get; set; }
            public OrderStatusModel OrderStatus { get; set; }
            public CustomerModel Customer { get; set; }
            public List<ProductsSoldModel> ProductsSold { get; set; }

            [JsonProperty("shipment")]
            public string Shipment { get; set; }

            [JsonProperty("shipment_integrator")]
            public string ShipmentIntegrator { get; set; }

            [JsonProperty("sending_code")]
            public string SendingCode { get; set; }

            [JsonProperty("sending_date")]
            public string SendingDate { get; set; }

            [JsonProperty("store_note")]
            public string StoreNote { get; set; }

            public class ProductsSoldModel
            {
                public ProductSold ProductsSold { get; set; }
            }

            public class ProductSold
            {
                [JsonProperty("order_id")]
                public string OrderId { get; set; }

                [JsonProperty("product_id")]
                public string ProductId { get; set; }

                [JsonProperty("reference")]
                public string Reference { get; set; }

                [JsonProperty("name")]
                public string Name { get; set; }

                [JsonProperty("price")]
                public string Price { get; set; }

                [JsonProperty("variant_id")]
                public string VariantId { get; set; }
                [JsonProperty("quantity")]
                public string Quantity { get; set; }
            }

            public class CustomerModel
            {
                [JsonProperty("name")]
                public string Name { get; set; }

                [JsonProperty("company_name")]
                public string CompanyName { get; set; }

                [JsonProperty("cnpj")]
                public string Cnpj { get; set; }

                [JsonProperty("cpf")]
                public string Cpf { get; set; }

                [JsonProperty("phone")]
                public string Phone { get; set; }

                [JsonProperty("cellphone")]
                public string Cellphone { get; set; }

                [JsonProperty("address")]
                public string Address { get; set; }

                [JsonProperty("zip_code")]
                public string ZipCode { get; set; }

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

                [JsonProperty("country")]
                public string Country { get; set; }

                public List<CustomerAddressModel> CustomerAddress { get; set; }
                public class CustomerAddressModel
                {
                    [JsonProperty("address")]
                    public string Address { get; set; }

                    [JsonProperty("zip_code")]
                    public string ZipCode { get; set; }

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

                    [JsonProperty("country")]
                    public string Country { get; set; }

                    [JsonProperty("active")]
                    public string Active { get; set; }

                }

                [JsonIgnore]
                public string FullAddress => $"{Address}, {Number}, {Complement}, {Neighborhood}. {City} - {Country}. CEP: {ZipCode}";
            }

            public class OrderStatusModel
            {
                [JsonProperty("id")]
                public string Id { get; set; }

                [JsonProperty("status")]
                public string Status { get; set; }
            }
        }
    }
}
