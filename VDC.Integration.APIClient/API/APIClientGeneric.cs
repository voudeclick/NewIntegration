using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VDC.Integration.APIClient.Converters;

namespace VDC.Integration.APIClient.API
{
    public class APIClientGeneric
    {
        readonly JsonSerializerOptions jsonSerializerOptions;
        readonly IHttpClientFactory _httpClientFactory;
        readonly string _url;
        readonly string _tenantId;
        readonly string _authorizationToken;

        public APIClientGeneric(IHttpClientFactory httpClientFactory, string tenantId, string urlBase, string authorizationToken = "")
        {
            _httpClientFactory = httpClientFactory;

            jsonSerializerOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = true
            };

            jsonSerializerOptions.Converters.Add(new LongToStringConverter());
            jsonSerializerOptions.Converters.Add(new DecimalToStringConverter());

            _url = urlBase;
            _tenantId = tenantId;
            _authorizationToken = authorizationToken;
        }

        public async Task<string> Get(string method, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();

            var response = await client.GetAsync(method, cancellationToken);

            if (response is null)
                throw new Exception($"APIClientGeneric - response is null -  Error in Get:43 | HttpStatusCode: {response.StatusCode} | TenantId: {_tenantId} | urlBase: {_url} | endPoint: {method}");

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
                throw new Exception($"APIClientGeneric - Error in Get:48 | HttpStatusCode: {response.StatusCode} | TenantId: {_tenantId} | urlBase: {_url} | endPoint: {method}");
            else
            {
                try
                {
                    content = new string(content.Where(c => !char.IsControl(c)).ToArray());
                    return content;
                }
                catch (Exception ex)
                {
                    throw new Exception($"APIClientGeneric - Error in Get:54 | TenantId: {_tenantId} | urlBase: {_url} | endPoint: {method}", ex);
                }
            }
        }

        public async Task<T> Get<T>(string method, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();

            var response = await client.GetAsync(method, cancellationToken);

            if (response is null)
                throw new Exception($"APIClientGeneric - response is null -  Error in Get:43 | HttpStatusCode: {response.StatusCode} | TenantId: {_tenantId} | urlBase: {_url} | endPoint: {method}");

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
                throw new Exception($"APIClientGeneric - Error in Get:48 | HttpStatusCode: {response.StatusCode} | TenantId: {_tenantId} | urlBase: {_url} | endPoint: {method}");
            else
            {
                try
                {
                    content = new string(content.Where(c => !char.IsControl(c)).ToArray());
                    return JsonSerializer.Deserialize<T>(content, jsonSerializerOptions);
                }
                catch (Exception ex)
                {
                    throw new Exception($"APIClientGeneric - Error in Get:54 | TenantId: {_tenantId} | urlBase: {_url} | endPoint: {method}", ex);
                }
            }
        }

        public async Task<(T, string)> GetContent<T>(string method, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();

            var response = await client.GetAsync(method, cancellationToken);

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
                throw new Exception($"APIClientGeneric - Error in GetContent:68 | HttpStatusCode: {response.StatusCode} | TenantId: {_tenantId} | urlBase: {_url} | endPoint: {method}");
            else
            {
                try
                {
                    content = new string(content.Where(c => !char.IsControl(c)).ToArray());
                    return (JsonSerializer.Deserialize<T>(content, jsonSerializerOptions), content);
                }
                catch (Exception ex)
                {
                    throw new Exception($"APIClientGeneric - Error in GetContent:78 | TenantId: {_tenantId} | urlBase: {_url} | endPoint: {method}", ex);
                }
            }
        }

        public async Task<T> Post<T>(string method, object body = null, CancellationToken cancellationToken = default)
        {

            var client = CreateClient();

            var data = new StringContent("");

            if (body != null)
                data = new StringContent(JsonSerializer.Serialize(body, jsonSerializerOptions), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(method, data, cancellationToken);

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
                throw new Exception($"APIClientGeneric - Error in Post<T>:98 | HttpStatusCode: {response.StatusCode} | TenantId: {_tenantId} | urlBase: {_url} | endPoint: {method} | body: {data}");
            else
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(content, jsonSerializerOptions);
                }
                catch (Exception ex)
                {
                    throw new Exception($"APIClientGeneric - Error in Post<T>:107 | HttpStatusCode: {response.StatusCode} | TenantId: {_tenantId} | urlBase: {_url} | endPoint: {method} | body: {data}", ex);
                }
            }
        }

        public async Task Post(string method, object body = null, CancellationToken cancellationToken = default)
        {

            var client = CreateClient();

            var data = new StringContent("");
            if (body != null)
                data = new StringContent(JsonSerializer.Serialize(body, jsonSerializerOptions), Encoding.UTF8, "application/json");


            var response = await client.PostAsync(method, data, cancellationToken);

            await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
                throw new Exception($"APIClientGeneric - Error in Post:126 | HttpStatusCode: {response.StatusCode} | TenantId: {_tenantId} | urlBase: {_url} | endPoint: {method} | body: {data}");

        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_url);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            if (!string.IsNullOrWhiteSpace(_authorizationToken))
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authorizationToken}");

            return client;
        }
    }
}
