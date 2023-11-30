using Samurai.Integration.Domain.Shopify.Models.Results;

namespace Samurai.Integration.APIClient.Shopify.Models.Request
{
    public class ProductByIdQuery : BaseQuery<ProductByIdQueryOutput>
    {
        private readonly long _id;
        private readonly string _cursor;
        private readonly string _first;
        public ProductByIdQuery(long id, string first, string cursor = null)
        {
            _id = id;
            _cursor = cursor;
            _first = first;
        }

        public override string GetQuery()
        {
            return $@"
                query productById {{
                    product(id: ""gid://shopify/Product/{_id}"") {{
                        id,
                        legacyResourceId,
                        handle,
                        tags,
                        options {{
                            name
                        }},
                        metafields(namespace: ""Samurai.Integration"", first: 10) {{
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
                            }}
                            edges {{
                                cursor,
                                node {{
                                    id,
                                    legacyResourceId,
                                    sku,
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
            ";
        }
    }

    public class ProductByIdQueryOutput : BaseQueryOutput
    {
        public ProductResult product { get; set; }
    }
}
