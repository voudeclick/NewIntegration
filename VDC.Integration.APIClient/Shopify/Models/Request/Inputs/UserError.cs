using System.Collections.Generic;

namespace VDC.Integration.APIClient.Shopify.Models.Request.Inputs
{
    public class UserError
    {
        public List<string> field { get; set; }
        public string message { get; set; }
    }
}
