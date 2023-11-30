using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Shopify.Models.Request.Inputs
{
    public class InventoryQuantity
    {
        public string locationId { get; set; }
        public int availableQuantity { get; set; }
    }
}
