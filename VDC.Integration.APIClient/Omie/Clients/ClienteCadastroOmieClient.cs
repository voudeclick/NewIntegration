using Akka.Event;
using System.Net.Http;

namespace VDC.Integration.APIClient.Omie.Clients
{
    public class ClienteCadastroOmieClient : BaseOmieClient
    {
        protected override string Path => "geral/clientes/";

        public ClienteCadastroOmieClient(IHttpClientFactory httpClientFactory, string appKey, string appSecret, ILoggingAdapter log)
            : base(httpClientFactory, appKey, appSecret, log)
        {
        }
    }
}
