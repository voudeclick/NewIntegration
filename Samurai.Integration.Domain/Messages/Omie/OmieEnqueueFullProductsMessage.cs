using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.Omie
{
    public class OmieEnqueueFullProductsMessage
    {
        public List<long> ProductsIds { get; set; }
    }
}
