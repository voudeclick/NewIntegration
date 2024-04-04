using Akka.Event;
using System.Net.Http;

namespace VDC.Integration.APIClient.Omie.Clients
{
    public class ProdutoCadastroOmieClient : BaseOmieClient
    {
        protected override string Path => "geral/produtos/";

        public ProdutoCadastroOmieClient(IHttpClientFactory httpClientFactory, string appKey, string appSecret, ILoggingAdapter log)
            : base(httpClientFactory, appKey, appSecret, log)
        {
        }
    }
}
