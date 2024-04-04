using VDC.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto.Inputs;
using VDC.Integration.APIClient.Omie.Models.Result.PedidoVendaProduto;

namespace VDC.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto
{
    public class ConsultarPedidoOmieRequest : BasePedidoVendaProdutoOmieRequest<ConsultarPedidoOmieRequestInput, ConsultarPedidoOmieRequestOutput>
    {
        public override string Method => "ConsultarPedido";

        public ConsultarPedidoOmieRequest(ConsultarPedidoOmieRequestInput variables)
            : base(variables)
        {
        }
    }
}
