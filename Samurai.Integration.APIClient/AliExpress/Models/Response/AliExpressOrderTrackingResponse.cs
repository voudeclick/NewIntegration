using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.AliExpress.Models.Response
{
    public class AliExpressOrderTrackingResponse
    {
        [JsonProperty("details")]
        public AliExpressOrderTrackingDetails OrderTracking { get; set; }

        [JsonProperty("official_website")]
        public string Website { get; set; }

        [JsonProperty("error_desc")]
        public string ErrorDesc { get; set; }

        [JsonProperty("result_success")]
        public bool ResultSuccess { get; set; }

        public class AliExpressOrderTrackingDetails
        {
            [JsonProperty("details")]
            public List<AliExpressOrderTrackingDetail> Details { get; set; }
        }

        public class AliExpressOrderTrackingDetail
        {
            [JsonProperty("event_desc")]
            public string PreviousStatus { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("signed_name")]
            public string SignedName { get; set; }

            [JsonProperty("address")]
            public string Address { get; set; }

            [JsonProperty("event_date")]
            public string EventDate { get; set; }
        }
    }
}
