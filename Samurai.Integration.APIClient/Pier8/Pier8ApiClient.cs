using Akka.Event;
using Samurai.Integration.APIClient.Converters;
using Samurai.Integration.APIClient.Pier8.Models;
using Samurai.Integration.APIClient.Pier8.Models.Requests;
using Samurai.Integration.APIClient.Pier8.Models.Response;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Samurai.Integration.APIClient.Pier8
{
    public class Pier8ApiClient
    {
        private readonly ILoggingAdapter _log;
        private JsonSerializerOptions jsonSerializerOptions;
        private IHttpClientFactory _httpClientFactory;
        private Credentials _credentials;
        private ILoggingAdapter log;
        private string _token = null;
        private string _url = null;


        public Pier8ApiClient(IHttpClientFactory httpClientFactory, string url, Credentials credentials, ILoggingAdapter log = null)
        {
            _httpClientFactory = httpClientFactory;
            jsonSerializerOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = true
            };
            jsonSerializerOptions.Converters.Add(new LongToStringConverter());
            jsonSerializerOptions.Converters.Add(new DecimalToStringConverter());
            _credentials = credentials;
            _url = url;
            _log = log;
        }

        public async Task<T> Post<T>(string method, IBodyXml body = null, CancellationToken cancellationToken = default)
        {
            Stopwatch sw = new Stopwatch();
       

            var client = CreateClient();

            var data = new StringContent("");
            if (body != null)
                data = GetDataXml(body);

            sw.Restart();
            var response = await client.PostAsync(method, data, cancellationToken);
            sw.Stop();
            _log?.Debug($"Pier8 api - POST {method} Took {sw.ElapsedMilliseconds} ms");


            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"Pier8 POST Call, for method {_url}{method}, failed with the message: {content}");
            }
            else
            {
                try
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.LoadXml(content);

                    XmlNodeList nodeList = xmldoc.GetElementsByTagName("parameters");
                    string jsonXml = string.Empty;
                    foreach (XmlNode node in nodeList)
                    {
                        jsonXml = node.InnerText;
                    }

                    return JsonSerializer.Deserialize<T>(jsonXml);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Pier8 POST Call, for method {_url}{method}, failed to deserialize message: {content}", ex);
                }
            }
        }

        public async Task Post(string method, IBodyXml body = null, CancellationToken cancellationToken = default)
        {
            Stopwatch sw = new Stopwatch();
            var client = CreateClient();

            var data = new StringContent("");
            if (body != null)
                data = GetDataXml(body);

            sw.Restart();
            var response = await client.PostAsync(method, data, cancellationToken);
            sw.Stop();
            _log?.Debug($"Pier8 api - POST {method} Took {sw.ElapsedMilliseconds} ms");
            
            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"Pier8 POST Call, for method {_url}{method}, failed with the message: {content}");
            }
            else
            {
                return;
            }
        }

        private StringContent GetDataXml(IBodyXml data)
        {
            var xmlBuilder = new StringBuilder();
            xmlBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            xmlBuilder.Append("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            xmlBuilder.Append("    <SOAP-ENV:Body>");
            xmlBuilder.Append($"        <apikey>{_credentials.ApiKey}</apikey>");
            xmlBuilder.Append($"        <token>{_credentials.Token}</token>");
            xmlBuilder.Append($"        {data.ToXml()}");
            xmlBuilder.Append("    </SOAP-ENV:Body>");
            xmlBuilder.Append("</SOAP-ENV:Envelope>");

            return new StringContent(xmlBuilder.ToString(), Encoding.UTF8, "application/xml");
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_url);
            return client;
        }

    }
}
