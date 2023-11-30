using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.PluggTo.Models.Results
{
    public class LoginResult
    {
        public string token_type { get; set; }
        public string bearer { get; set; }
        public string access_token { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
