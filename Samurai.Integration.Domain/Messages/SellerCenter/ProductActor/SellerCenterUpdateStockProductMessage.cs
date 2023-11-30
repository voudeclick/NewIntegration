using Samurai.Integration.Domain.Queues;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.SellerCenter.ProductActor
{
    public class SellerCenterUpdateStockProductMessage : ISecureMessage
    {
        public string ExternalProductId { get; set; }
        public ValueItem Stock { get; set; } = new ValueItem();

        public string ExternalMessageId => ExternalProductId;

        public bool CanSend()
        {
            return Stock.Quantity > 0;
        }

        public class ValueItem {
            public string Sku { get; set; }
            public int Quantity { get; set; }
        }
    }
}
