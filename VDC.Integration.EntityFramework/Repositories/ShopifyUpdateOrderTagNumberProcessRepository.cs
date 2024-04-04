using System.Linq;
using VDC.Integration.Domain.Entities.Database.Integrations.Shopify;
using VDC.Integration.EntityFramework.Database;

namespace VDC.Integration.EntityFramework.Repositories
{
    public class ShopifyUpdateOrderTagNumberProcessRepository
    {
        protected readonly DatabaseContext Database;

        public ShopifyUpdateOrderTagNumberProcessRepository(DatabaseContext context)
        {
            Database = context;
        }

        public void Save(ShopifyUpdateOrderTagNumberProcess objectToSave)
        {
            var existent = Database.ShopifyUpdateOrderTagNumberProcesses?.Where(w => w.Id == objectToSave.Id).FirstOrDefault();

            if (existent == null)
                Database.Set<ShopifyUpdateOrderTagNumberProcess>().Add(objectToSave);
            else
                Database.Set<ShopifyUpdateOrderTagNumberProcess>().Update(objectToSave);

            Database.SaveChanges();
        }
    }
}
