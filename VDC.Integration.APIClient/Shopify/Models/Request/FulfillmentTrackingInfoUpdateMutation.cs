using System.Collections.Generic;
using VDC.Integration.APIClient.Shopify.Models.Request.Inputs;
using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class FulfillmentTrackingInfoUpdateMutation : BaseMutation<FulfillmentTrackingInfoUpdateMutationInput, FulfillmentTrackingInfoUpdateMutationOutput>
    {
        public FulfillmentTrackingInfoUpdateMutation(FulfillmentTrackingInfoUpdateMutationInput variables)
            : base(variables)
        {
        }

        public override string GetQuery()
        {
            return $@"
                mutation fulfillmentTrackingInfoUpdate($fulfillmentId: ID!, $trackingInfoUpdateInput: TrackingInfoUpdateInput!) {{
                  fulfillmentTrackingInfoUpdate(fulfillmentId: $fulfillmentId, trackingInfoUpdateInput: $trackingInfoUpdateInput) {{
                    fulfillment {{
                        id,
                        legacyResourceId
                    }},
                    userErrors {{
                        field,
                        message
                    }}
                  }}
                }}
            ";
        }
    }

    public class FulfillmentTrackingInfoUpdateMutationInput : BaseMutationInput
    {
        public string fulfillmentId { get; set; }
        public TrackingInfoUpdateInput trackingInfoUpdateInput { get; set; }

        public class TrackingInfoUpdateInput
        {
            public bool notifyCustomer { get; set; }
            public string trackingCompany { get; set; }
            public TrackingInfoInput trackingDetails { get; set; }
        }

        public class TrackingInfoInput
        {
            public string number { get; set; }
            public string url { get; set; }
        }
    }

    public class FulfillmentTrackingInfoUpdateMutationOutput : BaseMutationOutput
    {
        public Result fulfillmentTrackingInfoUpdate { get; set; }

        public class Result
        {
            public FulfillmentResult fulfillment { get; set; }
            public List<UserError> userErrors { get; set; }
        }
    }
}
