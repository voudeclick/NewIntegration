using Samurai.Integration.APIClient.SellerCenter.Models.Requests.Inputs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Response
{
    public class UpdatePriceProductResponse
    {
        public class ValueItem
        {
            public Guid? Id { get; set; }
            public Guid? SellerId { get; set; }
            public string SellerName { get; set; }
            public string VariationSKU { get; set; }
            public StockItem Stocks { get; set; }
        }
        
    }
}
