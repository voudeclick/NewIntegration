using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.SellerCenter.OrderActor
{
    public class UpdateOrderSellerDeliveryPackageMessage
    {
        public Guid OrderId { get; set; }
        public string OrderClientId { get; set; }
        public Guid OrderSellerId { get; set; }
        public Guid OrderSellerDeliveryId { get; set; }
        public Guid OrderSellerDeliveryPackageId { get; set; }
        public int? DeliveryTime { get; set; }
        public string TrackingCode { get; set; }
        public string TrackingPostageStatus { get; set; }
        public string TrackingUrl { get; set; }
        public string InvoiceDate { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceKey { get; set; }
        public string InvoiceLink { get; set; }
        public string DeliveryClientId { get; set; }
    }
}
