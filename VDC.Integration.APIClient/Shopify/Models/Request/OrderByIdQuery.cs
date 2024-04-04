using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class OrderByIdQuery : BaseQuery<OrderByIdQueryOutput>
    {
        private readonly long _id;
        private readonly string _queryType;
        public OrderByIdQuery(long id, string queryType = null)
        {
            _id = id;
            _queryType = queryType;
        }

        public override string GetQuery()
        {
            return $@"
                query orderById {{
                    order(id: ""gid://shopify/Order/{_id}"") {{
                        {_queryType ?? OrderResult.OnlyId}
                    }}
                }}
            ";
        }
    }

    public class OrderByIdQueryOutput : BaseQueryOutput
    {
        public OrderResult order { get; set; }
    }
}
