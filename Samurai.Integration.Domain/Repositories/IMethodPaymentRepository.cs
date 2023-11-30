using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Messages.Shopify;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Samurai.Integration.Domain.Repositories
{
    public interface IMethodPaymentRepository
    {
        Task CreateConfigPaymentTypeAsync(Tenant tenant);
        Task<List<MethodPayment>> GetConfigPaymentTypeAsync(Tenant tenant);
        Task<string> GetMillenniumIssuerTypeAsync(long tenantId, ShopifySendOrderToERPMessage message);
        Task UpdateConfigPaymentTypeAsync(Tenant tenant);
    }
}