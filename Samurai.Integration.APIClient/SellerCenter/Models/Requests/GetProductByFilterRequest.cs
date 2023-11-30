using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Requests
{
    public class GetProductByFilterRequest
    {
        public string ProductCode { get; set; }
        public Guid? SellerId { get; set; }
        public string Name { get; set; }
        public int PageSize { get => 10000; }
        public string PageIndex { get => "0"; }
    }
}
