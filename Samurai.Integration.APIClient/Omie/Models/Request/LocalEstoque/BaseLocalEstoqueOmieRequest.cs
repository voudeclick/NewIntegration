using Akka.Event;
using Samurai.Integration.APIClient.Omie.Clients;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.LocalEstoque
{
    public abstract class BaseLocalEstoqueOmieRequest<I, O> : BaseOmieRequest<I, O>
    where I : BaseOmieInput
    where O : BaseOmieOutput, new()
    {
        public BaseLocalEstoqueOmieRequest(I variables)
            : base(variables)
        {
        }

        public override BaseOmieClient CreateClient(IHttpClientFactory httpClientFactory, string appKey, string appSecret, ILoggingAdapter log)
        {
            return new LocalEstoqueOmieClient(httpClientFactory, appKey, appSecret, log);
        }
    }
}
