using System;

namespace Samurai.Integration.Domain.Messages.SellerCenter.OrderActor
{
    public class UpdateOrderStatusMessage
    {
        public Guid? SellerCenterOrderId { get; set; }
        public string OrderExternalId { get; set; }
        public CancellationStatus Cancellation { get; set; }
        public PaymentStatus Payment { get; set; }
        public ShippingStatus Shipping { get; set; }

        public class CancellationStatus
        {
            public bool IsCancelled { get; set; }
        }

        public class PaymentStatus
        {
            public bool IsPaid { get; set; }
        }

        public class ShippingStatus
        {
            public bool IsShipped { get; set; }
            public bool IsDelivered { get; set; }
            public string TrackingObject { get; set; }
            public string TrackingUrl { get; set; }
        }
    }
}
