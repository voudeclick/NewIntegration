
using Samurai.Integration.Domain.Shopify.Models.Results;

namespace Samurai.Integration.APIClient.Shopify.Models.Request
{
    public class VariantByIdQuery : BaseQuery<VariantByIdQueryOutput>
    {
        private readonly long _id;
        public VariantByIdQuery(long id)
        {
            _id = id;
        }

        public override string GetQuery()
        {
            return $@"
                query variantById {{
                    productVariant(id: ""gid://shopify/ProductVariant/{_id}"") {{
                        id,
                        legacyResourceId,
                        sku,
                        selectedOptions{{
                            name,
                            value
                        }}
                        metafields(namespace: ""Samurai.Integration"", first: 10) {{
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
                            inventoryLevels(first:1) {{
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
            ";
        }
    }

    public class VariantByIdQueryOutput : BaseQueryOutput
    {
        public VariantResult productVariant { get; set; }
    }
}
