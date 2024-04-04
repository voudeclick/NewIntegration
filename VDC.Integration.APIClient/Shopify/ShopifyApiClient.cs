using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VDC.Integration.APIClient.Converters;
using VDC.Integration.APIClient.Shopify.Models.Request;
using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify
{
    public class ShopifyApiClient
    {
        private readonly JsonSerializerOptions jsonSerializerOptions;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _endpoint;
        private readonly string _version;
        private readonly string _password;
        private readonly string _tenantId;
        private DateTime? _nextExecution;

        public ShopifyApiClient(IHttpClientFactory httpClientFactory, string tenantId, string endpoint, string version, string password)
        {
            _httpClientFactory = httpClientFactory;
            jsonSerializerOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = true
            };
            jsonSerializerOptions.Converters.Add(new LongToStringConverter());
            jsonSerializerOptions.Converters.Add(new DecimalToStringConverter());
            jsonSerializerOptions.Converters.Add(new DecimalToOptionalConverter());

            _endpoint = endpoint;
            _version = version;
            _password = password;
            _tenantId = tenantId;
        }

        private async Task<T> Post<T>(string query, object variables = null, string specificVersion = null, CancellationToken cancellationToken = default)
        {

            if (_nextExecution != null)
            {
                var timeToSleep = (_nextExecution.Value - DateTime.Now);
                if (timeToSleep > TimeSpan.Zero)
                    await Task.Delay(timeToSleep, cancellationToken);
            }

            var data = new StringContent(JsonSerializer.Serialize(new
            {
                query,
                variables
            }, jsonSerializerOptions), Encoding.UTF8, "application/json");

            var sw = new Stopwatch();
            sw.Restart();
            var response = await CreateClient().PostAsync($"/admin/api/{specificVersion ?? _version}/graphql.json", data, cancellationToken);
            sw.Stop();

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(2000, cancellationToken);
                sw.Restart();
                response = await CreateClient().PostAsync($"/admin/api/{specificVersion ?? _version}/graphql.json", data, cancellationToken);
                sw.Stop();
            }

            var content = await response.Content.ReadAsStringAsync();
            GraphQLResponse<T> result = null;

            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
                throw new Exception($"ShopifyResponse {content}");
            else
            {
                try
                {
                    result = JsonSerializer.Deserialize<GraphQLResponse<T>>(content, jsonSerializerOptions);
                }
                catch (Exception ex)
                {
                    throw new Exception($"ShopifyApiClient-failed to deserialize message", ex);
                }
            }

            var cost = result.extensions?.cost;
            if (cost != null)
            {
                var restoreRate = cost.throttleStatus.restoreRate;
                var available = cost.throttleStatus.currentlyAvailable;
                var halfMaximun = cost.throttleStatus.maximumAvailable / 2;
                if (available < halfMaximun)
                {
                    var toHalf = halfMaximun - available;
                    TimeSpan timeToSleep = TimeSpan.FromSeconds(toHalf / restoreRate);
                    _nextExecution = DateTime.Now + timeToSleep;
                }
                else
                {
                    _nextExecution = null;
                }
            }

            if (result.errors?.Any() == true)
                throw new Exception($"ShopifyResponse {JsonSerializer.Serialize(result.errors)}");

            return result.data;
        }

        public async Task Post(string query, object variables = null, string specificVersion = null, CancellationToken cancellationToken = default)
        {

            await Post<dynamic>(query, variables, specificVersion, cancellationToken);
        }

        public async Task<O> Post<I, O>(BaseMutation<I, O> mutation, CancellationToken cancellationToken = default)
            where I : BaseMutationInput
            where O : BaseMutationOutput, new()
        {
            return await Post<O>(mutation.GetQuery(), mutation.Variables, mutation.ApiVersion, cancellationToken);
        }
        public async Task<O> Post<O>(BaseQuery<O> query, CancellationToken cancellationToken = default)
            where O : BaseQueryOutput, new()
        {
            return await Post<O>(query.GetQuery(), null, query.ApiVersion, cancellationToken);
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