using System.Linq;
using VDC.Integration.Domain.Entities.Database.Integrations.Shopify;
using VDC.Integration.EntityFramework.Database;

namespace VDC.Integration.EntityFramework.Repositories
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
