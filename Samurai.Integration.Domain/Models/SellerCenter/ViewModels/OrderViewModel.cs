using Samurai.Integration.Domain.Enums.Millennium;
using Samurai.Integration.Domain.Models.SellerCenter.API.Enums;
using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;
using Samurai.Integration.Domain.Models.SellerCenter.ViewModels;
using Samurai.Integration.Domain.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace Samurai.Integration.Domain.Models.SellerCenter.ViewModel
{
    public class OrderViewModel
    {
        public Guid Id { get; set; }
        
        public string Number { get; set; }

        public Guid BuyerId { get; set; }

        public BuyerPhoneViewModel Buyer { get; set; }

        public Currency Currency { get; set; }
                
        public DateTimeOffset CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }
                
        public Guid StatusId { get; set; }

        public SystemStatus? SystemStatusType { get; set; }

        public List<OrderStatusTranslationViewModel> StatusTranslations { get; set; }

        public decimal SubTotal { get; set; }

        public decimal TotalDiscount { get; set; }

        public decimal TotalTax { get; set; }

        public decimal Total { get; set; }

        public List<OrderPaymentDto> Payments { get; set; }

        public List<OrderSellerViewModel> OrderSellers { get; set; }

        public List<OrderRefundBankDataDto> OrderRefundBankDatas { get; set; }


        [JsonIgnore]
        public decimal AdjustmentValue => SubTotal + TotalTax - TotalDiscount; // Todo -> Validar

        [JsonIgnore]
        public long VitrineId { get; set; }

        [JsonIgnore]
        public MillenniumOperatorType OperatorType { get; set; }

        [JsonIgnore]
        public bool DisableCustomerDocument { get; set; }
    }
}
