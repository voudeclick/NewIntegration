using VDC.Integration.APIClient.Omie.Models.Request;

namespace VDC.Integration.APIClient.Omie.Models.Result.PedidoVendaFaturamento
{
    public class CancelarPedidoVendaOmieRequestOutput : BaseOmieOutput
    {
        public int nCodPed { get; set; }
        public string cCodIntPed { get; set; }
        public string cCodStatus { get; set; }
        public string cDescStatus { get; set; }
    }
}
