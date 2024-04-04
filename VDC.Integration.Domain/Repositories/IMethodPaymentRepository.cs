using System.Collections.Generic;
using System.Threading.Tasks;
using VDC.Integration.Domain.Entities.Database;
using VDC.Integration.Domain.Entities.Database.MethodPayment;
using VDC.Integration.Domain.Messages.Shopify;

namespace VDC.Integration.Domain.Repositories
{
    public interface IMethodPaymentRepository
    {
        Task CreateConfigPaymentTypeAsync(Tenant tenant);
        Task<List<MethodPayment>> GetConfigPaymentTypeAsync(Tenant tenant);
        Task<string> GetMillenniumIssuerTypeAsync(long tenantId, ShopifySendOrderToERPMessage message);
        Task UpdateConfigPaymentTypeAsync(Tenant tenant);
    }
}