namespace VDC.Integration.APIClient.Shopify.Models.Request.Inputs
{
    public class inventoryAdjustChange
    {
        public string inventoryItemId { get; set; }
        public string locationId { get; set; }
        public int quantity { get; set; }
    }
}
