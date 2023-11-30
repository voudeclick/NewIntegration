using Akka.Event;
using Samurai.Integration.APIClient.Omie.Clients;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.ContaCorrenteCadastro
{
    public abstract class BaseContaCorrenteCadastroOmieRequest<I, O> : BaseOmieRequest<I, O>
    where I : BaseOmieInput
    where O : BaseOmieOutput, new()
    {
        public BaseContaCorrenteCadastroOmieRequest(I variables)
            : base(variables)
        {
        }

        public override BaseOmieClient CreateClient(IHttpClientFactory httpClientFactory, string appKey, string appSecret, ILoggingAdapter log)
        {
            return new ContaCorrenteCadastroOmieClient(httpClientFactory, appKey, appSecret, log);
        }
    }
}
