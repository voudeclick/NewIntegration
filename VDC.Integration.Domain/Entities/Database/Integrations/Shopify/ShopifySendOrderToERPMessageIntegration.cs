using System;

namespace VDC.Integration.Domain.Entities.Database.Integrations.Shopify
{
    public class ShopifyListOrderIntegration
    {
        public Guid Id { get; set; }
        public long TenantId { get; set; }
        public long OrderId { get; set; }
        public string Payload { get; set; }
        public string Action { get; set; }
        public DateTime IntegrationDate { get; set; }
        public Guid? ShopifyListOrderProcessId { get; set; }
    }
}
