using System.Collections.Generic;
using VDC.Integration.APIClient.Shopify.Models.Request.Inputs;
using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class VariantCreateMutation : BaseMutation<VariantCreateMutationInput, VariantCreateMutationOutput>
    {
        public VariantCreateMutation(VariantCreateMutationInput variables)
            : base(variables)
        {
        }

        public override string GetQuery()
        {
            return $@"
                mutation ProductVariantsCreate($productId: ID!, $variants: [ProductVariantsBulkInput!]!) {{
                    productVariantsBulkCreate(productId: $productId, variants: $variants) {{
                        productVariants {{
                            id
                            title
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

    public class VariantCreateMutationInput : BaseMutationInput
    {
        public string productId { get; set; }
        public List<VariantCreateVariantsInput> variants { get; set; }
    }

    public class VariantCreateMutationOutput : BaseMutationOutput
    {
        public VariantItemResult productVariantsBulkCreate { get; set; }

        public class VariantItemResult
        {
            public ProductResult product { get; set; }
            public List<UserError> userErrors { get; set; }
        }
    }
}
