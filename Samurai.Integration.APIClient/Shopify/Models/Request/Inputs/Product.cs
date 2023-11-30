using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Shopify.Models.Request.Inputs
{
    public class Product
    {
        public string id { get; set; }
        public bool? published { get; set; }
        public string title { get; set; }
        public string descriptionHtml { get; set; }
        public string vendor { get; set; }
        public List<string> options { get; set; }
        public List<string> tags { get; set; }
        public List<Metafield> metafields { get; set; }
        public List<Variant> variants { get; set; }
        public List<Image> images { get; set; }
    }
}
