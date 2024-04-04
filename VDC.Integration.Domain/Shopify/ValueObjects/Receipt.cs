using System;

namespace VDC.Integration.Domain.Shopify.ValueObjects
{
    public class Receipt
    {
        public string X_account_id { get; set; }
        public string X_amount { get; set; }
        public string X_currency { get; set; }
        public string X_gateway_reference { get; set; }
        public string X_message { get; set; }
        public string X_reference { get; set; }
        public string X_result { get; set; }
        public string X_signature { get; set; }
        public string X_test { get; set; }
        public DateTime X_timestamp { get; set; }
    }
}
