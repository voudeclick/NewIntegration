using System.Collections.Generic;
using VDC.Integration.APIClient.Shopify.Models.Request.Inputs;
using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class ProductUpdateMutation : BaseMutation<ProductUpdateMutationInput, ProductUpdateMutationOutput>
    {
        public ProductUpdateMutation(ProductUpdateMutationInput variables)
            : base(variables)
        {
        }

        public override string GetQuery()
        {
            return $@"
                mutation productUpdate($input: ProductInput!) {{
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
                  }}
                }}
            ";
        }
    }

    public class ProductUpdateMutationInput : BaseMutationInput
    {
        public Product input { get; set; }
    }

    public class ProductUpdateMutationOutput : BaseMutationOutput
    {
        public Result productUpdate { get; set; }

        public class Result
        {
            public ProductResult product { get; set; }
            public List<UserError> userErrors { get; set; }
        }
    }
}
