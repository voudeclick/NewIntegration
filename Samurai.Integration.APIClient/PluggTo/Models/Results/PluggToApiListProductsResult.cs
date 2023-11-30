using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.PluggTo.Models.Results
{
    public class PluggToApiListProductsResult
    {
        public int? total { get; set; }
        public int? showing { get; set; }
        public int? limit { get; set; }
        public List<Result> result { get; set; }

        public class Result
        {
            public Produto Product { get; set; }
        }
        public class Produto
        {
            public string id { get; set; }
            public Dimension real_dimension { get; set; }
            public Dimension dimension { get; set; }
            public List<StockTable> stock_table { get; set; }
            public List<Category> categories { get; set; }
            public List<Photo> photos { get; set; }
            public List<Attribute> attributes { get; set; }
            public string type { get; set; }
            public string sku { get; set; }
            public string name { get; set; }
            public decimal? price { get; set; }
            public decimal? special_price { get; set; }
            public int? reserved { get; set; }
            public int? quantity { get; set; }
            public string brand { get; set; }
            public string model { get; set; }
            public string supplier_id { get; set; }
            public string description { get; set; }
            public string user_id { get; set; }
            public DateTime? created { get; set; }
            public string created_by { get; set; }
            public DateTime? modified { get; set; }
            public string modified_by { get; set; }
            public string stock_code { get; set; }
            public string external { get; set; }
            public string ean { get; set; }
            public string ncm { get; set; }
            public string short_description { get; set; }
            public List<Produto> variations { get; set; }
        }
        public class Dimension
        {
            public decimal? width { get; set; }
            public decimal? weight { get; set; }
            public decimal? length { get; set; }
            public decimal? height { get; set; }
        }

        public class StockTable
        {

            public int? reserved { get; set; }
            public int? quantity { get; set; }
            public decimal? price { get; set; }
            public decimal? special_price { get; set; }
            public string code { get; set; }
            public string type { get; set; }
            public string id { get; set; }
        }

        public class Category
        {

            public string id { get; set; }
            public string name { get; set; }
        }

        public class Photo
        {

            public string id { get; set; }
            public string url { get; set; }
            public int? order { get; set; }
        }

        public class Attribute
        {
            public Value value { get; set; }
            public string id { get; set; }
            public string code { get; set; }
            public string label { get; set; }

        }

        public class Value
        {
            public string code { get; set; }
            public string label { get; set; }
        }

    }
}