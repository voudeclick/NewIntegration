using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDC.Integration.APIClient.Shopify.Models.Request.Inputs
{
    public class VariantUpdateVariantsOptionValue
    {
        public string optionName { get; set; }
        public string name { get; set; }
        public string linkedMetafieldValue { get; set; }
    }
}
