using System;
using VDC.Integration.Domain.Enums.Millennium;

namespace VDC.Integration.Domain.Entities.Database.TenantData
{
    public class MillenniumTransId
    {
        public long Id { get; set; }
        public long MillenniumDataId { get; set; }
        public TransIdType Type { get; set; }
        public long Value { get; set; }
        public DateTime? MillenniumLastUpdateDate { get; set; }
    }
}
