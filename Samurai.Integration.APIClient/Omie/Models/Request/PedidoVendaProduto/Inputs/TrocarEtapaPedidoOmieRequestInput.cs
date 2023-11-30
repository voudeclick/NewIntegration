
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto.Inputs
{
    public class TrocarEtapaPedidoOmieRequestInput : BaseOmieInput
    {
        public long? codigo_pedido { get; set; }
        public string codigo_pedido_integracao { get; set; }
        public string etapa { get; set; }

    }
}
