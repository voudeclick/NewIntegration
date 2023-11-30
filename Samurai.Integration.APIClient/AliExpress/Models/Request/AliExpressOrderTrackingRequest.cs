using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.AliExpress.Models.Request
{
    public class AliExpressOrderTrackingRequest
    {
        public long AliExpressOrderId { get; set; }
        public string TrackingCode { get; set; }
    }
}
