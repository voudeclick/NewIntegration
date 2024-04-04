using System.Linq;
using VDC.Integration.Domain.Entities.Database.Integrations.Shopify;
using VDC.Integration.EntityFramework.Database;

namespace VDC.Integration.EntityFramework.Repositories
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
