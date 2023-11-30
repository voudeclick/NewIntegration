using Akka.Event;
using Samurai.Integration.APIClient.Converters;
using Samurai.Integration.APIClient.Omie.Models.Request;
using Samurai.Integration.APIClient.Omie.Models.Result;
using Samurai.Integration.Domain.Consts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.APIClient.Omie.Clients
{
    public abstract class BaseOmieClient
    {
        private string BaseUrl => "https://app.omie.com.br/api/v1/";
        protected abstract string Path { get; }
        private readonly string _appKey;
        private readonly string _appSecret;

        private JsonSerializerOptions jsonSerializerOptions;
        private IHttpClientFactory _httpClientFactory;

        protected BaseOmieClient(IHttpClientFactory httpClientFactory, string appKey, string appSecret, ILoggingAdapter log)
        {
            _httpClientFactory = httpClientFactory;
            jsonSerializerOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = true
            };
            jsonSerializerOptions.Converters.Add(new LongToStringConverter());
            jsonSerializerOptions.Converters.Add(new DecimalToStringConverter());
            _appKey = appKey;
            _appSecret = appSecret;
        }

        public async Task<O> Post<I, O>(BaseOmieRequest<I, O> request, CancellationToken cancellationToken = default)
            where I : BaseOmieInput
            where O : BaseOmieOutput, new()
        {
            return await Post<O>(request.Method, request.Variables, cancellationToken);
        }

        private async Task<T> Post<T>(string method, object content = null, CancellationToken cancellationToken = default)
        {
            var body = GetBody(method, content);
            var data = new StringContent(body);
            data.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var sw = new Stopwatch();
            sw.Restart();
            var response = await CreateClient().PostAsync(Path, data, cancellationToken);
            sw.Stop();

            var responseContent = await response.Content.ReadAsStringAsync();
            T result = default(T);
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                OmieError omieError = null;
                try
                {
                    omieError = JsonSerializer.Deserialize<OmieError>(responseContent, jsonSerializerOptions);
                }
                catch
                {
                    omieError = null;
                }

                if (!string.IsNullOrWhiteSpace(omieError?.faultcode))
                {
                    throw new OmieException(omieError, $"{OmieConsts.OmieApiPostCall}, failed with the message: {responseContent}");
                }                    
                else
                    throw new Exception($"{OmieConsts.OmieApiPostCall}, failed with the message: {responseContent}");
            }
            else
            {
                try
                {
                    result = JsonSerializer.Deserialize<T>(responseContent, jsonSerializerOptions);
                }
                catch (Exception ex)
                {
                    throw new Exception($"{OmieConsts.OmieApiPostCall}, failed to deserialize message: {responseContent} | ErrorMessage: {ex.Message}", ex);
                }
            }

            return result;
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(BaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            return client;
        }

        private string GetBody(string method, object content)
        {
            return JsonSerializer.Serialize(new
            {
                call = method,
                app_key = _appKey,
                app_secret = _appSecret,
                param = new List<object>
                {
                    content
                }
            }, jsonSerializerOptions);
        }
    }
}
