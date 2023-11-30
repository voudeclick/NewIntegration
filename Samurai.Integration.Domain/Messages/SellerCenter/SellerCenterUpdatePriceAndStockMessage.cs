using Samurai.Integration.Domain.Queues;
using System.Collections.Generic;

namespace Samurai.Integration.Domain.Messages.SellerCenter
{
    public class SellerCenterUpdatePriceAndStockMessage : ISecureMessage
    {
        public SellerCenterUpdatePriceAndStockMessage()
        {
            Values = new List<Models.Product.SkuPrice>();
        }
        public string ProductId { get; set; }
        public List<Models.Product.SkuPrice> Values { get; set; }
        public string ExternalMessageId => ProductId;

        public bool CanSend()
        {
            return Values.Count > 0;
        }
    }
}
