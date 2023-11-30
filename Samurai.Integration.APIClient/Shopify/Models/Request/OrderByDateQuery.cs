using Samurai.Integration.Domain.Shopify.Models.Results;

using System;

namespace Samurai.Integration.APIClient.Shopify.Models.Request
{
    public class OrderByDateQuery : BaseQuery<OrderByDateQueryOutput>
    {
        private readonly DateTime _beginDate;
        private readonly DateTime _endDate;
        private readonly string _cursor;
        private readonly string _queryType;
        private readonly string _filters;
        public OrderByDateQuery(DateTime beginDate, 
            DateTime endDate, 
            string cursor = null, 
            string queryType = null,
            string filters = null)
        {
            _beginDate = beginDate;
            _endDate = endDate;
            _cursor = cursor;
            _queryType = queryType;
            _filters = filters;
        }

        public override string GetQuery()
        {
            return $@"
                query orderByTag {{
                    orders(first:25, query:""created_at:>'{_beginDate:yyyy-MM-ddTHH:mmZ}' created_at:<'{_endDate:yyyy-MM-ddTHH:mmZ}' {_filters}"" {GetCursor(_cursor)}) {{
                        pageInfo {{
                            hasNextPage
                        }},
                        edges {{
                            cursor,
                            node{{
                                {_queryType ?? OrderResult.OnlyId}
                            }}
                        }}
                    }}
                }}
            ";
        }
    }

    public class OrderByDateAndStatusQuery : BaseQuery<OrderByDateQueryOutput>
    {
        private readonly DateTime _beginDate;
        private readonly DateTime _endDate;
        private readonly string _cursor;
        private readonly string _queryType;
        public OrderByDateAndStatusQuery(DateTime beginDate, DateTime endDate, string cursor = null, string queryType = null)
        {
            _beginDate = beginDate;
            _endDate = endDate;
            _cursor = cursor;
            _queryType = queryType;
        }

        public override string GetQuery()
        {
            //to do colocar o fulfillment_status como unshiped também.
            return $@"
                query orderByTag {{
                    orders(first:25, query:""fulfillment_status:null, financial_status:paid, created_at:>'{_beginDate:yyyy-MM-ddTHH:mmZ}' created_at:<'{_endDate:yyyy-MM-ddTHH:mmZ}'"" {GetCursor(_cursor)}) {{
                        pageInfo {{
                            hasNextPage
                        }},
                        edges {{
                            cursor,
                            node{{
                                {_queryType ?? OrderResult.OnlyId}
                            }}
                        }}
                    }}
                }}
            ";
        }
    }

    public class OrderByDateQueryOutput : BaseQueryOutput
    {
        public Connection<OrderResult> orders { get; set; }
    }
}
