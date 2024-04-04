namespace VDC.Integration.APIClient.Millennium.Models.Requests
{
    public class MillenniumApiListStocksRequest
    {
        public long? TransId { get; set; }
        public long? ProductId { get; set; }
        public int? Top { get; set; }
        public string DataAtualizacao { get; set; }

    }
}
