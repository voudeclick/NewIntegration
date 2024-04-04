using System.Collections.Generic;
using VDC.Integration.APIClient.Shopify.Models.Request.Inputs;
using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class ProductAppendImagesMutation : BaseMutation<ProductAppendImagesMutationInput, ProductAppendImagesMutationOutput>
    {
        public ProductAppendImagesMutation(ProductAppendImagesMutationInput variables)
            : base(variables)
        {
        }

        public override string GetQuery()
        {
            return $@"
                mutation productAppendImages($input: ProductAppendImagesInput!) {{
                  productAppendImages(input: $input) {{
                    newImages {{
                        id,
                        originalSrc
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

    public class ProductAppendImagesMutationInput : BaseMutationInput
    {
        public ProductAppendImage input { get; set; }
    }

    public class ProductAppendImagesMutationOutput : BaseMutationOutput
    {
        public Result productAppendImages { get; set; }

        public class Result
        {
            public List<ImageResult> newImages { get; set; }
            public List<UserError> userErrors { get; set; }
        }
    }
}
