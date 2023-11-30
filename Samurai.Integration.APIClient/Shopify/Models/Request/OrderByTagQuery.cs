using Samurai.Integration.Domain.Shopify.Models.Results;

namespace Samurai.Integration.APIClient.Shopify.Models.Request
{
    public class OrderByTagQuery : BaseQuery<OrderByTagQueryOutput>
    {
        private readonly string _tag;
        private readonly string _queryType;
        public OrderByTagQuery(string tag, string queryType = null)
        {
            _tag = tag;
            _queryType = queryType;
        }

        public override string GetQuery()
        {
            return $@"
                query orderByTag {{
                    orders(first:1, query:""tag:'{_tag}'"") {{
                        edges {{
                            node{{
                                {_queryType ?? OrderResult.OnlyId}
                            }}
                        }}
                    }}
                }}
            ";
        }
    }

    public class OrderByTagQueryOutput : BaseQueryOutput
    {
        public Connection<OrderResult> orders { get; set; }
    }
}
