using Microsoft.EntityFrameworkCore;
using System.Linq;
using VDC.Integration.Domain.Entities.Database;
using VDC.Integration.EntityFramework.Database;

namespace VDC.Integration.EntityFramework.Repositories
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
