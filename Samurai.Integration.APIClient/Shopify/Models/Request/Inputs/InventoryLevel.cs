using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Shopify.Models.Request.Inputs
{
    public class InventoryLevel
    {
        public string inventoryLevelId { get; set; }
        public int availableDelta { get; set; }
    }
}
