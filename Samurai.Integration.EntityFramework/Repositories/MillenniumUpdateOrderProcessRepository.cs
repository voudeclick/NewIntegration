using Samurai.Integration.Domain.Entities.Database.Integrations.Millenium;
using Samurai.Integration.EntityFramework.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samurai.Integration.EntityFramework.Repositories
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
