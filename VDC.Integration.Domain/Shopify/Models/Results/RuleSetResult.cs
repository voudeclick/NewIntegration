using System.Collections.Generic;

namespace VDC.Integration.Domain.Shopify.Models.Results
{
    public class RuleSetResult
    {
        public bool appliedDisjunctively { get; set; }
        public List<RuleResult> rules { get; set; }
    }
}