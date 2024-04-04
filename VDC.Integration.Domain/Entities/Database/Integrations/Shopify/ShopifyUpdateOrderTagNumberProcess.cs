using System;

namespace VDC.Integration.Domain.Entities.Database.Integrations.Shopify
{
    public class ShopifyUpdateOrderTagNumberProcess
    {
        public Guid Id { get; set; }
        public long TenantId { get; set; }
        public long OrderId { get; set; }
        public string OrderExternalId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime ProcessDate { get; set; }
        public string OrderUpdateMutationInput { get; set; }
        public string ShopifyResult { get; set; }
        public string Exception { get; set; }
        public Guid? ShopifyListOrderProcessReferenceId { get; set; }
    }
}
