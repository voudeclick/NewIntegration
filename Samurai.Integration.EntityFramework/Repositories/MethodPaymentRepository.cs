using Microsoft.EntityFrameworkCore;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Repositories;
using Samurai.Integration.EntityFramework.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Samurai.Integration.EntityFramework.Repositories
{
    public class MethodPaymentRepository : IMethodPaymentRepository
    {
        private readonly DatabaseContext _db;

        public MethodPaymentRepository(DatabaseContext db)
        {
            _db = db;
        }

        public async Task<string> GetMillenniumIssuerTypeAsync(long tenantId, ShopifySendOrderToERPMessage message)
        {
            try
            {
                var result = await _db.MethodPayment.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId
                                                                                                    && !string.IsNullOrWhiteSpace(message.PaymentData.PaymentType)
                                                                                                    && x.PaymentTypeShopify.ToUpper() == message.PaymentData.PaymentType.ToUpper());
                if (result is null)
                    result = await _db.MethodPayment.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId
                                                                                       && !string.IsNullOrWhiteSpace(message.PaymentData.Issuer)
                                                                                       && x.PaymentTypeShopify.ToUpper() == message.PaymentData.Issuer.ToUpper());

                if (result is null || string.IsNullOrWhiteSpace(result?.PaymentTypeMillenniun))
                    return "";

                return result?.PaymentTypeMillenniun;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public async Task<List<MethodPayment>> GetConfigPaymentTypeAsync(Tenant tenant)
        {
            return await _db.MethodPayment.Where(x => x.TenantId == tenant.Id).ToListAsync();
        }

        public async Task CreateConfigPaymentTypeAsync(Tenant tenant)
        {
            foreach (var item in tenant.MethodPayments)
            {
                var createFromToMethod = new MethodPayment();
                createFromToMethod.Up(paymentTypeShopify: item.PaymentTypeShopify, paymentTypeMillenniun: item.PaymentTypeMillenniun, tenantId: tenant.Id);

                await _db.Set<MethodPayment>().AddAsync(createFromToMethod);
                await Commit();
            }

            return;
        }

        public async Task UpdateConfigPaymentTypeAsync(Tenant tenant)
        {
            await DeleteConfigPaymentTypeAsync(tenant);

            await CreateConfigPaymentTypeAsync(tenant);

            return;
        }

        private async Task DeleteConfigPaymentTypeAsync(Tenant tenant)
        {
            var fromTo = await GetConfigPaymentTypeAsync(tenant);
            if (fromTo != null && fromTo.Count >= 1)
            {
                _db.MethodPayment.RemoveRange(fromTo);
                await Commit();
            }
        }

        private async Task Commit() => await _db.SaveChangesAsync();

    }
}
