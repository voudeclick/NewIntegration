using Samurai.Integration.Domain.Entities.Database.Integrations.Shopify;
using Samurai.Integration.EntityFramework.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samurai.Integration.EntityFramework.Repositories
{
    public class ShopifyProductImageIntegrationRepository
    {
        protected readonly DatabaseContext Database;

        public ShopifyProductImageIntegrationRepository(DatabaseContext context)
        {
            Database = context;
        }

        public void Save(ShopifyProductImageIntegration objectToSave)
        {
            var existent = Database.ShopifyProductImageIntegrations?.Where(w => w.Id == objectToSave.Id).FirstOrDefault();

            if (existent == null)
                Database.Set<ShopifyProductImageIntegration>().Add(objectToSave);
            else
                Database.Set<ShopifyProductImageIntegration>().Update(objectToSave);

            Database.SaveChanges();
        }
    }
}
