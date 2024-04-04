using VDC.Integration.APIClient.Omie.Models.Request.PedidoVendaFaturamento.Inputs;
using VDC.Integration.APIClient.Omie.Models.Result.PedidoVendaFaturamento;

namespace VDC.Integration.APIClient.Omie.Models.Request.PedidoVendaFaturamento
{
    public class CancelarPedidoVendaOmieRequest : BasePedidoVendaFaturamentoOmieRequest<CancelarPedidoVendaOmieRequestInput, CancelarPedidoVendaOmieRequestOutput>
    {
        public override string Method => "CancelarPedidoVenda";

        public CancelarPedidoVendaOmieRequest(CancelarPedidoVendaOmieRequestInput variables)
            : base(variables)
        {
        }
    }
}
