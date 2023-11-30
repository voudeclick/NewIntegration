using Microsoft.EntityFrameworkCore;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.EntityFramework.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Samurai.Integration.EntityFramework.Repositories
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
