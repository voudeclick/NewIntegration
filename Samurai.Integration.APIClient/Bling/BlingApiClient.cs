using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Akka.Event;

using Samurai.Integration.APIClient.Converters;
using Samurai.Integration.Domain.Enums.Bling;

namespace Samurai.Integration.APIClient.Bling
{
    public class BlingApiClient
    {
        private IHttpClientFactory _httpClientFactory;
        private readonly ILoggingAdapter _log;
        private JsonSerializerOptions jsonSerializerOptions;
        private string _url;
        private readonly string _apikey;

        public BlingApiClient(IHttpClientFactory httpClientFactory, string url, string apikey, ILoggingAdapter log)
        {
            _httpClientFactory = httpClientFactory;
            jsonSerializerOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = true
            };
            jsonSerializerOptions.Converters.Add(new LongToStringConverter());
            jsonSerializerOptions.Converters.Add(new DecimalToStringConverter());
            _url = url;
            _apikey = apikey;
            _log = log;
        }

        public async Task<T> Get<T>(string method, CancellationToken cancellationToken = default)
        {
            Stopwatch sw = new Stopwatch();

            var client = CreateClient();

            method += $"&apikey={_apikey}";

            sw.Restart();
            var response = await client.GetAsync(method, cancellationToken);
            sw.Stop();

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(2000, cancellationToken);

                sw.Restart();
                response = await client.GetAsync(method, cancellationToken);
                sw.Stop();
            }

            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"Bling GET Call, for method {_url}{method}, failed with the message: {content}");
            }
            else
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(content, jsonSerializerOptions);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Bling GET Call, for method {_url}{method}, failed to deserialize message: {content}", ex);
                }
            }
        }
        public async Task<T> PostXML<T>(string method, object body = null, CancellationToken cancellationToken = default)
        {
            return await SendXML<T>(HttpMethod.Post, method, body, cancellationToken);
        }

        public async Task<T> PutXML<T>(string method, object body = null, CancellationToken cancellationToken = default)
        {
            return await SendXML<T>(HttpMethod.Put, method, body, cancellationToken);
        }

        private async Task<T> SendXML<T>(HttpMethod httpMethod, string method, object body = null, CancellationToken cancellationToken = default)
        {
            Stopwatch sw = new Stopwatch();

            var client = CreateClient();

            var xmlSerializer = new XmlSerializer(body.GetType());
            string xmlString = string.Empty;

            using (var textWriter = new Utf8StringWriter())
            {
                xmlSerializer.Serialize(textWriter, body);
                xmlString = textWriter.ToString();
            }

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("apikey", _apikey),
                new KeyValuePair<string, string>("xml", xmlString)
            });

            sw.Restart();

            var response = httpMethod switch
            {
                HttpMethod m when m == HttpMethod.Post => await client.PostAsync(method, formContent, cancellationToken),
                HttpMethod m when m == HttpMethod.Put => await client.PutAsync(method, formContent, cancellationToken),
                _ => throw new ArgumentException("Invalid http method")
            };

            sw.Stop();

            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"Bling POST Call, for method {_url}{method}, failed with the message: {content}");
            }
            else
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(content, jsonSerializerOptions);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Bling POST Call, for method {_url}{method}, failed to deserialize message: {content}", ex);
                }
            }
        }

        public async Task<BlingStatusUser> ValidateBlingUser(string method, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();

            method = String.Concat(method, $"apikey={_apikey}");
            var response = await client.GetAsync(method, cancellationToken);

            if (response is null)
                throw new Exception($"ValidateUserBling - response is null");

            if (response.StatusCode != HttpStatusCode.Unauthorized && response.StatusCode != HttpStatusCode.Forbidden)
                return BlingStatusUser.Success;

            return BlingStatusUser.Fault;
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_url);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            return client;
        }

        public class Utf8StringWriter : StringWriter
        {
            // Use UTF8 encoding but write no BOM to the wire
            public override Encoding Encoding
            {
                get { return new UTF8Encoding(false); } // in real code I'll cache this encoding.
            }
        }
    }
}
