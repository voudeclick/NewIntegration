using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Samurai.Integration.WebApi.ViewModels
{
    public class QueueProductPluggToViewModel
    {
        public long TenantId { get; set; }
        public string ProductCode { get; set; }
        public string ExternalId { get; set; }

        public bool? ListAllProducts { get; set; }
    }
}
