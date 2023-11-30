using Samurai.Integration.Domain.Models.SellerCenter.API.Orders;

using System;

namespace Samurai.Integration.Domain.Messages.SellerCenter.OrderActor
{
    public class ProcessOrderMessage : Order
    {
        public string SITenantSellerId { get; set; }
        public string SITenantWarehouseId { get; set; }

        public ProcessOrderMessage()
        {

        }
        public ProcessOrderMessage(Order order, SellerCenterDataMessage sellerCenterData)
        {
            Id = order.Id;
            Number = order.Number;
            BuyerId = order.BuyerId;
            Buyer = order.Buyer;
            Currency = order.Currency;
            CreateDate = order.CreateDate;
            UpdateDate = order.UpdateDate;
            StatusId = order.StatusId;
            SystemStatusType = order.SystemStatusType;
            StatusTranslations = order.StatusTranslations;
            SubTotal = order.SubTotal;
            TotalDiscount = order.TotalDiscount;
            TotalTax = order.TotalTax;
            Total = order.Total;
            Payments = order.Payments;
            OrderSellers = order.OrderSellers;
            OrderRefundBankDatas = order.OrderRefundBankDatas;
            SITenantSellerId = sellerCenterData.SellerId;
            SITenantWarehouseId = sellerCenterData.SellerWarehouseId;
        }
    }
}
