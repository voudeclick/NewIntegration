using Samurai.Integration.APIClient.Shopify.Models.Request.Inputs;
using Samurai.Integration.Domain.Shopify.Models.Results;
using System.Collections.Generic;

namespace Samurai.Integration.APIClient.Shopify.Models.Request
{
    public class ProductAndInventoryUpdateMutation : BaseMutation<ProductAndInventoryUpdateMutationInput, ProductAndInventoryUpdateMutationOutput>
    {
        public ProductAndInventoryUpdateMutation(ProductAndInventoryUpdateMutationInput variables)
            : base(variables)
        {
        }

        public override string GetQuery()
        {
            return $@"
                mutation productAndInventoryUpdate($input: ProductInput!, $inventoryItemAdjustments: [InventoryAdjustItemInput!]!, $locationId: ID!) {{
                  productUpdate(input: $input) {{
                    product {{
                        id,
                        legacyResourceId,
                        handle
                    }},
                    userErrors {{
                        field,
                        message
                    }}
                  }},
                  inventoryBulkAdjustQuantityAtLocation(inventoryItemAdjustments: $inventoryItemAdjustments, locationId: $locationId) {{
                    inventoryLevels {{
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

    public class ProductAndInventoryUpdateMutationInput : BaseMutationInput
    {
        public Product input { get; set; }

        public List<InventoryAdjustment> inventoryItemAdjustments { get; set; }

        public string locationId { get; set; }
    }

    public class ProductAndInventoryUpdateMutationOutput : BaseMutationOutput
    {
        public ProductUpdateResult productUpdate { get; set; }
        public InventoryUpdateResult inventoryBulkAdjustQuantityAtLocation { get; set; }

        public class ProductUpdateResult
        {
            public ProductResult product { get; set; }
            public List<UserError> userErrors { get; set; }
        }

        public class InventoryUpdateResult
        {
            public List<InventoryLevelResult> inventoryLevels { get; set; }
            public List<UserError> userErrors { get; set; }
        }
    }
}
