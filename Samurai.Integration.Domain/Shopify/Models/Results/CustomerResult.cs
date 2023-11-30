namespace Samurai.Integration.Domain.Shopify.Models.Results
{
    public class CustomerResult
    {
        public string id { get; set; }
        public string legacyResourceId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string note { get; set; }
        public AddressResult defaultAddress { get; set; }
    }
}