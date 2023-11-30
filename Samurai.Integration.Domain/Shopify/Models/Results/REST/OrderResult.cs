using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Samurai.Integration.Domain.Shopify.Models.Results.REST
{
    public class OrderResult
    {
        public Order order { get; set; }

        public List<OrderFulfillment> fulfillment_orders { get; set; }


        public class OrderFulfillment
        {
            public long id { get; set; }
            public long? assigned_location_id { get; set; }
            public List<LineItemFulfillment> line_items { get; set; }

            public class LineItemFulfillment
            {
                public long id { get; set; }
                public long fulfillment_order_id { get; set; }
                public long line_item_id { get; set; }
                public long inventory_item_id { get; set; }
                public long variant_id { get; set; }

            }
        }
        public class Order
        {
            public long id { get; set; }
            public string email { get; set; }
            public DateTime? closed_at { get; set; }
            public DateTime created_at { get; set; }
            public DateTime? updated_at { get; set; }
            public long number { get; set; }
            public string note { get; set; }
            public string token { get; set; }
            public string gateway { get; set; }
            public decimal total_price { get; set; }
            public decimal subtotal_price { get; set; }
            public int total_weight { get; set; }
            public decimal total_tax { get; set; }
            public bool taxes_included { get; set; }
            public string currency { get; set; }
            public FinancialStatus? financial_status { get; set; }
            public OrderFulfillmentStatus? fulfillment_status { get; set; }
            public bool confirmed { get; set; }
            public decimal total_discounts { get; set; }
            public decimal total_line_items_price { get; set; }
            public string cart_token { get; set; }
            public bool buyer_accepts_marketing { get; set; }
            public string name { get; set; }
            public string referring_site { get; set; }
            public string landing_site { get; set; }
            public DateTime? cancelled_at { get; set; }
            public string cancel_reason { get; set; }
            public decimal? total_price_usd { get; set; }
            public string checkout_token { get; set; }
            public long? user_id { get; set; }
            public long? location_id { get; set; }
            public DateTime? processed_at { get; set; }
            public string phone { get; set; }
            public string customer_locale { get; set; }
            public long order_number { get; set; }
            public List<DiscountApplication> discount_applications { get; set; }
            public List<DiscountCode> discount_codes { get; set; }
            public List<NoteAttribute> note_attributes { get; set; }
            public List<string> payment_gateway_names { get; set; }
            public string processing_method { get; set; }
            public string source_name { get; set; }
            public string tags { get; set; }
            public string contact_email { get; set; }
            public string order_status_url { get; set; }
            public string presentment_currency { get; set; }
            public TotalLineItemsPriceSet total_line_items_price_set { get; set; }
            public TotalDiscountsSet total_discounts_set { get; set; }
            public TotalShippingPriceSet total_shipping_price_set { get; set; }
            public SubtotalPriceSet subtotal_price_set { get; set; }
            public TotalPriceSet total_price_set { get; set; }
            public TotalTaxSet total_tax_set { get; set; }
            public List<ShippingLine> shipping_lines { get; set; }
            public Address billing_address { get; set; } = new Address();
            public Address shipping_address { get; set; } = new Address();
            public List<Refunds> refunds { get; set; }
            public Customer customer { get; set; } = new Customer();
            public List<LineItem> line_items { get; set; }
            public List<Fulfillment> fulfillments { get; set; }

            public int? GetPrazoEntrega()
            {
                int dias;
                var noteAttributes = note_attributes.FirstOrDefault(n => n.name == "aditional_info_extra_entrega_prazo")?.value ?? "";
                var prazoEntrega = Regex.Replace(noteAttributes, @"[^0-9]|\s", "");

                return int.TryParse(prazoEntrega, out dias) ? (int?)dias : null;
            }
        }

        public class DiscountApplication
        {
            public DiscountApplicationType type { get; set; }
            public decimal value { get; set; }
            public DiscountApplicationValueType value_type { get; set; }
            public DiscountApplicationAllocationType allocation_method { get; set; }
            public DiscountApplicationTargetSelectionType target_selection { get; set; }
            public DiscountApplicationTargetType target_type { get; set; }
            public string description { get; set; }
            public string title { get; set; }
        }

        public class DiscountCode
        {
            public decimal amount { get; set; }
            public string code { get; set; }

            public DiscountType type { get; set; }
        }

        public class NoteAttribute
        {
            //[JsonConverter(typeof(IntToStringConverter))]
            public string name { get; set; }
            //[JsonConverter(typeof(IntToStringConverter))]
            public string value { get; set; }
        }

        public class LineItemProperty
        {
            public string name { get; set; }
            public string value { get; set; }
        }

        public class ShopMoney
        {
            public decimal amount { get; set; }
            public string currency_code { get; set; }
        }

        public class PresentmentMoney
        {
            public decimal amount { get; set; }
            public string currency_code { get; set; }
        }

        public class TotalLineItemsPriceSet
        {
            public ShopMoney shop_money { get; set; }
            public PresentmentMoney presentment_money { get; set; }
        }

        public class TotalDiscountsSet
        {
            public ShopMoney shop_money { get; set; }
            public PresentmentMoney presentment_money { get; set; }
        }

        public class TotalShippingPriceSet
        {
            public ShopMoney shop_money { get; set; }
            public PresentmentMoney presentment_money { get; set; }
        }

        public class SubtotalPriceSet
        {
            public ShopMoney shop_money { get; set; }
            public PresentmentMoney presentment_money { get; set; }
        }

        public class TotalPriceSet
        {
            public ShopMoney shop_money { get; set; }
            public PresentmentMoney presentment_money { get; set; }
        }

        public class TotalTaxSet
        {
            public ShopMoney shop_money { get; set; }
            public PresentmentMoney presentment_money { get; set; }
        }

        public class PriceSet
        {
            public ShopMoney shop_money { get; set; }
            public PresentmentMoney presentment_money { get; set; }
        }

        public class DiscountedPriceSet
        {
            public ShopMoney shop_money { get; set; }
            public PresentmentMoney presentment_money { get; set; }
        }

        public class ShippingLine
        {
            public long id { get; set; }
            public string title { get; set; }
            public string price { get; set; }
            public string code { get; set; }
            public string source { get; set; }
            public string phone { get; set; }
            public string requested_fulfillment_service_id { get; set; }
            public decimal discounted_price { get; set; }
            public PriceSet price_set { get; set; }
            public DiscountedPriceSet discounted_price_set { get; set; }
            public List<DiscountAllocation> discount_allocations { get; set; }
            public List<TaxLine> tax_lines { get; set; }
        }

        public class TaxLine
        {
            public decimal price { get; set; }
            public decimal rate { get; set; }
            public string title { get; set; }

            public PriceSet price_set { get; set; }

        }

        public class DiscountAllocation
        {
            public decimal amount { get; set; }
            public int discount_application_index { get; set; }

            public AmountSet amount_set { get; set; }
        }

        public class AmountSet
        {
            public ShopMoney shop_money { get; set; }
            public PresentmentMoney presentment_money { get; set; }
        }

        public class Refunds
        {
            public long id { get; set; }
            public string admin_graphql_api_id { get; set; }
            public DateTime? created_at { get; set; }
            public long order_id { get; set; }
            public DateTime? processed_at { get; set; }
            public bool? restock { get; set; }
            public long? user_id { get; set; }
            public List<RefundLineItems> refund_line_items { get; set; }
        }

        public class RefundLineItems
        {
            public long id { get; set; }
            public long line_item_id { get; set; }
            public long? location_id { get; set; }
            public int quantity { get; set; }
            public decimal subtotal { get; set; }
            public string restock_type { get; set; }
            public LineItem line_item { get; set; }
        }

        public class Address
        {
            public string first_name { get; set; }
            public string address1 { get; set; }
            public string phone { get; set; }
            public string city { get; set; }
            public string zip { get; set; }
            public string province { get; set; }
            public string country { get; set; }
            public string last_name { get; set; }
            public string address2 { get; set; }
            public string company { get; set; }
            public decimal? latitude { get; set; }
            public decimal? longitude { get; set; }
            public string name { get; set; }
            public string country_code { get; set; }
            public string province_code { get; set; }
        }

        public class Customer
        {
            public long id { get; set; }
            public string email { get; set; }
            public bool accepts_marketing { get; set; }
            public DateTime? created_at { get; set; }
            public DateTime? updated_at { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public int orders_count { get; set; }
            public string state { get; set; }
            public decimal total_spent { get; set; }
            public long? last_order_id { get; set; }
            public string note { get; set; }
            public bool verified_email { get; set; }
            public string multipass_identifier { get; set; }
            public bool tax_exempt { get; set; }
            public string phone { get; set; }
            public string tags { get; set; }
            public string last_order_name { get; set; }
            public string currency { get; set; }
            public Address default_address { get; set; }
        }

        public class TotalDiscountSet
        {
            public ShopMoney shop_money { get; set; }
            public PresentmentMoney presentment_money { get; set; }
        }

        public class LineItem
        {
            public long id { get; set; }
            public long? variant_id { get; set; }
            public string title { get; set; }
            public int quantity { get; set; }
            public string sku { get; set; }
            public string variant_title { get; set; }
            public string vendor { get; set; }
            public string fulfillment_service { get; set; }
            public long? product_id { get; set; }
            public bool requires_shipping { get; set; }
            public bool taxable { get; set; }
            public bool gift_card { get; set; }
            public string name { get; set; }
            public string variant_inventory_management { get; set; }
            public List<LineItemProperty> properties { get; set; }
            public bool product_exists { get; set; }
            public int fulfillable_quantity { get; set; }
            public int grams { get; set; }
            public decimal price { get; set; }
            public decimal total_discount { get; set; }
            public FulfillmentLineItemStatus? fulfillment_status { get; set; }
            public PriceSet price_set { get; set; }
            public TotalDiscountSet total_discount_set { get; set; }
            public List<DiscountAllocation> discount_allocations { get; set; }
            public List<TaxLine> tax_lines { get; set; }

            [JsonIgnore]
            [Newtonsoft.Json.JsonIgnore]
            public long? location_id { get; set; }
        }

        public class Fulfillment
        {
            public DateTime created_at { get; set; }
            public long id { get; set; }
            public long order_id { get; set; }
            public FulfillmentStatus status { get; set; }
            public string tracking_company { get; set; }
            public string tracking_number { get; set; }
            public ShipmentStatus? shipment_status { get; set; }
            public DateTime updated_at { get; set; }
        }

        public enum DiscountApplicationValueType
        {
            fixed_amount = 1,
            percentage = 2
        }

        public enum DiscountApplicationAllocationType
        {
            across = 1,
            each = 2,
            one = 3
        }

        public enum DiscountApplicationTargetSelectionType
        {
            all = 1,
            entitled = 2,
            @explicit = 3
        }

        public enum DiscountApplicationTargetType
        {
            line_item = 1,
            shipping_line = 2
        }

        public enum DiscountApplicationType
        {
            discount_code = 1,
            manual = 2,
            script = 3,
            automatic = 4
        }

        public enum DiscountType
        {
            fixed_amount = 1,
            percentage = 2,
            shipping = 3
        }

        public enum FinancialStatus
        {
            pending = 1,
            authorized = 2,
            partially_paid = 3,
            paid = 4,
            partially_refunded = 5,
            refunded = 6,
            voided = 7
        }

        public enum FulfillmentEventStatus
        {
            label_printed = 1,
            label_purchased = 2,
            attempted_delivery = 3,
            ready_for_pickup = 4,
            confirmed = 5,
            in_transit = 6,
            out_for_delivery = 7,
            delivered = 8,
            failure = 9,
            picked_up = 10
        }

        public enum FulfillmentLineItemStatus
        {
            fulfilled = 1,
            partial = 2,
            not_eligible = 3,
            restocked = 4,
        }

        public enum FulfillmentStatus
        {
            pending = 1,
            open = 2,
            success = 3,
            cancelled = 4,
            error = 5,
            failure = 6
        }

        public enum OrderFulfillmentStatus
        {
            fulfilled = 1,
            @null = 2,
            partial = 3,
            restocked = 4,
        }

        public enum ShipmentStatus
        {
            label_printed = 1,
            label_purchased = 2,
            attempted_delivery = 3,
            ready_for_pickup = 4,
            confirmed = 5,
            in_transit = 6,
            out_for_delivery = 7,
            delivered = 8,
            failure = 9,
            picked_up = 10
        }

        public enum ShippingStatus
        {
            label_printed = 1,
            label_purchased = 2,
            attempted_delivery = 3,
            ready_for_pickup = 4,
            confirmed = 5,
            in_transit = 6,
            out_for_delivery = 7,
            delivered = 8,
            failure = 9,
            picked_up = 10
        }
    }
}
