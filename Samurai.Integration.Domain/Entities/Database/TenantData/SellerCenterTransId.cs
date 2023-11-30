using Samurai.Integration.Domain.Enums.SellerCenter;
using System;

namespace Samurai.Integration.Domain.Entities.Database.TenantData
{
    public class SellerCenterTransId
    {
        public long Id { get; set; }
        public long SellerCenterDataId { get; set; }
        public DateTimeOffset LastProcessedDate { get; set; }
        public TransIdType Type { get; set; }
    }
}
