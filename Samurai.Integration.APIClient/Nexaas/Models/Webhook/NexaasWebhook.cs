using Samurai.Integration.APIClient.Nexaas.Models.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Nexaas.Models.Webhook
{    
    public class NexaasWebhook
    {
        public bool test { get; set; } 
        public NexaasWebhookType object_type { get; set; }
        public long? object_id { get; set; }
        public long? organization_id { get; set; }
        public string @event { get; set; }
    }
}
