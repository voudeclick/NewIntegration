using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.APIClient.Tray.Models.Response.Inputs
{
    public class Variant
    {
        [JsonProperty("Variant")]
        public VariationModel Variation { get; set; }

    }

    public class VariationModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("ean")]
        public string Ean { get; set; }

        [JsonProperty("order")]
        public string Order { get; set; }

        [JsonProperty("product_id")]
        public string ProductId { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("cost_price")]
        public string CostPrice { get; set; }

        [JsonProperty("stock")]
        public string Stock { get; set; }

        [JsonProperty("minimum_stock")]
        public string MinimumStock { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("weight")]
        public string Weight { get; set; }

        [JsonProperty("length")]
        public string Length { get; set; }

        [JsonProperty("width")]
        public string Width { get; set; }

        [JsonProperty("height")]
        public string Height { get; set; }


        //[JsonProperty("start_promotion")]
        //public DateTime StartPromotion { get; set; }

        //[JsonProperty("end_promotion")]
        //public DateTime EndPromotion { get; set; }

        //[JsonProperty("promotional_price")]
        //public string PromotionalPrice { get; set; }

        [JsonProperty("available")]
        public string Available { get; set; }

        [JsonProperty("quantity_sold")]
        public string QuantitySold { get; set; }

        public List<SkuModel> Sku { get; set; }
        public List<VariantImageModel> VariantImage { get; set; }

        public class SkuModel
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("value")]
            public string Value { get; set; }

            [JsonProperty("image")]
            public string Image { get; set; }

            [JsonProperty("image_secure")]
            public string ImageSecure { get; set; }
        }
        public class VariantImageModel
        {
            [JsonProperty("http")]
            public string Http { get; set; }

            [JsonProperty("https")]
            public string Https { get; set; }
        }

    }
}
