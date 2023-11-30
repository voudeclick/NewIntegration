using Samurai.Integration.Domain.Shopify.Models.Results;

namespace Samurai.Integration.APIClient.Shopify.Models.Request
{
    public class VariantParentProductsBySkuQuery : BaseQuery<VariantParentProductsBySkuQueryOutput>
    {
        private readonly string _sku;
        public VariantParentProductsBySkuQuery(string sku)
        {
            _sku = sku;
        }

        public override string GetQuery()
        {
            return $@"
                query variantById {{
                    productVariants(first:25, query:""sku:'{_sku}'"") {{
                        edges {{
                            node{{
                                id,
                                legacyResourceId,
                                sku,
                                product {{
                                  id,
                                  legacyResourceId,
                                  tags
                                }}
                            }}
                        }}
                    }}
                }}
            ";
        }
    }

    public class VariantParentProductsBySkuQueryOutput : BaseQueryOutput
    {
        public Connection<VariantResult> productVariants { get; set; }
    }
}
