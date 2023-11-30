using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Response
{
    public class GetProductVariationByClientCodeResponse
    {
        public ValueItem Value { get; set; }

        public class ValueItem
        {
            public Guid Id { get; set; }
            public string ProductClientCode { get; set; }
            public List<Variation> ProductVariations { get; set; }

        }

        public class Variation
        {
            public string SellerId { get; set; }
            public string ProductClientCode { get; set; }
            public string VariationSKU { get; set; }

            public List<StockItem> Stocks { get; set; }


            public class StockItem {
                public string SellerWarehouseId { get; set; }
                public string CurrencyCode { get => "BRL"; }
                public decimal? FromPrice { get; set; }
                public decimal? ByPrice { get; set; }
                public int Quantity { get; set; }
                public bool SellWithoutStock { get; set; }
            }
        }

        public  Variation.StockItem GetStockBySku(string sku) => Value.ProductVariations.Where(x => x.VariationSKU.ToLower() == sku.ToLower()).SelectMany(x => x.Stocks).FirstOrDefault();

        public bool Exists => Value.ProductVariations.Count > 0;
    }
}
