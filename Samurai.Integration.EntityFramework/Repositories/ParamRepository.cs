using Microsoft.EntityFrameworkCore;
using Samurai.Integration.Domain.Consts;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.EntityFramework.Database;
using System.Threading.Tasks;

namespace Samurai.Integration.EntityFramework.Repositories
{
    public class ParamRepository
    {
        protected readonly DatabaseContext _database;

        public ParamRepository(DatabaseContext databaseContext)
        {
            _database = databaseContext;
        }

        public Task<bool> ExistsByKeyAsync(string key)
        {
            return _database.Params.AnyAsync(p => p.Key == key);
        }

        public Task<Param> GetByKeyAsync(string key)
        {
            return _database.Params.FirstOrDefaultAsync(p => p.Key == key);
        }

        public Task<Param> GetByIntegrationMonitorHangfireKeyAsync()
        {
            return GetByKeyAsync(ParamConsts.IntegrationMonitorHangfire);
        }

        public Task AddAsync(Param param)
        {
            _database.Params.Add(param);         
        
            return _database.SaveChangesAsync();
        }

        public Task UpdateAsync(Param param)
        {
            _database.Params.Update(param);
            return _database.SaveChangesAsync();
        }

        public Task DeleteAsync(Param param)
        {
            _database.Params.Remove(param);
            return _database.SaveChangesAsync();
        }
    }
}
