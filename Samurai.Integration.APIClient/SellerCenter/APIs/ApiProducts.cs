using Akka.Event;
using Samurai.Integration.APIClient.SellerCenter.Models;
using System.Net.Http;

namespace Samurai.Integration.APIClient.SellerCenter.APIs
{
    public class ApiProducts : ApiBase
    {
        public ApiProducts(IHttpClientFactory httpClientFactory, string url, string token, Credentials credentials, ILoggingAdapter log = null) : base(httpClientFactory, url, token, credentials, log)
        {
        }
    }
}
