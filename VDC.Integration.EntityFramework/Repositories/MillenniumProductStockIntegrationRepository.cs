using System.Linq;
using VDC.Integration.Domain.Entities.Database.Integrations.Millenium;
using VDC.Integration.EntityFramework.Database;

namespace VDC.Integration.EntityFramework.Repositories
{
    public class MillenniumProductStockIntegrationRepository
    {
        protected readonly DatabaseContext Database;

        public MillenniumProductStockIntegrationRepository()
        {

        }

        public MillenniumProductStockIntegrationRepository(DatabaseContext context)
        {
            Database = context;
        }

        public virtual void Save(MillenniumProductStockIntegration objectToSave)
        {
            var existent = Database.MillenniumProductStockIntegrations?.Where(w => w.Id == objectToSave.Id).FirstOrDefault();

            if (existent == null)
                Database.Set<MillenniumProductStockIntegration>().Add(objectToSave);
            else
                Database.Set<MillenniumProductStockIntegration>().Update(objectToSave);

            Database.SaveChanges();
        }
    }
}
