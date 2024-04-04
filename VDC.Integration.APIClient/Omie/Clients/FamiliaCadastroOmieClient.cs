using Akka.Event;
using System.Net.Http;

namespace VDC.Integration.APIClient.Omie.Clients
{
    public class FamiliaCadastroOmieClient : BaseOmieClient
    {
        protected override string Path => "geral/familias/";

        public FamiliaCadastroOmieClient(IHttpClientFactory httpClientFactory, string appKey, string appSecret, ILoggingAdapter log)
            : base(httpClientFactory, appKey, appSecret, log)
        {
        }
    }
}
