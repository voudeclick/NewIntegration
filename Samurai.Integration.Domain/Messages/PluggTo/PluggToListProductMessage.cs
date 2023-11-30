using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.PluggTo
{
    public class PluggToListProductMessage
    {
        public string ExternalId { get; set; }
        public string Sku { get; set; }
        public string AccountUserId { get; set; }
        public string AccountSellerId { get; set; }
        public string Product { get; set; }

    }
}
