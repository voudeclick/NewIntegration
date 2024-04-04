using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class LocationQuery : BaseQuery<LocationQueryOutput>
    {
        private readonly long? _top;
        private readonly string _cursor;
        public LocationQuery(long? top = null, string cursor = null)
        {
            _top = top;
            _cursor = cursor;
        }

        public override string GetQuery()
        {
            return $@"
                query productById {{
                    locations (first: {_top ?? 1} {GetCursor(_cursor)}) {{
                        edges {{
                            node {{
                                id,
                                legacyResourceId,
                                name
                            }}
                        }}
                    }}
                }}
            ";
        }
    }

    public class LocationQueryOutput : BaseQueryOutput
    {
        public Connection<LocationResult> locations { get; set; }
    }
}
