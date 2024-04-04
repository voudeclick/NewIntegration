using Akka.Event;
using System.Net.Http;

namespace VDC.Integration.APIClient.Omie.Clients
{
    public class LocalEstoqueOmieClient : BaseOmieClient
    {
        protected override string Path => "estoque/local/";

        public LocalEstoqueOmieClient(IHttpClientFactory httpClientFactory, string appKey, string appSecret, ILoggingAdapter log)
            : base(httpClientFactory, appKey, appSecret, log)
        {
        }
    }
}
