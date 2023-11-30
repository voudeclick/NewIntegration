using Samurai.Integration.Domain.Entities.Database.Integrations.Shopify;
using Samurai.Integration.EntityFramework.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samurai.Integration.EntityFramework.Repositories
{
    public class ShopifyProductPriceIntegrationRepository
    {
        protected readonly DatabaseContext Database;

        public ShopifyProductPriceIntegrationRepository(DatabaseContext context)
        {
            Database = context;
        }

        public virtual void Save(ShopifyProductPriceIntegration objectToSave)
        {
            var existent = Database.ShopifyProductPriceIntegrations?.Where(w => w.Id == objectToSave.Id).FirstOrDefault();

            if (existent == null)
                Database.Set<ShopifyProductPriceIntegration>().Add(objectToSave);
            else
                Database.Set<ShopifyProductPriceIntegration>().Update(objectToSave);

            Database.SaveChanges();
        }
    }
}
