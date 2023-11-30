using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Shopify.Models.Request.Inputs
{
    public class Collection
    {
        public string title { get; set; }
        public RuleSet ruleSet { get; set; }
    }
}
