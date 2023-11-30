using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Tray.Models.Requests.Order
{
    public class GetOrderByFilterRequest
    {
        public long? OrderId { get; set; }
        public string Status { get; set; }
        public long? CustomerId { get; set; }
        public int Limit { get => 50; }
        public int Page { get => 1; }
    }
}
