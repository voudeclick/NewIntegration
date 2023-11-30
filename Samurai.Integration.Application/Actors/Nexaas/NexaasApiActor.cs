using Akka.Actor;
using Microsoft.AspNetCore.WebUtilities;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Nexaas;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Samurai.Integration.APIClient.Nexaas;
using Samurai.Integration.APIClient.Nexaas.Models.Requests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Samurai.Integration.APIClient.Nexaas.Models.Results;
using System.Net.Http;

namespace Samurai.Integration.Application.Actors.Nexaas
{
    public class NexaasApiActor : BaseNexaasTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly NexaasApiClient _client;

        public NexaasApiActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, NexaasData nexaasData)
            : base("NexaasApiActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _nexaasData = nexaasData;

            using (var scope = _serviceProvider.CreateScope())
            {
                var _configuration = scope.ServiceProvider.GetService<IConfiguration>();
                var httpClientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();
                _client = new NexaasApiClient(httpClientFactory, _nexaasData.Url, _nexaasData.Token, _configuration.GetSection("Nexaas")["Version"], _log);
            }

            ReceiveAsync((Func<NexaasApiListAllProductsRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting NexaasApiListAllProductsRequest");
                    string url = $"products";
                    var param = new Dictionary<string, string>() { };
                    param.Add("page", message.Page.ToString());
                    var response = await _client.Get<NexaasApiListAllProductsResult>(QueryHelpers.AddQueryString(url, param), _cancellationToken);
                    LogDebug("Ending NexaasApiListAllProductsRequest");
                    Sender.Tell(new ReturnMessage<NexaasApiListAllProductsResult> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in NexaasApiListAllProductsRequest");
                    Sender.Tell(new ReturnMessage<NexaasApiListAllProductsResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<NexaasApiListProductRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting NexaasApiListProductRequest");
                    string url = $"products/{message.Id}";
                    var response = await _client.Get<NexaasApiListProductResult>(url, _cancellationToken);
                    LogDebug("Ending NexaasApiListProductRequest");
                    Sender.Tell(new ReturnMessage<NexaasApiListProductResult> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in NexaasApiListProductRequest");
                    Sender.Tell(new ReturnMessage<NexaasApiListProductResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<NexaasApiListSkuRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting NexaasApiListSkuRequest");
                    string url = $"products/product_skus/{message.Id}";
                    var response = await _client.Get<NexaasApiListSkuResult>(url, _cancellationToken);
                    LogDebug("Ending NexaasApiListSkuRequest");
                    Sender.Tell(new ReturnMessage<NexaasApiListSkuResult> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in NexaasApiListSkuRequest");
                    Sender.Tell(new ReturnMessage<NexaasApiListSkuResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<NexaasApiListProductStocksRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting NexaasApiListProductStocksRequest");
                    string url = $"stock_skus/search";
                    var param = new Dictionary<string, string>() { };
                    param.Add("page", message.Page.ToString());
                    var response = await _client.Post<NexaasApiListProductStocksResult>(QueryHelpers.AddQueryString(url, param), message, _cancellationToken);
                    LogDebug("Ending NexaasApiListProductStocksRequest");
                    Sender.Tell(new ReturnMessage<NexaasApiListProductStocksResult> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in NexaasApiListProductStocksRequest");
                    Sender.Tell(new ReturnMessage<NexaasApiListProductStocksResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<NexaasApiListVendorRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting NexaasApiListVendorRequest");
                    string url = $"product_brands/{message.Id}";
                    var response = await _client.Get<NexaasApiListVendorResult>(url, _cancellationToken);
                    LogDebug("Ending NexaasApiListVendorRequest");
                    Sender.Tell(new ReturnMessage<NexaasApiListVendorResult> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in NexaasApiListVendorRequest");
                    Sender.Tell(new ReturnMessage<NexaasApiListVendorResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<NexaasApiListCategoryRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting NexaasApiListCategoryRequest");
                    string url = $"product_categories/{message.Id}";
                    var response = await _client.Get<NexaasApiListCategoryResult>(url, _cancellationToken);
                    LogDebug("Ending NexaasApiListCategoryRequest");
                    Sender.Tell(new ReturnMessage<NexaasApiListCategoryResult> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in NexaasApiListCategoryRequest");
                    Sender.Tell(new ReturnMessage<NexaasApiListCategoryResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<NexaasApiListStockSkuRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting NexaasApiListStockSkuRequest");
                    string url = $"stock_skus/{message.StockSkuId}";
                    var response = await _client.Get<NexaasApiListStockSkuResult>(url, _cancellationToken);
                    LogDebug("Ending NexaasApiListStockSkuRequest");
                    Sender.Tell(new ReturnMessage<NexaasApiListStockSkuResult> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in NexaasApiListStockSkuRequest");
                    Sender.Tell(new ReturnMessage<NexaasApiListStockSkuResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<NexaasApiListProductPricesRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting NexaasApiListProductPricesRequest");
                    string url = $"products/product_skus/{message.ProductSkuId}/sku_price?sale_channel_id={nexaasData.SaleChannelId}";

                    var response = await _client.Get<NexaasApiListProductPricesResult>(url, _cancellationToken);
                    Sender.Tell(new ReturnMessage<NexaasApiListProductPricesResult> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Preço Sku não encontrado(a)")) //quando não tem preço a api retorna 404
                    {
                        Sender.Tell(new ReturnMessage<NexaasApiListProductPricesResult> { Result = Result.OK, Data = null });
                    }
                    else
                    {
                        LogError(ex, "Error in NexaasApiListProductPricesRequest");
                        Sender.Tell(new ReturnMessage<NexaasApiListProductPricesResult> { Result = Result.Error, Error = ex });
                    }
                }
            }));

            ReceiveAsync((Func<NexaasApiCreateOrderRequest, Task>)(async message => {
                try
                {
                    LogDebug("Starting NexaasApiCreateOrderRequest");
                    string url = $"orders";
                    await _client.Post(url, message, _cancellationToken);
                    LogDebug("Ending NexaasApiCreateOrderRequest");
                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    LogError(ex, $"Error in NexaasApiCreateOrderRequest");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<NexaasApiUpdateOrderRequest, Task>)(async message => {
                try
                {
                    LogDebug("Starting NexaasApiUpdateOrderRequest");
                    string url = $"orders/{message.order.id}";
                    await _client.Put(url, message, _cancellationToken);
                    LogDebug("Ending NexaasApiUpdateOrderRequest");
                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    LogError(ex, $"Error in NexaasApiCreateOrderRequest");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<NexaasApiListOrderRequest, Task>)(async message => {
                try
                {
                    LogDebug("Starting NexaasApiListOrderRequest");
                    string url = $"orders/{message.Id}";
                    var response = await _client.Get<NexaasApiListOrderResult>(url, _cancellationToken);
                    LogDebug("Ending NexaasApiListOrderRequest");
                    Sender.Tell(new ReturnMessage<NexaasApiListOrderResult> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, $"Error in NexaasApiListOrderRequest");
                    Sender.Tell(new ReturnMessage<NexaasApiListOrderResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<NexaasApiSearchOrdersRequest, Task>)(async message => {
                try
                {
                    LogDebug("Starting NexaasApiSearchOrdersRequest");
                    string url = $"orders?search[code]={message.ExternalId}&search[organization_id]={nexaasData.OrganizationId}";
                    var response = await _client.Get<NexaasApiSearchOrdersResult>(url, _cancellationToken);
                    Sender.Tell(new ReturnMessage<NexaasApiSearchOrdersResult> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, $"Error in NexaasApiSearchOrdersRequest");
                    Sender.Tell(new ReturnMessage<NexaasApiSearchOrdersResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<NexaasApiCancelOrderRequest, Task>)(async message => {
                try
                {
                    LogDebug("Starting NexaasApiCancelOrderRequest");
                    string url = $"orders/{message.Id}/cancel";
                    await _client.Post(url, message, _cancellationToken);
                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    LogError(ex, $"Error in NexaasApiCancelOrderRequest");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<NexaasApiPickupPointSearchRequest, Task>)(async message => {
                try
                {
                    LogDebug("Starting NexaasApiPickupPointSearchRequest");
                    string url = $"pickup_points/search";
                    var response = await _client.Post<NexaasApiPickupPointSearchResult>(url, message, _cancellationToken);
                    Sender.Tell(new ReturnMessage<NexaasApiPickupPointSearchResult> { Result = Result.OK, Data = response });
                }
                catch (Exception ex)
                {
                    LogError(ex, $"Error in NexaasApiPickupPointSearchRequest");
                    Sender.Tell(new ReturnMessage<NexaasApiSearchOrdersResult> { Result = Result.Error, Error = ex });
                }
            }));
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, NexaasData nexaasData)
        {
            return Akka.Actor.Props.Create(() => new NexaasApiActor(serviceProvider, cancellationToken, nexaasData));
        }
    }
}
