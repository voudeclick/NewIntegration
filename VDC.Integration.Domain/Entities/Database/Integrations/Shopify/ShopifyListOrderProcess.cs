using System;

namespace VDC.Integration.Domain.Entities.Database.Integrations.Shopify
{
    public class ShopifyListOrderProcess
    {
        public Guid Id { get; set; }
        public long TenantId { get; set; }
        public long OrderId { get; set; }
        public DateTime ProcessDate { get; set; }
        public string ShopifyResult { get; set; }
        public string Exception { get; set; }
    }
}
