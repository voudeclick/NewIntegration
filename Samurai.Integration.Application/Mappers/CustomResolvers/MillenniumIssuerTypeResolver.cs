using AutoMapper;
using Samurai.Integration.Domain.Enums.Millennium;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Models.Millennium;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Samurai.Integration.Application.Mappers.CustomResolvers
{
    public class MillenniumIssuerTypeResolver : IValueResolver<ShopifySendOrderToERPMessage, MilenniumApiCreateOrderPaymentDataRequest, int>
    {
        public int Resolve(ShopifySendOrderToERPMessage source, MilenniumApiCreateOrderPaymentDataRequest paymentType, int destMember, ResolutionContext context)
        {

            if (source.PaymentData.PaymentType.ToUpper().Equals("BOLETO"))
                return (int)MillenniumIssuerType.BOLETO_BANCARIO;
            else if (source.PaymentData.PaymentType.ToUpper().Equals("PAYPAL"))
                return (int)MillenniumIssuerType.PAYPAL;
            else if (new List<string> { "DEPÓSITO BANCÁRIO", "TRANSFERÊNCIA BANCÁRIA", "MANUAL" }.Contains(source.PaymentData.PaymentType.ToUpper()))
                return (int)MillenniumIssuerType.OUTROS;
            else
                return (int)(MillenniumIssuerType)Enum.Parse(typeof(MillenniumIssuerType), source.PaymentData.Issuer, true);
        }
    }

    public class SellerCenterPaymentTypeResolver : IValueResolver<Domain.Models.SellerCenter.API.Orders.Order, MilenniumApiCreateOrderPaymentDataRequest, int>
    {
        public int Resolve(Domain.Models.SellerCenter.API.Orders.Order source, MilenniumApiCreateOrderPaymentDataRequest paymentType, int destMember, ResolutionContext context)
        {

            if (source.Payments.FirstOrDefault().PaymentMethod == Domain.Models.SellerCenter.API.Orders.ValueObjects.PaymentMethod.Boleto)
                return (int)MillenniumIssuerType.BOLETO_BANCARIO;
           else
                return (int)MillenniumIssuerType.OUTROS;
        }
    }

}
