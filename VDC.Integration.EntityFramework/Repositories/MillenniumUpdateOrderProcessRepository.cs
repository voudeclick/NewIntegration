using System.Linq;
using VDC.Integration.Domain.Entities.Database.Integrations.Millenium;
using VDC.Integration.EntityFramework.Database;

namespace VDC.Integration.EntityFramework.Repositories
{
    public class MillenniumUpdateOrderProcessRepository
    {
        protected readonly DatabaseContext Database;

        public MillenniumUpdateOrderProcessRepository()
        {

        }

        public MillenniumUpdateOrderProcessRepository(DatabaseContext context)
        {
            Database = context;
        }

        public virtual void Save(MillenniumUpdateOrderProcess objectToSave)
        {
            var existent = Database.MillenniumUpdateOrderProcesses?.Where(w => w.Id == objectToSave.Id).FirstOrDefault();

            if (existent == null)
                Database.Set<MillenniumUpdateOrderProcess>().Add(objectToSave);
            else
                Database.Set<MillenniumUpdateOrderProcess>().Update(objectToSave);

            Database.SaveChanges();
        }
    }
}
