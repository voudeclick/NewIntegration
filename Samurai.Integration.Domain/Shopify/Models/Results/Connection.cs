using System.Collections.Generic;

namespace Samurai.Integration.Domain.Shopify.Models.Results
{
    public class Connection<T>
    {
        public PageInfo pageInfo { get; set; }
        public List<Edges<T>> edges { get; set; }
    }

    public class Edges<T>
    {
        public string cursor { get; set; }
        public T node { get; set; }
    }
    public class PageInfo
    {
        public bool hasNextPage { get; set; }
        public bool hasPreviousPage { get; set; }
    }
}
