using Akka.Event;
using Polly;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VDC.Integration.APIClient.Converters;
using VDC.Integration.APIClient.Millennium.Models.Results;
using VDC.Integration.Domain.Consts;
using VDC.Integration.Domain.Messages.Millennium;
using VDC.Integration.Domain.Results.Logger;
using VDC.Integration.EntityFramework.Repositories;

namespace VDC.Integration.APIClient.Millennium
{
    public class MillenniumApiClient
    {
        private readonly JsonSerializerOptions jsonSerializerOptions;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _url;
        private readonly string _login;
        private readonly string _password;
        private readonly string _tenantId;
        private readonly ILoggingAdapter _log;
        private string _session;
        private readonly MillenniumSessionToken _sessionToken;

        public MillenniumApiClient(IHttpClientFactory httpClientFactory,
                                   MillenniumData millenniumData,
                                   string url,
                                   string login,
                                   string password,
                                   MillenniumSessionToken millenniumSessionToken,
                                   ILoggingAdapter log = null)
        {
            _httpClientFactory = httpClientFactory;
            jsonSerializerOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = true
            };
            jsonSerializerOptions.Converters.Add(new LongToStringConverter());
            jsonSerializerOptions.Converters.Add(new DecimalToStringConverter());

            _url = url;
            _login = login;
            _password = password;
            _log = log;
            _tenantId = millenniumData.Id.ToString();
            _sessionToken = millenniumSessionToken;
            _session = millenniumData.SessionToken;
        }

        public async Task<T> Get<T>(string method, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_session))
                await GetSession(cancellationToken);

            var client = CreateClient();
            client.DefaultRequestHeaders.Add("WTS-Session", _session);

            var response = await client.GetAsync(method, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await GetSession(cancellationToken);
                client = CreateClient();
                client.DefaultRequestHeaders.Add("WTS-Session", _session);
                response = await client.GetAsync(method, cancellationToken);
            }

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                var resultContent = string.IsNullOrWhiteSpace(content) ? "Sem dados no retorno" : content;
                throw new Exception($"{MillenniumConsts.MillenniumApiClient} {resultContent} - StatusCode: {response.StatusCode}");
            }

            else
            {
                try
                {
                    content = new string(content.Where(c => !char.IsControl(c)).ToArray());
                    return JsonSerializer.Deserialize<T>(content, jsonSerializerOptions);
                }
                catch (Exception ex)
                {
                    throw new Exception($"{MillenniumConsts.MillenniumApiClient} {content}", ex);
                }
            }
        }

        public async Task<(T, string)> GetContent<T>(string method, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_session))
                await GetSession(cancellationToken);

            var client = CreateClient();
            client.DefaultRequestHeaders.Add("WTS-Session", _session);

            var response = await client.GetAsync(method, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await GetSession(cancellationToken);
                client = CreateClient();
                client.DefaultRequestHeaders.Add("WTS-Session", _session);
                response = await client.GetAsync(method, cancellationToken);
            }

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
                throw new Exception($"{MillenniumConsts.MillenniumApiClient} {content}");
            else
            {
                try
                {
                    content = new string(content.Where(c => !char.IsControl(c)).ToArray());
                    return (JsonSerializer.Deserialize<T>(content, jsonSerializerOptions), content);
                }
                catch (Exception ex)
                {
                    throw new Exception($"{MillenniumConsts.MillenniumApiClient} {content}", ex);
                }
            }
        }

        public async Task<T> Post<T>(string method, object body = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_session))
                await GetSession(cancellationToken);

            var client = CreateClient();
            client.DefaultRequestHeaders.Add("WTS-Session", _session);

            var data = new StringContent("");
            string bodyRequest = string.Empty;

            if (body != null)
            {
                bodyRequest = JsonSerializer.Serialize(body, jsonSerializerOptions);
                data = new StringContent(bodyRequest, Encoding.UTF8, "application/json");
            }

            var response = await client.PostAsync(method, data, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await GetSession(cancellationToken);
                client = CreateClient();
                client.DefaultRequestHeaders.Add("WTS-Session", _session);
                response = await client.PostAsync(method, data, cancellationToken);
            }

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
                throw new Exception($"{MillenniumConsts.MillenniumApiClient}  {content}");
            else
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(content, jsonSerializerOptions);
                }
                catch (Exception ex)
                {
                    throw new Exception($"{MillenniumConsts.MillenniumApiClient}  {content}", ex);
                }
            }
        }

        public async Task Post(string method, object body = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_session))
                await GetSession(cancellationToken);

            var client = CreateClient();
            client.DefaultRequestHeaders.Add("WTS-Session", _session);

            var data = new StringContent("");
            string bodyRequest = string.Empty;
            if (body != null)
            {
                bodyRequest = JsonSerializer.Serialize(body, jsonSerializerOptions);
                data = new StringContent(bodyRequest, Encoding.UTF8, "application/json");
            }

            var response = await client.PostAsync(method, data, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await GetSession(cancellationToken);
                client = CreateClient();
                client.DefaultRequestHeaders.Add("WTS-Session", _session);
                response = await client.PostAsync(method, data, cancellationToken);
            }

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
                throw new Exception($"{MillenniumConsts.MillenniumApiClient}  {content}");
            else
                _log?.Info("MillenniumApiClient - {0}", LoggerApiDescription.From(_tenantId, $"POST [{_url}{method}]", bodyRequest, content));
        }

        private async Task GetSession(CancellationToken cancellationToken)
        {
            var session = string.Empty;
            try
            {
                var client = CreateClient();
                client.DefaultRequestHeaders.Add("WTS-Authorization", $"{_login}/{_password}");

                var retryPolicy = Policy
                    .Handle<HttpRequestException>()
                    .RetryAsync(3, async (exception, retryCount) => await Task.Delay(250).ConfigureAwait(false));

                var fallbackPolicy = Policy
                    .Handle<HttpRequestException>()
                    .FallbackAsync(async x => await HandlePolicyLoginError(_tenantId, x).ConfigureAwait(false));

                await fallbackPolicy
                    .WrapAsync(retryPolicy)
                    .ExecuteAsync(async () =>
                    {
                        var response = await client.PostAsync("api/login", new StringContent(""), cancellationToken);
                        response.EnsureSuccessStatusCode();

                        var content = await response.Content.ReadAsStringAsync();
                        session = JsonSerializer.Deserialize<LoginResult>(content, jsonSerializerOptions).session;
                        _log?.Info("millenium tenant id: {0} session {1}", _tenantId, session);

                    }).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(session))
                {
                    await _sessionToken.Save(long.Parse(_tenantId), session);
                }
            }
            catch (Exception ex)
            {
                _log?.Warning("Erro ao salvar session da Millennium - TenantId:{0} | Erro:{1}", _tenantId, ex.Message);
            }

            _session = session;
        }

        private async Task<string> HandlePolicyLoginError(string _tenantId, CancellationToken cancellationToken)
        {
            _log?.Info("Tentativa de login sem sucesso - millenium tenant id: {0} ", _tenantId);
            return await Task.FromResult(string.Empty);
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_url);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            //rever 
            if (_tenantId == "13")
                client.Timeout = TimeSpan.FromMinutes(4);
            return client;
        }
    }
}
