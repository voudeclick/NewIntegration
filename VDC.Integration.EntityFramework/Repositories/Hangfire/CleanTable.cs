using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VDC.Integration.EntityFramework.Database;

namespace VDC.Integration.EntityFramework.Repositories.Hangfire
{
    public class CleanTable<T> where T : class
    {
        protected readonly DatabaseContext Database;
        protected readonly DbSet<T> DbSet;
        private readonly ILogger<CleanTable<T>> _logger;
        private readonly IServiceProvider _serviceProvider;

        public CleanTable(DatabaseContext context, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Database = context;
            Database.Database.SetCommandTimeout(3800);
            DbSet = Database.Set<T>();
            using var scope = _serviceProvider.CreateScope();
            _logger = scope.ServiceProvider.GetRequiredService<ILogger<CleanTable<T>>>();
        }

        public async Task ClearDatabase(Expression<Func<T, bool>> query)
        {
            using var transaction = Database.Database.BeginTransaction();
            try
            {
                var existent = await DbSet.Where(query).AsNoTracking().ToListAsync();

                if (existent == null || existent.Count < 1)
                    return;

                DbSet.RemoveRange(existent);
                await Database.SaveChangesAsync();

                transaction.Commit();
            }
            catch (SqlException sx)
            {
                transaction.Rollback();
                _logger.LogWarning($"SqlException: {sx}");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogWarning($"Exception: {ex}");
            }
        }
    }
}