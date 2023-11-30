using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.PluggTo.Models.Webhooks
{
    public class PluggToWebhook
    {
        public string type { get; set; }
        public string id { get; set; }
        public string action { get; set; }
        public int user { get; set; }
        public Changes changes { get; set; }
    }

    public class Changes
    {
        public bool status { get; set; }
        public bool stock { get; set; }
        public bool price { get; set; }
    }
}