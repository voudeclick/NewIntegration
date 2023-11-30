using Microsoft.EntityFrameworkCore;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.EntityFramework.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samurai.Integration.EntityFramework.Repositories
{
    public class MillenniumNewStockProcessRepository
    {
        protected readonly DatabaseContext Database;

        public MillenniumNewStockProcessRepository()
        {

        }

        public MillenniumNewStockProcessRepository(DatabaseContext context)
        {
            Database = context;
        }

        public MillenniumNewStockProcess GetLastIdProcess(long tenantId)
        {
            try
            {
                return Database.MillenniumNewStockProcesses?.Where(x => x.TenantId == tenantId)
               .OrderByDescending(o => o.ProcessDate)
               .AsNoTracking()
               .FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public virtual void Save(MillenniumNewStockProcess objectToSave)
        {
            var existent = Database.MillenniumNewStockProcesses?.Where(w => w.Id == objectToSave.Id).FirstOrDefault();

            if (existent == null)
                Database.Set<MillenniumNewStockProcess>().Add(objectToSave);
            else
                Database.Set<MillenniumNewStockProcess>().Update(objectToSave);

            Database.SaveChanges();
        }
    }
}
