using Samurai.Integration.Domain.Models.SellerCenter.API.Enums;
using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;
using Samurai.Integration.Domain.Models.SellerCenter.ViewModel;
using System;
using System.Collections.Generic;

namespace Samurai.Integration.Domain.Models.ViewModels
{
    public class OrderSellerViewModel
    {
        public Guid Id { get; set; }        
        public Guid SellerId { get; set; }        
        public string SellerName { get; set; }        
        public Guid StatusId { get; set; }
        public SystemStatus? SystemStatusType { get; set; }
        public OrderStatusTranslationViewModel CultureStatusTranslation { get; set; }
        public List<OrderStatusTranslationViewModel> StatusTranslations { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal Total { get; set; }
        public string Notes { get; set; }
        public string ClientId { get; set; }
        public List<OrderPaymentDto> Payments { get; set; }
        public List<OrderSellerDeliveryViewModel> Deliveries { get; set; }
    }
}
