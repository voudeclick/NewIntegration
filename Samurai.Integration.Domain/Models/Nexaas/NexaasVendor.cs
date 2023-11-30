using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Models.Nexaas
{
    public class NexaasVendor
    {
        public long id { get; set; }
        public string name { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }
}