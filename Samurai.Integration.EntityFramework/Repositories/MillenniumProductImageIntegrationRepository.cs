using Samurai.Integration.Domain.Entities.Database.Integrations.Millenium;
using Samurai.Integration.EntityFramework.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samurai.Integration.EntityFramework.Repositories
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
