using System.Collections.Generic;
using VDC.Integration.APIClient.Shopify.Models.Request.Inputs;
using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class OrderUpdateMutation : BaseMutation<OrderUpdateMutationInput, OrderUpdateMutationOutput>
    {
        public OrderUpdateMutation(OrderUpdateMutationInput variables)
            : base(variables)
        {
        }

        public override string GetQuery()
        {
            return $@"
                mutation orderUpdate($input: OrderInput!) {{
                  orderUpdate(input: $input) {{
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

    public class OrderUpdateMutationInput : BaseMutationInput
    {
        public Order input { get; set; }
    }

    public class OrderUpdateMutationOutput : BaseMutationOutput
    {
        public Result orderUpdate { get; set; }

        public class Result
        {
            public OrderResult order { get; set; }
            public List<UserError> userErrors { get; set; }
        }
    }
}
