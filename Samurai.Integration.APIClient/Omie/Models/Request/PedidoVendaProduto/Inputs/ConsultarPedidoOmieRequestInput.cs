
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto.Inputs
{
    public class ConsultarPedidoOmieRequestInput : BaseOmieInput
    {
        public long? codigo_pedido { get; set; }
        public string codigo_pedido_integracao { get; set; }
        public string numero_pedido { get; set; }
    }
}
