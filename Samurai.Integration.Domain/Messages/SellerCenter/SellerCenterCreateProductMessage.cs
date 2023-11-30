using Samurai.Integration.Domain.Models;

namespace Samurai.Integration.Domain.Messages.SellerCenter
{
    public class SellerCenterCreateProductMessage : BaseProduct
    {
        public override Models.Product.Info ProductInfo { get; set; }
        public bool HasPriceStock { get; set; }
    }
}
