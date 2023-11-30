using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samurai.Integration.Domain.Dtos
{
    public class StoreLostedOrdersDto
    {        
        public string StoreName { get; set; }
        public string ShopifyStoreDomain { get; set; }
        public int TotalOrdersLosted { get => Orders.Count();}
        public IEnumerable<OrderDto> Orders { get; set; } = new List<OrderDto>();
    }
}
