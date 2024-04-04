using System.Linq;
using VDC.Integration.Domain.Entities.Database.Integrations.Millenium;
using VDC.Integration.EntityFramework.Database;

namespace VDC.Integration.EntityFramework.Repositories
{
    public class MillenniumProductPriceIntegrationRepository
    {
        protected readonly DatabaseContext Database;

        public MillenniumProductPriceIntegrationRepository()
        {

        }

        public MillenniumProductPriceIntegrationRepository(DatabaseContext context)
        {
            Database = context;
        }

        public virtual void Save(MillenniumProductPriceIntegration objectToSave)
        {
            var existent = Database.MillenniumProductPriceIntegrations?.Where(w => w.Id == objectToSave.Id).FirstOrDefault();

            if (existent == null)
                Database.Set<MillenniumProductPriceIntegration>().Add(objectToSave);
            else
                Database.Set<MillenniumProductPriceIntegration>().Update(objectToSave);

            Database.SaveChanges();
        }
    }
}
