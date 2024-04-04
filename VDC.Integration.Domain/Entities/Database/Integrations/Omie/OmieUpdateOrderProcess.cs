using System;

namespace VDC.Integration.Domain.Entities.Database.Integrations.Omie
{
    public class OmieUpdateOrderProcess
    {
        public Guid Id { get; set; }
        public long TenantId { get; set; }
        public long OrderId { get; set; }
        public string Payload { get; set; }
        public Guid? ShopifyListOrderProcessReferenceId { get; set; }
        public DateTime ProcessDate { get; set; }
        public string OmieResponse { get; set; }
        public string Exception { get; set; }
    }
}
