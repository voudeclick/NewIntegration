using System.Collections.Generic;

namespace VDC.Integration.APIClient.Shopify.Models.Request.Inputs
{
    public class ProductAppendImage
    {
        public string id { get; set; }
        public List<Image> images { get; set; }
    }
}
