using System.Linq;
using VDC.Integration.Domain.Entities.Database.Integrations.Millenium;
using VDC.Integration.EntityFramework.Database;

namespace VDC.Integration.EntityFramework.Repositories
{
    public class MillenniumProductIntegrationRepository
    {
        protected readonly DatabaseContext Database;

        public MillenniumProductIntegrationRepository()
        {

        }

        public MillenniumProductIntegrationRepository(DatabaseContext context)
        {
            Database = context;
        }

        public virtual void Save(MillenniumProductIntegration objectToSave)
        {
            var existent = Database.MillenniumProductIntegrations?.Where(w => w.Id == objectToSave.Id).FirstOrDefault();

            if (existent == null)
                Database.Set<MillenniumProductIntegration>().Add(objectToSave);
            else
                Database.Set<MillenniumProductIntegration>().Update(objectToSave);

            Database.SaveChanges();
        }
    }
}
