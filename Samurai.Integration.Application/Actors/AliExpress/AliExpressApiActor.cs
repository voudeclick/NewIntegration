using Akka.Actor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.APIClient.AliExpress;
using Samurai.Integration.APIClient.AliExpress.Models.Request;
using Samurai.Integration.APIClient.AliExpress.Models.Response;
using Samurai.Integration.Domain.Messages;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.AliExpress
{
    public class AliExpressApiActor : BaseAliExpressTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly AliExpressSdkClient _client;

        public AliExpressApiActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, TenantDataMessage tenantData) : base("AliExpressApiActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _tenantData = tenantData;

            using (var scope = _serviceProvider.CreateScope())
            {
                var _configuration = scope.ServiceProvider.GetService<IConfiguration>();

                var httpClientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();

                _client = new AliExpressSdkClient(httpClientFactory,
                                                  _configuration.GetSection("UrlAliExpress").Value,
                                                  _configuration.GetSection("UrlAuthentication").Value,
                                                  _configuration.GetSection("TrayAppKey").Value,
                                                  _tenantData.Id.ToString(),
                                                  _log);
            }


            ReceiveAsync((Func<AliExpressListProductRequest, Task>)(async message =>
            {
                try
                {
                    var products = _client.GetProductInfo(message);

                    Sender.Tell(new ReturnMessage<AliExpressListProductResponse>
                    {
                        Result = Result.OK,
                        Data = products
                    });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<AliExpressListProductResponse> { Result = Result.Error, Error = ex });
                }

            }));

            ReceiveAsync((Func<AliexpressCreateOrderDropShippingRequest, Task>)(async message =>
            {
                try
                {
                    var response = _client.CreateOrderDropShipping(message);

                    Sender.Tell(new ReturnMessage<AliexpressCreateOrderDropShippingResponse>
                    {
                        Result = Result.OK,
                        Data = response
                    });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<AliexpressCreateOrderDropShippingResponse> { Result = Result.Error, Error = ex });
                }

            }));

            ReceiveAsync((Func<AliExpressOrderRequest, Task>)(async message =>
            {
                try
                {
                    var response = _client.GetOrderInfo(message);

                    Sender.Tell(new ReturnMessage<AliExpressOrderResponse>
                    {
                        Result = Result.OK,
                        Data = response
                    });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<AliExpressOrderResponse> { Result = Result.Error, Error = ex });
                }

            }));

            ReceiveAsync((Func<AliExpressOrderTrackingRequest, Task>)(async message =>
            {
                try
                {
                    var response = _client.GetOrderTrackingInfo(message);

                    Sender.Tell(new ReturnMessage<AliExpressOrderTrackingResponse>
                    {
                        Result = Result.OK,
                        Data = response
                    });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage<AliExpressOrderTrackingResponse> { Result = Result.Error, Error = ex });
                }

            }));
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, TenantDataMessage tenantData)
        {
            return Akka.Actor.Props.Create(() => new AliExpressApiActor(serviceProvider, cancellationToken, tenantData));
        }
    }
}
