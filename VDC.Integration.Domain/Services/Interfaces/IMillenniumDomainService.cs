using System.Threading.Tasks;
using VDC.Integration.Domain.Enums.Millennium;
using VDC.Integration.Domain.Messages.Millennium;
using VDC.Integration.Domain.Messages.Shopify;

namespace VDC.Integration.Domain.Services.Interfaces
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
