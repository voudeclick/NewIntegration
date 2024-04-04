using System.Linq;
using VDC.Integration.Domain.Entities.Database.Integrations.Millenium;
using VDC.Integration.EntityFramework.Database;

namespace VDC.Integration.EntityFramework.Repositories
{
    public class MillenniumImageIntegrationRepository
    {
        protected readonly DatabaseContext Database;

        public MillenniumImageIntegrationRepository()
        {

        }

        public MillenniumImageIntegrationRepository(DatabaseContext context)
        {
            Database = context;
        }

        public virtual void Save(MillenniumImageIntegration objectToSave)
        {
            var existent = Database.MillenniumImageIntegrations?.Where(w => w.Id == objectToSave.Id).FirstOrDefault();

            if (existent == null)
                Database.Set<MillenniumImageIntegration>().Add(objectToSave);
            else
                Database.Set<MillenniumImageIntegration>().Update(objectToSave);

            Database.SaveChanges();
        }
    }
}
