using Microsoft.EntityFrameworkCore;
using Samurai.Integration.Domain.Entities.Database.Integrations.Shopify;
using Samurai.Integration.EntityFramework.Database;
using System.Linq;
using System.Threading.Tasks;

namespace Samurai.Integration.EntityFramework.Repositories
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
