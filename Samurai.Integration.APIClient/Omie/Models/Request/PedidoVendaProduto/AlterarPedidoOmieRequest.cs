using Samurai.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto.Inputs;
using Samurai.Integration.APIClient.Omie.Models.Result.PedidoVendaProduto;

namespace Samurai.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto
{
    public class AlterarPedidoOmieRequest : BasePedidoVendaProdutoOmieRequest<IncluirPedidoOmieRequestInput, IncluirPedidoOmieRequestOutput>
    {
        public override string Method => "AlterarPedidoVenda";

        public AlterarPedidoOmieRequest(IncluirPedidoOmieRequestInput variables)
            : base(variables)
        {
        }
    }
}
