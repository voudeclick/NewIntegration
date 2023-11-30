using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;
using Samurai.Integration.Domain.Models.SellerCenter.ViewModel;
using System.Linq;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders
{
    public class SellerCenterOrderViewModel
    {
        public OrderViewModel Value { get; set; }

        public Order GetOrder()
        {
            return new Order
            {
                Id = Value.Id,
                Number = Value.Number,
                BuyerId = Value.BuyerId,
                Buyer = new BuyerDto(Value),
                CreateDate = Value.CreateDate,
                UpdateDate = Value.UpdateDate,
                Currency = Value.Currency,
                DisableCustomerDocument = Value.DisableCustomerDocument,
                OrderRefundBankDatas = Value.OrderRefundBankDatas,
                OperatorType = Value.OperatorType,
                OrderSellers = Value?.OrderSellers?.Select(orders => new OrderSellerDto(orders))?.ToList(),
                Payments = Value.Payments,
                SubTotal = Value.SubTotal,
                Total = Value.Total,
                TotalDiscount = Value.TotalDiscount,
                TotalTax = Value.TotalTax,
                StatusId = Value.StatusId,
                StatusTranslations = Value?.StatusTranslations?.Select(s=> new OrderStatusTranslationDto(s))?.ToList(),
                SystemStatusType = Value.SystemStatusType,
                VitrineId = Value.VitrineId
            };

        }
    }
}
