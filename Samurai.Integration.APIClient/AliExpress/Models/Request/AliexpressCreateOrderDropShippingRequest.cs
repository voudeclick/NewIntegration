using Samurai.Integration.Domain.Messages.AliExpress.Order;
using System.Collections.Generic;

namespace Samurai.Integration.APIClient.AliExpress.Models.Request
{
    public class AliexpressCreateOrderDropShippingRequest { 
        public AliexpressAddressDropShipping LogisticsAddress { get; set; }
        public List<AliexpressOrderItem> Items { get; set; }
    }
}
