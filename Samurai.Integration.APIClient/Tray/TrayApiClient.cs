using Akka.Event;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using Samurai.Integration.APIClient.Tray.Extension;
using Samurai.Integration.APIClient.Tray.Models.Requests.Product;
using Samurai.Integration.APIClient.Tray.Models.Response;
using Samurai.Integration.APIClient.Tray.Models.Response.Product;
using Samurai.Integration.Domain.Messages.Tray;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.APIClient.Tray
{
    public class TrayApiClient
    {
        private readonly ILoggingAdapter _log;

        public string _urlAuthentication { get; set; }
        public string _methodAuthentication { get; set; }
        public string _tenantId { get; set; }
        public string _keySecret { get; set; }

        //public TrayAuthenticationResponse _trayAuthentication { get; set; }

        public TrayApiClient(string urlAuthentication, string methodAuthentication, string keySecret, string tenantId, ILoggingAdapter log = null)
        {
            _log = log;
            _urlAuthentication = urlAuthentication;
            _methodAuthentication = methodAuthentication;
            _keySecret = keySecret;
            _tenantId = tenantId;
            //if (_tenantId != tenantId)
            //{
            //    _tenantId = tenantId;
            //    _trayAuthentication = null;
            //}
        }

        private RestClient CreateClient(string url)
        {
            var client = new RestClient(url);
            client.AddDefaultHeader("Content-Type", "application/json");

            return client;
        }

        public async Task<T> Get<T>(string method, IDictionary<string, string> param = null, bool generateToken = false)
        {
            try
            {
                var trayAuthentication = await GetToken(generateToken);

                method = string.Format("{0}{1}", trayAuthentication.ApiHost, method);

                if (!string.IsNullOrEmpty(trayAuthentication.AccessToken))
                    method += $"?access_token={trayAuthentication.AccessToken}";

                var client = CreateClient(trayAuthentication.ApiHost);

                if (param != null)
                    method = QueryHelpers.AddQueryString(method, param);

                var request = new RestRequest(method, Method.Get);

                await Task.Delay(500);

                var response = await client.ExecuteAsync(request);

                if (!generateToken && response.StatusCode == HttpStatusCode.Unauthorized)
                    await Get<T>(method, param, true);

                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.NotFound)
                {
                    //_log.Error($"Tray GET Call, for method {method}, failed with the message: {response.Content}");
                    throw new Exception($"Tray GET Call for method {method}, failed with the message: {(string.IsNullOrEmpty(response.Content) ? response.ErrorMessage : response.Content)}");
                }

                try
                {
                    return JsonConvert.DeserializeObject<T>(response.Content);
                }
                catch (Exception ex)
                {
                    //_log.Error($"Tray GET Call, for method {method}, failed to deserialize message: {response.Content}, ex: {ex}");
                    throw new Exception($"Tray GET Call for method {method}, failed to deserialize message: {response.Content}", ex);
                }

            }
            catch (Exception ex)
            {
                //_log.Error(ex.Message);
                throw ex;
            }
        }

        public async Task<T> Post<T>(string method, object body = null, IDictionary<string, string> param = null, bool generateToken = false)
        {
            try
            {
                var trayAuthentication = await GetToken(generateToken);

                method = string.Format("{0}{1}", trayAuthentication.ApiHost, method);

                if (!string.IsNullOrEmpty(trayAuthentication.AccessToken))
                    method += $"?access_token={trayAuthentication.AccessToken}";

                var client = CreateClient(trayAuthentication.ApiHost);

                var request = new RestRequest();
                request.Method = Method.Post;
                request.Timeout = 60000;

                if (param != null)
                    method = QueryHelpers.AddQueryString(method, param);

                request.Resource = method;

                if (body != null)
                {
                    var jsonSettings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };

                    var json = JsonConvert.SerializeObject(body, Formatting.None, jsonSettings);
                    request.AddBody(json, "application/json");
                }

                var response = await client.ExecuteAsync(request);

                if (!generateToken && response.StatusCode == HttpStatusCode.Unauthorized)
                    await Post<T>(method, body, param, true);

                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.NotFound)
                {
                    //_log.Error($"Tray POST Call, for method {method}, failed with the message: {response.Content}, body: {JsonConvert.SerializeObject(body)}");
                    throw new Exception($"Tray POST Call for method {method}, failed with the message: {(string.IsNullOrEmpty(response.Content) ? response.ErrorMessage : response.Content)}, body: {JsonConvert.SerializeObject(body)}");
                }

                try
                {
                    return JsonConvert.DeserializeObject<T>(response.Content);
                }
                catch (Exception ex)
                {
                    //_log.Error($"Tray POST Call, for method {method}, failed to deserialize message: {response.Content}, body: {JsonConvert.SerializeObject(body)}, ex: {ex}");
                    throw new Exception($"Tray POST Call for method {method}, failed to deserialize message: {response.Content}, body: {JsonConvert.SerializeObject(body)}", ex);
                }

            }
            catch (Exception ex)
            {
                //_log.Error(ex.Message);
                throw ex;
            }
        }

        public async Task<T> Put<T>(string method, object body = null, IDictionary<string, string> param = null, bool generateToken = false)
        {
            try
            {
                var trayAuthentication = await GetToken(generateToken);

                method = string.Format("{0}{1}", trayAuthentication.ApiHost, method);

                if (!string.IsNullOrEmpty(trayAuthentication.AccessToken))
                    method += $"?access_token={trayAuthentication.AccessToken}";

                var client = CreateClient(trayAuthentication.ApiHost);

                var request = new RestRequest();
                request.Method = Method.Put;
                request.Timeout = 60000;

                if (param != null)
                    method = QueryHelpers.AddQueryString(method, param);

                request.Resource = method;

                if (body != null)
                {
                    var jsonSettings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };

                    var json = JsonConvert.SerializeObject(body, Formatting.None, jsonSettings);
                    request.AddBody(json, "application/json");
                }

                var response = await client.ExecuteAsync(request);

                if (!generateToken && response.StatusCode == HttpStatusCode.Unauthorized)
                    await Put<T>(method, body, param, true);

                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.NotFound)
                {
                    //_log.Error($"Tray PUT Call, for method {method}, failed with the message: {response.Content}, body: {JsonConvert.SerializeObject(body)}");
                    throw new Exception($"Tray PUT Call for method {method}, failed with the message: {(string.IsNullOrEmpty(response.Content) ? response.ErrorMessage : response.Content)}, body: {JsonConvert.SerializeObject(body)}");
                }

                try
                {
                    return JsonConvert.DeserializeObject<T>(response.Content);
                }
                catch (Exception ex)
                {
                    //_log.Error($"Tray PUT Call, for method {method}, failed to deserialize message: {response.Content}, body: {JsonConvert.SerializeObject(body)}, ex: {ex}");
                    throw new Exception($"Tray PUT Call for method {method}, failed to deserialize message: {response.Content}, body: {JsonConvert.SerializeObject(body)}", ex);
                }

            }
            catch (Exception ex)
            {
                //_log.Error(ex.Message);
                throw ex;
            }
        }

        public async Task<T> Delete<T>(string method, IDictionary<string, string> param = null, bool generateToken = false)
        {

            try
            {
                var trayAuthentication = await GetToken(generateToken);

                method = string.Format("{0}{1}", trayAuthentication.ApiHost, method);

                if (!string.IsNullOrEmpty(trayAuthentication.AccessToken))
                    method += $"?access_token={trayAuthentication.AccessToken}";

                var client = CreateClient(trayAuthentication.ApiHost);

                var request = new RestRequest();
                request.Method = Method.Delete;
                request.Timeout = 60000;

                if (param != null)
                    method = QueryHelpers.AddQueryString(method, param);

                request.Resource = method;

                var response = await client.ExecuteAsync(request);

                if (!generateToken && response.StatusCode == HttpStatusCode.Unauthorized)
                    await Delete<T>(method, param, true);

                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.NotFound)
                {
                    //_log.Error($"Tray DELETE Call, for method {method}, failed with the message: {response.Content}");
                    throw new Exception($"Tray DELETE Call for method {method}, failed with the message: {(string.IsNullOrEmpty(response.Content) ? response.ErrorMessage : response.Content)}");
                }

                try
                {
                    return JsonConvert.DeserializeObject<T>(response.Content);
                }
                catch (Exception ex)
                {
                    //_log.Error($"Tray DELETE Call, for method {method}, failed to deserialize message: {response.Content}, ex: {ex}");
                    throw new Exception($"Tray DELETE Call for method {method}, failed to deserialize message: {response.Content}", ex);
                }

            }
            catch (Exception ex)
            {
                //_log.Error(ex.Message);
                throw ex;
            }
        }

        public async Task<TrayAuthenticationResponse> GetToken(bool generateToken = false)
        {
            var method = string.Format("{0}/{1}", _urlAuthentication, _methodAuthentication);

            try
            {
                var samuraiIntegrationId = _tenantId;
                var body = new
                {
                    SamuraiIntegrationId = samuraiIntegrationId,
                    GenerateToken = generateToken
                };

                //HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_keySecret));
                //string hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(body)));

                var client = new RestClient(method);
                //client.AddDefaultHeader("Content-Type", "application/json");
                //client.AddDefaultHeader("X-SamuraiIntegrations-Hmac-SHA256", hash);

                var request = new RestRequest
                {
                    Method = Method.Post,
                    Resource = method
                };

                request.AddJsonBody(body);

                var response = await client.ExecuteAsync(request);

                if (response.StatusCode != HttpStatusCode.OK || string.IsNullOrEmpty(response.Content))
                    return await GetToken(true);

                try
                {
                    var result = JsonConvert.DeserializeObject<TrayAuthenticationResponse>(response.Content);

                    if (result.ApiHost.Substring(result.ApiHost.Length - 1, 1) != "/")
                        result.ApiHost += "/";

                    return result;
                }
                catch (Exception ex)
                {
                    //_log.Error($"Tray POST Call, for method {method}, body {body}, failed to deserialize message: {response.Content}, ex: {ex}");

                    throw new Exception($"Tray POST Call for method {method}, body {body}, failed to deserialize message: {response.Content}, ex: {ex}", ex);
                }
            }
            catch (Exception ex)
            {
                //_log.Error(ex.Message);
                throw ex;
            }
        }

        public async Task<bool> UpdateProductProcessing(UpdateProductProcessingRequest message)
        {
            var method = string.Format("{0}/aliexpress/ProcessProductIntegration", _urlAuthentication);

            try
            {
                var body = JsonConvert.SerializeObject(message.TrayAppMessage);

                HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_keySecret));
                string hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(body)));

                var client = new RestClient(method);
                client.AddDefaultHeader("Content-Type", "application/json");
                client.AddDefaultHeader("X-SamuraiIntegrations-Hmac-SHA256", hash);

                var request = new RestRequest
                {
                    Method = Method.Post,
                    Resource = method
                };

                request.AddBody(message.TrayAppMessage, "application/json");

                var response = await client.ExecuteAsync(request);

                if (response.StatusCode != HttpStatusCode.OK)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<GetProductProcessResponse> GetProductProcess(GetProductProcessRequest message)
        {
            var method = string.Format("{0}/aliexpress/GetProductIntegration/{1}", _urlAuthentication, message.ProductId);

            try
            {
                var body = JsonConvert.SerializeObject(message.ProductId);

                HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_keySecret));
                string hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(body)));

                var client = new RestClient(method);
                client.AddDefaultHeader("Content-Type", "application/json");
                client.AddDefaultHeader("X-SamuraiIntegrations-Hmac-SHA256", hash);

                var request = new RestRequest
                {
                    Method = Method.Get,
                    Resource = method
                };

                var response = await client.ExecuteAsync(request);

                if (response.StatusCode != HttpStatusCode.OK)
                    return null;

                try
                {
                    return JsonConvert.DeserializeObject<GetProductProcessResponse>(response.Content);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Tray POST Call for method {method}, failed to deserialize message: {response.Content}, body: {JsonConvert.SerializeObject(body)}", ex);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
