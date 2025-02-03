using System;
using System.Collections.Generic;
using VDC.Integration.APIClient.Shopify.Models.Request.Inputs;
using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class InventorySetQuantitiesMutation : BaseMutation<InventorySetQuantitiesInput, InventorySetMutationOutput>
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
        public InventorySetQuantitiesInputInput input { get; set; }

        public class InventorySetQuantitiesInputInput
        {
            public bool ignoreCompareQuantity { get; set; }
            public string reason { get; set; }
            public string name { get; set; }
            public List<InventorySetQuantitiesInputInputQuantities> quantities { get; set; }
        }

        public class InventorySetQuantitiesInputInputQuantities
        {
            public string inventoryItemId { get; set; }
            public string locationId { get; set; }
            public decimal? quantity { get; set; }
        }
    }

    public class InventorySetMutationOutput : BaseMutationOutput
    {
        public InventoryUpdateResult inventorySetQuantities { get; set; }

        public class InventoryUpdateResult
        {
            public List<UserError> userErrors { get; set; }
        }
    }
}