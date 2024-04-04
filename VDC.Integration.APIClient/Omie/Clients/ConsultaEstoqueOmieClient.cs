using Akka.Event;
using System.Net.Http;

namespace VDC.Integration.APIClient.Omie.Clients
{
    public class ConsultaEstoqueOmieClient : BaseOmieClient
    {
        protected override string Path => "estoque/consulta/";

        public ConsultaEstoqueOmieClient(IHttpClientFactory httpClientFactory, string appKey, string appSecret, ILoggingAdapter log)
            : base(httpClientFactory, appKey, appSecret, log)
        {
        }
    }
}
