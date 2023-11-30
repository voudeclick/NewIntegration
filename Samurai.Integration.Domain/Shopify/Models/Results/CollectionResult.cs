using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Shopify.Models.Results
{
    public class CollectionResult
    {
        public string id { get; set; }
        public string title { get; set; }
        public RuleSetResult ruleSet { get; set; }
    }
}
