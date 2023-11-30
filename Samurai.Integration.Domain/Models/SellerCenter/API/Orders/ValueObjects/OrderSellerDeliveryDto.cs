using Samurai.Integration.Domain.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CoreEnums = Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects
{
    public class OrderSellerDeliveryDto
    {
        public OrderSellerDeliveryDto() { }
        public OrderSellerDeliveryDto(OrderSellerDeliveryViewModel orderSeller)
        {
            Id = orderSeller.Id;
            ShipmentServiceType = orderSeller.ShipmentServiceType;
            ShipmentServiceId = orderSeller.ShipmentServiceId;
            ShipmentServiceName = orderSeller.ShipmentServiceName ?? "";
            DeliveryServiceCode = orderSeller.DeliveryServiceCode ?? "";
            DeliveryServiceName = orderSeller.DeliveryServiceName ?? "";
            TrackingPostageStatus = orderSeller.TrackingPostageStatus;
            OriginAddress = orderSeller.OriginAddress ?? new Address();
            DestinationAddress = orderSeller.DestinationAddress ?? new Address();
            WarehouseId = orderSeller.WarehouseId;
            MaxTime = orderSeller.MaxTime;
            TotalPrice = orderSeller.TotalPrice;
            SellerReceivesShippingAmount = orderSeller.SellerReceivesShippingAmount;
            TenantShipmentServiceId = orderSeller.TenantShipmentServiceId;
            SellerShipmentServiceId = orderSeller.SellerShipmentServiceId;
            Items = orderSeller?.Items?.Select(s=> new OrderSellerDeliveryItemDto(s))?.ToList() ?? new List<OrderSellerDeliveryItemDto>();
            Packages = orderSeller.Packages ?? new List<OrderSellerDeliveryPackageViewModel>();
        }

        public Guid Id { get; set; }

        [Display(Name = "Tipo do serviço de remessa")]
        public ShipmentServiceType? ShipmentServiceType { get; set; }

        [Display(Name = "Id do serviço de remessa")]
        public Guid? ShipmentServiceId { get; set; }

        [Display(Name = "Nome do serviço de remessa")]
        public string ShipmentServiceName { get; set; }

        [Display(Name = "Código do serviço de entrega")]
        public string DeliveryServiceCode { get; set; }

        [Display(Name = "Nome do serviço de entrega")]
        public string DeliveryServiceName { get; set; }

        [Display(Name = "Status de postagem de rastreamento")]
        public TrackingPostageStatus TrackingPostageStatus { get; set; }

        [Display(Name = "Endereço de origem")]
        public Address OriginAddress { get; set; }

        [Display(Name = "Endereço de destino")]
        public Address DestinationAddress { get; set; }

        [Display(Name = "Id do centro de distribuição")]
        public Guid? WarehouseId { get; set; }

        [Display(Name = "Prazo máximo")]
        public int MaxTime { get; set; }

        [Display(Name = "Valor total")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "O vendedor recebe o valor da remessa")]
        public bool SellerReceivesShippingAmount { get; set; }

        [Display(Name = "Id do serviço de remessa do tenant")]
        public Guid? TenantShipmentServiceId { get; set; }

        [Display(Name = "Id do serviço de remessa do vendedor")]
        public Guid? SellerShipmentServiceId { get; set; }

        [Display(Name = "JadLog")]
        public bool IsJadLog { get { return ShipmentServiceType == CoreEnums.ShipmentServiceType.JadLog; } }

        public List<OrderSellerDeliveryItemDto> Items { get; set; }
        public List<OrderSellerDeliveryPackageViewModel> Packages { get; set; }
    }
}