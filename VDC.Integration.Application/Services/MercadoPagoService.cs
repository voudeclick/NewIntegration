using System;
using VDC.Integration.Domain.Shopify.Models.Results.REST;

namespace VDC.Integration.Application.Services
{
    public class MercadoPagoService : IGatewayService
    {
        public PaymentDataDTO GetPaymentData(OrderResult.Order order)
        {
            throw new NotImplementedException();
        }
    }
}
