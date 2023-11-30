using Samurai.Integration.Domain.Messages.Millennium;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Models.Millennium;

namespace Samurai.Integration.Domain.Services.Interfaces
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
