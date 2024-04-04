using System.Collections.Generic;
using VDC.Integration.APIClient.Shopify.Models.Request.Inputs;
using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class InventoryActivateMutation : BaseMutation<InventoryActivateMutationInput, InventoryActivateMutationOutput>
    {
        public InventoryActivateMutation(InventoryActivateMutationInput variables)
            : base(variables)
        {
        }

        public override string GetQuery()
        {
            return $@"
                mutation inventoryActivate($inventoryItemId: ID!, $locationId: ID!, $available: Int ) {{
                  inventoryActivate(inventoryItemId: $inventoryItemId, locationId: $locationId, available: $available) {{
                    inventoryLevel {{
                      id
                    }}
                    userErrors {{
                      field
                      message
                    }}
                  }}
                }}

            ";
        }
    }

    public class InventoryActivateMutationInput : BaseMutationInput
    {
        public string inventoryItemId { get; set; }
        public string locationId { get; set; }
        public int available { get; set; } = 0;

    }

    public class InventoryActivateMutationOutput : BaseMutationOutput
    {
        public VariantResult inventoryActivate { get; set; }

        public class VariantResult
        {
            public InventoryLevelResult inventoryLevel { get; set; }
            public List<UserError> userErrors { get; set; }
        }
    }
}
