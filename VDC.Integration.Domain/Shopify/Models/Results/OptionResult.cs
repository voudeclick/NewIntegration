using System.Collections.Generic;

namespace VDC.Integration.Domain.Shopify.Models.Results
{
    public class OptionResult
    {
        public string name { get; set; }
        public List<string> values { get; set; }
    }
}