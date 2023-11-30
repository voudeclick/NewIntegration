using Samurai.Integration.Domain.Models.SellerCenter.API.Orders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Response
{
    public class GetOrderByFilterResponse
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public List<Order> Value { get; set; }
    }
}
