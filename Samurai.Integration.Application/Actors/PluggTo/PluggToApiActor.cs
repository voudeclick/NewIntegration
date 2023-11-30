using Akka.Actor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.APIClient.PluggTo;
using Samurai.Integration.APIClient.PluggTo.Models.Requests;
using Samurai.Integration.APIClient.PluggTo.Models.Results;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.PluggTo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.PluggTo
{
    public class PluggToApiActor : BasePluggToTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly PluggToApiClient _client;

        public PluggToApiActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, PluggToData pluggToData) : base("PluggToApiActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _pluggToData = pluggToData;

            using (var scope = _serviceProvider.CreateScope())
            {
                var _configuration = scope.ServiceProvider.GetService<IConfiguration>();

                var httpClientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();

                _client = new PluggToApiClient(httpClientFactory, _configuration.GetSection("PluggTo")["Url"],
                                                                  _pluggToData.ClientId, pluggToData.ClientSecret,
                                                                  pluggToData.Username, pluggToData.Password, _log);               
            }


            ReceiveAsync((Func<PluggToApiListProductRequest, Task>)(async message =>
            {
                try
                {
                    var products = await GetAllProducts(message);

                    LogInfo($"{(products.result != null ? products.result.Count : 0)} products.", new { ObjectName = nameof(PluggToApiListProductRequest),
                        MessageId = message.ProductCode, AccSellerId = message.AccountSellerId,  AccUserId = message.AccountUserId });

                    Sender.Tell(new ReturnMessage<PluggToApiListProductsResult>
                    {
                        Result = Result.OK,
                        Data = products
                    });
                }
                catch (Exception ex)
                {
                    LogError(ex, "PluggToApiActor - Error in PluggToApiListProductRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(),message, null));
                    Sender.Tell(new ReturnMessage<PluggToApiListProductRequest> { Result = Result.Error, Error = ex });
                }

            }));


            #region orders

            ReceiveAsync((Func<PluggToApiCreateOrderRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting PluggToApiCreateOrderRequest");

                    var response = await CreateOrder(message);


                    if (response.Order != null)
                    {
                        LogInfo($"received order to create from PluggTo.",
                            new { ObjectName = nameof(PluggToApiCreateOrderRequest),
                                Id = response.Order.id, OrderOriginal = response.Order.original_id, OrderId = response.Order.order_id });
                    }

                    Sender.Tell(new ReturnMessage<PluggToApiOrderResult>
                    {
                        Result = Result.OK,
                        Data = response
                    });
                }
                catch (Exception ex)
                {
                    LogError(ex, "PluggToApiActor - Error in PluggToApiCreateOrderRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage<PluggToApiOrderResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<PluggToApiListOrderRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting PluggToApiListOrderRequest");

                    var response = await GetOrder(message);
                    if (response.Order != null)
                    {
                        LogInfo($"received order from PluggTo.",
                            new { ObjectName = nameof(PluggToApiListOrderRequest), 
                                Id = response.Order.id, OrderOriginal = response.Order.original_id, OrderId = response.Order.order_id });
                    }

                    Sender.Tell(new ReturnMessage<PluggToApiOrderResult>
                    {
                        Result = Result.OK,
                        Data = response
                    });
                }
                catch (Exception ex)
                {
                    LogError(ex, "PluggToApiActor - Error in PluggToApiListOrderRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage<PluggToApiListOrderRequest> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<PluggToApiUpdateOrderRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting PluggToApiUpdateOrderRequest");

                    var response = await UpdateOrder(message);

                    if(response.Order != null)
                    {
                        LogInfo("received update order from PluggTo.", 
                            new { ObjectName = nameof(PluggToApiUpdateOrderRequest), Id = response.Order.id, OrderOriginal = response.Order.original_id, OrderId = response.Order.order_id });
                    }

                    Sender.Tell(new ReturnMessage<PluggToApiOrderResult>
                    {
                        Result = Result.OK,
                        Data = response
                    });
                }
                catch (Exception ex)
                {
                    LogError(ex, "PluggToApiActor - Error in PluggToApiUpdateOrderRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage<PluggToApiOrderResult> { Result = Result.Error, Error = ex });
                }
            }));

            #endregion
        }

        #region Products

        private async Task<PluggToApiListProductsResult> GetAllProducts(PluggToApiListProductRequest request)
        {
            PluggToApiListProductsResult response = null;
            var products = new List<PluggToApiListProductsResult.Produto>();

            var param = new Dictionary<string, string>
            {
                { "limit", "100" },
                { "user_id", request.AccountUserId } //id do user na plugg to
            };

            #region filters

            if (!string.IsNullOrEmpty(request.AccountSellerId))
                param.Add("supplier_id", request.AccountSellerId);

            if (!string.IsNullOrEmpty(request.ExternalId))
                param.Add("id", request.ExternalId);
            else if (!string.IsNullOrEmpty(request.ProductCode))
                param.Add("sku", request.ProductCode);

            if (request.CreatedAt.HasValue)
            {
                var now = $"{TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")):yyyy-MM-dd}";
                param.Add("created", $"{request.CreatedAt.Value:yyyy-MM-dd}to{now}");
            }

            LogInfo($"getting all products.", param);

            #endregion

            string last_pluggto_id = string.Empty;

            while (true)
            {
                try
                {
                    if (!string.IsNullOrEmpty(last_pluggto_id))
                    {
                        if (!param.ContainsKey("next"))
                            param.Add("next", last_pluggto_id);
                        else
                            param["next"] = last_pluggto_id;
                    }

                    response = await _client.Get<PluggToApiListProductsResult>("products", param, _cancellationToken);

                    if (response?.result?.Any() == false)
                        break;

                    products.AddRange(response.result.Select(x => x.Product).ToList());

                    last_pluggto_id = products.Select(x => x.id).LastOrDefault();

                    if (response?.result?.Count < 100)
                        break;
                }
                catch
                {
                    last_pluggto_id = string.Empty;
                    break;
                }
            }

            return response;
        }

        #endregion

        #region Orders

        private async Task<PluggToApiOrderResult> CreateOrder(PluggToApiCreateOrderRequest request)
        {
            return await _client.Post<PluggToApiOrderResult>("orders", request, param: null, _cancellationToken);
        }

        private async Task<PluggToApiOrderResult> GetOrder(PluggToApiListOrderRequest request)
        {
            return await _client.Get<PluggToApiOrderResult>($"orders/{request.ExternalOrderId}", param: null, _cancellationToken);
        }

        private async Task<PluggToApiOrderResult> UpdateOrder(PluggToApiUpdateOrderRequest body)
        {
            return await _client.Put<PluggToApiOrderResult>($"orders/{body.Order.id}", body, param: null, _cancellationToken);
        }

        #endregion

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, PluggToData pluggToData)
        {
            return Akka.Actor.Props.Create(() => new PluggToApiActor(serviceProvider, cancellationToken, pluggToData));
        }

    }

}
