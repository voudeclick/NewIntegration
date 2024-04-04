namespace VDC.Integration.Domain.Messages.Shopify
{
    public class ShopifyUpdateStockKitMessage
    {
        public long ExternalProductId { get; set; }
        public string Sku { get; set; }
        public int Quantity { get; set; }
    }
}
