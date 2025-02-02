using System.Collections.Generic;
using VDC.Integration.APIClient.Shopify.Models.Request.Inputs;
using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class VariantUpdateMutation : BaseMutation<VariantUpdateMutationInput, VariantUpdateMutationOutput>
    {
        public VariantUpdateMutation(VariantUpdateMutationInput variables)
            : base(variables)
        {
        }

        public override string GetQuery()
        {
            return $@"
                mutation productVariantsBulkUpdate($productId: ID!, $variants: [ProductVariantsBulkInput!]!) {{
                    productVariantsBulkUpdate(productId: $productId, variants: $variants) {{
                        product {{
                            id
                        }}
                        productVariants {{
                            id
                            metafields(first: 2) {{
                                edges {{
                                    node {{
                                        namespace
                                        key
                                        value
                                    }}
                                }}
                            }}
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

    public class VariantUpdateMutationInput : BaseMutationInput
    {
        public string productId { get; set; }
        public List<VariantUpdateVariantsInput> variants { get; set; }
    }

    public class VariantUpdateMutationOutput : BaseMutationOutput
    {
        public VariantItemResult productVariantsBulkUpdate { get; set; }

        public class VariantItemResult
        {
            public ProductResult product { get; set; }
            public List<UserError> userErrors { get; set; }
        }
    }
}
