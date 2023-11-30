using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.Nexaas
{
    public class NexaasEnqueueFullProductsMessage
    {
        public List<long> ProductsIds { get; set; }
    }
}
