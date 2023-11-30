using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.APIClient.Tray.Models.Response.Inputs
{
    public class Products
    {
        public ProductModel Product { get; set; }
    }

    public class ProductModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("ean")]
        public string Ean { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("ncm")]
        public string Ncm { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("description_small")]
        public string DescriptionSmall { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("cost_price")]
        public string CostPrice { get; set; }

        [JsonProperty("promotional_price")]
        public string PromotionalPrice { get; set; }

        [JsonProperty("start_promotion")]
        public string StartPromotion { get; set; }

        [JsonProperty("end_promotion")]
        public string EndPromotion { get; set; }

        [JsonProperty("ipi_value")]
        public string IpiValue { get; set; }

        [JsonProperty("brand")]
        public string Brand { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("weight")]
        public string Weight { get; set; }

        [JsonProperty("length")]
        public string Length { get; set; }

        [JsonProperty("width")]
        public string Width { get; set; }

        [JsonProperty("height")]
        public string Height { get; set; }

        [JsonProperty("stock")]
        public string Stock { get; set; }

        [JsonProperty("category_id")]
        public string CategoryId { get; set; }

        [JsonProperty("available")]
        public string Available { get; set; }

        [JsonProperty("availability")]
        public string Availability { get; set; }

        [JsonProperty("availability_days")]
        public string AvailabilityDays { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("related_categories")]
        public List<object> RelatedCategories { get; set; }

        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; }

        [JsonProperty("picture_source_1")]
        public string PictureSource1 { get; set; }

        [JsonProperty("picture_source_2")]
        public string PictureSource2 { get; set; }

        [JsonProperty("picture_source_3")]
        public string PictureSource3 { get; set; }

        [JsonProperty("picture_source_4")]
        public string PictureSource4 { get; set; }

        [JsonProperty("picture_source_5")]
        public string PictureSource5 { get; set; }

        [JsonProperty("picture_source_6")]
        public string PictureSource6 { get; set; }

        [JsonProperty("metatag")]
        public List<MetatagModel> Metatag { get; set; }

        [JsonProperty("virtual_product")]
        public string VirtualProduct { get; set; }

        [JsonProperty("all_categories")]
        public List<string> AllCategories { get; set; }

        [JsonProperty("Variant")]
        public List<VariantModel> Variant { get; set; }

        public class VariantModel {

            [JsonProperty("id")]
            public string Id { get; set; }
        }
        public class MetatagModel
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("content")]
            public string Content { get; set; }

            [JsonProperty("local")]
            public string Local { get; set; }
        }
    }
}
