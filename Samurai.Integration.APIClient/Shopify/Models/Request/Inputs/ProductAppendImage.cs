
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Shopify.Models.Request.Inputs
{
    public class ProductAppendImage
    {
        public string id { get; set; }
        public List<Image> images { get; set; }
    }
}
