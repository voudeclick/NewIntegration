using Canducci.Pagination;
using Microsoft.EntityFrameworkCore;

using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.EntityFramework.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.EntityFramework.Repositories
{
    public class TenantRepository : BaseRepository<Tenant>
    {
        public TenantRepository()
            : base(context: null)
        {

        }

        public TenantRepository(DatabaseContext context)
            : base(context)
        {
        }
        protected override IQueryable<Tenant> GetBaseQuery()
        {
            return Database.Tenants
                .Include(x => x.MillenniumData)
                .Include(x => x.NexaasData)
                .Include(x => x.SellerCenterData)
                .Include(x => x.ShopifyData)
                .Include(x => x.OmieData)
                .Include(x => x.LocationMap)
                .Include(x => x.Pier8Data)
                .Include(x => x.BlingData)
                .Include(x => x.PluggToData)
                .Include(x => x.MethodPayments);
        }

        public Task<Paginated<Tenant>> GetWhere(bool? status,
                                                string ERP,
                                                string Shop,
                                                string search,
                                                int pageNumber,
                                                int pageSize)
        {
            var query = GetBaseQuery();

            if (status != null)
            {
                query = query.Where(x => x.Status == status);
            }

            if (ERP != "0")
                query = query.Where(x => x.Type == (TenantType)Enum.Parse(typeof(TenantType), ERP));

            if (Shop != "0")
                query = query.Where(x => x.IntegrationType == (IntegrationType)Enum.Parse(typeof(IntegrationType), Shop));

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.Search.Contains(search));
            }

            if (pageSize == -1)
            {
                pageSize = query.Count();
                pageNumber = 1;
            }

            return query.ToPaginatedAsync(pageNumber, pageSize);
        }

        public override void Save(Tenant objectToSave)
        {
            objectToSave.BeforeSaving();
            base.Save(objectToSave);
        }
        public override void Update(Tenant tenant)
        {
            if (tenant.Type == TenantType.Millennium && tenant.MillenniumData != null)
                tenant.MillenniumData.UpdateDate = GetDateNow();

            if (tenant.Type == TenantType.Nexaas && tenant.NexaasData != null)
                tenant.NexaasData.UpdateDate = GetDateNow();

            if (tenant.Type == TenantType.Omie && tenant.OmieData != null)
                tenant.OmieData.UpdateDate = GetDateNow();

            if (tenant.Type == TenantType.Bling && tenant.BlingData != null)
                tenant.BlingData.UpdateDate = GetDateNow();

            if (tenant.Type == TenantType.PluggTo && tenant.PluggToData != null)
                tenant.PluggToData.UpdateDate = GetDateNow();

            tenant.BeforeSaving();
            base.Update(tenant);
        }

        public async Task<Tenant> GetActiveById(long id, CancellationToken cancellationToken = default)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(x => x.Status == true && x.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Tenant>> GetActive(CancellationToken cancellationToken = default)
        {
            return await GetBaseQuery().Where(x => x.Status == true).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Tenant>> GetActiveByIntegrationType(IntegrationType integrationType, CancellationToken cancellationToken = default)
        {
            return await GetBaseQuery().Where(x => x.Status == true && x.IntegrationType == integrationType).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Tenant>> GetActiveOrderIntegrationByIntegrationType(IntegrationType integrationType, CancellationToken cancellationToken = default)
        {
            return await GetBaseQuery().Where(x => x.Status && x.OrderIntegrationStatus && x.IntegrationType == integrationType).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Tenant>> GetActivePier8(CancellationToken cancellationToken = default)
        {
            return await GetBaseQuery().Include(x => x.Pier8Data).Where(x => x.Status == true && x.EnablePier8Integration == true).ToListAsync(cancellationToken);
        }

        public IntegrationType GetErpType(long id)
        {
            return GetBaseQuery().Where(x => x.MillenniumData.Id == id).Select(x => x.IntegrationType).FirstOrDefault();
        }

        public async Task<IEnumerable<Tenant>> GetActiveByTenantType(TenantType type, CancellationToken cancellationToken = default)
        {
            return await GetBaseQuery().Where(x => x.Type == type && x.Status == true).ToListAsync(cancellationToken);
        }

        public async Task<Tenant> GetUserPluggTo(string userId, CancellationToken cancellationToken = default)
        {
            var userPluggTo = await GetBaseQuery().Include(x => x.PluggToData)
                              .Where(x => x.Status == true && (x.PluggToData.AccountUserId == userId || x.PluggToData.AccountSellerId == userId))
                              .FirstOrDefaultAsync(cancellationToken);

            return userPluggTo;
        }

        public async Task<Tenant> GetTenantWithShopifyData(string domain, CancellationToken cancellationToken = default)
        {
            return await GetBaseQuery()
                .Include(x => x.ShopifyData)
                .Include(x => x.MillenniumData)
                   .FirstOrDefaultAsync(x => x.ShopifyData.ShopifyStoreDomain == domain, cancellationToken);
        }

        public async Task<Tenant> GetTetantBySellerId(string sellerId, CancellationToken cancellationToken = default)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(x => x.Status == true && x.SellerCenterData.SellerId == sellerId, cancellationToken);
        }
    }
}
