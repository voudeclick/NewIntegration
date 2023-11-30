using System.Collections.Generic;

namespace Samurai.Integration.APIClient.Millennium.Models.Requests
{
    public class MillenniumApiListOrdersStatusRequest
    {
        public List<long> OrderIds { get; set; }
    }
}
