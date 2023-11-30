using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.APIClient.Tray.Models.Requests.Inputs
{
    public class Product
    {

        [JsonProperty("ean")]
        public string Ean { get; set; }


        [JsonProperty("name")]
        public string Name { get; set; }


        //[JsonProperty("ncm")]
        //public string Ncm { get; set; }


        [JsonProperty("description")]
        public string Description { get; set; }


        [JsonProperty("description_small")]
        public string DescriptionSmall { get; set; }


        [JsonProperty("price")]
        public double? Price { get; set; }


        [JsonProperty("cost_price")]
        public double? CostPrice { get; set; }


        //[JsonProperty("promotional_price")]
        //public double PromotionalPrice { get; set; }


        //[JsonProperty("start_promotion")]
        //public DateTime? StartPromotion { get; set; }


        //[JsonProperty("end_promotion")]
        //public DateTime? EndPromotion { get; set; }


        [JsonProperty("ipi_value")]
        public decimal? IpiValue { get; set; }


        [JsonProperty("brand")]
        public string Brand { get; set; }


        [JsonProperty("model")]
        public string Model { get; set; }


        [JsonProperty("weight")]
        public long? Weight { get; set; }

        [JsonProperty("length")]
        public long? Length { get; set; }

        [JsonProperty("width")]
        public long? Width { get; set; }

        [JsonProperty("height")]
        public long? Height { get; set; }

        [JsonProperty("stock")]
        public long? Stock { get; set; }

        [JsonProperty("category_id")]
        public long? CategoryId { get; set; }

        [JsonProperty("available")]
        public int? Available { get; set; }

        [JsonProperty("availability")]
        public string Availability { get; set; }

        [JsonProperty("availability_days")]
        public int? AvailabilityDays { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("related_categories")]
        public List<int> RelatedCategories { get; set; }

        //[JsonProperty("release_date")]
        //public string ReleaseDate { get; set; }

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

        public class MetatagModel
        {
            [JsonProperty("type")]
            public string Type { get; set; }
            [JsonProperty("content")]
            public string Content { get; set; }
            [JsonProperty("local")]
            public int Local { get; set; }
        }
    }
}
