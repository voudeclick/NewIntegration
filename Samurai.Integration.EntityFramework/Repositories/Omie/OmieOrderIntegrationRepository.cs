﻿using Microsoft.EntityFrameworkCore;
using Samurai.Integration.Domain.Entities.Database.Integrations.Omie;
using Samurai.Integration.EntityFramework.Database;
using System.Linq;
using System.Threading.Tasks;

namespace Samurai.Integration.EntityFramework.Repositories.Omie
{
    public class OmieOrderIntegrationRepository
    {
        protected readonly DatabaseContext Database;

        public OmieOrderIntegrationRepository(DatabaseContext context)
        {
            Database = context;
        }

        public Task<OmieUpdateOrderProcess> GetByTenantAndOrderAsync(long tenantId,long orderId)
        {
            return Database.OmieUpdateOrderProcesses
                .Where(o => o.TenantId == tenantId && o.OrderId == orderId)
                .OrderByDescending(o => o.ProcessDate)
                .Take(1)
                .FirstOrDefaultAsync();
        }

        public void Save(OmieUpdateOrderProcess objectToSave)
        {
            try
            {
                var existent = Database.OmieUpdateOrderProcesses?.Where(w => w.Id == objectToSave.Id).FirstOrDefault();

                if (existent == null)
                    Database.Set<OmieUpdateOrderProcess>().Add(objectToSave);
                else
                    Database.Set<OmieUpdateOrderProcess>().Update(objectToSave);

                Database.SaveChanges();
            }
            catch (System.Exception){}           
        }
    }
}
