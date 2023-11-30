using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Entities.Database.TenantData;

namespace Samurai.Integration.Domain.Entities
{
    public  class MercadoPago : BaseEntity
    {
        public long MillenniumDataId { get; set; }        
        public string Authorization { get; set; }

        public virtual MillenniumData MillenniumData { get; set; }
    }
}
