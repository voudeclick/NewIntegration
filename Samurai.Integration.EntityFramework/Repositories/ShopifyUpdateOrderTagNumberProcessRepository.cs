using Samurai.Integration.Domain.Entities.Database.Integrations.Shopify;
using Samurai.Integration.EntityFramework.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samurai.Integration.EntityFramework.Repositories
{
    public class ShopifyUpdateOrderTagNumberProcessRepository
    {
        protected readonly DatabaseContext Database;

        public ShopifyUpdateOrderTagNumberProcessRepository(DatabaseContext context)
        {
            Database = context;
        }

        public void Save(ShopifyUpdateOrderTagNumberProcess objectToSave)
        {
            var existent = Database.ShopifyUpdateOrderTagNumberProcesses?.Where(w => w.Id == objectToSave.Id).FirstOrDefault();

            if (existent == null)
                Database.Set<ShopifyUpdateOrderTagNumberProcess>().Add(objectToSave);
            else
                Database.Set<ShopifyUpdateOrderTagNumberProcess>().Update(objectToSave);

            Database.SaveChanges();
        }
    }
}
