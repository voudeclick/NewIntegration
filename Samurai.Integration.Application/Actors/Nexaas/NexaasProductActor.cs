using Akka.Actor;
using Akka.Dispatch;
using Akka.Event;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.APIClient.Nexaas.Models.Requests;
using Samurai.Integration.APIClient.Nexaas.Models.Results;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Nexaas;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.Nexaas
{
    public class NexaasProductActor : BaseNexaasTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly IActorRef _apiActorGroup;
        private readonly QueueClient _nexaasFullProductQueueClient;
        private readonly QueueClient _shopifyFullProductQueueClient;
        private readonly QueueClient _shopifyPartialProductQueueClient;
        private readonly QueueClient _shopifyPartialSkuQueueClient;
        private readonly QueueClient _shopifyPriceQueueClient;
        private readonly QueueClient _shopifyStockQueueClient;
        private readonly QueueClient _shopifyUpdateVendorQueueClient;

        public NexaasProductActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, NexaasData nexaasData, IActorRef apiActorGroup)
            : base("NexaasProductActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _nexaasData = nexaasData;
            _apiActorGroup = apiActorGroup;


            using (var scope = _serviceProvider.CreateScope())
            {
                var tenantService = scope.ServiceProvider.GetService<TenantService>();
                _nexaasFullProductQueueClient = tenantService.GetQueueClient(_nexaasData, NexaasQueue.ListFullProductQueue);
                _shopifyFullProductQueueClient = tenantService.GetQueueClient(_nexaasData, ShopifyQueue.UpdateFullProductQueue);
                _shopifyPartialProductQueueClient = tenantService.GetQueueClient(_nexaasData, ShopifyQueue.UpdatePartialProductQueue);
                _shopifyPartialSkuQueueClient = tenantService.GetQueueClient(_nexaasData, ShopifyQueue.UpdatePartialSkuQueue);
                 _shopifyPriceQueueClient = tenantService.GetQueueClient(_nexaasData, ShopifyQueue.UpdatePriceQueue);
                _shopifyStockQueueClient = tenantService.GetQueueClient(_nexaasData, ShopifyQueue.UpdateStockQueue);
                _shopifyUpdateVendorQueueClient = tenantService.GetQueueClient(_nexaasData, ShopifyQueue.UpdateVendorQueue);
            }

            ReceiveAsync((Func<NexaasApiListAllProductsRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting NexaasApiListAllProductsRequest");
                    var response = await _apiActorGroup.Ask<ReturnMessage<NexaasApiListAllProductsResult>>(message, _cancellationToken);
                    LogDebug("Ending NexaasApiListAllProductsRequest");
                    Sender.Tell(response);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in NexaasApiListAllProductsRequest");
                    Sender.Tell(new ReturnMessage<NexaasApiListAllProductsResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<NexaasEnqueueFullProductsMessage, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting NexaasEnqueueFullProductsMessage");
                    await Task.WhenAll(message.ProductsIds.Select(p => 
                                                                _nexaasFullProductQueueClient.SendAsync(
                                                                    new ServiceBusMessage(
                                                                        new ShopifyListERPFullProductMessage {
                                                                            ExternalId = p.ToString() 
                                                                        })
                                                                    .GetMessage(p))));
                    LogDebug("Ending NexaasEnqueueFullProductsMessage");
                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in NexaasEnqueueFullProductsMessage");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<NexaasApiListProductRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug($"Starting NexaasApiListProductRequest id: {message.Id}");
                    var response = await _apiActorGroup.Ask<ReturnMessage<NexaasApiListProductResult>>(message, _cancellationToken);
                    LogDebug("Ending NexaasApiListProductRequest");
                    Sender.Tell(response);
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
                    LogDebug($"Starting NexaasApiListSkuRequest id: {message.Id}");
                    var response = await _apiActorGroup.Ask<ReturnMessage<NexaasApiListSkuResult>>(message, _cancellationToken);
                    LogDebug("Ending NexaasApiListSkuRequest");
                    Sender.Tell(response);
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
                    var response = await _apiActorGroup.Ask<ReturnMessage<NexaasApiListProductStocksResult>>(message, _cancellationToken);
                    LogDebug("Ending NexaasApiListProductStocksRequest");
                    Sender.Tell(response);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in NexaasApiListProductStocksRequest");
                    Sender.Tell(new ReturnMessage<NexaasApiListProductStocksResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<NexaasApiListProductPricesRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug("Starting NexaasApiListProductPricesRequest");
                    var response = await _apiActorGroup.Ask<ReturnMessage<NexaasApiListProductPricesResult>>(message, _cancellationToken);
                    LogDebug("Ending NexaasApiListProductPricesRequest");
                    Sender.Tell(response);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in NexaasApiListProductPricesRequest");
                    Sender.Tell(new ReturnMessage<NexaasApiListProductPricesResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<NexaasApiListVendorRequest, Task>)(async message =>
            {
                try
                {
                    LogDebug($"Starting NexaasApiListVendorRequest id: {message.Id}");
                    var response = await _apiActorGroup.Ask<ReturnMessage<NexaasApiListVendorResult>>(message, _cancellationToken);
                    LogDebug("Ending NexaasApiListVendorRequest");
                    Sender.Tell(response);
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
                    LogDebug($"Starting NexaasApiListCategoryRequest id: {message.Id}");
                    var response = await _apiActorGroup.Ask<ReturnMessage<NexaasApiListCategoryResult>>(message, _cancellationToken);
                    LogDebug("Ending NexaasApiListCategoryRequest");
                    Sender.Tell(response);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in NexaasApiListCategoryRequest");
                    Sender.Tell(new ReturnMessage<NexaasApiListCategoryResult> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<NexaasSendFullProductShopifyMessage, Task>)(async message =>
            {
                try
                {
                    LogDebug($"Starting NexaasSendFullProductShopifyMessage id: {message.Product.id}");
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var nexaasService = scope.ServiceProvider.GetService<NexaasService>();
                        nexaasService.Init(_apiActorGroup, GetLog());
                        result = await nexaasService.SendFullProductShopifyMessage(message, _nexaasData,_shopifyFullProductQueueClient, _cancellationToken);
                    }
                    LogDebug("Ending NexaasSendFullProductShopifyMessage");
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, $"Error in NexaasSendFullProductShopifyMessage id: {message.Product.id}");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<NexaasListPartialProductMessage, Task>)(async message =>
            {
                try
                {
                    LogDebug($"Starting NexaasListPartialProductMessage id: {message.ProductSkuId}");
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var nexaasService = scope.ServiceProvider.GetService<NexaasService>();
                        nexaasService.Init(_apiActorGroup, GetLog());
                        result = await nexaasService.ListPartialProduct(message, _nexaasData, _nexaasFullProductQueueClient, _shopifyPartialProductQueueClient, _shopifyPartialSkuQueueClient, _cancellationToken);
                    }
                    LogDebug("Ending NexaasListPartialProductMessage");
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in NexaasListPartialProductMessage");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));
			
            ReceiveAsync((Func<NexaasListVendorMessage, Task>)(async message =>
            {
                try
                {
                    LogDebug($"Starting NexaasListVendorMessage id: {message.Id}");
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var nexaasService = scope.ServiceProvider.GetService<NexaasService>();
                        nexaasService.Init(_apiActorGroup, GetLog());
                        result = await nexaasService.ListVendor(message, _nexaasData, _shopifyUpdateVendorQueueClient, _cancellationToken);
                    }
                    LogDebug("Ending NexaasListVendorMessage");
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, $"Error in NexaasListVendorMessage id: {message.Id}");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<NexaasListStockSkuMessage, Task>)(async message =>
            {
                try
                {
                    LogDebug($"Starting NexaasListStockSkuMessage id: {message.StockSkuId}");
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var nexaasService = scope.ServiceProvider.GetService<NexaasService>();
                        nexaasService.Init(_apiActorGroup, GetLog());
                        result = await nexaasService.ListStockSku(message.StockSkuId, _nexaasData, _shopifyStockQueueClient, _cancellationToken);
                    }
                    LogDebug("Ending NexaasListStockSkuMessage");
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in NexaasListStockSkuMessage");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ShopifyListERPProductCategoriesMessage, Task>)(async message =>
            {
                try
                {
                    LogDebug($"Starting ShopifyListERPProductCategoriesMessage id: {message.ExternalId}");
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var nexaasService = scope.ServiceProvider.GetService<NexaasService>();
                        nexaasService.Init(_apiActorGroup, GetLog());
                        result = await nexaasService.ListProductCategories(message.ExternalId, _nexaasData, _shopifyPartialProductQueueClient, _cancellationToken);
                    }
                    LogDebug("Ending ShopifyListERPProductCategoriesMessage");
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in ShopifyListERPProductCategoriesMessage");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));
        }

        protected override void PostStop()
        {
            base.PostStop();
            ActorTaskScheduler.RunTask(async () =>
            {
                if (_nexaasFullProductQueueClient != null && !_nexaasFullProductQueueClient.IsClosedOrClosing)
                    await _nexaasFullProductQueueClient.CloseAsync();
                if (_shopifyFullProductQueueClient != null && !_shopifyFullProductQueueClient.IsClosedOrClosing)
                    await _shopifyFullProductQueueClient.CloseAsync();
                if (_shopifyPartialProductQueueClient != null && !_shopifyPartialProductQueueClient.IsClosedOrClosing)
                    await _shopifyPartialProductQueueClient.CloseAsync();
                if (_shopifyPartialSkuQueueClient != null && !_shopifyPartialSkuQueueClient.IsClosedOrClosing)
                    await _shopifyPartialSkuQueueClient.CloseAsync(); 
                if (_shopifyPriceQueueClient != null && !_shopifyPriceQueueClient.IsClosedOrClosing)
                    await _shopifyPriceQueueClient.CloseAsync();
                if (_shopifyStockQueueClient != null && !_shopifyStockQueueClient.IsClosedOrClosing)
                    await _shopifyStockQueueClient.CloseAsync();
            });
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, NexaasData nexaasData, IActorRef apiActorGroup)
        {
            return Akka.Actor.Props.Create(() => new NexaasProductActor(serviceProvider, cancellationToken, nexaasData, apiActorGroup));
        }
    }
}
