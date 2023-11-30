using Newtonsoft.Json;
using Samurai.Integration.APIClient.PluggTo.Enum;
using Samurai.Integration.APIClient.PluggTo.Models;
using Samurai.Integration.Domain.Messages.PluggTo;
using Samurai.Integration.Domain.Messages.SellerCenter.OrderActor;
using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Samurai.Integration.APIClient.PluggTo.Models.Requests
{
    public class PluggToApiCreateOrderRequest
    {
        #region Receiver

        public string channel { get; set; }
        public string receiver_name { get; set; }
        public string receiver_lastname { get; set; }
        public string receiver_zipcode { get; set; }
        public string receiver_address { get; set; }
        public string receiver_address_number { get; set; }
        public string receiver_neighborhood { get; set; }
        public string receiver_city { get; set; }
        public string receiver_state { get; set; }
        public string receiver_country { get; set; }
        public string receiver_address_complement { get; set; }

        public string receiver_email { get; set; }
        public string receiver_phone_area { get; set; }
        public string receiver_phone { get; set; }

        #endregion

        #region Payer
        public string payer_name { get; set; }
        public string payer_lastname { get; set; }

        public string payer_zipcode { get; set; }
        public string payer_address { get; set; }
        public string payer_address_number { get; set; }
        public string payer_neighborhood { get; set; }
        public string payer_city { get; set; }
        public string payer_state { get; set; }
        public string payer_address_complement { get; set; }
        public string payer_email { get; set; }
        public string payer_phone_area { get; set; }
        public string payer_phone { get; set; }
        public string payer_cpf { get; set; }
        public string payer_cnpj { get; set; }
        public string payer_document { get; set; }
        public string payer_razao_social { get; set; }
        public string payer_company_name { get; set; }
        public string payer_country { get; set; }

        #endregion

        public string total_paid { get; set; }
        public decimal shipping { get; set; }
        public decimal subtotal { get; set; }
        public decimal discount { get; set; }
        public decimal total { get; set; }
        public int payment_installments { get; set; }
        public string user_client_id { get; set; }
        public string status { get; set; }
        public string original_id { get; set; }
        public bool auto_reserve { get; set; }
        public string user_id { get; set; }
        public string external { get; set; }
        public List<Payment> payments { get; set; }
        public List<Item> items { get; set; }
        public List<Shipment> shipments { get; set; }
        public string sale_intermediary { get; set; }
        public string payment_intermediary { get; set; }
        public string intermediary_seller_id { get; set; }

        public static PluggToApiCreateOrderRequest From(ProcessOrderMessage message, OrderSellerDto orderSeller, PluggToData pluggToData, string status)
        {
            try
            {
                var userId = pluggToData.AccountSellerId ?? pluggToData.AccountUserId;

                var delivery = orderSeller?.Deliveries?.FirstOrDefault(x => x.WarehouseId?.ToString().ToUpper() == message.SITenantWarehouseId.ToUpper());

                var address = delivery.DestinationAddress;
                var phone = message.Buyer?.Phones.FirstOrDefault();

                var cpf = message.Buyer?.DocumentType == DocumentType.CPF ? message.Buyer?.DocumentNumber : string.Empty;

                var payment = message.Payments?.FirstOrDefault();

                var total = (orderSeller.SubTotal + delivery.TotalPrice) - orderSeller.TotalDiscount;                

                return new PluggToApiCreateOrderRequest()
                {
                    original_id = message.Number,
                    external = message.Number.ToString(),
                    user_id = userId,
                    user_client_id = pluggToData.ClientId,
                    auto_reserve = true,
                    status = status,
                    channel = pluggToData.StoreName,

                    receiver_name = message.Buyer?.FirstName,
                    receiver_lastname = message.Buyer?.LastName,
                    receiver_email = message.Buyer?.Email,

                    receiver_phone_area = phone?.AreaCode,
                    receiver_phone = phone?.Number?.Replace("-", string.Empty),

                    receiver_zipcode = address?.ZipCode,
                    receiver_address = address?.Street,
                    receiver_address_number = address?.Number,
                    receiver_neighborhood = address?.District,
                    receiver_city = address?.City,
                    receiver_state = address?.State,
                    receiver_address_complement = address?.Complement,
                    receiver_country = address?.Country,

                    payer_name = message.Buyer?.FirstName,
                    payer_lastname = message.Buyer?.LastName,
                    payer_email = message.Buyer?.Email,
                    payer_cpf = cpf,

                    payer_phone_area = phone?.AreaCode,
                    payer_phone = phone?.Number?.Replace("-", string.Empty),

                    payer_zipcode = address?.ZipCode,
                    payer_address = address?.Street,
                    payer_address_number = address?.Number,
                    payer_neighborhood = address?.District,
                    payer_city = address?.City,
                    payer_state = address?.State,
                    payer_country = address?.Country,
                    payer_address_complement = address?.Complement,

                    total = total,
                    total_paid = total.ToString(new CultureInfo("en-US")),
                    subtotal = orderSeller.SubTotal,
                    discount = orderSeller.TotalDiscount,
                    shipping = delivery.TotalPrice,

                    payment_installments = payment.InstallmentCount,
                    payments = message.Payments?.Select(x => Payment.From(x, total)).ToList(),

                    items = delivery?.Items?.Select(x => Item.From(x, pluggToData.AccountSellerId)).ToList(),
                    shipments = new List<Shipment>() { Shipment.From(delivery) },

                    intermediary_seller_id = orderSeller.SellerId.ToString(),
                    sale_intermediary = "28.594.748/0001-89", //Cnpj da Samurai Experts
                    payment_intermediary = "87.184.310/001-08", // Cnpj da Moip
                };
            }
            catch (Exception)
            {
                return new PluggToApiCreateOrderRequest();
            }
        }
    }

    public class PluggToOriginalId
    {
        public string UserId { get; set; }
        public Guid OrderSellerId { get; set; }
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; }
        //public Guid Id { get; set; }

        public PluggToOriginalId()
        {
            //Id = Guid.NewGuid();
        }

    }

    public class Payment
    {
        public string payment_type { get; set; }
        public string payment_method { get; set; }
        public int payment_installments { get; set; }
        public decimal payment_total { get; set; }

        public static string GetPaymentMethodTypePluggTo(string paymentMethod)
        {
            return paymentMethod == "CreditCard" ? PluggToPaymentMethod.credit.ToString() :
                   paymentMethod == "Debit" ? PluggToPaymentMethod.debit.ToString() :
                   PluggToPaymentMethod.ticket.ToString();
        }

        public static Payment From(OrderPaymentDto payment, decimal total)
        {
            return new Payment()
            {
                payment_method = payment.PaymentMethod.ToString(),
                payment_type = GetPaymentMethodTypePluggTo(payment.PaymentMethod.ToString()),
                payment_installments = payment.InstallmentCount,
                payment_total = total
            };
        }
    }

    public class Item
    {
        public int quantity { get; set; }
        public string sku { get; set; }
        public decimal price { get; set; }
        public decimal total { get; set; }
        public string name { get; set; }

        public string supplier_id { get; set; }
        public static Item From(OrderSellerDeliveryItemDto item, string accountSellerId)
        {
            var found = item.Sku.IndexOf("-") + 1;
            return new Item()
            {
                quantity = item.Quantity,
                sku = item.Sku[found..item.Sku.Length],
                price = item.UnitPrice,
                total = item.FinalPrice,
                name = item.ProductName,
                supplier_id = accountSellerId
            };
        }
    }

    public class Shipment
    {
        public string id { get; set; }
        public string shipping_company { get; set; }
        public string shipping_method { get; set; }
        public string track_code { get; set; }
        public string track_url { get; set; }
        public string status { get; set; }
        public DateTime? estimate_delivery_date { get; set; }
        public DateTime? date_shipped { get; set; }
        public DateTime? date_delivered { get; set; }
        public DateTime? date_cancelled { get; set; }
        public string nfe_key { get; set; }
        public string nfe_link { get; set; }
        public string nfe_number { get; set; }
        public string nfe_serie { get; set; }
        public string nfe_date { get; set; }
        public string cfops { get; set; }
        public string external { get; set; }
        public List<ShippingItems> shipping_items { get; set; }
        public static string GetShipmentMethodPluggTo(string shipmentMethod)
        {
            return (shipmentMethod == "PickupInStore" ||
                    shipmentMethod == "Correios" ||
                    shipmentMethod == "JadLog")
                ? PluggToShipmentMethod.pickup.ToString() : PluggToShipmentMethod.standard.ToString();
        }
        public static string GetShipmentStatusPluggTo(string status)
        {
            return status == "Posted" ? PluggToShipmentStatus.shipped.ToString() :
                   status == "Delivered" ? PluggToShipmentStatus.delivered.ToString() :
                   PluggToShipmentStatus.approved.ToString();
        }

        public static Shipment From(OrderSellerDeliveryDto delivery)
        {
            return new Shipment()
            {
                shipping_company = string.IsNullOrEmpty(delivery.ShipmentServiceName) ? "-" : delivery.ShipmentServiceName,
                shipping_method = string.IsNullOrWhiteSpace(delivery.ShipmentServiceType.ToString()) ? "-" : GetShipmentMethodPluggTo(delivery.ShipmentServiceType.ToString()),
                status = GetShipmentStatusPluggTo(delivery.TrackingPostageStatus.ToString()),
                shipping_items = delivery.Items?.Select(x => ShippingItems.From(x)).ToList(),
                external = delivery.Id.ToString(),
                estimate_delivery_date = DateTime.Now.AddDays(1)
            };
        }
        public static Shipment FromPackage(OrderSellerDeliveryDto delivery, OrderSellerDeliveryPackageViewModel package)
        {
            return new Shipment()
            {
                shipping_company = string.IsNullOrEmpty(delivery.ShipmentServiceName) ? "-" : delivery.ShipmentServiceName,
                shipping_method = string.IsNullOrWhiteSpace(delivery.ShipmentServiceType.ToString()) ? "-" : GetShipmentMethodPluggTo(delivery.ShipmentServiceType.ToString()),
                status = GetShipmentStatusPluggTo(delivery.TrackingPostageStatus.ToString()),
                track_code = package.TrackingCode,
                shipping_items = delivery.Items?.Select(x => ShippingItems.From(x)).ToList(),
                external = delivery.Id.ToString(),
                estimate_delivery_date = DateTime.Now.AddDays(1)
            };
        }
    }

    public class ShippingItems
    {
        public string sku { get; set; }
        public int quantity { get; set; }

        public static ShippingItems From(OrderSellerDeliveryItemDto item)
        {
            return new ShippingItems()
            {
                sku = item.Sku,
                quantity = item.Quantity
            };
        }
    }

}