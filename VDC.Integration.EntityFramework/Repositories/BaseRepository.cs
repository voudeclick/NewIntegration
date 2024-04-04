using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDC.Integration.Domain.Entities.Database;
using VDC.Integration.EntityFramework.Database;

namespace VDC.Integration.EntityFramework.Repositories
{
    public class BaseRepository<T> where T : BaseEntity
    {
        protected readonly DatabaseContext Database;

        public BaseRepository(DatabaseContext context)
        {
            Database = context;
        }

        protected virtual IQueryable<T> GetBaseQuery()
        {
            return Database.Set<T>();
        }

        public async virtual Task<IEnumerable<T>> GetAll(CancellationToken cancellationToken = default)
        {
            return await GetBaseQuery().ToListAsync(cancellationToken);
        }

        public async virtual Task<T> GetById(long id, CancellationToken cancellationToken = default)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public virtual T GetByIdSync(long id, CancellationToken cancellationToken = default)
        {
            return GetBaseQuery().FirstOrDefault(x => x.Id == id);
        }

        protected DateTime GetDateNow()
        {
            DateTime timeUtc = DateTime.UtcNow;
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Brazilian Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(timeUtc, cstZone);
        }

        public virtual void Save(T objectToSave)
        {
            objectToSave.CreationDate = GetDateNow();
            objectToSave.UpdateDate = objectToSave.CreationDate;
            Database.Set<T>().Add(objectToSave);
        }

        public virtual void Update(T objectToUpdate)
        {
            objectToUpdate.UpdateDate = GetDateNow();
            Database.Set<T>().Update(objectToUpdate);
        }

        public virtual void Upsert(T objectToUpsert)
        {
            if (objectToUpsert.Id != 0)
                Update(objectToUpsert);
            else
                Save(objectToUpsert);
        }

        public virtual void Delete(T objectToDelete)
        {
            Database.Set<T>().Remove(objectToDelete);
        }

        public void Commit()
        {
            Database.SaveChanges();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return Database.SaveChangesAsync(cancellationToken);
        }
    }
}
