using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class VariantBySkuQuery : BaseQuery<VariantBySkuQueryOutput>
    {
        private readonly string _sku;
        public VariantBySkuQuery(string sku)
        {
            _sku = sku;
        }

        public override string GetQuery()
        {
            return $@"
                query variantById {{
                    productVariants(first:1, query:""sku:'{_sku}'"") {{
                        edges {{
                            node{{
                                id,
                                legacyResourceId,
                                sku,
                                product{{
                                    title,
                                    legacyResourceId,
                                    id
                                }},
                                selectedOptions{{
                                    name,
                                    value
                                }}
                                metafields(namespace: ""VDC.Integration"", first: 10) {{
                                  edges {{
                                    node {{
                                      id,
                                      key,
                                      value
                                    }}
                                  }}
                                }},
                                inventoryItem {{
                                    id,
                                    inventoryLevels(first:10) {{
                                        edges {{
                                            node {{
                                                id,
                                                available,
                                                location {{
                                                    id,
                                                    legacyResourceId
                                                }}
                                            }}
                                        }}
                                    }}
                                }}
                            }}
                        }}
                    }}
                }}
            ";
        }
    }

    public class VariantBySkuQueryOutput : BaseQueryOutput
    {
        public Connection<VariantResult> productVariants { get; set; }
    }
}
