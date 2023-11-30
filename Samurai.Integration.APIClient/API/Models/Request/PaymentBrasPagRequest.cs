using System.Collections.Generic;

namespace Samurai.Integration.APIClient.Millennium.Models.Results
{
    public class PaymentExtraInfoRequest
    {
        public string CheckoutToken { get; set; }
        public string StoreHandle { get; set; }
        public IDictionary<string, string> Params { get; set; }
        public string TypeOf { get; set; }
        public string Method { get; set; }
    }
}
