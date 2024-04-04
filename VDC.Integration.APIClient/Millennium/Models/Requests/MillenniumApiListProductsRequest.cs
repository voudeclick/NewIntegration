namespace VDC.Integration.APIClient.Millennium.Models.Requests
{
    public class MillenniumApiListProductsRequest
    {
        public long? TransId { get; set; }
        public string DataAtualizacao { get; set; }
        public long? ProductId { get; set; }
        public int? Top { get; set; }
        public bool ListaPreco { get; set; } = false;

    }
}
