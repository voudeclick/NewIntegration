using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.AliExpress
{
    public class AliExpressListAllProductsMessage
    {
        public long ProductId { get; set; }
        public string LocalCountry { get; set; }
        public string LocalLanguage { get; set; }
    }
}
