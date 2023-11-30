namespace Samurai.Integration.Domain.Shopify.Models.Results
{
    public class InventoryItemResult
    {
        public string id { get; set; }
        public Connection<InventoryLevelResult> inventoryLevels { get; set; }
    }
}
