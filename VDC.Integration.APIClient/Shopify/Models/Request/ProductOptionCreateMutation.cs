using System.Collections.Generic;
using VDC.Integration.APIClient.Shopify.Models.Request.Inputs;
using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class ProductOptionCreateMutation : BaseMutation<ProductOptionCreateMutationInput, ProductOptionCreateMutationOutput>
    {
        public ProductOptionCreateMutation(ProductOptionCreateMutationInput variables)
            : base(variables)
        {
        }

        public override string GetQuery()
        {
            return $@"
                mutation createOptions($productId: ID!, $options: [OptionCreateInput!]!) {{
                    productOptionsCreate(productId: $productId, options: $options) {{ 
                        userErrors {{
                            field
                            message
                            code
                        }}
                        product {{
                            variants(first: 1) {{
                                edges {{
                                    node {{
                                        id
                                    }}
                                }}
                            }}
                        }}
                    }}
                }}
            ";
        }
    }

    public class ProductOptionCreateMutationInput : BaseMutationInput
    {
        public string productId { get; set; }
        public List<ProductOptionCreateOptionMutationInput> options { get; set; }

        public class ProductOptionCreateOptionMutationInput
        {
            public string name { get; set; }
            public List<ProductOptionCreateOptionValueMutationInput> values { get; set; }
        }

        public class ProductOptionCreateOptionValueMutationInput
        {
            public string name { get; set; }
        }

    }

    public class ProductOptionCreateMutationOutput : BaseMutationOutput
    {
        public ProductOptionsCreate productOptionsCreate { get; set; }

        public class ProductOptionsCreate
        {
            public ProductOptionCreateProductMutationOutput product { get; set; }
            public List<UserError> userErrors { get; set; }
        }

        public class ProductOptionCreateProductMutationOutput
        {
            public Connection<ProductOptionCreateProductVariantMutationOutput> variants { get; set; }
        }

        public class ProductOptionCreateProductVariantMutationOutput
        {
            public string id { get; set; }
        }
    }
}
