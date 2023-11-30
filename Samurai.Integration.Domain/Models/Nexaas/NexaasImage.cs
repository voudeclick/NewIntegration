using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Models.Nexaas
{
    public class NexaasImage
    {
        public long id { get; set; }
        public string caption { get; set; }
        public string name { get; set; }
        public string dataURL { get; set; }
        public long size { get; set; }
    }
}
