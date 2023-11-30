using Samurai.Integration.Domain.Models.SellerCenter.API.Enums;
using Samurai.Integration.Domain.Models.SellerCenter.ViewModel;
using System;
using System.Collections.Generic;

namespace Samurai.Integration.Domain.Models.ViewModels
{
    public class OrderSellerDeliveryItemViewModel
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductClientCode { get; set; }        
        public string ProductName { get; set; }
        public Guid SkuId { get; set; }
        public string Sku { get; set; }        
        public string SkuName { get; set; }        
        public Guid StatusId { get; set; }
        public List<OrderStatusTranslationViewModel> StatusTranslations { get; set; }
        public OrderStatusTranslationViewModel CultureStatusTranslation { get; set; }
        public SystemStatus? SystemStatusType { get; set; }        
        public int Quantity { get; set; }        
        public decimal Discount { get; set; }        
        public decimal UnitPrice { get; set; }        
        public decimal FinalPrice { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Diameter { get; set; }
        public Guid? OrderSellerPackageId { get; set; }
    }
}
