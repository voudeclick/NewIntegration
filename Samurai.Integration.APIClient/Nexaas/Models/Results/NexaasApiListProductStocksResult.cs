using Samurai.Integration.Domain.Models.Nexaas;
using System.Collections.Generic;

namespace Samurai.Integration.APIClient.Nexaas.Models.Results
{
    public class NexaasApiListProductStocksResult
    {
        public List<NexaasStockSku> stock_skus { get; set; }           
    }
}
