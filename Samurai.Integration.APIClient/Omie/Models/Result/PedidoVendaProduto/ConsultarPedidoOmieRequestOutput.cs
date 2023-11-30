using Samurai.Integration.APIClient.Omie.Models.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Result.PedidoVendaProduto
{
    public class ConsultarPedidoOmieRequestOutput : BaseOmieOutput
    {
        public PedidoVendaResult pedido_venda_produto { get; set; }
    }
}
