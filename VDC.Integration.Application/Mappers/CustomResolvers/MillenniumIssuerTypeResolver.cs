using AutoMapper;
using System;
using System.Collections.Generic;
using VDC.Integration.Domain.Enums.Millennium;
using VDC.Integration.Domain.Messages.Shopify;
using VDC.Integration.Domain.Models.Millennium;

namespace VDC.Integration.Application.Mappers.CustomResolvers
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
}
