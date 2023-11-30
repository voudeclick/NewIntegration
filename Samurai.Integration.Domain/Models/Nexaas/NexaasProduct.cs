using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Models.Nexaas
{
    public class NexaasProduct
    {
        public long id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public long product_brand_id { get; set; }
        public long? product_category_id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string measure { get; set; }
        public string description { get; set; }
        public string status { get; set; }
        public string visibility { get; set; }
        public bool IsActive()
        {
            return this.status == "active" && this.visibility != "offline";
        }
        public List<NexaasImage> product_images { get; set; }
        public List<NexaasSku> product_skus { get; set; }
    }
}
