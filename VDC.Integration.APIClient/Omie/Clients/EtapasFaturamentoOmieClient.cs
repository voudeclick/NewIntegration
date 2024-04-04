using Akka.Event;
using System.Net.Http;

namespace VDC.Integration.APIClient.Omie.Clients
{
    public class EtapasFaturamentoOmieClient : BaseOmieClient
    {
        protected override string Path => "produtos/etapafat/";

        public EtapasFaturamentoOmieClient(IHttpClientFactory httpClientFactory, string appKey, string appSecret, ILoggingAdapter log)
            : base(httpClientFactory, appKey, appSecret, log)
        {
        }
    }
}
