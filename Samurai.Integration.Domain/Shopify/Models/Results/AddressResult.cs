namespace Samurai.Integration.Domain.Shopify.Models.Results
{
    public class AddressResult
    {
        public string id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string company { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string zip { get; set; }
        public string city { get; set; }
        public string provinceCode { get; set; }
        public string phone { get; set; }
        public string country { get; set; }
        public string countryCodeV2 { get; set; }
    }
}