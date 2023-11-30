namespace Samurai.Integration.APIClient.Millennium.Models.Requests
{
    public class MillenniumApiListOrdersRequest
    {
        public long? TransId { get; set; }
        public string ExternalOrderId { get; set; }
        public int? Top { get; set; }
        public string DataInicial { get; set; }
        public string DataFinal { get; set; }
    }
}
