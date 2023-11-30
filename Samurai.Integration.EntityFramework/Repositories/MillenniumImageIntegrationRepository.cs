using Samurai.Integration.Domain.Entities.Database.Integrations.Millenium;
using Samurai.Integration.EntityFramework.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samurai.Integration.EntityFramework.Repositories
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
