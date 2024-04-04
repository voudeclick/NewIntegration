using System;
using System.Collections.Generic;
using VDC.Integration.APIClient.Shopify.Enum.Webhook;

namespace VDC.Integration.APIClient.Shopify.Models.Webhook
{
    public class WebhookOrder
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
        public WebhookFinancialStatus? financial_status { get; set; }
        public WebhookOrderFulfillmentStatus? fulfillment_status { get; set; }
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
        public Address billing_address { get; set; }
        public Address shipping_address { get; set; }
        public Customer customer { get; set; }
        public List<LineItem> line_items { get; set; }
        public List<Fulfillment> fulfillments { get; set; }

        public class DiscountApplication
        {
            public WebhookDiscountApplicationType type { get; set; }
            public decimal value { get; set; }
            public WebhookDiscountApplicationValueType value_type { get; set; }
            public WebhookDiscountApplicationAllocationType allocation_method { get; set; }
            public WebhookDiscountApplicationTargetSelectionType target_selection { get; set; }
            public WebhookDiscountApplicationTargetType target_type { get; set; }
            public string description { get; set; }
            public string title { get; set; }
        }

        public class DiscountCode
        {
            public decimal amount { get; set; }
            public string code { get; set; }

            public WebhookDiscountType type { get; set; }
        }

        public class NoteAttribute
        {
            public string name { get; set; }
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
            public WebhookFulfillmentLineItemStatus? fulfillment_status { get; set; }
            public PriceSet price_set { get; set; }
            public TotalDiscountSet total_discount_set { get; set; }
            public List<DiscountAllocation> discount_allocations { get; set; }
            public List<TaxLine> tax_lines { get; set; }
        }

        public class Fulfillment
        {
            public DateTime created_at { get; set; }
            public long id { get; set; }
            public long order_id { get; set; }
            public WebhookFulfillmentStatus status { get; set; }
            public string tracking_company { get; set; }
            public string tracking_number { get; set; }
            public WebhookShipmentStatus? shipment_status { get; set; }
            public DateTime updated_at { get; set; }
        }
    }

    public class WebhookOrderDelete
    {
        public long id { get; set; }
    }
}
