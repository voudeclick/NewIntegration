using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.APIClient.Tray.Models.Response.Order
{
    public class GetOrderByFilterResponse : BaseResponse
    {
        public List<OrderItem> Orders { get; set; }
       
        public class OrderItem
        {
            [JsonProperty("Order")]
            public OrderItemDetail Order { get; set; }
        }

        public class OrderItemDetail
        {
            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("date")]
            public string Date { get; set; }

            [JsonProperty("customer_id")]
            public string CustomerId { get; set; }

            [JsonProperty("partial_total")]
            public string PartialTotal { get; set; }

            [JsonProperty("taxes")]
            public string Taxes { get; set; }

            [JsonProperty("discount")]
            public string Discount { get; set; }

            [JsonProperty("point_sale")]
            public string PointSale { get; set; }

            [JsonProperty("shipment")]
            public string Shipment { get; set; }

            [JsonProperty("shipment_value")]
            public string ShipmentValue { get; set; }

            [JsonProperty("shipment_date")]
            public string ShipmentDate { get; set; }

            [JsonProperty("store_note")]
            public string StoreNote { get; set; }

            [JsonProperty("discount_coupon")]
            public string DiscountCoupon { get; set; }

            [JsonProperty("payment_method_rate")]
            public string PaymentMethodRate { get; set; }

            [JsonProperty("value_1")]
            public string Value1 { get; set; }

            [JsonProperty("payment_form")]
            public string PaymentForm { get; set; }

            [JsonProperty("sending_code")]
            public string SendingCode { get; set; }

            [JsonProperty("session_id")]
            public string SessionId { get; set; }

            [JsonProperty("total")]
            public string Total { get; set; }

            [JsonProperty("payment_date")]
            public string PaymentDate { get; set; }

            [JsonProperty("access_code")]
            public string AccessCode { get; set; }

            [JsonProperty("progressive_discount")]
            public string ProgressiveDiscount { get; set; }

            [JsonProperty("shipping_progressive_discount")]
            public string ShippingProgressiveDiscount { get; set; }

            [JsonProperty("shipment_integrator")]
            public string ShipmentIntegrator { get; set; }

            [JsonProperty("modified")]
            public string Modified { get; set; }

            [JsonProperty("printed")]
            public string Printed { get; set; }

            [JsonProperty("interest")]
            public string Interest { get; set; }

            [JsonProperty("id_quotation")]
            public string IdQuotation { get; set; }

            [JsonProperty("estimated_delivery_date")]
            public string EstimatedDeliveryDate { get; set; }

            [JsonProperty("external_code")]
            public string ExternalCode { get; set; }

            [JsonProperty("has_payment")]
            public string HasPayment { get; set; }

            [JsonProperty("has_shipment")]
            public string HasShipment { get; set; }

            [JsonProperty("has_invoice")]
            public string HasInvoice { get; set; }

            [JsonProperty("total_comission_user")]
            public string TotalComissionUser { get; set; }

            [JsonProperty("total_comission")]
            public string TotalComission { get; set; }

            [JsonProperty("is_traceable")]
            public string IsTraceable { get; set; }

            [JsonProperty("OrderStatus")]
            public OrderStatus OrderStatus { get; set; }

            [JsonProperty("PickupLocation")]
            public List<object> PickupLocation { get; set; }

            [JsonProperty("ProductsSold")]
            public List<ProductsSold> ProductsSold { get; set; }

            [JsonProperty("Payment")]
            public List<object> Payment { get; set; }

            [JsonProperty("OrderInvoice")]
            public List<object> OrderInvoice { get; set; }

            [JsonProperty("MlOrder")]
            public List<object> MlOrder { get; set; }

            [JsonProperty("OrderTransactions")]
            public List<OrderTransaction> OrderTransactions { get; set; }

            [JsonProperty("MarketplaceOrder")]
            public List<object> MarketplaceOrder { get; set; }

            [JsonProperty("Extensions")]
            public List<object> Extensions { get; set; }
        }

        public class OrderStatus
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("default")]
            public string Default { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("show_backoffice")]
            public string ShowBackoffice { get; set; }

            [JsonProperty("allow_edit_order")]
            public string AllowEditOrder { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("show_status_central")]
            public string ShowStatusCentral { get; set; }

            [JsonProperty("background")]
            public string Background { get; set; }

            [JsonProperty("display_name")]
            public string DisplayName { get; set; }

            [JsonProperty("font_color")]
            public string FontColor { get; set; }
        }

        public class ProductsSold
        {
            [JsonProperty("id")]
            public string Id { get; set; }
        }

        public class OrderTransaction
        {
            [JsonProperty("url_payment")]
            public string UrlPayment { get; set; }
        }



    }
}
