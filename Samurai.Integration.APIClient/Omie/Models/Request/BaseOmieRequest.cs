using Akka.Event;
using Samurai.Integration.APIClient.Omie.Clients;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request
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
