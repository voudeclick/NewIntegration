using Samurai.Integration.APIClient.Shopify.Models.Request.Inputs;
using Samurai.Integration.Domain.Shopify.Models.Results;
using System.Collections.Generic;

namespace Samurai.Integration.APIClient.Shopify.Models.Request
{
    public class FulfillmentCreateMutation : BaseMutation<FulfillmentCreateMutationInput, FulfillmentCreateMutationOutput>
    {
        public FulfillmentCreateMutation(FulfillmentCreateMutationInput variables)
            : base(variables)
        {
        }

        public override string GetQuery()
        {
            return $@"
                mutation fulfillmentCreate($input: FulfillmentInput!) {{
                  fulfillmentCreate(input: $input) {{
                    fulfillment {{
                        id,
                        legacyResourceId
                    }},
                    order {{
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

    public class FulfillmentCreateMutationInput : BaseMutationInput
    {
        public FullfillmentInput input { get; set; }

        public class FullfillmentInput
        {
            public string locationId { get; set; }
            public string orderId { get; set; }
            public bool notifyCustomer { get; set; }
            public string shippingMethod { get; set; }
            public string trackingCompany { get; set; }
            public List<string> trackingNumbers { get; set; }
            public List<string> trackingUrls { get; set; }
        }
    }

    public class FulfillmentCreateMutationOutput : BaseMutationOutput
    {
        public Result fulfillmentCreate { get; set; }

        public class Result
        {
            public FulfillmentResult fulfillment { get; set; }
            public OrderResult order { get; set; }
            public List<UserError> userErrors { get; set; }
        }
    }
}
