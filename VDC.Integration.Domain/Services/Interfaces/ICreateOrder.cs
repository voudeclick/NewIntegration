using VDC.Integration.Domain.Messages.Millennium;
using VDC.Integration.Domain.Messages.Shopify;
using VDC.Integration.Domain.Models.Millennium;

namespace VDC.Integration.Domain.Services.Interfaces
{
    public interface ICreateOrder
    {
        bool TypeDiscount(MillenniumData millenniumData);
        MillenniumApiCreateOrderRequest CreateOrder(MillenniumData millenniumData,
                                                            ShopifySendOrderToERPMessage message,
                                                            MilenniumApiCreateOrderPaymentDataRequest milenniumApiCreateOrderPaymentDataRequest,
                                                            decimal adjustmentValue);
    }
}
