using Samurai.Integration.Domain.Shopify.Models.Results;

namespace Samurai.Integration.APIClient.Shopify.Models.Request
{
    public class AllCollectionsQuery : BaseQuery<AllCollectionsQueryOutput>
    {
        private readonly string _cursor;
        public AllCollectionsQuery(string cursor = null)
        {
            _cursor = cursor;
        }

        public override string GetQuery()
        {
            return $@"
                query allCollections {{
                     collections (first:50 {GetCursor(_cursor)}) {{
                          pageInfo {{
                               hasNextPage
                          }},
                            edges {{
                               cursor,
                               node {{
                                    id,
                                    title,
                                    ruleSet {{
                                         appliedDisjunctively,
                                         rules {{
                                              column,
                                              condition,
                                              relation
                                         }}
                                    }}
                               }}
                          }}
                     }} 
                }}
            ";
        }
    }

    public class AllCollectionsQueryOutput : BaseQueryOutput
    {
        public Connection<CollectionResult> collections { get; set; }
    }
}
