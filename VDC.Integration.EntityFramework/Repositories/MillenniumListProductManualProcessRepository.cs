using System.Linq;
using VDC.Integration.Domain.Entities.Database;
using VDC.Integration.EntityFramework.Database;

namespace VDC.Integration.EntityFramework.Repositories
{
    public class MillenniumListProductManualProcessRepository
    {
        protected readonly DatabaseContext Database;

        public MillenniumListProductManualProcessRepository()
        {

        }

        public MillenniumListProductManualProcessRepository(DatabaseContext context)
        {
            Database = context;
        }

        public virtual void Save(MillenniumListProductManualProcess objectToSave)
        {
            var existent = Database.MillenniumListProductManualProcesses?.Where(w => w.Id == objectToSave.Id).FirstOrDefault();

            if (existent == null)
                Database.Set<MillenniumListProductManualProcess>().Add(objectToSave);
            else
                Database.Set<MillenniumListProductManualProcess>().Update(objectToSave);

            Database.SaveChanges();
        }
    }
}
