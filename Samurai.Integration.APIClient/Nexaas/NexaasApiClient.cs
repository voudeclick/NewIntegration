using Akka.Event;
using Samurai.Integration.APIClient.Converters;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.APIClient.Nexaas
{
    public class NexaasApiClient
    {
        private readonly ILoggingAdapter _log;
        private readonly string _version;
        private JsonSerializerOptions jsonSerializerOptions;
        private IHttpClientFactory _httpClientFactory;
        private string _url;
        private string _token;        

        public NexaasApiClient(IHttpClientFactory httpClientFactory, string url, string token, string version, ILoggingAdapter log = null)
        {
            _httpClientFactory = httpClientFactory;
            jsonSerializerOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = true
            };
            jsonSerializerOptions.Converters.Add(new LongToStringConverter());
            jsonSerializerOptions.Converters.Add(new DecimalToStringConverter());
            _url = url;
            _token = token;
            _version = version;
            _log = log;
        }

        public async Task<T> Get<T>(string method, CancellationToken cancellationToken = default)
        {
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            var response = await CreateClient().GetAsync($"/api/{_version}/{method}", cancellationToken);
            sw.Stop();
            _log?.Debug($"Nexaas api - GET {method} Took {sw.ElapsedMilliseconds} ms");

            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"Nexaas GET Call, for method {_url}{method}, failed with the message: {content}");
            }
            else
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(content, jsonSerializerOptions);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Nexaas GET Call, for method {_url}{method}, failed to deserialize message: {content}", ex);
                }
            }
        }

        public async Task<T> Post<T>(string method, object body = null, CancellationToken cancellationToken = default)
        {
            Stopwatch sw = new Stopwatch();
            var data = new StringContent("");
            if (body != null)
                data = new StringContent(JsonSerializer.Serialize(body, jsonSerializerOptions), Encoding.UTF8, "application/json");

            sw.Restart();
            var response = await CreateClient().PostAsync($"/api/{_version}/{method}", data, cancellationToken);
            sw.Stop();
            _log?.Debug($"Nexaas api - POST {method} Took {sw.ElapsedMilliseconds} ms");

            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"Nexaas POST Call, for method {_url}{method}, failed with the message: {content}");
            }
            else
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(content, jsonSerializerOptions);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Nexaas POST Call, for method {_url}{method}, failed to deserialize message: {content}", ex);
                }
            }
        }

        public async Task Post(string method, object body = null, CancellationToken cancellationToken = default)
        {
            Stopwatch sw = new Stopwatch();
            var data = new StringContent("");
            if (body != null)
                data = new StringContent(JsonSerializer.Serialize(body, jsonSerializerOptions), Encoding.UTF8, "application/json");

            sw.Restart();
            var response = await CreateClient().PostAsync($"/api/{_version}/{method}", data, cancellationToken);
            sw.Stop();
            _log?.Debug($"Nexaas api - POST {method} Took {sw.ElapsedMilliseconds} ms");

            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"Nexaas POST Call, for method {_url}{method}, failed with the message: {content}");
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
                data = new StringContent(JsonSerializer.Serialize(body, jsonSerializerOptions), Encoding.UTF8, "application/json");

            sw.Restart();
            var response = await CreateClient().PutAsync($"/api/{_version}/{method}", data, cancellationToken);
            sw.Stop();
            _log?.Debug($"Nexaas api - PUT {method} Took {sw.ElapsedMilliseconds} ms");

            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"Nexaas PUT Call, for method {_url}{method}, failed with the message: {content}");
            }
            else
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(content, jsonSerializerOptions);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Nexaas PUT Call, for method {_url}{method}, failed to deserialize message: {content}", ex);
                }
            }
        }

        public async Task Put(string method, object body = null, CancellationToken cancellationToken = default)
        {
            Stopwatch sw = new Stopwatch();
            var data = new StringContent("");
            if (body != null)
                data = new StringContent(JsonSerializer.Serialize(body, jsonSerializerOptions), Encoding.UTF8, "application/json");

            sw.Restart();
            var response = await CreateClient().PutAsync($"/api/{_version}/{method}", data, cancellationToken);
            sw.Stop();
            _log?.Debug($"Nexaas api - PUT {method} Took {sw.ElapsedMilliseconds} ms");

            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"Nexaas PUT Call, for method {_url}{method}, failed with the message: {content}");
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
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Authorization", $"Token token={_token}");

            return client;
        }
    }
}
