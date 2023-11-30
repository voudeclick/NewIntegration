
using Samurai.Integration.APIClient.Omie.Models.Request.PedidoVendaFaturamento.Inputs;
using Samurai.Integration.APIClient.Omie.Models.Result.PedidoVendaFaturamento;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.PedidoVendaFaturamento
{
    public class CancelarPedidoVendaOmieRequest : BasePedidoVendaFaturamentoOmieRequest<CancelarPedidoVendaOmieRequestInput, CancelarPedidoVendaOmieRequestOutput>
    {
        public override string Method => "CancelarPedidoVenda";

        public CancelarPedidoVendaOmieRequest(CancelarPedidoVendaOmieRequestInput variables)
            : base(variables)
        {
        }
    }
}
