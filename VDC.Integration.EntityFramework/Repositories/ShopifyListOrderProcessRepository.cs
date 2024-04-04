using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using VDC.Integration.Domain.Entities.Database.Integrations.Shopify;
using VDC.Integration.EntityFramework.Database;

namespace VDC.Integration.EntityFramework.Repositories
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
