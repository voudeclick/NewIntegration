using Akka.Event;
using Samurai.Integration.APIClient.Omie.Clients;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.PedidoVendaFaturamento
{
    public abstract class BasePedidoVendaFaturamentoOmieRequest<I, O> : BaseOmieRequest<I, O>
    where I : BaseOmieInput
    where O : BaseOmieOutput, new()
    {
        public BasePedidoVendaFaturamentoOmieRequest(I variables)
            : base(variables)
        {
        }

        public override BaseOmieClient CreateClient(IHttpClientFactory httpClientFactory, string appKey, string appSecret, ILoggingAdapter log)
        {
            return new PedidoVendaFaturamentoOmieClient(httpClientFactory, appKey, appSecret, log);
        }
    }
}
