namespace VDC.Integration.APIClient.Shopify.Models.Request.Inputs
{
    public class InventoryAdjustment
    {
        public string inventoryItemId { get; set; }
        public int availableDelta { get; set; }
    }
}
