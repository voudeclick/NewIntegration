using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.Omie
{
    public class OmieListStockSkuMessage
    {
        public long ProductSkuId { get; set; }
        public string ProductSkuCode { get; set; }
    }
}
