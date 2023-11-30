using Microsoft.EntityFrameworkCore;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.EntityFramework.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samurai.Integration.EntityFramework.Repositories
{
    public class IntegrationErrorRepository
    {
        protected readonly DatabaseContext _database;

        public IntegrationErrorRepository(DatabaseContext database)
        {
            _database = database;
        }

        public Task<List<IntegrationError>> GetAllAsync()
        {
            return _database.IntegrationErrors.ToListAsync();
        }

        public Task<bool> ExistsByTagAsync(string tag)
        {
            return _database.IntegrationErrors.AnyAsync(p => p.Tag == tag);
        }

        public Task<IntegrationError> GetByTagAsync(string tag)
        {
            return _database.IntegrationErrors.FirstOrDefaultAsync(p => p.Tag == tag);
        }

        public Task<List<IntegrationError>> GetBySourceAsync(IntegrationErrorSource integrationErrorSource)
        {
            return _database.IntegrationErrors.Where(x => x.SourceId == integrationErrorSource).ToListAsync();
        }

        public Task AddAsync(IntegrationError integrationError)
        {
            _database.IntegrationErrors.Add(integrationError);

            return _database.SaveChangesAsync();
        }

        public Task UpdateAsync(IntegrationError integrationError)
        {
            _database.IntegrationErrors.Update(integrationError);
            return _database.SaveChangesAsync();
        }

        public Task DeleteAsync(IntegrationError integrationError)
        {
            _database.IntegrationErrors.Remove(integrationError);
            return _database.SaveChangesAsync();
        }
    }
}
