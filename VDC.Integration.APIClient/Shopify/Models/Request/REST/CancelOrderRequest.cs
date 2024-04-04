namespace VDC.Integration.APIClient.Shopify.Models.Request.REST
{
    public class CancelOrderRequest
    {
        public string OrderId { get; set; }
        public bool SendEmail { get; set; }
    }
}
