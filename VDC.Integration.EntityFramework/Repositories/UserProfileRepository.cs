using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using VDC.Integration.Domain.Entities.Database;
using VDC.Integration.EntityFramework.Database;

namespace VDC.Integration.EntityFramework.Repositories
{
    public class UserProfileRepository
    {
        protected readonly DatabaseContext _database;


        public UserProfileRepository(DatabaseContext databaseContext)
        {
            _database = databaseContext;
        }

        public Task<bool> ExistsByIdAsync(string id)
        {
            return _database.UsersProfile.AnyAsync(p => p.Id == id);
        }

        public Task<List<UserProfile>> GetAllAsync()
        {
            return _database.UsersProfile.ToListAsync();
        }


        public UserProfile GetById(string id)
        {
            return _database.UsersProfile.Find(id);
        }

        public string GetNameById(string id)
        {
            return _database.UsersProfile.Find(id)?.Name;
        }

        public Task AddAsync(UserProfile userProfile)
        {
            _database.UsersProfile.Add(userProfile);

            return _database.SaveChangesAsync();
        }

        public Task UpdateAsync(UserProfile userProfile)
        {
            _database.UsersProfile.Update(userProfile);
            return _database.SaveChangesAsync();
        }

        public Task DeleteAsync(UserProfile userProfile)
        {
            _database.UsersProfile.Remove(userProfile);
            return _database.SaveChangesAsync();
        }
    }
}
