using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.EntityFramework.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Samurai.Integration.EntityFramework.Repositories
{
    public class MillenniumNewPricesProcessRepository
    {
        protected readonly DatabaseContext Database;

        public MillenniumNewPricesProcessRepository()
        {

        }

        public MillenniumNewPricesProcessRepository(DatabaseContext context)
        {
            Database = context;
        }

        public MillenniumNewPricesProcess GetLastIdProcess(long tenantId)
        {
            try
            {
                return Database.MillenniumNewPricesProcesses?.Where(x => x.TenantId == tenantId)
               .OrderByDescending(o => o.ProcessDate)
               .AsNoTracking()
               .FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public virtual void Save(MillenniumNewPricesProcess objectToSave)
        {
            var existent = Database.MillenniumNewPricesProcesses?.Where(w => w.Id == objectToSave.Id).FirstOrDefault();

            if (existent == null)
                Database.Set<MillenniumNewPricesProcess>().Add(objectToSave);
            else
                Database.Set<MillenniumNewPricesProcess>().Update(objectToSave);

            Database.SaveChanges();
        }
    }
}
