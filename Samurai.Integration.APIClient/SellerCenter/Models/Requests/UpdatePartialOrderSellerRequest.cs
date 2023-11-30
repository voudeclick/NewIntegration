using System;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Requests
{
    public class UpdatePartialOrderSellerRequest
    {
        public Guid OrderId { get; set; }
        public Guid OrderSellerId { get; set; }
        public string OrderClientId { get; set; }
        public Guid? StatusId { get; set; }
        public string Notes { get; set; }
    }
}
