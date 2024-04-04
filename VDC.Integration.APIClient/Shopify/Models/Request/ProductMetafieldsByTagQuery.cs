using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class ProductMetafieldsByTagQuery : BaseQuery<ProductMetafieldsByTagQueryOutput>
    {
        private readonly string _tag;
        private readonly string _cursor;
        public ProductMetafieldsByTagQuery(string tag, string cursor = null)
        {
            _tag = tag;
            _cursor = cursor;
        }

        public override string GetQuery()
        {
            return $@"
                query productListByTag {{
                    products(first:25, query:""tag:'{_tag}'"" {GetCursor(_cursor)}) {{
                        pageInfo {{
                            hasNextPage
                        }},
                        edges {{
                            cursor,
                            node{{
                                id,
                                legacyResourceId,
                                tags,
                                handle,
                                metafields(namespace: ""VDC.Integration"", first: 10) {{
                                  edges {{
                                    node {{
                                      id,
                                      key,
                                      value
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

    public class ProductMetafieldsByTagQueryOutput : BaseQueryOutput
    {
        public Connection<ProductResult> products { get; set; }
    }
}
