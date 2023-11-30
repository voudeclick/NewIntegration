using Samurai.Integration.Domain.Entities.Database.Integrations.Millenium;
using Samurai.Integration.EntityFramework.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samurai.Integration.EntityFramework.Repositories
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
