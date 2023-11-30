
using Samurai.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto.Inputs;
using Samurai.Integration.APIClient.Omie.Models.Result.PedidoVendaProduto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto
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
