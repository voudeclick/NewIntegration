using Samurai.Integration.Domain.Entities.Database.Integrations.Shopify;
using Samurai.Integration.EntityFramework.Database;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Samurai.Integration.EntityFramework.Repositories
{
    public class ShopifyListOrderProcessRepository
    {
        protected readonly DatabaseContext Database;

        public ShopifyListOrderProcessRepository(DatabaseContext context)
        {
            Database = context;
        }

        public async Task Save(ShopifyListOrderProcess objectToSave)
        {
            try
            {
                var existent = await Database.ShopifyListOrderProcesses?.Where(w => w.Id == objectToSave.Id).AsNoTracking().FirstOrDefaultAsync();

                if (existent == null)
                    await Database.Set<ShopifyListOrderProcess>().AddAsync(objectToSave);
                else
                    Database.Set<ShopifyListOrderProcess>().Update(objectToSave);

                Database.SaveChanges();
            }
            catch (System.Exception ex)
            {
                var teste = ex.Message;
                throw ex;
            }            
        }
    }
}
