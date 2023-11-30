using Akka.Event;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Clients
{
    public class CategoriaCadastroOmieClient : BaseOmieClient
    {
        protected override string Path => "geral/categorias/";

        public CategoriaCadastroOmieClient(IHttpClientFactory httpClientFactory, string appKey, string appSecret, ILoggingAdapter log)
            : base(httpClientFactory, appKey, appSecret, log)
        {
        }
    }
}
