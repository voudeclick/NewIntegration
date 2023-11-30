using Samurai.Integration.Domain.Models.SellerCenter.API.Enums;
using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Requests.Inputs
{
    public class OrderSellerDeliveryPackage
    {
        public Guid PackageId { get; set; }
        public Guid OrderId { get; set; }
        public Guid SubOrderId { get; set; }
        public Guid DeliveryId { get; set; }
        public int? DeliveryTime { get; set; }
        public decimal? Price { get; set; }
        public Currency Currency { get; set; }
        public decimal? TenantMarkupAmount { get; set; }
        public decimal? SellerMarkupAmount { get; set; }
        public Guid? TrackingOrderDeliveryPackageId { get; set; }
        public string TrackingCode { get; set; }
        public string TrackingPackagePostageStatus { get; set; }
        public List<Guid> DeliveryItens { get; set; }
        public string TrackingUrl { get; set; }
        public string InvoiceDate { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceKey { get; set; }
        public string InvoiceLink { get; set; }
        public string DeliveryClientId { get; set; }
        public bool IsCreatedManually { get; set; } = true;
    }
}
