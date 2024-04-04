using Akka.Event;
using System.Net.Http;
using VDC.Integration.APIClient.Omie.Clients;

namespace VDC.Integration.APIClient.Omie.Models.Request.ConsultaEstoque
{
    public abstract class BaseConsultaEstoqueOmieRequest<I, O> : BaseOmieRequest<I, O>
    where I : BaseOmieInput
    where O : BaseOmieOutput, new()
    {
        public BaseConsultaEstoqueOmieRequest(I variables)
            : base(variables)
        {
        }

        public override BaseOmieClient CreateClient(IHttpClientFactory httpClientFactory, string appKey, string appSecret, ILoggingAdapter log)
        {
            return new ConsultaEstoqueOmieClient(httpClientFactory, appKey, appSecret, log);
        }
    }
}
