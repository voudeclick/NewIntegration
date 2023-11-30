using Akka.Event;
using Samurai.Integration.APIClient.SellerCenter.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Samurai.Integration.APIClient.SellerCenter.APIs
{
    public class ApiOrders : ApiBase
    {
        public ApiOrders(IHttpClientFactory httpClientFactory, string url, string token, Credentials credentials, ILoggingAdapter log = null) : base(httpClientFactory, url, token, credentials, log)
        {
        }
    }
}
