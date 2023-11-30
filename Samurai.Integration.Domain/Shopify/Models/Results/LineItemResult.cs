namespace Samurai.Integration.Domain.Shopify.Models.Results
{
    public class LineItemResult
    {
        public string id { get; set; }
        public int quantity { get; set; }
        public string sku { get; set; }
        public PriceSetResult originalUnitPriceSet { get; set; }
    }
}