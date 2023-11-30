using Samurai.Integration.Domain.Models.Nexaas;
using System.Collections.Generic;

namespace Samurai.Integration.Domain.Messages.Nexaas
{
    public class NexaasSendFullProductShopifyMessage
    {
        public NexaasProduct Product { get; set; }
        public List<NexaasSku> Skus { get; set; }
        public List<NexaasStockSku> StocksSkus { get; set; }
        public NexaasVendor Vendor { get; set; }
        public List<NexaasCategory> Categories { get; set; }
    }
}
