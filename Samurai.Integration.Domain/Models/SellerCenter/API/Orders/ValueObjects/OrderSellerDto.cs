using Samurai.Integration.Domain.Models.SellerCenter.API.Enums;
using Samurai.Integration.Domain.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects
{
    public class OrderSellerDto
    {
        public OrderSellerDto(){}
        public OrderSellerDto(OrderSellerViewModel order)
        {
            Id = order.Id;
            SellerId = order.SellerId;
            SellerName = order.SellerName ?? "";
            StatusId = order.StatusId;
            SystemStatusType = order.SystemStatusType;
            CultureStatusTranslation = order.CultureStatusTranslation != null? new OrderStatusTranslationDto(order.CultureStatusTranslation) : new OrderStatusTranslationDto();
            StatusTranslations = order.StatusTranslations?.Select(s=> new OrderStatusTranslationDto(s))?.ToList() ?? new List<OrderStatusTranslationDto>();
            SubTotal = order.SubTotal;
            TotalDiscount = order.TotalDiscount;
            Total = order.Total;
            Notes = order.Notes ?? "";
            ClientId = order.ClientId ?? "";
            Payments = order.Payments ?? new List<OrderPaymentDto>();
            Deliveries = order.Deliveries?.Select(s=> new OrderSellerDeliveryDto(s))?.ToList() ?? new List<OrderSellerDeliveryDto>();
        }

        public Guid Id { get; set; }

        [Display(Name = "Id do vendedor")]
        public Guid SellerId { get; set; }

        [Display(Name = "Vendedor")]
        public string SellerName { get; set; }

        [Display(Name = "Status")]
        public Guid StatusId { get; set; }

        public SystemStatus? SystemStatusType { get; set; }

        public OrderStatusTranslationDto CultureStatusTranslation { get; set; }
        
        public List<OrderStatusTranslationDto> StatusTranslations { get; set; }

        public decimal SubTotal { get; set; }

        public decimal TotalDiscount { get; set; }

        public decimal Total { get; set; }

        public string Notes { get; set; }

        public string ClientId { get; set; }

        public List<OrderPaymentDto> Payments { get; set; }

        public List<OrderSellerDeliveryDto> Deliveries { get; set; }
    }
}
