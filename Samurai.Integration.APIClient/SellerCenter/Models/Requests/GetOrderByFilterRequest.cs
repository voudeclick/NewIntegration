using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Requests
{
    public class GetOrderByFilterRequest
    {
        public DateTime? StartUpdateDate { get; set; }
        public string OrderBy { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get => 0; }
        public string OrderNumber { get; set; }
    }
}
