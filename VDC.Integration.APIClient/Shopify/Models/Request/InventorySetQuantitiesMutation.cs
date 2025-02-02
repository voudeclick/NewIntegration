using System.Collections.Generic;
using VDC.Integration.APIClient.Shopify.Models.Request.Inputs;
using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class InventorySetQuantitiesMutation : BaseMutation<InventorySetQuantitiesInput, InventoryUpdateMutationOutput>
    {
        public InventorySetQuantitiesMutation(InventorySetQuantitiesInput variables)
            : base(variables)
        {
        }

        public override string GetQuery()
        {
            return $@"
                mutation InventorySet($input: InventorySetQuantitiesInput!) {{
                    inventorySetQuantities(input: $input) {{
                        userErrors {{
                            field,
                            message
                        }}
                    }}
                }}
            ";
        }
    }

    public class InventorySetQuantitiesInput : BaseMutationInput
    {
        public InventoryLevel input { get; set; }
    }

    public class InventoryUpdateMutationOutput : BaseMutationOutput
    {
        public InventoryUpdateResult inventoryAdjustQuantity { get; set; }

        public class InventoryUpdateResult
        {
            public List<UserError> userErrors { get; set; }
        }
    }
}
