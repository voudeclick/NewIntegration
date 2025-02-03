using System.Collections.Generic;
using VDC.Integration.APIClient.Shopify.Models.Request.Inputs;
using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class ProductCreateMutation : BaseMutation<ProductCreateMutationInput, ProductCreateMutationOutput>
    {
        public ProductCreateMutation(ProductCreateMutationInput variables)
            : base(variables)
        {
        }

        public override string GetQuery()
        {
            return $@"
                mutation productCreate($input: ProductInput!) {{
                  productCreate(input: $input) {{
                    product {{
                        id,
                        legacyResourceId,
                        handle,
                        variants(first: {Variables.input.variants.Count}) {{
                            edges {{
                                node {{
                                    id,
                                    legacyResourceId,
                                    sku
                                }}
                            }}
                        }}
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

    public class ProductCreateMutationInput : BaseMutationInput
    {
        public Product input { get; set; }
    }

    public class ProductCreateMutationOutput : BaseMutationOutput
    {
        public Result productCreate { get; set; }

        public class Result
        {
            public ProductResult product { get; set; }
            public List<UserError> userErrors { get; set; }
        }
    }
}
