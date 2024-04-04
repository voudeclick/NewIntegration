using Akka.Event;
using System.Net.Http;
using VDC.Integration.APIClient.Omie.Clients;

namespace VDC.Integration.APIClient.Omie.Models.Request.CenarioImposto
{
    public abstract class BaseCenarioImpostoOmieRequest<I, O> : BaseOmieRequest<I, O>
    where I : BaseOmieInput
    where O : BaseOmieOutput, new()
    {
        public BaseCenarioImpostoOmieRequest(I variables)
            : base(variables)
        {
        }

        public override BaseOmieClient CreateClient(IHttpClientFactory httpClientFactory, string appKey, string appSecret, ILoggingAdapter log)
        {
            return new CenarioImpostoOmieClient(httpClientFactory, appKey, appSecret, log);
        }
    }
}
