using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using VDC.Integration.Domain.Entities.Database.Integrations.Shopify;
using VDC.Integration.EntityFramework.Database;

namespace VDC.Integration.EntityFramework.Repositories
{
    public class ShopifyProductIntegrationRepository
    {
        protected readonly DatabaseContext Database;

        public ShopifyProductIntegrationRepository(DatabaseContext context)
        {
            Database = context;
        }

        public async Task Save(ShopifyProductIntegration objectToSave)
        {
            var existent = await Database.ShopifyProductIntegrations?.Where(w => w.Id == objectToSave.Id).AsNoTracking().FirstOrDefaultAsync();

            if (existent == null)
                Database.Set<ShopifyProductIntegration>().Add(objectToSave);
            else
                Database.Set<ShopifyProductIntegration>().Update(objectToSave);

            Database.SaveChanges();
        }
    }
}
