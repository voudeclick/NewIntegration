using Akka.Event;
using System.Net.Http;

namespace VDC.Integration.APIClient.Omie.Clients
{
    public class ContaCorrenteCadastroOmieClient : BaseOmieClient
    {
        protected override string Path => "geral/contacorrente/";

        public ContaCorrenteCadastroOmieClient(IHttpClientFactory httpClientFactory, string appKey, string appSecret, ILoggingAdapter log)
            : base(httpClientFactory, appKey, appSecret, log)
        {
        }
    }
}
