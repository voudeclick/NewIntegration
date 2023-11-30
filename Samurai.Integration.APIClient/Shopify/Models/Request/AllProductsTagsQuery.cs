using Samurai.Integration.Domain.Shopify.Models.Results;

namespace Samurai.Integration.APIClient.Shopify.Models.Request
{
    public class AllProductsTagsQuery : BaseQuery<AllProductsTagsQueryOutput>
    {
        private readonly string _cursor;
        public AllProductsTagsQuery(string cursor = null)
        {
            _cursor = cursor;
        }

        public override string GetQuery()
        {
            return $@"
                query allProductsTags {{
                    products(first:100 {GetCursor(_cursor)}) {{
                        pageInfo {{
                            hasNextPage
                        }},
                        edges {{
                            cursor,
                            node{{
                                id,
                                legacyResourceId,
                                tags
                            }}
                        }}
                    }}
                }}
            ";
        }
    }

    public class AllProductsTagsQueryOutput : BaseQueryOutput
    {
        public Connection<ProductResult> products { get; set; }
    }
}
