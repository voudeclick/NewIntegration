using Samurai.Integration.Domain.Enums.Millennium;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Entities.Database.TenantData
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
