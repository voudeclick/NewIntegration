using System.Collections.Generic;

namespace VDC.Integration.Domain.Messages.Omie
{
    public class OmieEnqueueFullProductsMessage
    {
        public List<long> ProductsIds { get; set; }
    }
}
