using Akka.Event;
using System.Net.Http;
using VDC.Integration.APIClient.Omie.Clients;

namespace VDC.Integration.APIClient.Omie.Models.Request.ProdutoCadastro
{
    public abstract class BaseProdutoCadastroOmieRequest<I, O> : BaseOmieRequest<I, O>
    where I : BaseOmieInput
    where O : BaseOmieOutput, new()
    {
        public BaseProdutoCadastroOmieRequest(I variables)
            : base(variables)
        {
        }

        public override BaseOmieClient CreateClient(IHttpClientFactory httpClientFactory, string appKey, string appSecret, ILoggingAdapter log)
        {
            return new ProdutoCadastroOmieClient(httpClientFactory, appKey, appSecret, log);
        }
    }
}
