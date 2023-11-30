using Newtonsoft.Json;
using Samurai.Integration.Domain.Models.SellerCenter.API.Enums;
using System;
using System.Collections.Generic;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Requests
{
    public class UpdatePartialOrderRequest
    {
        public Guid OrderId { get; set; }
        public Guid StatusId { get; set; }
        public Guid CancelId { get; set; }

    }
}
