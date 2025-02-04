using System.Collections.Generic;
using VDC.Integration.APIClient.Shopify.Models.Request.Inputs;
using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class ProductCreateMutation : BaseMutation<ProductCreateInput, ProductCreateOutput>
    {
        public ProductCreateMutation(ProductCreateInput variables)
            : base(variables)
        {
        }

        public override string GetQuery()
        {
            return $@"
                mutation CreateProductWithNewMedia($product: ProductCreateInput!, $media: [CreateMediaInput!]!) {{
                  productCreate(product: $product, media: $media) {{
                    product {{
                        id
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

    public class ProductCreateInput : BaseMutationInput
    {
        public ProductCreateProductInput product {  get; set; }
        public List<ProductCreateMediaInput> media { get; set; }

        public class ProductCreateProductInput
        {
            public string title { get; set; }
            public string descriptionHtml { get; set; }
            public string vendor { get; set; }
            //public List<string> productOptions { get; set; }
            public List<string> tags { get; set; }
            public List<Metafield> metafields { get; set; }
        }

        public class ProductCreateMediaInput
        {
            public string mediaContentType { get; set; }
            public string originalSource { get; set; }
        }
    }

    public class ProductCreateOutput : BaseMutationOutput
    {
        public Result productCreate { get; set; }

        public class Result
        {
            public ProductResult product { get; set; }
            public List<UserError> userErrors { get; set; }
        }
    }
}
