using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.PedidoVendaFaturamento.Inputs
{
    public class CancelarPedidoVendaOmieRequestInput : BaseOmieInput
    {
        public long? nCodPed { get; set; }
        public string cCodIntPed { get; set; }
    }
}
