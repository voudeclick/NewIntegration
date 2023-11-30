using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Models
{
    public class ProductImages
    {
        public List<string> ImageUrls { get; set; }
        public List<SkuImage> SkuImages { get; set; }
        public class SkuImage
        {
            public string Sku { get; set; }
            public string SkuImageUrl { get; set; }

        }
    }
}
