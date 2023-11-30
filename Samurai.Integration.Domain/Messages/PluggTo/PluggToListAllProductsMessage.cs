using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.PluggTo
{
    public class PluggToListAllProductsMessage
    {
        public DateTime? CreatedAt { get; set; }
        public string AccountUserId { get; set; }
        public string AccountSellerId { get; set; }
    }
}
