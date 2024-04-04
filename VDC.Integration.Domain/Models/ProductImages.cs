using System.Collections.Generic;

namespace VDC.Integration.Domain.Models
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
