using Newtonsoft.Json;
using Samurai.Integration.APIClient.SellerCenter.Models.Requests.Inputs;
using System.Collections.Generic;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Requests
{
    public class UpdatePriceProductRequest
    {
        public UpdatePriceProductRequest()
        {
            Stocks = new List<StockItem>();
        }
        /// <summary>
        /// codigo do produto
        /// </summary>
        public string ProductClientCode { get; set; }

        [JsonProperty("variationSKU")]
        public string VariationSku { get; set; }

        public List<StockItem> Stocks { get; set; }

        [JsonIgnore]
        public virtual string SellerId { get; set; }

        public UpdatePriceProductRequest AddStocksItem(StockItem item) { Stocks.Add(item); return this; }
    }
}
