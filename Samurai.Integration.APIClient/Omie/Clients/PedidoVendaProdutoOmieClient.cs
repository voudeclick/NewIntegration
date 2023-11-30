﻿using Akka.Event;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Clients
{
    public class PedidoVendaProdutoOmieClient : BaseOmieClient
    {
        protected override string Path => "produtos/pedido/";

        public PedidoVendaProdutoOmieClient(IHttpClientFactory httpClientFactory, string appKey, string appSecret, ILoggingAdapter log)
            : base(httpClientFactory, appKey, appSecret, log)
        {
        }
    }
}
