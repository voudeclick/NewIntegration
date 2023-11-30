using Samurai.Integration.Domain.Messages.Shopify;

namespace Samurai.Integration.Domain.Messages.Shopify
{
    public class ShopifyUpdateVendorMessage
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
