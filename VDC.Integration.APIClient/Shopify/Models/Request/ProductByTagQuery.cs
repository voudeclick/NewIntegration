using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class ProductByTagQuery : BaseQuery<ProductByTagQueryOutput>
    {
        private readonly string _tag;
        private readonly string _cursor;
        private readonly string _first;

        public ProductByTagQuery(string tag, string first, string cursor = null)
        {
            _tag = tag;
            _cursor = cursor;
            _first = first;
        }

        public override string GetQuery()
        {
            return $@"
                query productByTag {{
                    products(first:1, query:""tag:'{_tag}'"") {{
                        edges {{
                            node{{
                                id,
                                legacyResourceId,
                                handle,
                                tags,
                                options {{
                                    name
                                }},
                                metafields(namespace: ""VDC.Integration"", first: 10) {{
                                  edges {{
                                    node {{
                                      id,
                                      key,
                                      value
                                    }}
                                  }}
                                }}
                                variants(first: {_first} {GetCursor(_cursor)}) {{
                                    pageInfo {{
                                      hasNextPage
                                    }},
                                    edges {{
                                        cursor,
                                        node {{
                                            id,
                                            legacyResourceId,
                                            sku,
                                            price,
                                            compareAtPrice,
                                            selectedOptions {{
                                                name,
                                                value
                                            }}
                                            inventoryItem {{
                                                id,
                                                inventoryLevels(first:10) {{
                                                    edges {{
                                                        node {{
                                                            id,
                                                            available,
                                                            location {{
                                                                legacyResourceId,
                                                                id
                                                            }}
                                                        }}
                                                    }}
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

    public class ProductByTagQueryOutput : BaseQueryOutput
    {
        public Connection<ProductResult> products { get; set; }
    }
}
