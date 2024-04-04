using System.Linq;
using VDC.Integration.Domain.Entities.Database.Integrations.Millenium;
using VDC.Integration.EntityFramework.Database;

namespace VDC.Integration.EntityFramework.Repositories
{
    public class MillenniumProductImageIntegrationRepository
    {
        protected readonly DatabaseContext Database;

        public MillenniumProductImageIntegrationRepository()
        {

        }

        public MillenniumProductImageIntegrationRepository(DatabaseContext context)
        {
            Database = context;
        }

        public virtual void Save(MillenniumProductImageIntegration objectToSave)
        {
            var existent = Database.MillenniumProductImageIntegrations?.Where(w => w.Id == objectToSave.Id).FirstOrDefault();

            if (existent == null)
                Database.Set<MillenniumProductImageIntegration>().Add(objectToSave);
            else
                Database.Set<MillenniumProductImageIntegration>().Update(objectToSave);

            Database.SaveChanges();
        }
    }
}
