using Akka.Event;
using Microsoft.Extensions.Logging;
using Samurai.Integration.APIClient.Converters;
using Samurai.Integration.Domain.Results.Logger;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.APIClient.Shopify
{
    public class ShopifyRESTClient
    {
        private JsonSerializerOptions jsonSerializerOptions;
        private IHttpClientFactory _httpClientFactory;
        private readonly string _endpoint;
        private readonly string _version;
        private readonly string _password;
        private readonly string _tenantId;
        private readonly ILoggingAdapter _log;
        private DateTime? _nextExecution;

        private readonly ILogger<ShopifyRESTClient> _logger;

        public ShopifyRESTClient(ILogger<ShopifyRESTClient> logger, IHttpClientFactory httpClientFactory, string tenantId, string endpoint, string version, string password, ILoggingAdapter log = null)
        {
            _httpClientFactory = httpClientFactory;
            jsonSerializerOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = true
            };
            jsonSerializerOptions.Converters.Add(new LongToStringConverter());
            jsonSerializerOptions.Converters.Add(new DecimalToStringConverter());
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            _endpoint = endpoint;
            _version = version;
            _password = password;
            _tenantId = tenantId;
            _log = log;
            _logger = logger;
        }

        public async Task<T> Get<T>(string method, string specificVersion = null, CancellationToken cancellationToken = default)
        {
            if (_nextExecution != null)
            {
                var timeToSleep = (_nextExecution.Value - DateTime.Now);
                if (timeToSleep > TimeSpan.Zero)
                    await Task.Delay(timeToSleep, cancellationToken);
            }

            var client = CreateClient();

            Stopwatch sw = new Stopwatch();
            sw.Restart();
            var response = await client.GetAsync($"/admin/api/{specificVersion ?? _version}/{method}", cancellationToken);
            sw.Stop();

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(2000, cancellationToken);
                sw = new Stopwatch();
                sw.Restart();
                response = await client.GetAsync($"/admin/api/{specificVersion ?? _version}/{method}", cancellationToken);
                sw.Stop();
            }

            CalculateThrottle(response);

            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
                throw new Exception($"ShopifyRESTClient - {LoggerApiDescription.From(_tenantId, $"GET [https://{_endpoint}.myshopify.com/{method}]", "", $"failed with the message: {content}")}");
            else
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(content, jsonSerializerOptions);
                }
                catch (Exception ex)
                {
                    throw new Exception($"ShopifyRESTClient - {LoggerApiDescription.From(_tenantId, $"GET [https://{_endpoint}.myshopify.com/{method}]", "", $"failed to deserialize message: {content}")}", ex);
                }
            }
        }

        public async Task<T> Post<T>(string method, object body = null, string specificVersion = null, CancellationToken cancellationToken = default)
        {
            if (_nextExecution != null)
            {
                var timeToSleep = (_nextExecution.Value - DateTime.Now);
                if (timeToSleep > TimeSpan.Zero)
                    await Task.Delay(timeToSleep, cancellationToken);
            }

            var client = CreateClient();

            Stopwatch sw = new Stopwatch();
            var data = new StringContent("");
            if (body != null)
                data = new StringContent(JsonSerializer.Serialize(body, jsonSerializerOptions), Encoding.UTF8, "application/json");

            sw.Restart();
            var response = await client.PostAsync($"/admin/api/{specificVersion ?? _version}/{method}", data, cancellationToken);
            sw.Stop();

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(2000, cancellationToken);
                sw = new Stopwatch();
                sw.Restart();
                response = await client.PostAsync($"/admin/api/{specificVersion ?? _version}/{method}", data, cancellationToken);
                sw.Stop();
            }

            CalculateThrottle(response);

            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"ShopifyRESTClient - {LoggerApiDescription.From(_tenantId, $"POST [https://{_endpoint}.myshopify.com/{method}]", data, $"failed with the message: {content}")}");
            }
            else
            {
                try
                {
                    _logger.LogInformation($"ShopifyRESTClient - {LoggerApiDescription.From(_tenantId, $"POST [https://{_endpoint}.myshopify.com/{method}]", data, content)}");
                    return JsonSerializer.Deserialize<T>(content, jsonSerializerOptions);
                }
                catch (Exception ex)
                {
                    throw new Exception($"ShopifyRESTClient - {LoggerApiDescription.From(_tenantId, $"POST [https://{_endpoint}.myshopify.com/{method}]", data, $"failed to deserialize message: {content}")}", ex);
                }
            }
        }

        public async Task Post(string method, object body = null, string specificVersion = null, CancellationToken cancellationToken = default)
        {
            
            if (_nextExecution != null)
            {
                var timeToSleep = (_nextExecution.Value - DateTime.Now);
                if (timeToSleep > TimeSpan.Zero)
                    await Task.Delay(timeToSleep, cancellationToken);
            }

            var client = CreateClient();

            Stopwatch sw = new Stopwatch();
            var data = new StringContent("");
            if (body != null)
                data = new StringContent(JsonSerializer.Serialize(body, jsonSerializerOptions), Encoding.UTF8, "application/json");

            sw.Restart();
            var response = await client.PostAsync($"/admin/api/{specificVersion ?? _version}/{method}", data, cancellationToken);
            sw.Stop();

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(2000, cancellationToken);
                sw = new Stopwatch();
                sw.Restart();
                response = await client.PostAsync($"/admin/api/{specificVersion ?? _version}/{method}", data, cancellationToken);
                sw.Stop();
            }

            CalculateThrottle(response);

            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"ShopifyRESTClient - {LoggerApiDescription.From(_tenantId, $"POST [https://{_endpoint}.myshopify.com/{method}]", data, $"failed with the message: {content}")}");
            }
            else
            {
                _logger.LogInformation($"ShopifyRESTClient - {LoggerApiDescription.From(_tenantId, $"POST [https://{_endpoint}.myshopify.com/{method}]", data, content)}");
                return;
            }
        }

        public async Task<T> Put<T>(string method, object body = null, string specificVersion = null, CancellationToken cancellationToken = default)
        {
            if (_nextExecution != null)
            {
                var timeToSleep = (_nextExecution.Value - DateTime.Now);
                if (timeToSleep > TimeSpan.Zero)
                    await Task.Delay(timeToSleep, cancellationToken);
            }

            var client = CreateClient();

            Stopwatch sw = new Stopwatch();
            var data = new StringContent("");
            if (body != null)
                data = new StringContent(JsonSerializer.Serialize(body, jsonSerializerOptions), Encoding.UTF8, "application/json");

            sw.Restart();
            var response = await client.PutAsync($"/admin/api/{specificVersion ?? _version}/{method}", data, cancellationToken);
            sw.Stop();

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(2000, cancellationToken);
                sw = new Stopwatch();
                sw.Restart();
                response = await client.PutAsync($"/admin/api/{specificVersion ?? _version}/{method}", data, cancellationToken);
                sw.Stop();
            }

            CalculateThrottle(response);

            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"ShopifyRESTClient - {LoggerApiDescription.From(_tenantId, $"PUT [https://{_endpoint}.myshopify.com/{method}]", data, $"failed with the message: {content}")}");
            }
            else
            {
                try
                {
                    _logger.LogInformation($"ShopifyRESTClient - {LoggerApiDescription.From(_tenantId, $"PUT [https://{_endpoint}.myshopify.com/{method}]", data, content)}");
                    return JsonSerializer.Deserialize<T>(content, jsonSerializerOptions);
                }
                catch (Exception ex)
                {
                    throw new Exception($"ShopifyRESTClient - {LoggerApiDescription.From(_tenantId, $"PUT [https://{_endpoint}.myshopify.com/{method}]", data, $"failed to deserialize message: {content}")}", ex);
                }
            }
        }

        public async Task Put(string method, object body = null, string specificVersion = null, CancellationToken cancellationToken = default)
        {
            if (_nextExecution != null)
            {
                var timeToSleep = (_nextExecution.Value - DateTime.Now);
                if (timeToSleep > TimeSpan.Zero)
                    await Task.Delay(timeToSleep, cancellationToken);
            }

            var client = CreateClient();

            Stopwatch sw = new Stopwatch();
            var data = new StringContent("");
            if (body != null)
                data = new StringContent(JsonSerializer.Serialize(body, jsonSerializerOptions), Encoding.UTF8, "application/json");

            sw.Restart();
            var response = await client.PutAsync($"/admin/api/{specificVersion ?? _version}/{method}", data, cancellationToken);
            sw.Stop();

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(2000, cancellationToken);
                sw = new Stopwatch();
                sw.Restart();
                response = await client.PutAsync($"/admin/api/{specificVersion ?? _version}/{method}", data, cancellationToken);
                sw.Stop();
            }

            CalculateThrottle(response);

            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"ShopifyRESTClient - {LoggerApiDescription.From(_tenantId, $"PUT [https://{_endpoint}.myshopify.com/{method}]", data, $"failed with the message: {content}")}");
            }
            else
            {
                _logger.LogInformation($"ShopifyRESTClient - {LoggerApiDescription.From(_tenantId, $"PUT [https://{_endpoint}.myshopify.com/{method}]", data, content)}");
                return;
            }
        }

        public async Task<T> Delete<T>(string method, string specificVersion = null, CancellationToken cancellationToken = default)
        {
            if (_nextExecution != null)
            {
                var timeToSleep = (_nextExecution.Value - DateTime.Now);
                if (timeToSleep > TimeSpan.Zero)
                    await Task.Delay(timeToSleep, cancellationToken);
            }
            var client = CreateClient();

            Stopwatch sw = new Stopwatch();
            sw.Restart();
            var response = await client.DeleteAsync($"/admin/api/{specificVersion ?? _version}/{method}", cancellationToken);
            sw.Stop();

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(2000, cancellationToken);
                sw = new Stopwatch();
                sw.Restart();
                response = await client.DeleteAsync($"/admin/api/{specificVersion ?? _version}/{method}", cancellationToken);
                sw.Stop();
            }

            CalculateThrottle(response);

            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"ShopifyRESTClient - {LoggerApiDescription.From(_tenantId, $"DELETE [https://{_endpoint}.myshopify.com/{method}]", "", $"failed with the message: {content}")}");
            }
            else
            {
                try
                {
                    _logger.LogInformation($"ShopifyRESTClient - {LoggerApiDescription.From(_tenantId, $"DELETE [https://{_endpoint}.myshopify.com/{method}]", "", content)}");
                    return JsonSerializer.Deserialize<T>(content, jsonSerializerOptions);
                }
                catch (Exception ex)
                {
                    throw new Exception($"ShopifyRESTClient - {LoggerApiDescription.From(_tenantId, $"DELETE [https://{_endpoint}.myshopify.com/{method}]", "", $"failed to deserialize message: {content}")}", ex);
                }
            }
        }

        public async Task Delete(string method, object body = null, string specificVersion = null, CancellationToken cancellationToken = default)
        {
            if (_nextExecution != null)
            {
                var timeToSleep = (_nextExecution.Value - DateTime.Now);
                if (timeToSleep > TimeSpan.Zero)
                    await Task.Delay(timeToSleep, cancellationToken);
            }
            var client = CreateClient();

            Stopwatch sw = new Stopwatch();
            sw.Restart();
            var response = await client.DeleteAsync($"/admin/api/{specificVersion ?? _version}/{method}", cancellationToken);
            sw.Stop();

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(2000, cancellationToken);
                sw = new Stopwatch();
                sw.Restart();
                response = await client.DeleteAsync($"/admin/api/{specificVersion ?? _version}/{method}", cancellationToken);
                sw.Stop();
            }

            CalculateThrottle(response);

            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"ShopifyRESTClient - {LoggerApiDescription.From(_tenantId, $"DELETE [https://{_endpoint}.myshopify.com/{method}]", "", $"failed with the message: {content}")}");
            }
            else
            {
                _logger.LogInformation($"ShopifyRESTClient - {LoggerApiDescription.From(_tenantId, $"DELETE [https://{_endpoint}.myshopify.com/{method}]", "", content)}");
                return;
            }
        }

        private void CalculateThrottle(HttpResponseMessage response)
        {
            var rate = response.Headers
                                .GetValues("X-Shopify-Shop-Api-Call-Limit")
                                .FirstOrDefault();

            if (rate != null)
            {
                var values = rate.Split('/');

                var used = int.Parse(values[0]);
                var max = int.Parse(values[1]);
                var halfMaximun = max / 2;

                if (used > halfMaximun)
                {
                    var toHalf = used - halfMaximun;
                    TimeSpan timeToSleep = TimeSpan.FromSeconds(toHalf / 2);
                    _nextExecution = DateTime.Now + timeToSleep;
                }
                else
                {
                    _nextExecution = null;
                }
            }
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri($"https://{_endpoint}.myshopify.com");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("X-Shopify-Access-Token", _password);

            return client;
        }
    }
}
