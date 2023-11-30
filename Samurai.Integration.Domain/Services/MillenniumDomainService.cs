using Samurai.Integration.Domain.Consts;
using Samurai.Integration.Domain.Dtos;
using Samurai.Integration.Domain.Enums.Millennium;
using Samurai.Integration.Domain.Messages.Millennium;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Repositories;
using Samurai.Integration.Domain.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samurai.Integration.Domain.Services
{
    public class MillenniumDomainService : IMillenniumDomainService
    {
        private IMethodPaymentRepository _methodPaymentRepository;

        public MillenniumDomainService(IMethodPaymentRepository methodPaymentRepository)
        {
            _methodPaymentRepository = methodPaymentRepository;
        }

        public async Task<string> GetIssuerTypeAsync(MillenniumData millenniumData, ShopifySendOrderToERPMessage message)
        {
            if (millenniumData.Id == Tenants.PitBull)
            {
                return string.Empty;
            }

            var issuerType = await _methodPaymentRepository.GetMillenniumIssuerTypeAsync(millenniumData.Id, message);
            return issuerType;
        }

        public void ValidateNoteAtributtes(MillenniumData millenniumData, ShopifySendOrderToERPMessage message)
        {
            if (millenniumData.Id != Tenants.PitBull && (message.NoteAttributes is null || message.NoteAttributes.Count <= 0))
                throw new ArgumentException($"NoteAttributes is null, ${message.SourceName}");
        }

        public bool IsMethodPaymentValid(ShopifySendOrderToERPMessage message)
        {
            if(message.NoteAttributes?.FirstOrDefault(x => x.Name == "Gateway")?.Value == "mercado_pago")
                    return false;

            return new List<string> {
                        "DEPÓSITO BANCÁRIO",
                        "PIX",
                        "OUTROS",
                        "PAY2", "MERCADO_PAGO" }.Contains(message.PaymentData.PaymentType.ToUpper());
        }

        public decimal CalculateAdjusmentValue(ShopifySendOrderToERPMessage message)
        {
            return message.InterestValue + message.TaxValue - message.DiscountsValues;
        }

        public MillenniumIssuerType GetBandeira(ShopifySendOrderToERPMessage message)
        {
            var isMercadoPago = message.NoteAttributes?.FirstOrDefault(x => x.Name == "Gateway")?.Value == "mercado_pago";

            if (isMercadoPago)
            {
                if (string.IsNullOrWhiteSpace(message.PaymentData.PaymentType) && string.IsNullOrWhiteSpace(message.PaymentData.Issuer)) return MillenniumIssuerType.OUTROS;

                if (message.PaymentData.Issuer.ToUpper() == "PIX") return MillenniumIssuerType.MERCADO_PAGO;

                if(message.PaymentData.PaymentType.ToUpper().Equals("TICKET") && message.PaymentData.Issuer.ToUpper().Contains("BOL")) return MillenniumIssuerType.BOLETO_BANCARIO;

                if (message.PaymentData.PaymentType.Equals("credit_card") && message.PaymentData?.Issuer?.ToUpper() == "MASTER") return MillenniumIssuerType.MASTERCARD;

                if (Enum.TryParse(typeof(MillenniumIssuerType), message.PaymentData.Issuer?.ToUpper(), false, out object paymentMethod) && paymentMethod != null)
                    return (MillenniumIssuerType)paymentMethod;

                return MillenniumIssuerType.OUTROS;
            }

            if (string.IsNullOrWhiteSpace(message.PaymentData.PaymentType) && string.IsNullOrWhiteSpace(message.PaymentData.Issuer))
                throw new ArgumentNullException("PaymentType or Issuer must have a valid value");
            
            if (!string.IsNullOrWhiteSpace(message.PaymentData.PaymentType))
            {
                if (message.PaymentData.PaymentType.ToUpper().Equals("BOLETO")) return MillenniumIssuerType.BOLETO_BANCARIO;
                               

                if (message.PaymentData.PaymentType.ToUpper().Equals("PAYPAL")) return MillenniumIssuerType.PAYPAL;

                if (new List<string> { "DEPÓSITO BANCÁRIO", "TRANSFERÊNCIA BANCÁRIA", "MANUAL", "TICKET", "VOUCHER", "CREDIT", "CREDITO", "DEBIT", "PIX", "OUTROS", }.Contains(message.PaymentData.PaymentType.ToUpper()))
                    return MillenniumIssuerType.OUTROS;
            }

            if (string.IsNullOrWhiteSpace(message.PaymentData.Issuer))
                throw new ArgumentNullException("Issuer is null");

            if (!Enum.TryParse(typeof(MillenniumIssuerType), message.PaymentData.Issuer, false, out object result))
                throw new ArgumentNullException($"Issuer must have a valid value - actual value: {message.PaymentData.Issuer}");

            return (MillenniumIssuerType)result;
        }

        public MillenniumPaymentType GetTipoPgto(ShopifySendOrderToERPMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.PaymentData.PaymentType))
                throw new ArgumentNullException("PaymentType must have a valid value");

            if (!string.IsNullOrWhiteSpace(message.PaymentData.PaymentType))
            {
                if (message.PaymentData.PaymentType.ToUpper().Equals("BOLETO"))
                    return MillenniumPaymentType.BOLETO;
                else if (message.PaymentData.PaymentType.ToUpper().Equals("CARTÃO"))
                    return MillenniumPaymentType.CARTAO;
                else if (new List<string> { "DEPÓSITO BANCÁRIO", "TRANSFERÊNCIA BANCÁRIA", "PIX", "DEPÓSITO" }.Contains(message.PaymentData.PaymentType.ToUpper()))
                    return MillenniumPaymentType.ONLINEPAYMENTS;
                else if (message.PaymentData.PaymentType == "Pay2")
                    return MillenniumPaymentType.ONLINEPAYMENTS;
            }

            if (!Enum.TryParse(typeof(MillenniumPaymentType), message.PaymentData.PaymentType, true, out object result))
                return MillenniumPaymentType.OUTROS;
            //throw new ArgumentNullException($"PaymentType must have a valid value - actual value: {message.PaymentData.PaymentType}");

            return (MillenniumPaymentType)result;
        }

        public decimal CalculateInitialValue(ShopifySendOrderToERPMessage message, decimal adjustmentValue)
        {
            return message.Subtotal + message.ShippingValue + adjustmentValue;
        }

        public int GetNumeroParcelas(ShopifySendOrderToERPMessage message, string descricaoTipo, MillenniumIssuerType bandeira)
        {
            return bandeira == MillenniumIssuerType.PAYPAL || !string.IsNullOrWhiteSpace(descricaoTipo) ? 1 : message.PaymentData.InstallmentQuantity;
        }


        public string GetLocation(MillenniumData millenniumData, long? locationId)
        {
            if (millenniumData.HasMultiLocation)
            {
                var locationItem = millenniumData.LocationMap.GetLocationByIdEcommerce(locationId.Value.ToString());

                if (locationItem is null) throw new Exception($"{millenniumData.StoreName} - Not found mapping for LocationId: {locationId}");

                return locationItem.ErpLocation;
            }

            return default;
        }

        public decimal GetValuesWithFeesOrder(decimal valorInicial, ShopifySendOrderToERPMessage message)
        {
            var value = message.NoteAttributes.FirstOrDefault(x => x.Name == "aditional_info_valor_total_pedido")?.Value ?? "";

            if (string.IsNullOrWhiteSpace(value) || value.Contains("-"))
                return valorInicial;

            return decimal.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}
