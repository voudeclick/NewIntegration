using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.EntityFramework.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samurai.Integration.EntityFramework.Repositories
{
    public class MillenniumListProductManualProcessRepository
    {
        protected readonly DatabaseContext Database;

        public MillenniumListProductManualProcessRepository()
        {

        }

        public MillenniumListProductManualProcessRepository(DatabaseContext context)
        {
            Database = context;
        }

        public virtual void Save(MillenniumListProductManualProcess objectToSave)
        {
            var existent = Database.MillenniumListProductManualProcesses?.Where(w => w.Id == objectToSave.Id).FirstOrDefault();

            if (existent == null)
                Database.Set<MillenniumListProductManualProcess>().Add(objectToSave);
            else
                Database.Set<MillenniumListProductManualProcess>().Update(objectToSave);

            Database.SaveChanges();
        }
    }
}
