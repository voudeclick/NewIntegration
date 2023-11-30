using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Samurai.Integration.WebApi.ViewModels
{
    public class QueueProductOmieViewModel
    {
        public long TenantId { get; set; }

        public long? ProductId { get; set; }

        public bool? ListAllProducts { get; set; }
    }
}
