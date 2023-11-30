using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;
using System;
using System.Collections.Generic;
using CoreEnums = Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;

namespace Samurai.Integration.Domain.Models.ViewModels
{
    public class OrderSellerDeliveryViewModel
    {
        public Guid Id { get; set; }                
        public ShipmentServiceType? ShipmentServiceType { get; set; }                
        public Guid? ShipmentServiceId { get; set; }                
        public string ShipmentServiceName { get; set; }                
        public string DeliveryServiceCode { get; set; }                
        public string DeliveryServiceName { get; set; }                
        public TrackingPostageStatus TrackingPostageStatus { get; set; }                
        public Address OriginAddress { get; set; }                
        public Address DestinationAddress { get; set; }                
        public Guid? WarehouseId { get; set; }                
        public int MaxTime { get; set; }                
        public decimal TotalPrice { get; set; }                
        public bool SellerReceivesShippingAmount { get; set; }                
        public Guid? TenantShipmentServiceId { get; set; }                
        public Guid? SellerShipmentServiceId { get; set; }                
        public bool IsJadLog { get { return ShipmentServiceType == CoreEnums.ShipmentServiceType.JadLog; } }

        public List<OrderSellerDeliveryItemViewModel> Items { get; set; }
        public List<OrderSellerDeliveryPackageViewModel> Packages { get; set; }
    }
}
