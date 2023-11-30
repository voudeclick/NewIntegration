using System;

namespace Samurai.Integration.Domain.Messages.SellerCenter.OrderActor
{
    public class UpdatePartialOrderSellerMessage
    {
        public Guid OrderId { get; set; }
        public Guid OrderSellerId { get; set; }
        public string OrderClientId { get; set; }
        public Guid? StatusId { get; set; }
        public string Notes { get; set; }
    }
}
