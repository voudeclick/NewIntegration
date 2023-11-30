using Akka.Event;
using Newtonsoft.Json;
using Samurai.Integration.APIClient.AliExpress.Models.Request;
using Samurai.Integration.APIClient.AliExpress.Models.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;
using static Top.Api.Request.AliexpressTradeBuyPlaceorderRequest;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Samurai.Integration.APIClient.AliExpress
{
    public class AliExpressSdkClient
    {
        private readonly ILoggingAdapter _log;
        private IHttpClientFactory _httpClientFactory;

        private string _aliExpressUrl;
        public string _authenticationUrl { get; set; }
        public string _keySecret { get; set; }
        public string _tenantId { get; set; }

        private AliExpressAuthenticationResponse _authentication = null;

        public AliExpressSdkClient(IHttpClientFactory httpClientFactory, string aliExpressUrl, string authenticationUrl, string keySecret, string tenantId, ILoggingAdapter log = null)
        {
            _httpClientFactory = httpClientFactory;
            _log = log;

            _aliExpressUrl = aliExpressUrl;
            _authenticationUrl = authenticationUrl;
            _keySecret = keySecret;
            _tenantId = tenantId;

        }

        public AliExpressListProductResponse GetProductInfo(AliExpressListProductRequest request)
        {
            if (_authentication == null)
                _authentication = SetToken(_authenticationUrl, _tenantId);

            ITopClient client = new DefaultTopClient(_aliExpressUrl, _authentication.AppKey, _authentication.AppSecret, "json");

            AliexpressPostproductRedefiningFindaeproductbyidfordropshipperRequest req = new AliexpressPostproductRedefiningFindaeproductbyidfordropshipperRequest();

            req.ProductId = request.ProductId;
            req.LocalCountry = request.LocalCountry;
            req.LocalLanguage = request.LocalLanguage;

            AliexpressPostproductRedefiningFindaeproductbyidfordropshipperResponse rsp = client.Execute(req, _authentication.AccessToken);

            var product = JsonConvert.DeserializeObject<AliExpressListProductResponse>(rsp.Body);

            return product;
        }

        public AliExpressOrderResponse GetOrderInfo(AliExpressOrderRequest request)
        {
            if (_authentication == null)
                _authentication = SetToken(_authenticationUrl, _tenantId);

            ITopClient client = new DefaultTopClient(_aliExpressUrl, _authentication.AppKey, _authentication.AppSecret, "json");

            AliexpressTradeDsOrderGetRequest req = new AliexpressTradeDsOrderGetRequest();

            AliexpressTradeDsOrderGetRequest.AeopSingleOrderQueryDomain order = new AliexpressTradeDsOrderGetRequest.AeopSingleOrderQueryDomain();
            order.OrderId = request.AliExpressOrderId;
            req.SingleOrderQuery_ = order;

            AliexpressTradeDsOrderGetResponse rsp = client.Execute(req, _authentication.AccessToken);

            var response = JsonConvert.DeserializeObject<AliExpressOrderResponse>(rsp.Body);

            return response;
        }
        public AliExpressOrderTrackingResponse GetOrderTrackingInfo(AliExpressOrderTrackingRequest request)
        {
            if (_authentication == null)
                _authentication = SetToken(_authenticationUrl, _tenantId);

            ITopClient client = new DefaultTopClient(_aliExpressUrl, _authentication.AppKey, _authentication.AppSecret, "json");
            ;
            AliexpressLogisticsDsTrackinginfoQueryRequest req = new AliexpressLogisticsDsTrackinginfoQueryRequest();

            req.LogisticsNo = request.TrackingCode;
            //req.Origin = "ESCROW";
            req.OutRef = request.AliExpressOrderId.ToString();
            //req.ServiceName = "UPS";
            //req.ToArea = "DE";

            AliexpressLogisticsDsTrackinginfoQueryResponse rsp = client.Execute(req, _authentication.AccessToken);

            var response = JsonConvert.DeserializeObject<AliExpressOrderTrackingResponse>(rsp.Body);

            return response;
        }

        public AliexpressCreateOrderDropShippingResponse CreateOrderDropShipping(AliexpressCreateOrderDropShippingRequest request)
        {
            if (_authentication == null)
                _authentication = SetToken(_authenticationUrl, _tenantId);

            ITopClient client = new DefaultTopClient(_aliExpressUrl, _authentication.AppKey, _authentication.AppSecret, "json");

            AliexpressTradeBuyPlaceorderRequest req = new AliexpressTradeBuyPlaceorderRequest();

            var address = new AliexpressTradeBuyPlaceorderRequest.MaillingAddressRequestDtoDomain()
            {
                Address = request.LogisticsAddress.Address,
                Address2 = request.LogisticsAddress.Address2,
                City = request.LogisticsAddress.City,
                ContactPerson = request.LogisticsAddress.ContactPerson,
                Country = "BR",
                Cpf = request.LogisticsAddress.Cpf.Replace(".", "").Replace("-", ""),
                FullName = request.LogisticsAddress.FullName,
                Locale = "pt-BR",
                MobileNo = request.LogisticsAddress.MobileNo,
                PhoneCountry = request.LogisticsAddress.PhoneCountry,
                Province = request.LogisticsAddress.Province,
                Zip = request.LogisticsAddress.Zip.Replace(".", "").Replace("-", ""),
            };


            var items = request.Items.Select(x => new AliexpressTradeBuyPlaceorderRequest.ProductBaseItemDomain()
            {
                ProductCount = x.ProductCount,
                ProductId = x.ProductCount,
                SkuAttr = x.SkuAttr
            }).ToList();

            req.ParamPlaceOrderRequest4OpenApiDTO_ = new AliexpressTradeBuyPlaceorderRequest.PlaceOrderRequest4OpenApiDtoDomain()
            {
                LogisticsAddress = address,
                ProductItems = items
            };

            AliexpressTradeBuyPlaceorderResponse rsp = client.Execute(req, _authentication.AccessToken);

            var product = JsonConvert.DeserializeObject<AliexpressCreateOrderDropShippingResponse>(rsp.Body);

            return product;
        }

        private AliExpressAuthenticationResponse SetToken(string authenticationUrl, string tenantId)
        {
            try
            {
                //return new AliExpressAuthenticationResponse()
                //{
                //    AppKey = "32824060",
                //    AppSecret = "8969d278e3f8c552915ef18dbdb0d336",
                //    StoreId = "1021540",
                //    AccessToken = "50002600304cv9ba194cb4a6bqzht9HkvZqsd8dlxfqPgxQFXjhx5kvWDGxTSzd7grq1"

                //};

                var task = Task.Run(async () => await Authenticate<AliExpressAuthenticationResponse>(authenticationUrl, tenantId));
                return task.Result;
            }
            catch (Exception ex)
            {
                //_log?.Error($"AliExpress POST Call, failed to deserialize token", ex);
                throw new Exception($"AliExpress POST Call, failed to deserialize token", ex);
            }
        }
        public async Task<T> Authenticate<T>(string authenticationUrl, string tenantId, CancellationToken cancellationToken = default)
        {
            Stopwatch sw = new Stopwatch();

            sw.Restart();

            string method = $"{authenticationUrl}/";

            var body = JsonSerializer.Serialize(new
            {
                SamuraiIntegrationId = tenantId,
                GenerateToken = false,
            });

            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_keySecret));
            string hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(body)));

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(authenticationUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("X-SamuraiIntegrations-Hmac-SHA256", hash);

            var request = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(method, request, cancellationToken);

            sw.Stop();

            //_log?.Debug($"AliExpress Api - POST {method} Took {sw.ElapsedMilliseconds} ms");

            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK )
            {
                throw new Exception($"AliExpress Api POST Call, for method {method}, failed with the message: {content}");
            }
            else
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(content);
                }
                catch (Exception ex)
                {
                    throw new Exception($"AliExpress Api POST Call, for method {method}, failed to deserialize message: {content}", ex);
                }
            }
        }

    }
}
