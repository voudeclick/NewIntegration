using System.Linq;
using VDC.Integration.Domain.Entities.Database.Integrations.Shopify;
using VDC.Integration.EntityFramework.Database;

namespace VDC.Integration.EntityFramework.Repositories
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
