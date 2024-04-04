using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using VDC.Integration.Domain.Enums;
using VDC.Integration.Domain.Enums.Millennium;
using static VDC.Integration.Domain.Shopify.Models.Results.REST.OrderResult;

namespace VDC.Integration.Domain.Messages.Shopify
{
    public class ShopifySendOrderToERPMessage
    {
        public ShopifySendOrderToERPMessage()
        {
            NoteAttributes = new List<ShopifySendOrderNoteAttributeToERPMessage>();
        }
        public long ID { get; set; }

        public string ExternalID { get; set; }

        public string Name { get; set; }

        public long Number { get; set; }

        public bool Approved { get; set; }

        public bool Shipped { get; set; }

        public string TrackingNumber { get; set; }

        public bool Delivered { get; set; }

        public bool Cancelled { get; set; }

        public DateTime CreatedAt { get; set; }

        public int DaysToDelivery { get; set; }

        public decimal Subtotal { get; set; }

        public decimal Total { get; set; }

        public decimal DiscountsValues { get; set; }

        public decimal InterestValue { get; set; }

        public decimal TaxValue { get; set; }

        public decimal ShippingValue { get; set; }

        public string CarrierName { get; set; }

        public bool? IsPickup { get; set; }

        public string Checkout_Token { get; set; }

        public List<string> PickupAdditionalData { get; set; }

        public string Note { get; set; }

        public string vendor { get; set; }

        public List<ShopifySendOrderItemToERPMessage> Items { get; set; }

        public ShopifySendOrderCustomerToERPMessage Customer { get; set; }

        public ShopifySendOrderPaymentDataToERPMessage PaymentData { get; set; }

        public List<ShopifySendOrderNoteAttributeToERPMessage> NoteAttributes { get; set; }

        public List<Refunds> Refunds { get; set; }

        public int DeliveryCount { get; set; } = 0;

        public string GetDataEntregaToString()
        {
            return CreatedAt.AddDays(DaysToDelivery).ToString("yyyy-MM-dd");
        }

        public OrderStatus GetOrderStatus()
        {
            if (this.Cancelled)
                return OrderStatus.Cancelled;
            else if (this.Delivered)
                return OrderStatus.Delivered;
            else if (this.Shipped)
                return OrderStatus.Shipped;
            else if (this.Approved)
                return OrderStatus.Paid;
            else
                return OrderStatus.Pending;
        }


        [JsonIgnore]
        public bool DisableCustomerDocument { get; set; }
        [JsonIgnore]
        public long VitrineId { get; set; }
        [JsonIgnore]
        public decimal AdjustmentValue => InterestValue + TaxValue - DiscountsValues;
        public MillenniumOperatorType OperatorType { get; set; }
        public Guid? ShopifyListOrderProcessId { get; set; }
        public string GetNoteAttributeByName(string name)
            => NoteAttributes != null ? NoteAttributes.Where(n => n.Name == name).FirstOrDefault()?.Value : null;

        public string SourceName { get; set; }

    }

    public class ShopifySendOrderNoteAttributeToERPMessage
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class ShopifySendOrderItemToERPMessage
    {
        public long? LocationId { get; set; }
        public long Id { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountValue { get; set; }
        public bool ProductGift { get; set; }


        public string GetFlagGift()
        {
            return ProductGift ? "T" : null;
        }

    }

    public class ShopifySendOrderCustomerToERPMessage
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DDD { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string Note { get; set; }

        public string Company { get; set; }

        public DateTime? BirthDate { get; set; }

        public ShopifySendOrderAddressToERPMessage BillingAddress { get; set; }

        public ShopifySendOrderAddressToERPMessage DeliveryAddress { get; set; }

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string FullName => $"{FirstName} {LastName}";
    }

    public class ShopifySendOrderAddressToERPMessage
    {
        public string Address { get; set; }

        public string Number { get; set; }

        public string Complement { get; set; }

        public string District { get; set; }

        public string ZipCode { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Contact { get; set; }

        public string DDD { get; set; }

        public string Phone { get; set; }

        public string Country { get; set; }

        public string CountryCode { get; set; }
    }

    public class ShopifySendOrderPaymentDataToERPMessage
    {
        public string Issuer { get; set; }

        public int InstallmentQuantity { get; set; }

        public decimal InstallmentValue { get; set; }

        public string PaymentType { get; set; }
    }
}
