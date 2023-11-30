using Samurai.Integration.Domain.Dtos;
using Samurai.Integration.Domain.Enums.Millennium;
using Samurai.Integration.Domain.Messages.Millennium;
using Samurai.Integration.Domain.Messages.Shopify;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Samurai.Integration.Domain.Services.Interfaces
{
    public interface IMillenniumDomainService
    {
        void ValidateNoteAtributtes(MillenniumData millenniumData, ShopifySendOrderToERPMessage message);
        bool IsMethodPaymentValid(ShopifySendOrderToERPMessage message);
        decimal CalculateAdjusmentValue(ShopifySendOrderToERPMessage message);
        MillenniumIssuerType GetBandeira(ShopifySendOrderToERPMessage message);
        MillenniumPaymentType GetTipoPgto(ShopifySendOrderToERPMessage message);
        decimal CalculateInitialValue(ShopifySendOrderToERPMessage message, decimal adjustmentValue);
        int GetNumeroParcelas(ShopifySendOrderToERPMessage message, string descricaoTipo, MillenniumIssuerType bandeira);
        string GetLocation(MillenniumData millenniumData, long? locationId);
        Task<string> GetIssuerTypeAsync(MillenniumData millenniumData, ShopifySendOrderToERPMessage message);
        decimal GetValuesWithFeesOrder(decimal valorInicial, ShopifySendOrderToERPMessage message);
    }
}
