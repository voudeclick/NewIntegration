using Samurai.Integration.Domain.Models.Nexaas;
using System.Collections.Generic;

namespace Samurai.Integration.APIClient.Nexaas.Models.Results
{
    public class NexaasApiSearchOrdersResult
    {
        public List<NexaasOrder> orders { get; set; }           
    }
}
