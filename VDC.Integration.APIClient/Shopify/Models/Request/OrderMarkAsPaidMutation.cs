using System.Collections.Generic;
using VDC.Integration.APIClient.Shopify.Models.Request.Inputs;
using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class OrderMarkAsPaidMutation : BaseMutation<OrderMarkAsPaidMutationInput, OrderMarkAsPaidMutationOutput>
    {
        public OrderMarkAsPaidMutation(OrderMarkAsPaidMutationInput variables)
            : base(variables)
        {
        }

        public override string GetQuery()
        {
            return $@"
                mutation orderMarkAsPaid($input: OrderMarkAsPaidInput!) {{
                  orderMarkAsPaid(input: $input) {{
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

    public class OrderMarkAsPaidMutationInput : BaseMutationInput
    {
        public MarkAsPaidInput input { get; set; }
        public class MarkAsPaidInput
        {
            public string id { get; set; }
        }
    }

    public class OrderMarkAsPaidMutationOutput : BaseMutationOutput
    {
        public Result orderMarkAsPaid { get; set; }

        public class Result
        {
            public OrderResult order { get; set; }
            public List<UserError> userErrors { get; set; }
        }
    }
}
