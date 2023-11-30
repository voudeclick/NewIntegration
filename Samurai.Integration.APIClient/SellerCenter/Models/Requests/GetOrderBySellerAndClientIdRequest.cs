using System;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Requests
{
    public class GetOrderBySellerAndClientIdRequest
    {
        public Guid SellerId { get; set; }
        public string ClientId { get; set; }
    }
}
