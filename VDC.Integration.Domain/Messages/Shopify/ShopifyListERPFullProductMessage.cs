using System;

namespace VDC.Integration.Domain.Messages.Shopify
{
    public class ShopifyListERPFullProductMessage
    {
        public string ExternalId { get; set; }
        public Guid? IntegrationId { get; set; }
    }
}
