using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.APIClient.Shopify.Models.Request.Inputs
{
    public class Variant
    {
        public string id { get; set; }
        public string sku { get; set; }
        public decimal? weight { get; set; }
        public string weightUnit { get { return "KILOGRAMS"; } }
        public string barcode { get; set; }
        public Optional<decimal?> compareAtPrice { get; set; } = new Optional<decimal?>(null);
        public decimal? price { get; set; }
        public string imageId { get; set; }
        public string inventoryManagement { get { return "SHOPIFY"; } }
        public List<string> options { get; set; }
        public List<InventoryQuantity> inventoryQuantities { get; set; }
        public List<Metafield> metafields { get; set; }
        public string inventoryPolicy  { get; set; }
    }
}
