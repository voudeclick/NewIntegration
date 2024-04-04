namespace VDC.Integration.APIClient.Shopify.Models.Request.Inputs
{
    public class WebhookSubscription
    {
        public string callbackUrl { get; set; }
        public string format { get { return "JSON"; } }
        public string metafieldNamespaces { get { return "VDC.Integration"; } }
    }
}
