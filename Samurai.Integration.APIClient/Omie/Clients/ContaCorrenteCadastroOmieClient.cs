using Akka.Event;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Clients
{
    public class ContaCorrenteCadastroOmieClient : BaseOmieClient
    {
        protected override string Path => "geral/contacorrente/";

        public ContaCorrenteCadastroOmieClient(IHttpClientFactory httpClientFactory, string appKey, string appSecret, ILoggingAdapter log)
            : base(httpClientFactory, appKey, appSecret, log)
        {
        }
    }
}
