using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.Shopify
{
    public class ShopifyListERPFullProductMessage
    {
        public string ExternalId { get; set; }
        public Guid? IntegrationId { get; set; }
    }
}
