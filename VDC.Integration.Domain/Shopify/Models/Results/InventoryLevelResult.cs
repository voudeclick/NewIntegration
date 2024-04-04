namespace VDC.Integration.Domain.Shopify.Models.Results
{
    public class InventoryLevelResult
    {
        public string id { get; set; }
        public int available { get; set; }
        public LocationResult location { get; set; }
    }
}
