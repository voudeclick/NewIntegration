namespace Samurai.Integration.APIClient.Millennium.Models.Requests
{
    public class MillenniumApiListPricesRequest
    {
        public long? TransId { get; set; }
        public string DataAtualizacao { get; set; }
        public long? ProductId { get; set; }
        public int? Top { get; set; }
    }
}
