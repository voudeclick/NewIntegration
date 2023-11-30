using Akka.Event;
using Microsoft.AspNetCore.WebUtilities;
using Samurai.Integration.APIClient.Converters;
using Samurai.Integration.APIClient.PluggTo.Models.Results;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.APIClient.PluggTo
{
    public class PluggToApiClient
    {
        private readonly ILoggingAdapter _log;
        private IHttpClientFactory _httpClientFactory;
        private string _clientId;
        private string _clientSecret;
        private string _username;
        private string _password;

        private string _url;
        private LoginResult _session;

        private JsonSerializerOptions jsonSerializerOptions;
        public PluggToApiClient(IHttpClientFactory httpClientFactory, string url, string clientId, string clientSecret, string username, string password, ILoggingAdapter log = null)
        {
            _httpClientFactory = httpClientFactory;
            _log = log;

            _url = url;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _username = username;
            _password = password;

            jsonSerializerOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
            };

            jsonSerializerOptions.Converters.Add(new LongToStringConverter());
            jsonSerializerOptions.Converters.Add(new DecimalToStringConverter());

        }

        public async Task<T> Get<T>(string method, Dictionary<string, string> param = null, CancellationToken cancellationToken = default)
        {
            Stopwatch sw = new Stopwatch();

            if (string.IsNullOrWhiteSpace(_session?.access_token) || _session?.CreatedAt <= DateTime.Now)
                _session = await GetSession(cancellationToken);

            method += $"?access_token={_session.access_token}";

            sw.Restart();

            if (param != null)
                method = QueryHelpers.AddQueryString(method, param);

            var response = await CreateClient().GetAsync(method, cancellationToken);

            sw.Stop();

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"PluggTo GET Call, for method {_url}{method}, failed with the message: {content}");
            }
            else
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(content, jsonSerializerOptions);
                }
                catch (Exception ex)
                {
                    throw new Exception($"PluggTo GET Call, for method {_url}{method}, failed to deserialize message: {content}", ex);
                }
            }
        }

        public async Task<T> Post<T>(string method, object body = null, Dictionary<string, string> param = null, CancellationToken cancellationToken = default)
        {
            Stopwatch sw = new Stopwatch();

            if (string.IsNullOrWhiteSpace(_session?.access_token) || _session?.CreatedAt <= DateTime.Now)
                _session = await GetSession(cancellationToken);

            var data = new StringContent("");

            if (body != null)
                data = new StringContent(JsonSerializer.Serialize(body, jsonSerializerOptions), Encoding.UTF8, "application/json");

            method += $"?access_token={_session.access_token}";

            sw.Restart();

            if (param != null)
                method = QueryHelpers.AddQueryString(method, param);

            var response = await CreateClient().PostAsync(method, data, cancellationToken);

            sw.Stop();

            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"PluggTo POST Call, for method {_url}{method}, failed with the message: {content}");
            }
            else
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(content, jsonSerializerOptions);
                }
                catch (Exception ex)
                {
                    throw new Exception($"PluggTo POST Call, for method {_url}{method}, failed to deserialize message: {content}", ex);
                }
            }
        }

        public async Task<T> Put<T>(string method, object body = null, Dictionary<string, string> param = null, CancellationToken cancellationToken = default)
        {
            Stopwatch sw = new Stopwatch();

            if (string.IsNullOrWhiteSpace(_session?.access_token) || _session?.CreatedAt <= DateTime.Now)
                _session = await GetSession(cancellationToken);

            var data = new StringContent("");

            if (body != null)
                data = new StringContent(JsonSerializer.Serialize(body, jsonSerializerOptions), Encoding.UTF8, "application/json");

            method += $"?access_token={_session.access_token}";

            sw.Restart();

            if (param != null)
                method = QueryHelpers.AddQueryString(method, param);

            var response = await CreateClient().PutAsync(method, data, cancellationToken);

            sw.Stop();

            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"PluggTo PUT Call, for method {_url}{method}, failed with the message: {content}");
            }
            else
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(content, jsonSerializerOptions);
                }
                catch (Exception ex)
                {
                    throw new Exception($"PluggTo PUT Call, for method {_url}{method}, failed to deserialize message: {content}", ex);
                }
            }
        }

        public async Task Delete(string method, CancellationToken cancellationToken = default)
        {
            Stopwatch sw = new Stopwatch();

            if (string.IsNullOrWhiteSpace(_session?.access_token) || _session?.CreatedAt <= DateTime.Now)
                _session = await GetSession(cancellationToken);

            method += $"?access_token={_session.access_token}";

            sw.Restart();

            var response = await CreateClient().DeleteAsync(method, cancellationToken);

            sw.Stop();

            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"PluggTo DELETE Call, for method {_url}{method}, failed with the message: {content}");
            }
        }

        private async Task<LoginResult> GetSession(CancellationToken cancellationToken)
        {
            try
            {
                Stopwatch sw = new Stopwatch();

                var client = CreateClient();

                var param = new Dictionary<string, string>() { };

                param.Add("grant_type", "password");
                param.Add("client_id", _clientId);
                param.Add("client_secret", _clientSecret);
                param.Add("username", _username);
                param.Add("password", _password);

                var data = new FormUrlEncodedContent(param);

                sw.Restart();

                var response = await client.PostAsync("oauth/token", data, cancellationToken);

                sw.Stop();

                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = JsonSerializer.Deserialize<LoginResult>(content, jsonSerializerOptions);
                    result.CreatedAt = DateTime.Now.AddHours(1); //Validade do token

                    return result;
                }
                else
                {
                    throw new Exception($"Could not login to PluggTo (username:{_username})");
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_url);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            return client;
        }
    }
}
