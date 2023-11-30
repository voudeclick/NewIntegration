using System.Collections.Generic;

namespace Samurai.Integration.Domain.Shopify.Models.Results
{
    public class GraphQLResponse<T>
    {
        public T data { get; set; }
        public List<dynamic> errors { get; set; }
        public Extension extensions { get; set; }
    }

    public class GraphQLResponse : GraphQLResponse<dynamic> { }

    public class Extension
    {
        public Cost cost { get; set; }
        public class Cost
        {
            public double requestedQueryCost { get; set; }
            public double? actualQueryCost { get; set; }
            public Throttlestatus throttleStatus { get; set; }
        }

        public class Throttlestatus
        {
            public double maximumAvailable { get; set; }
            public double currentlyAvailable { get; set; }
            public double restoreRate { get; set; }
        }
    }
}
