using Samurai.Integration.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Entities.Database.Integrations.Shopify
{
    public class ShopifyProductStockIntegration
    {
        public Guid Id { get; set; }
        public long TenantId { get; set; }
        public long? ProductShopifyId { get; set; }
        public string ProductShopifySku { get; set; }
        public string Payload { get; set; }
        public IntegrationStatus Status { get; set; }
        public Guid? ReferenceIntegrationId { get; set; }
        public string Result { get; set; }
        public DateTime IntegrationDate { get; set; }
    }
}
