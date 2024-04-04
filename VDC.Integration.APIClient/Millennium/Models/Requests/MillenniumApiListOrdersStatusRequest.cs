using System.Collections.Generic;

namespace VDC.Integration.APIClient.Millennium.Models.Requests
{
    public class MillenniumApiListOrdersStatusRequest
    {
        public List<long> OrderIds { get; set; }
    }
}
