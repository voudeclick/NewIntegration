using System;
using System.Collections.Generic;

namespace VDC.Integration.Domain.Messages.Millennium
{
    public class ProcessProductImageMessage
    {
        public long IdProduto { get; set; }
        public string ExternalId { get; set; }
        public string CodProduto { get; set; }
        public List<SkuInfo> Variants { get; set; } = new List<SkuInfo>();
        public class SkuInfo
        {
            public long? ShopifyId { get; set; }
            public string Sku { get; set; }
            public bool Status { get; set; }
        }

        public Guid ProductIntegrationRefenceId { get; set; }
    }
}
