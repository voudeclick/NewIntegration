using System.Linq;
using VDC.Integration.Domain.Entities.Database.Integrations.Shopify;
using VDC.Integration.EntityFramework.Database;

namespace VDC.Integration.EntityFramework.Repositories
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
