using Samurai.Integration.Domain.Entities.Database.Integrations.Shopify;
using Samurai.Integration.EntityFramework.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samurai.Integration.EntityFramework.Repositories
{
    public class ShopifyProductStockIntegrationRepository
    {
        protected readonly DatabaseContext Database;

        public ShopifyProductStockIntegrationRepository(DatabaseContext context)
        {
            Database = context;
        }

        public void Save(ShopifyProductStockIntegration objectToSave)
        {
            var existent = Database.ShopifyProductStockIntegrations?.Where(w => w.Id == objectToSave.Id).FirstOrDefault();

            if (existent == null)
                Database.Set<ShopifyProductStockIntegration>().Add(objectToSave);
            else
                Database.Set<ShopifyProductStockIntegration>().Update(objectToSave);

            Database.SaveChanges();
        }
    }
}
