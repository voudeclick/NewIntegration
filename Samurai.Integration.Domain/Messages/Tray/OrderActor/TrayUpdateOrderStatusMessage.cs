using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.Tray.OrderActor
{
    public class TrayUpdateOrderStatusMessage
    {
        public string StoreId { get; set; }
        public long TrayOrderId { get; set; }
        public List<AliExpressOrder> AliExpressOrders { get; set; }
        public bool HasProductVirtual { get; set; }
        public long SendStatusId { get; set; }
        public long DeliveryStatusId { get; set; }
        public long CancelStatusId { get; set; }
        public class AliExpressOrder
        {
            public long Id { get; set; }
            public string TrayStatus { get; set; }
            public TrayOrderTracking OrderTracking { get; set; }
            public TrayOrderCancellation OrderCancellation { get; set; }
        }
        public class TrayOrderCancellation
        {
            public string StoreNote { get; set; }
            public string Products { get; set; }
        }

        public class TrayOrderTracking
        {
            public string TrackingCode { get; set; }
            public string LogisticServiceName { get; set; }
        }
    }
}
