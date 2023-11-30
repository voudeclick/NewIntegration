using Samurai.Integration.Domain.Shopify.Models.Results;

namespace Samurai.Integration.APIClient.Shopify.Models.Request
{
    public class ProductIdsByTagQuery : BaseQuery<ProductIdsByTagQueryOutput>
    {
        private readonly string _tag;
        private readonly string _cursor;
        public ProductIdsByTagQuery(string tag, string cursor = null)
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
                                onlineStoreUrl,
                                title
                            }}
                        }}
                    }}
                }}
            ";
        }
    }

    public class ProductIdsByTagQueryOutput : BaseQueryOutput
    {
        public Connection<ProductResult> products { get; set; }
    }
}
