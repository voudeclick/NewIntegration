using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Tray.Models.Requests.Manufacture
{
    public class GetManufactureByFilterRequest
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public int Limit { get => 100; }
        public int Page { get => 1; }
    }
}
