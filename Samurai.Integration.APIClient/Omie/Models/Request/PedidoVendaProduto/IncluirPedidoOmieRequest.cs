
using Samurai.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto.Inputs;
using Samurai.Integration.APIClient.Omie.Models.Result.PedidoVendaProduto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto
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
