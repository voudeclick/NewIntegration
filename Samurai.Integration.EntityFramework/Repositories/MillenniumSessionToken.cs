using Microsoft.EntityFrameworkCore;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.EntityFramework.Database;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Samurai.Integration.EntityFramework.Repositories
{
    public class MillenniumSessionToken
    {
        protected readonly DatabaseContext Database;

        public MillenniumSessionToken(DatabaseContext context)
        {
            Database = context;
        }

        public async Task Save(long millenniumDataId, string sessionToken)
        {
            try
            {
                var existent = await Database.Tenants.Where(w => w.Id == millenniumDataId).FirstOrDefaultAsync();
                if (existent != null)
                {
                    existent.MillenniumData.SessionToken = sessionToken;
                    Database.SaveChanges();

                    return;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            throw new Exception("Tenant não encontrado");
        }
    }
}
