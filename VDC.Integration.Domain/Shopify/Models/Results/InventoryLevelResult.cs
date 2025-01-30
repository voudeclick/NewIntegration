using System.Collections.Generic;

namespace VDC.Integration.Domain.Shopify.Models.Results
{
    public class InventoryLevelResult
    {
        public string id { get; set; }
        public List<QuantitiesResult> quantities { get; set; }
        public LocationResult location { get; set; }
    }
}
