using System.Collections.Generic;

namespace VDC.Integration.APIClient.Shopify.Models.Request.Inputs
{
    public class RuleSet
    {
        public bool appliedDisjunctively { get; set; }
        public List<Rule> rules { get; set; }
    }
}