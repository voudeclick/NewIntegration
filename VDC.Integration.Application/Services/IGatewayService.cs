using OrderResult = VDC.Integration.Domain.Shopify.Models.Results.REST.OrderResult;

namespace VDC.Integration.Application.Services
{
    public interface IGatewayService 
    {
        PaymentDataDTO GetPaymentData(OrderResult.Order order);
    }
}
