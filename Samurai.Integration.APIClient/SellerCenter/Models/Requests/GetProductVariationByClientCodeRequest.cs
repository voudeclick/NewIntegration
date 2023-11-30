using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Requests
{
    public class GetProductVariationByClientCodeRequest
    {
        public string ClientCode { get; set; }
        public string SellerId { get; set; }
    }
}
