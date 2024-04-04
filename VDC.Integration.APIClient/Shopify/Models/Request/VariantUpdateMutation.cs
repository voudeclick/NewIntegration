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
                mutation productVariantUpdate($input: ProductVariantInput!) {{
                  productVariantUpdate(input: $input) {{
                    product {{
                        id,
                        legacyResourceId
                    }},
                    productVariant {{
                        id,
                        legacyResourceId,
                        sku
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

    public class VariantUpdateMutationInput : BaseMutationInput
    {
        public Variant input { get; set; }
    }

    public class VariantUpdateMutationOutput : BaseMutationOutput
    {
        public VariantResult productVariantUpdate { get; set; }

        public class VariantResult
        {
            public ProductResult product { get; set; }
            public VariantResult productVariant { get; set; }
            public List<UserError> userErrors { get; set; }
        }
    }
}
