using Microsoft.EntityFrameworkCore;
using System.Linq;
using VDC.Integration.Domain.Entities.Database;
using VDC.Integration.EntityFramework.Database;

namespace VDC.Integration.EntityFramework.Repositories
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
