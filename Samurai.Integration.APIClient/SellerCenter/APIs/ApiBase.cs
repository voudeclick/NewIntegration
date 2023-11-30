using Akka.Event;

using Newtonsoft.Json;

using Samurai.Integration.APIClient.SellerCenter.Models;

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Samurai.Integration.APIClient.SellerCenter.APIs
{
    public abstract class ApiBase
    {
        private readonly ILoggingAdapter _log;
        private JsonSerializerSettings jsonSerializerOptions;
        private IHttpClientFactory _httpClientFactory;
        private string _url;
        private string _token;
        private Credentials _credentials;
        private ILoggingAdapter log;

        public ApiBase(IHttpClientFactory httpClientFactory, string url, string token, Credentials credentials, ILoggingAdapter log = null)
        {
            _httpClientFactory = httpClientFactory;
            jsonSerializerOptions = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            _url = url;
            _token = token;
            _credentials = credentials;
            _log = log;
        }

        public async Task<T> Get<T>(string method, CancellationToken cancellationToken = default)
        {
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            var response = await CreateClient().GetAsync($"{method}", cancellationToken);
            sw.Stop();

            var content = await response.Content.ReadAsStringAsync();            
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"SellerCenter GET Call, for method {_url}{method}, failed with the message: {content}");
            }
            else
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(content);
                }
                catch (Exception ex)
                {
                    throw new Exception($"SellerCenter GET Call, for method {_url}{method}, failed to deserialize message: {content}", ex);
                }
            }
        }

        public async Task<T> Post<T>(string method, object body = null, CancellationToken cancellationToken = default)
        {
            Stopwatch sw = new Stopwatch();
            var data = new StringContent("");
            if (body != null)
                data = new StringContent(JsonConvert.SerializeObject(body, jsonSerializerOptions), Encoding.UTF8, "application/json");

            sw.Restart();
            var response = await CreateClient().PostAsync($"{method}", data, cancellationToken);
            sw.Stop();

            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"SellerCenter POST Call, for method {_url}{method}, failed with the message: {content}");
            }
            else
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(content);
                }
                catch (Exception ex)
                {
                    throw new Exception($"SellerCenter POST Call, for method {_url}{method}, failed to deserialize message: {content}", ex);
                }
            }
        }

        public async Task Post(string method, object body = null, CancellationToken cancellationToken = default)
        {
            Stopwatch sw = new Stopwatch();
            var data = new StringContent("");
            if (body != null)
                data = new StringContent(JsonConvert.SerializeObject(body, jsonSerializerOptions), Encoding.UTF8, "application/json");

            sw.Restart();
            var response = await CreateClient().PostAsync($"{method}", data, cancellationToken);
            sw.Stop();
            
            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"SellerCenter POST Call, for method {_url}{method}, failed with the message: {content}");
            }
            else
            {
                return;
            }
        }


        public async Task<T> Put<T>(string method, object body = null, CancellationToken cancellationToken = default)
        {
            Stopwatch sw = new Stopwatch();
            var data = new StringContent("");
            if (body != null)
                data = new StringContent(JsonConvert.SerializeObject(body, jsonSerializerOptions), Encoding.UTF8, "application/json");

            sw.Restart();
            var response = await CreateClient().PutAsync($"{method}", data, cancellationToken);
            sw.Stop();

            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"SellerCenter PUT Call, for method {_url}{method}, failed with the message: {content}");
            }
            else
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(content);
                }
                catch (Exception ex)
                {
                    throw new Exception($"SellerCenter PUT Call, for method {_url}{method}, failed to deserialize message: {content}", ex);
                }
            }
        }

        public async Task Put(string method, object body = null, CancellationToken cancellationToken = default)
        {
            Stopwatch sw = new Stopwatch();
            var data = new StringContent("");
            if (body != null)
                data = new StringContent(JsonConvert.SerializeObject(body, jsonSerializerOptions), Encoding.UTF8, "application/json");

            sw.Restart();
            var response = await CreateClient().PutAsync($"{method}", data, cancellationToken);
            sw.Stop();

            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"SellerCenter PUT Call, for method {_url}{method}, failed with the message: {content}");
            }
            else
            {
                return;
            }
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_url);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            return client;
        }
    }
}
