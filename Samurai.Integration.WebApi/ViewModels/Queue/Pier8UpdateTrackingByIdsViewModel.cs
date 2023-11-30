using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Samurai.Integration.WebApi.ViewModels.Queue
{
    public class Pier8UpdateTrackingByIdsViewModel
    {
        public long TenantId { get; set; }
        public string ExternalOrderId { get; set; }
        public List<string> Orders => ExternalOrderId.Split(",").Select(x => { x.Trim() ; return x.Trim(); }).Distinct().ToList();
    }
}
