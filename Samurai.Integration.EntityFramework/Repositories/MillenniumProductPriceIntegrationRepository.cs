using Samurai.Integration.Domain.Entities.Database.Integrations.Millenium;
using Samurai.Integration.EntityFramework.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samurai.Integration.EntityFramework.Repositories
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
