
using Samurai.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto.Inputs;
using Samurai.Integration.APIClient.Omie.Models.Result.PedidoVendaProduto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto
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
