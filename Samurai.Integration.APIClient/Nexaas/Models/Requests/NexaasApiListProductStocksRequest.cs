using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Nexaas.Models.Requests
{
    public class NexaasApiListProductStocksRequest
    {
        public int Page { get; set; }
        public ProductStocksRequest search { get; set; }

        public NexaasApiListProductStocksRequest()
        {
            search = new ProductStocksRequest();
        }

        public class ProductStocksRequest
        {
            public List<SkuStockRequest> stock_skus { get; set; }
            public ProductStocksRequest()
            {
                stock_skus = new List<SkuStockRequest>();
            }
        }

        public class SkuStockRequest
        {
            public long product_sku_id { get; set; }
            public int min_quantity { get; set; }
        }
    }

}
