﻿using Akka.Event;
using Samurai.Integration.APIClient.Omie.Clients;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.PedidoVendaProduto
{
    public abstract class BasePedidoVendaProdutoOmieRequest<I, O> : BaseOmieRequest<I, O>
    where I : BaseOmieInput
    where O : BaseOmieOutput, new()
    {
        public BasePedidoVendaProdutoOmieRequest(I variables)
            : base(variables)
        {
        }

        public override BaseOmieClient CreateClient(IHttpClientFactory httpClientFactory, string appKey, string appSecret, ILoggingAdapter log)
        {
            return new PedidoVendaProdutoOmieClient(httpClientFactory, appKey, appSecret, log);
        }
    }
}
