using Akka.Event;
using System.Net.Http;
using VDC.Integration.APIClient.Omie.Clients;

namespace VDC.Integration.APIClient.Omie.Models.Request
{
    public abstract class BaseOmieRequest<I, O>
    where I : BaseOmieInput
    where O : BaseOmieOutput, new()
    {
        public BaseOmieRequest(I variables)
        {
            Variables = variables;
        }
        public abstract string Method { get; }

        public abstract BaseOmieClient CreateClient(IHttpClientFactory httpClientFactory, string appKey, string appSecret, ILoggingAdapter log);

        public I Variables { get; set; }
    }

    public abstract class BaseOmieInput
    {
    }

    public abstract class BaseOmieOutput
    {
    }
}
