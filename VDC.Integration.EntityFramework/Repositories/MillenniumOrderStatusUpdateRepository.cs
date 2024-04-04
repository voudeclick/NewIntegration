using System.Linq;
using VDC.Integration.Domain.Entities.Database;
using VDC.Integration.EntityFramework.Database;

namespace VDC.Integration.EntityFramework.Repositories
{
    public class MillenniumOrderStatusUpdateRepository
    {
        protected readonly DatabaseContext _db;

        public MillenniumOrderStatusUpdateRepository(DatabaseContext db)
        {
            _db = db;
        }

        public void Save(MillenniumOrderStatusUpdate objectToSave)
        {
            var existent = _db.MillenniumOrderStatusUpdate?.Where(w => w.Id == objectToSave.Id).FirstOrDefault();

            if (existent == null)
                _db.Set<MillenniumOrderStatusUpdate>().Add(objectToSave);
            else
                _db.Set<MillenniumOrderStatusUpdate>().Update(objectToSave);

            _db.SaveChanges();
        }
    }
}
