using Samurai.Integration.Domain.Entities;
using Samurai.Integration.EntityFramework.Database;
using System.Linq;

namespace Samurai.Integration.EntityFramework.Repositories
{
    public class MillenniumNewProductProcessRepository
    {
        protected readonly DatabaseContext Database;

        public MillenniumNewProductProcessRepository()
        {

        }

        public MillenniumNewProductProcessRepository(DatabaseContext context)
        {
            Database = context;
        }

        public MillenniumNewProductProcess GetLastByTenantId(long tenantId)
        {
            return Database.MillenniumNewProductProcesses?
                .Where(w => w.TenantId == tenantId)
                .OrderByDescending(o => o.ProcessDate)
                .FirstOrDefault();
        }

        public virtual void Save(MillenniumNewProductProcess objectToSave)
        {
            var existent = Database.MillenniumNewProductProcesses?.Where(w => w.Id == objectToSave.Id).FirstOrDefault();

            if (existent == null)
                Database.Set<MillenniumNewProductProcess>().Add(objectToSave);
            else
                Database.Set<MillenniumNewProductProcess>().Update(objectToSave);

            Database.SaveChanges();
        }
    }
}
