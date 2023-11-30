using Akka.Event;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Samurai.Integration.APIClient.Converters;
using Samurai.Integration.APIClient.SellerCenter.APIs;
using Samurai.Integration.APIClient.SellerCenter.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


namespace Samurai.Integration.APIClient.SellerCenter
{
    public class SellerCenterApiClient
    {

        private readonly ILoggingAdapter _log;
        private JsonSerializerOptions jsonSerializerOptions;
        private IHttpClientFactory _httpClientFactory;
        private SellerApiAdresses _apiAdresses;
        private Credentials _credentials;
        private ILoggingAdapter log;
        private string _token = null;
        public string Token => _token ?? SetToken();

        public SellerCenterApiClient(IHttpClientFactory httpClientFactory, SellerApiAdresses apiAdresses, Credentials credentials, ILoggingAdapter log = null)
        {
            _httpClientFactory = httpClientFactory;
            jsonSerializerOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = true
            };
            jsonSerializerOptions.Converters.Add(new LongToStringConverter());
            jsonSerializerOptions.Converters.Add(new DecimalToStringConverter());
            _apiAdresses = apiAdresses;
            _credentials = credentials;
            _log = log;

            this.Products = new ApiProducts(httpClientFactory, apiAdresses.ApiProducts, Token, credentials, log);
            this.Seller = new ApiSeller(httpClientFactory, apiAdresses.ApiSellers, Token, credentials, log);
            this.Orders = new ApiOrders(httpClientFactory, apiAdresses.ApiOrders, Token, credentials, log);


        }

        public ApiProducts Products { get; private set; }
        public ApiSeller Seller { get; private set; }
        public ApiOrders Orders { get; private set; }



        private async Task<T> Post<T>(string method, CancellationToken cancellationToken = default)
        {
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            var client = CreateClient();
            var response = await client.PostAsync($"{method}", new StringContent(""), cancellationToken);
            sw.Stop();
            _log?.Debug($"SellerCenter api - GET {method} Took {sw.ElapsedMilliseconds} ms");

            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"SellerCenter GET Call, for method {_apiAdresses.ApiControlAccess}{method}, failed with the message: {content}");
            }
            else
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(content);
                }
                catch (Exception ex)
                {
                    throw new Exception($"SellerCenter GET Call, for method {_apiAdresses.ApiControlAccess}{method}, failed to deserialize message: {content}", ex);
                }
            }
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_apiAdresses.ApiControlAccess);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            return client;
        }

        private string  SetToken()
        {
            string url = "Authentication/Authenticate";
            var param = new Dictionary<string, string>() { };
            param.Add("userName", _credentials.Username);
            param.Add("password", _credentials.Password);
            param.Add("tenantId", _credentials.TenantId);
            try
            {
                var task = Task.Run(async () => await Post<SellerCenterToken>(QueryHelpers.AddQueryString(url, param)));
                 _token = task.Result?.AccessToken;
            }
            catch (Exception ex)
            {
                _log?.Error($"SellerCenter POST Call, for method {_apiAdresses.ApiControlAccess}{url}, failed to deserialize token", ex);
                throw new Exception($"SellerCenter POST Call, for method {_apiAdresses.ApiControlAccess}{url}, failed to deserialize token", ex);
            }
            return _token;
        }
    }
}
