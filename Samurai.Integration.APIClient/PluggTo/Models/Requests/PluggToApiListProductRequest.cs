using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.PluggTo.Models.Requests
{
    public class PluggToApiListProductRequest
    {
        public DateTime? CreatedAt { get; set; }
        public string ProductCode { get; set; }
        public string ExternalId { get; set; }
        public string AccountUserId { get; set; }
        public string AccountSellerId { get; set; }
    }
}
