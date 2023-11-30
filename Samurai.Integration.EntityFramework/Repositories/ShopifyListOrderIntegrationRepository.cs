using Samurai.Integration.Domain.Entities.Database.Integrations.Shopify;
using Samurai.Integration.EntityFramework.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samurai.Integration.EntityFramework.Repositories
{
    public class ShopifyListOrderIntegrationRepository
    {
        protected readonly DatabaseContext Database;

        public ShopifyListOrderIntegrationRepository(DatabaseContext context)
        {
            Database = context;
        }

        public virtual void Save(ShopifyListOrderIntegration objectToSave)
        {
            var existent = Database.ShopifyListOrderIntegrations?.Where(w => w.Id == objectToSave.Id).FirstOrDefault();

            if (existent == null)
                Database.Set<ShopifyListOrderIntegration>().Add(objectToSave);
            else
                Database.Set<ShopifyListOrderIntegration>().Update(objectToSave);

            Database.SaveChanges();
        }
    }
}
