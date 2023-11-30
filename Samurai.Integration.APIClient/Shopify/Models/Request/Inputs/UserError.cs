using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Shopify.Models.Request.Inputs
{
    public class UserError
    {
        public List<string> field { get; set; }
        public string message { get; set; }
    }
}
