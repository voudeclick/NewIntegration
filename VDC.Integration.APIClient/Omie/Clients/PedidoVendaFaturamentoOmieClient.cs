using Akka.Event;
using System.Net.Http;

namespace VDC.Integration.APIClient.Omie.Clients
{
    public class PedidoVendaFaturamentoOmieClient : BaseOmieClient
    {
        protected override string Path => "produtos/pedidovendafat/";

        public PedidoVendaFaturamentoOmieClient(IHttpClientFactory httpClientFactory, string appKey, string appSecret, ILoggingAdapter log)
            : base(httpClientFactory, appKey, appSecret, log)
        {
        }
    }
}
