using VDC.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto.Inputs;
using VDC.Integration.APIClient.Omie.Models.Result.PedidoVendaProduto;

namespace VDC.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto
{
    public class IncluirPedidoOmieRequest : BasePedidoVendaProdutoOmieRequest<IncluirPedidoOmieRequestInput, IncluirPedidoOmieRequestOutput>
    {
        public override string Method => "IncluirPedido";

        public IncluirPedidoOmieRequest(IncluirPedidoOmieRequestInput variables)
            : base(variables)
        {
        }
    }
}
