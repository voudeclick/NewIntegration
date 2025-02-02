using System;
using System.Collections.Generic;

namespace VDC.Integration.APIClient.Shopify.Models.Request.Inputs
{
    public class InventoryLevel
    {
        public Boolean ignoreCompareQuantity {  get; set; }
        public string reason { get; set; }
        public string name { get; set; }
        public List<inventoryAdjustChange> quantities { get; set; }
    }
}
