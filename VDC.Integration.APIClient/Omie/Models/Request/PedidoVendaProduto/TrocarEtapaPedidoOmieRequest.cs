using VDC.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto.Inputs;
using VDC.Integration.APIClient.Omie.Models.Result.PedidoVendaProduto;

namespace VDC.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto
{
    public class TrocarEtapaPedidoOmieRequest : BasePedidoVendaProdutoOmieRequest<TrocarEtapaPedidoOmieRequestInput, TrocarEtapaPedidoOmieRequestOutput>
    {
        public override string Method => "TrocarEtapaPedido";

        public TrocarEtapaPedidoOmieRequest(TrocarEtapaPedidoOmieRequestInput variables)
            : base(variables)
        {
        }
    }
}
