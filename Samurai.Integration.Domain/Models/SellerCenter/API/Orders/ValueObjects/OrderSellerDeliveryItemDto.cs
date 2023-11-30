using Samurai.Integration.Domain.Models.SellerCenter.API.Enums;
using Samurai.Integration.Domain.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects
{
    public class OrderSellerDeliveryItemDto
    {
        public OrderSellerDeliveryItemDto(){}
        public OrderSellerDeliveryItemDto(OrderSellerDeliveryItemViewModel orderSeller)
        {
            Id = orderSeller.Id;
            ProductId = orderSeller.ProductId;
            ProductClientCode = orderSeller.ProductClientCode ?? "";
            ProductName = orderSeller.ProductName ?? "";
            SkuId = orderSeller.SkuId;
            Sku = orderSeller.Sku ?? "";
            SkuName = orderSeller.SkuName ?? "";
            StatusId = orderSeller.StatusId;
            StatusTranslations = orderSeller?.StatusTranslations?.Select(s => new OrderStatusTranslationDto(s))?.ToList() ?? new List<OrderStatusTranslationDto>();
            CultureStatusTranslation = new OrderStatusTranslationDto(orderSeller.CultureStatusTranslation) ?? new OrderStatusTranslationDto();
            SystemStatusType = orderSeller.SystemStatusType;
            Quantity = orderSeller.Quantity;
            Discount = orderSeller.Discount;
            UnitPrice = orderSeller.UnitPrice;
            FinalPrice = orderSeller.FinalPrice;
            Weight = orderSeller.Weight;
            Height = orderSeller.Height;
            Length = orderSeller.Length;
            Width = orderSeller.Width;
            Diameter = orderSeller.Diameter;
            OrderSellerPackageId = orderSeller.OrderSellerPackageId;
        }

        public Guid Id { get; set; }

        public Guid ProductId { get; set; }

        public string ProductClientCode { get; set; }

        [Display(Name = "Name")]
        public string ProductName { get; set; }

        public Guid SkuId { get; set; }

        public string Sku { get; set; }

        [Display(Name = "SKU")]
        public string SkuName { get; set; }

        [Display(Name = "Status")]
        public Guid StatusId { get; set; }

        public List<OrderStatusTranslationDto> StatusTranslations { get; set; }

        public OrderStatusTranslationDto CultureStatusTranslation { get; set; }

        public SystemStatus? SystemStatusType { get; set; }

        [Display(Name = "Qtde")]
        public int Quantity { get; set; }

        [Display(Name = "Desconto")]
        public decimal Discount { get; set; }

        [Display(Name = "Valor unitário")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Total")]
        public decimal FinalPrice { get; set; }

        public decimal? Weight { get; set; }

        public decimal? Height { get; set; }

        public decimal? Length { get; set; }

        public decimal? Width { get; set; }

        public decimal? Diameter { get; set; }

        public Guid? OrderSellerPackageId { get; set; }
    }
}