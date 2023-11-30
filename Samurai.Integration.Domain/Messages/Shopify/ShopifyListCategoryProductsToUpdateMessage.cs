using Samurai.Integration.Domain.Messages.Shopify;

namespace Samurai.Integration.Domain.Messages.Shopify
{
    public class ShopifyListCategoryProductsToUpdateMessage
    {
        public string CategoryId { get; set; }
    }
}
