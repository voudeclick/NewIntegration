using Akka.Event;
using System.Net.Http;

namespace VDC.Integration.APIClient.Omie.Clients
{
    public class CenarioImpostoOmieClient : BaseOmieClient
    {
        protected override string Path => "geral/cenarios/";

        public CenarioImpostoOmieClient(IHttpClientFactory httpClientFactory, string appKey, string appSecret, ILoggingAdapter log)
            : base(httpClientFactory, appKey, appSecret, log)
        {
        }
    }
}
