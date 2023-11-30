using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.AliExpress.Order
{
    public class AliExpressGetOrderMessage
    {
        public string StoreId { get; set; }
        public long TrayId { get; set; }
        public bool HasProductVirtual { get; set; }
        public long SendStatusId { get; set; }
        public long DeliveryStatusId { get; set; }
        public long CancelStatusId { get; set; }
        public List<AliExpressOrder> AliExpressOrdersIds { get; set; }

        public class AliExpressOrder
        {
            public long AliExpressOrderId { get; set; }
            public string Status { get; set; }
        }
    }
}
