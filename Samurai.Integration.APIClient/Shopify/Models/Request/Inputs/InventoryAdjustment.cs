using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Shopify.Models.Request.Inputs
{
    public class InventoryAdjustment
    {
        public string inventoryItemId { get; set; }
        public int availableDelta { get; set; }
    }
}
