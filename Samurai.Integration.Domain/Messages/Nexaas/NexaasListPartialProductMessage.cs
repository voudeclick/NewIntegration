using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.Nexaas
{
    public class NexaasListPartialProductMessage
    {
        public long ProductSkuId { get; set; }
        public bool NewSku { get; set; }
    }
}
