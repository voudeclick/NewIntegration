using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Tray.Models.Requests.Customer
{
    public class GetCustomerByFilterRequest
    {
        public long CustomerId { get; set; }
        public int Limit { get => 50; }
        public int Page { get => 1; }

        public bool IncludeAddress { get; set; } = false;
    }
}
