using System.Collections.Generic;
using VDC.Integration.APIClient.Shopify.Models.Request.Inputs;
using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class InventoryUpdateMutation : BaseMutation<InventoryUpdateMutationInput, InventoryUpdateMutationOutput>
    {
        public InventoryUpdateMutation(InventoryUpdateMutationInput variables)
            : base(variables)
        {
        }

        public override string GetQuery()
        {
            return $@"
                mutation inventoryUpdate($input: InventoryAdjustQuantityInput!) {{
                    inventoryAdjustQuantity(input: $input) {{
                        inventoryLevel {{
                            id
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

    public class InventoryUpdateMutationInput : BaseMutationInput
    {
        public InventoryLevel input { get; set; }
    }

    public class InventoryUpdateMutationOutput : BaseMutationOutput
    {
        public InventoryUpdateResult inventoryAdjustQuantity { get; set; }

        public class InventoryUpdateResult
        {
            public InventoryLevelResult inventoryLevel { get; set; }
            public List<UserError> userErrors { get; set; }
        }
    }
}
