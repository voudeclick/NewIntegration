using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samurai.Integration.Domain.Models.Nexaas
{
    public class NexaasSku
    {
        public long id { get; set; }
        public long product_id { get; set; }
        public string code { get; set; }
        public string ean { get; set; }
        public string name { get; set; }
        public decimal? width { get; set; }
        public decimal? height { get; set; }
        public decimal? length { get; set; }
        public decimal? weight { get; set; }
        public bool active { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public string description { get; set; }
        //public bool display_without_stock { get; set; }
        public NexaasFeatures product_features { get; set; }
        public NexaasProduct product { get; set; }
        public NexaasSkuPrice SkuPrice { get; set; }
        public List<NexaasImage> product_images { get; set; }
    }

    public class NexaasFeatures
    {
        public List<NexaasFeature> @default { get; set; }
        public List<NexaasFeature> additional { get; set; }

        public List<NexaasFeature> GetAllFeatures()
        {
            List<NexaasFeature> result = new List<NexaasFeature>();
            if (@default?.Any() == true)
                result.AddRange(@default);
            if (additional?.Any() == true)
                result.AddRange(additional);
            return result;
        }
    }

    public class NexaasFeature
    {
        public long id { get; set; }
        public string name { get; set; }
        public NexaasFeatureVariant feature_variant { get; set; }
    }

    public class NexaasFeatureVariant
    {
        public long id { get; set; }
        public string name { get; set; }
    }
}
