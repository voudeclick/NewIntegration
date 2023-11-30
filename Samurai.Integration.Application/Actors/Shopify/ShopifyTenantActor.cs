using Akka.Actor;
using Akka.Dispatch;
using Akka.Routing;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.APIClient.Shopify.Models;
using Samurai.Integration.APIClient.Shopify.Models.Request;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Application.Tools;
using Samurai.Integration.Domain.Consts;
using Samurai.Integration.Domain.Entities.Database.Logs;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Messages.Shopify.OrderActor;
using Samurai.Integration.Domain.Models;
using Samurai.Integration.Domain.Queues;
using Samurai.Integration.Domain.Results.Logger;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Inputs = Samurai.Integration.APIClient.Shopify.Models.Request.Inputs;
using Results = Samurai.Integration.Domain.Shopify.Models.Results;

namespace Samurai.Integration.Application.Actors.Shopify
{
    public class ShopifyTenantActor : BaseShopifyTenantActor
    {
        private readonly int _maximumRetryCount = 3;
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _webJobCancellationToken;
        private CancellationTokenSource _taskCancellationTokenSource;

        #region Actors
        private List<IActorRef> _apiActors;
        private IActorRef _apiActorGroup;
        private IActorRef _productActor;
        private IActorRef _orderActor;
        #endregion

        #region QueueClients
        private QueueClient _updateFullProductQueue;
        private QueueClient _updatePartialProductQueue;
        private QueueClient _updatePartialSkuQueue;
        private QueueClient _updateProductPriceQueue;
        private QueueClient _updateProductStockQueue;
        private QueueClient _listOrderQueue;
        private QueueClient _updateOrderTagNumberQueue;
        private QueueClient _updateOrderStatusQueue;
        private QueueClient _updateVendorQueue;
        private QueueClient _updateProductGroupingQueue;
        private QueueClient _updateProductImagesQueue;
        private QueueClient _listCategoryProductsToUpdateQueue;
        private QueueClient _updateAllCollectionsQueue;
        private QueueClient _updateTrackingOrderQueue;
        private QueueClient _updateProductStockKitQueue;
        private QueueClient _updateProductKitQueue;


        #endregion

        #region Tasks
        private Task _listNewOrdersTask;
        private Task _listLostOrdersTask;
        #endregion

        public ShopifyTenantActor(IServiceProvider serviceProvider, CancellationToken cancellationToken)
            : base("ShopifyTenantActor")
        {
            _serviceProvider = serviceProvider;
            _webJobCancellationToken = cancellationToken;

            ReceiveAsync((Func<InitializeShopifyTenantMessage, Task>)(async message =>
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        Initialize(message.Data, scope);
                    }
                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    LogError(ex, "ShopifyTenantActor - Error in InitializeShopifyTenantMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync<UpdateShopifyTenantMessage>(async message =>
            {
                try
                {
                    if (_shopifyData.EqualsTo(message.Data) == false)
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            await Stop();
                            Initialize(message.Data, scope);
                        }
                    }
                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    LogError(ex, "ShopifyTenantActor - Error in UpdateShopifyTenantMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            });

            ReceiveAsync((Func<StopShopifyTenantMessage, Task>)(async message =>
            {
                try
                {
                    await Stop();
                    Context.Stop(Self);
                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    LogError(ex, "ShopifyTenantActor - Error in StopShopifyTenantMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));
        }

        private async Task Stop()
        {
            if (_shopifyData != null && _taskCancellationTokenSource != null)
            {
                #region Tasks

                _taskCancellationTokenSource.Cancel();

                if (_shopifyData.OrderIntegrationStatus == true)
                {
                    if (_listNewOrdersTask != null)
                        await _listNewOrdersTask;
                    _listNewOrdersTask = null;

                    if (_listLostOrdersTask != null)
                        await _listLostOrdersTask;
                    _listLostOrdersTask = null;

                }

                #endregion

                #region QueueClients

                if (_shopifyData.ProductIntegrationStatus == true)
                {
                    if (_updateFullProductQueue != null && !_updateFullProductQueue.IsClosedOrClosing)
                        await _updateFullProductQueue.CloseAsync();
                    _updateFullProductQueue = null;

                    if (_updatePartialProductQueue != null && !_updatePartialProductQueue.IsClosedOrClosing)
                        await _updatePartialProductQueue.CloseAsync();
                    _updatePartialProductQueue = null;

                    if (_updateProductKitQueue != null && !_updateProductKitQueue.IsClosedOrClosing)
                        await _updateProductKitQueue.CloseAsync();
                    _updateProductKitQueue = null;

                    if (_updatePartialSkuQueue != null && !_updatePartialSkuQueue.IsClosedOrClosing)
                        await _updatePartialSkuQueue.CloseAsync();
                    _updatePartialSkuQueue = null;

                    if (_updateProductPriceQueue != null && !_updateProductPriceQueue.IsClosedOrClosing)
                        await _updateProductPriceQueue.CloseAsync();
                    _updateProductPriceQueue = null;

                    if (_updateProductStockQueue != null && !_updateProductStockQueue.IsClosedOrClosing)
                        await _updateProductStockQueue.CloseAsync();
                    _updateProductStockQueue = null;

                    if (_updateVendorQueue != null && !_updateVendorQueue.IsClosedOrClosing)
                        await _updateVendorQueue.CloseAsync();
                    _updateVendorQueue = null;

                    if (_updateProductGroupingQueue != null && !_updateProductGroupingQueue.IsClosedOrClosing)
                        await _updateProductGroupingQueue.CloseAsync();
                    _updateProductGroupingQueue = null;

                    if (_updateProductImagesQueue != null && !_updateProductImagesQueue.IsClosedOrClosing)
                        await _updateProductImagesQueue.CloseAsync();
                    _updateProductImagesQueue = null;

                    if (_listCategoryProductsToUpdateQueue != null && !_listCategoryProductsToUpdateQueue.IsClosedOrClosing)
                        await _listCategoryProductsToUpdateQueue.CloseAsync();
                    _listCategoryProductsToUpdateQueue = null;

                    if (_updateAllCollectionsQueue != null && !_updateAllCollectionsQueue.IsClosedOrClosing)
                        await _updateAllCollectionsQueue.CloseAsync();
                    _updateAllCollectionsQueue = null;
                }

                if (_shopifyData.OrderIntegrationStatus == true)
                {
                    if (_listOrderQueue != null && !_listOrderQueue.IsClosedOrClosing)
                        await _listOrderQueue.CloseAsync();
                    _listOrderQueue = null;

                    if (_updateOrderTagNumberQueue != null && !_updateOrderTagNumberQueue.IsClosedOrClosing)
                        await _updateOrderTagNumberQueue.CloseAsync();
                    _updateOrderTagNumberQueue = null;

                    if (_updateOrderStatusQueue != null && !_updateOrderStatusQueue.IsClosedOrClosing)
                        await _updateOrderStatusQueue.CloseAsync();
                    _updateOrderStatusQueue = null;
                }

                #endregion

                #region Actors

                if (_shopifyData.ProductIntegrationStatus == true)
                {
                    if (_productActor != null)
                        await _productActor.GracefulStop(TimeSpan.FromSeconds(30));
                    _productActor = null;
                }

                if (_shopifyData.OrderIntegrationStatus == true)
                {
                    if (_orderActor != null)
                        await _orderActor.GracefulStop(TimeSpan.FromSeconds(30));
                    _orderActor = null;
                }

                if (_apiActors != null)
                {
                    foreach (var apiActor in _apiActors)
                    {
                        await apiActor.GracefulStop(TimeSpan.FromSeconds(30));
                    }
                    if (_apiActorGroup != null)
                        await _apiActorGroup.GracefulStop(TimeSpan.FromSeconds(30));
                    _apiActors = null;
                    _apiActorGroup = null;
                }

                #endregion

                _taskCancellationTokenSource.Dispose();
                _taskCancellationTokenSource = null;
            }
        }

        private void Initialize(ShopifyDataMessage data, IServiceScope scope)
        {
            _shopifyData = data;

            var _tenantService = scope.ServiceProvider.GetService<TenantService>();
            var _configuration = scope.ServiceProvider.GetService<IConfiguration>();

            _taskCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_webJobCancellationToken);

            #region Actors
            _apiActors = new List<IActorRef>();
            foreach (var app in _shopifyData.ShopifyApps)
            {
                _apiActors.Add(Context.ActorOf(ShopifyApiActor.Props(_serviceProvider, _webJobCancellationToken, _shopifyData, app)));
            }
            _apiActorGroup = Context.ActorOf(Akka.Actor.Props.Empty.WithRouter(new RoundRobinGroup(_apiActors)));

            if (_shopifyData.ProductIntegrationStatus == true)
            {
                _productActor = Context.ActorOf(new RoundRobinPool(_shopifyData.ShopifyApps.Count)
                                                    .Props(ShopifyProductActor.Props(_serviceProvider, _webJobCancellationToken, _shopifyData, _apiActorGroup)));
            }

            if (_shopifyData.OrderIntegrationStatus == true)
            {
                _orderActor = Context.ActorOf(new RoundRobinPool(_shopifyData.ShopifyApps.Count)
                                                .Props(ShopifyOrderActor.Props(_serviceProvider, _webJobCancellationToken, _shopifyData, _apiActorGroup)));
            }

            #endregion

            #region QueueClients

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandlerAsync)
            {
                MaxConcurrentCalls = _shopifyData.ShopifyApps.Count,
                AutoComplete = false
            };

            if (_shopifyData.ProductIntegrationStatus == true)
            {
                _updateFullProductQueue = _tenantService.GetQueueClient(_shopifyData, ShopifyQueue.UpdateFullProductQueue);
                _updateFullProductQueue.RegisterMessageHandler(ProcessFullProductMessageAsync, messageHandlerOptions);

                _updatePartialProductQueue = _tenantService.GetQueueClient(_shopifyData, ShopifyQueue.UpdatePartialProductQueue);
                _updatePartialProductQueue.RegisterMessageHandler(ProcessPartialProductMessageAsync, messageHandlerOptions);

                _updatePartialSkuQueue = _tenantService.GetQueueClient(_shopifyData, ShopifyQueue.UpdatePartialSkuQueue);
                _updatePartialSkuQueue.RegisterMessageHandler(ProcessPartialSkuMessageAsync, messageHandlerOptions);

                _updateProductPriceQueue = _tenantService.GetQueueClient(_shopifyData, ShopifyQueue.UpdatePriceQueue);
                _updateProductPriceQueue.RegisterMessageHandler(ProcessProductPriceMessageAsync, messageHandlerOptions);

                _updateProductStockQueue = _tenantService.GetQueueClient(_shopifyData, ShopifyQueue.UpdateStockQueue);
                _updateProductStockQueue.RegisterMessageHandler(ProcessProductStockMessageAsync, messageHandlerOptions);

                _updateVendorQueue = _tenantService.GetQueueClient(_shopifyData, ShopifyQueue.UpdateVendorQueue);
                _updateVendorQueue.RegisterMessageHandler(ProcessUpdateVendorMessageAsync, messageHandlerOptions);

                _updateProductGroupingQueue = _tenantService.GetQueueClient(_shopifyData, ShopifyQueue.UpdateProductGroupingQueue);
                _updateProductGroupingQueue.RegisterMessageHandler(ProcessUpdateProductGroupingMessageAsync, messageHandlerOptions);

                if (_shopifyData.ImageIntegrationEnabled)
                {
                    _updateProductImagesQueue = _tenantService.GetQueueClient(_shopifyData, ShopifyQueue.UpdateProductImagesQueue);
                    _updateProductImagesQueue.RegisterMessageHandler(ProcessUpdateProductImagesMessageAsync,
                        new MessageHandlerOptions(ExceptionReceivedHandlerAsync)
                        {
                            MaxAutoRenewDuration = TimeSpan.FromMinutes(20),
                            MaxConcurrentCalls = _shopifyData.ShopifyApps.Count,
                            AutoComplete = false
                        }
                     );
                }


                _listCategoryProductsToUpdateQueue = _tenantService.GetQueueClient(_shopifyData, ShopifyQueue.ListCategoryProductsToUpdateQueue);
                _listCategoryProductsToUpdateQueue.RegisterMessageHandler(ProcessListCategoryProductsToUpdateMessageAsync, messageHandlerOptions);

                _updateProductStockKitQueue = _tenantService.GetQueueClient(_shopifyData, ShopifyQueue.UpdateStockKitQueue);
                _updateProductStockKitQueue.RegisterMessageHandler(ProcessProductStockKitMessageAsync, messageHandlerOptions);

                _updateProductKitQueue = _tenantService.GetQueueClient(_shopifyData, ShopifyQueue.UpdateProductKit);
                _updateProductKitQueue.RegisterMessageHandler(ProcessFullProductKitMessageAsync, messageHandlerOptions);

                _updateAllCollectionsQueue = _tenantService.GetQueueClient(_shopifyData, ShopifyQueue.UpdateAllCollectionsQueue);
                _updateAllCollectionsQueue.RegisterMessageHandler(ProcessUpdateAllCollectionsMessageAsync,
                    new MessageHandlerOptions(ExceptionReceivedHandlerAsync)
                    {
                        MaxConcurrentCalls = 1,
                        AutoComplete = false,
                        MaxAutoRenewDuration = TimeSpan.FromMinutes(30)
                    }
                );
            }

            if (_shopifyData.OrderIntegrationStatus == true)
            {
                _listOrderQueue = _tenantService.GetQueueClient(_shopifyData, ShopifyQueue.ListOrderQueue);
                _listOrderQueue.RegisterMessageHandler(ProcessListOrderMessageAsync, messageHandlerOptions);

                _updateOrderTagNumberQueue = _tenantService.GetQueueClient(_shopifyData, ShopifyQueue.UpdateOrderNumberTagQueue);
                _updateOrderTagNumberQueue.RegisterMessageHandler(ProcessUpdateOrderTagNumberMessageAsync, messageHandlerOptions);

                _updateOrderStatusQueue = _tenantService.GetQueueClient(_shopifyData, ShopifyQueue.UpdateOrderStatusQueue);
                _updateOrderStatusQueue.RegisterMessageHandler(ProcessUpdateOrderStatusMessageAsync, messageHandlerOptions);

                _updateTrackingOrderQueue = _tenantService.GetQueueClient(_shopifyData, ShopifyQueue.UpdateTrackingOrderQueue);
                _updateTrackingOrderQueue.RegisterMessageHandler(UpdateTrackingOrderMessageAsync, messageHandlerOptions);
            }

            #endregion

            #region Tasks


            if (_shopifyData.OrderIntegrationStatus == true)
            {
                _listNewOrdersTask = ProcessListNewOrdersTaskAsync(int.Parse(_configuration.GetSection("Schedulle")["NewOrderSleep"]));
                _listLostOrdersTask = ProcessListLostOrdersTaskAsync(int.Parse(_configuration.GetSection("Schedulle")["NewOrderSleep"]));
            }

            #endregion
        }

        private async Task ProcessFullProductKitMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<ShopifyUpdateProductKitMessage>();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                    await _updateProductKitQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _updateFullProductQueue, "ProcessFullProductKitMessageAsync", "product", messageValue, result);
            }
        }

        private async Task ProcessFullProductMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                    await _updateFullProductQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _updateFullProductQueue, "ProcessFullProductMessageAsync", "product", messageValue, result);
            }
        }

        private async Task ProcessPartialProductMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                    await _updatePartialProductQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _updatePartialProductQueue, "ProcessPartialProductMessageAsync", "product", messageValue, result, true);
            }
        }

        private async Task ProcessPartialSkuMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                    await _updatePartialSkuQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _updatePartialSkuQueue, "ProcessPartialSkuMessageAsync", "product", messageValue, result);
            }
        }

        private async Task ProcessProductPriceMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                    await _updateProductPriceQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _updateProductPriceQueue, "ProcessProductPriceMessageAsync", "product", messageValue, result, true);
            }
        }

        private async Task ProcessProductStockMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                    await _updateProductStockQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _updateProductStockQueue, "ProcessProductStockMessageAsync", "product", messageValue, result, true);
            }
        }

        private async Task ProcessProductStockKitMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<ShopifyUpdateStockKitMessage>();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                    await _updateProductStockKitQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _updateProductStockKitQueue, "ProcessProductStockKitMessageAsync", "product", messageValue, result);
            }
        }
        private async Task ProcessListOrderMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue();

                var result = await _orderActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                    await _listOrderQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _listOrderQueue, "ProcessListOrderMessageAsync", "order", messageValue, result, true);
            }
        }

        private async Task ProcessUpdateOrderTagNumberMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue();

                var result = await _orderActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                    await _updateOrderTagNumberQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _updateOrderTagNumberQueue, "ProcessUpdateOrderTagNumberMessageAsync", "order", messageValue, result, true);
            }
        }

        private async Task ProcessUpdateOrderStatusMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue();

                var result = await _orderActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                    await _updateOrderStatusQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _updateOrderStatusQueue, "ProcessUpdateOrderStatusMessageAsync", "order", messageValue, result, true);
            }
        }

        private async Task UpdateTrackingOrderMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<ShopifyUpdateTrackingOrder>();

                var result = await _orderActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                    await _updateTrackingOrderQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _updateTrackingOrderQueue, "UpdateTrackingOrderMessageAsync", "order", messageValue, result);
            }
        }
        private async Task ProcessUpdateVendorMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<ShopifyUpdateVendorMessage>();

                var result = await UpdateVendor(messageValue);

                if (result.Result == Result.OK)
                    await _updateVendorQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _updateVendorQueue, "ProcessUpdateVendorMessageAsync", "product", messageValue, result);
            }
        }

        public async Task<ReturnMessage> UpdateVendor(ShopifyUpdateVendorMessage message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _shopifyService = scope.ServiceProvider.GetService<ShopifyService>();
                var queryByTagResult = await _productActor.Ask<ReturnMessage<ProductIdsByTagQueryOutput>>(new ProductIdsByTagQuery(_shopifyService.SetTagValue(Tags.ProductVendorId, message.Id.ToString())), _webJobCancellationToken);

                if (queryByTagResult.Result == Result.Error)
                    return new ReturnMessage { Result = Result.Error, Error = queryByTagResult.Error };

                var data = queryByTagResult.Data;
                while (queryByTagResult.Data.products.pageInfo.hasNextPage == true)
                {
                    queryByTagResult = await _apiActorGroup.Ask<ReturnMessage<ProductIdsByTagQueryOutput>>(
                        new ProductIdsByTagQuery(_shopifyService.SetTagValue(Tags.ProductVendorId, message.Id.ToString()), queryByTagResult.Data.products.edges.Last().cursor), _webJobCancellationToken
                    );

                    if (queryByTagResult.Result == Result.Error)
                        return new ReturnMessage { Result = Result.Error, Error = queryByTagResult.Error };

                    data.products.edges.AddRange(queryByTagResult.Data.products.edges);
                }

                List<Product.Info> partialProducts = new List<Product.Info>();
                foreach (var product in data.products.edges)
                {
                    partialProducts.Add(
                        new Product.Info
                        {
                            ShopifyId = long.Parse(product.node.legacyResourceId),
                            Vendor = message.Name,
                            VendorId = message.Id
                        });
                }

                foreach (var infos in partialProducts.Chunk(10))
                {
                    var enqueueResult = await _productActor.Ask<ReturnMessage>(
                        new ShopifyEnqueueUpdatePartialProductMessage { ProductInfos = infos.ToList() },
                        _webJobCancellationToken);
                    if (enqueueResult.Result == Result.Error)
                        return new ReturnMessage { Result = Result.Error, Error = enqueueResult.Error };
                }

                return new ReturnMessage { Result = Result.OK };
            }
        }

        private async Task ProcessUpdateProductGroupingMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<ShopifyUpdateProductGroupingMessage>();

                var result = await UpdateProductGrouping(messageValue);

                if (result.Result == Result.OK)
                    await _updateProductGroupingQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _updateProductGroupingQueue, "ProcessUpdateProductGroupingMessageAsync", "product", messageValue, result);
            }
        }

        private async Task<ReturnMessage> UpdateProductGrouping(ShopifyUpdateProductGroupingMessage messageValue)
        {
            if (_shopifyData.ProductGroupingEnabled)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var _shopifyService = scope.ServiceProvider.GetService<ShopifyService>();

                    List<Results.ProductResult> productList;
                    var queryByTagResult = await _productActor.Ask<ReturnMessage<ProductMetafieldsByTagQueryOutput>>(new ProductMetafieldsByTagQuery(_shopifyService.SetTagValue(Tags.ProductGroupingReference, messageValue.GroupingReference)), _webJobCancellationToken);

                    if (queryByTagResult.Result == Result.Error)
                        return new ReturnMessage { Result = Result.Error, Error = queryByTagResult.Error };

                    productList = queryByTagResult.Data.products.edges.Select(x => x.node).ToList();
                    while (queryByTagResult.Data.products.pageInfo.hasNextPage == true)
                    {
                        queryByTagResult = await _productActor.Ask<ReturnMessage<ProductMetafieldsByTagQueryOutput>>(
                            new ProductMetafieldsByTagQuery(_shopifyService.SetTagValue(Tags.ProductGroupingReference, messageValue.GroupingReference), queryByTagResult.Data.products.edges.Last().cursor), _webJobCancellationToken
                        );

                        if (queryByTagResult.Result == Result.Error)
                            return new ReturnMessage { Result = Result.Error, Error = queryByTagResult.Error };

                        productList.AddRange(queryByTagResult.Data.products.edges.Select(x => x.node));
                    }

                    foreach (var product in productList)
                    {
                        var metafields = GetProductGroupingMetafields(_shopifyData, productList, product);
                        if (metafields != null)
                        {
                            var updateMetafieldResult = await _productActor.Ask<ReturnMessage<ProductUpdateMutationOutput>>(
                                new ProductUpdateMutation(new ProductUpdateMutationInput
                                {
                                    input = new Inputs.Product
                                    {
                                        id = product.id,
                                        metafields = metafields
                                    }
                                }), _webJobCancellationToken);

                            if (updateMetafieldResult.Result == Result.Error)
                                return new ReturnMessage { Result = Result.Error, Error = updateMetafieldResult.Error };

                            if (updateMetafieldResult.Data.productUpdate.userErrors?.Any() == true)
                                throw new Exception($"Error in update shopify product metafield: {JsonSerializer.Serialize(updateMetafieldResult.Data.productUpdate.userErrors)}");
                        }
                    }
                }
            }

            return new ReturnMessage { Result = Result.OK };
        }

        private async Task ProcessUpdateProductImagesMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<ShopifyUpdateProductImagesMessage>();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                    await _updateProductImagesQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _updateProductImagesQueue, "ProcessUpdateProductImagesMessageAsync", "product", messageValue, result);
            }
        }

        private List<Inputs.Metafield> GetProductGroupingMetafields(ShopifyDataMessage shopifyData, List<Results.ProductResult> productGrouping, Results.ProductResult currentData = null)
        {
            List<Inputs.Metafield> result = new List<Inputs.Metafield>();
            if (shopifyData.ProductGroupingEnabled && productGrouping != null)
            {
                Inputs.Metafield productGroupingMetafield = null;
                var currentGroupingMetafield = currentData?.metafields.edges.Select(x => x.node).FirstOrDefault(x => x.key == "ProductGroupingHandles");
                if (currentGroupingMetafield == null)
                    productGroupingMetafield = new Inputs.Metafield { key = "ProductGroupingHandles", valueType = "STRING" };
                else
                    productGroupingMetafield = new Inputs.Metafield { id = currentGroupingMetafield.id, key = "ProductGroupingHandles", value = currentGroupingMetafield.value, valueType = "STRING" };

                var handles = string.Join("|", productGrouping.Where(x => currentData == null || x.id != currentData.id).Select(x => x.handle).OrderBy(x => x));
                if (string.IsNullOrWhiteSpace(handles))
                    handles = "|"; //metafields não podem ser nulos, mas preciso de uma maneira simples de limpar ele
                if (productGroupingMetafield.id == null || productGroupingMetafield.value != handles)
                {
                    productGroupingMetafield.value = handles;
                    result.Add(productGroupingMetafield);
                }
            }

            if (result.Any())
                return result;
            else
                return null;
        }

        private async Task ProcessListCategoryProductsToUpdateMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<ShopifyListCategoryProductsToUpdateMessage>();

                var result = await ListCategoryProductsToUpdate(messageValue);

                if (result.Result == Result.OK)
                    await _listCategoryProductsToUpdateQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _listCategoryProductsToUpdateQueue, "ProcessListCategoryProductsToUpdateMessageAsync", "product", messageValue, result);
            }
        }

        public async Task<ReturnMessage> ListCategoryProductsToUpdate(ShopifyListCategoryProductsToUpdateMessage message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _shopifyService = scope.ServiceProvider.GetService<ShopifyService>();
                var queryByTagResult = await _productActor.Ask<ReturnMessage<ProductIdsByTagQueryOutput>>(new ProductIdsByTagQuery(_shopifyService.SetTagValue(Tags.ProductCollectionId, message.CategoryId.ToString())), _webJobCancellationToken);

                if (queryByTagResult.Result == Result.Error)
                    return new ReturnMessage { Result = Result.Error, Error = queryByTagResult.Error };

                var data = queryByTagResult.Data;
                while (queryByTagResult.Data.products.pageInfo.hasNextPage == true)
                {
                    queryByTagResult = await _apiActorGroup.Ask<ReturnMessage<ProductIdsByTagQueryOutput>>(
                        new ProductIdsByTagQuery(_shopifyService.SetTagValue(Tags.ProductCollectionId, message.CategoryId.ToString()), queryByTagResult.Data.products.edges.Last().cursor), _webJobCancellationToken
                    );

                    if (queryByTagResult.Result == Result.Error)
                        return new ReturnMessage { Result = Result.Error, Error = queryByTagResult.Error };

                    data.products.edges.AddRange(queryByTagResult.Data.products.edges);
                }

                foreach (var ids in data.products.edges.Select(p => _shopifyService.SearchTagValue(p.node.tags, Tags.OrderExternalId).First()).Chunk(10))
                {
                    var enqueueResult = await _productActor.Ask<ReturnMessage>(
                        new ShopifyEnqueueListERPProductCategoriesMessage { ExternalIds = ids.ToList() },
                        _webJobCancellationToken);
                    if (enqueueResult.Result == Result.Error)
                        return new ReturnMessage { Result = Result.Error, Error = enqueueResult.Error };
                }

                return new ReturnMessage { Result = Result.OK };
            }
        }

        private async Task ProcessUpdateAllCollectionsMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<ShopifyUpdateAllCollectionsMessage>();

                var result = await UpdateAllCollections(messageValue);

                if (result.Result == Result.OK)
                    await _updateAllCollectionsQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _updateAllCollectionsQueue, "ProcessUpdateAllCollectionsMessageAsync", "product", messageValue, result, true);
            }
        }

        public async Task<ReturnMessage> UpdateAllCollections(ShopifyUpdateAllCollectionsMessage message)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var _shopifyService = scope.ServiceProvider.GetService<ShopifyService>();
                    HashSet<string> AllCategories = new HashSet<string>();

                    var queryAllProductTagsResult = await _productActor.Ask<ReturnMessage<AllProductsTagsQueryOutput>>(new AllProductsTagsQuery(), _webJobCancellationToken);

                    if (queryAllProductTagsResult.Result == Result.Error)
                        return new ReturnMessage { Result = Result.Error, Error = queryAllProductTagsResult.Error };

                    queryAllProductTagsResult.Data.products.edges.SelectMany(e => e.node.tags)
                        .Where(x => _shopifyService.IsTag(x, Tags.ProductCollection))
                        .ToList()
                        .ForEach(x =>
                        {
                            AllCategories.Add(x);
                        });

                    while (queryAllProductTagsResult.Data.products.pageInfo.hasNextPage == true)
                    {
                        queryAllProductTagsResult = await _apiActorGroup.Ask<ReturnMessage<AllProductsTagsQueryOutput>>(
                            new AllProductsTagsQuery(queryAllProductTagsResult.Data.products.edges.Last().cursor), _webJobCancellationToken
                        );

                        if (queryAllProductTagsResult.Result == Result.Error)
                            return new ReturnMessage { Result = Result.Error, Error = queryAllProductTagsResult.Error };

                        queryAllProductTagsResult.Data.products.edges.SelectMany(e => e.node.tags)
                            .Where(x => _shopifyService.IsTag(x, Tags.ProductCollection))
                            .ToList()
                            .ForEach(x =>
                            {
                                AllCategories.Add(x);
                            });
                    }

                    var allCollectionsResult = await _productActor.Ask<ReturnMessage<AllCollectionsQueryOutput>>(new AllCollectionsQuery(), _webJobCancellationToken);

                    if (allCollectionsResult.Result == Result.Error)
                        return new ReturnMessage { Result = Result.Error, Error = allCollectionsResult.Error };

                    var allCollections = allCollectionsResult.Data;
                    while (allCollectionsResult.Data.collections.pageInfo.hasNextPage == true)
                    {
                        allCollectionsResult = await _apiActorGroup.Ask<ReturnMessage<AllCollectionsQueryOutput>>(
                            new AllCollectionsQuery(allCollectionsResult.Data.collections.edges.Last().cursor), _webJobCancellationToken
                        );

                        if (allCollectionsResult.Result == Result.Error)
                            return new ReturnMessage { Result = Result.Error, Error = allCollectionsResult.Error };

                        allCollections.collections.edges.AddRange(allCollectionsResult.Data.collections.edges);
                    }

                    foreach (var category in AllCategories)
                    {
                        var existingCollection = allCollections.collections.edges
                                                    .Where(x => x.node.ruleSet?.rules?.Count == 1 &&
                                                                x.node.ruleSet.rules[0].column == "TAG" &&
                                                                x.node.ruleSet.rules[0].relation == "EQUALS" &&
                                                                x.node.ruleSet.rules[0].condition == category).FirstOrDefault();
                        if (existingCollection == null)
                        {
                            var createCollectionResult = await _apiActorGroup.Ask<ReturnMessage<CollectionCreateMutationOutput>>(
                            new CollectionCreateMutation(new CollectionCreateMutationInput
                            {
                                input = new Inputs.Collection
                                {
                                    title = _shopifyService.GetTagValue(category, Tags.ProductCollection).Split(new[] { ">>" }, StringSplitOptions.None).Last(),
                                    ruleSet = new Inputs.RuleSet
                                    {
                                        appliedDisjunctively = false,
                                        rules = new List<Inputs.Rule>
                                        {
                                        new Inputs.Rule
                                        {
                                            column = "TAG",
                                            relation = "EQUALS",
                                            condition = category
                                        }
                                        }
                                    }
                                }
                            }), _webJobCancellationToken);

                            if (createCollectionResult.Result == Result.Error)
                                return new ReturnMessage { Result = Result.Error, Error = createCollectionResult.Error };

                            if (createCollectionResult.Data.collectionCreate.userErrors?.Any() == true)
                                throw new Exception($"Error in create shopify collection: {JsonSerializer.Serialize(createCollectionResult.Data.collectionCreate.userErrors)}");
                        }
                    }

                    return new ReturnMessage { Result = Result.OK };
                }
            }
            catch (Exception ex)
            {
                return new ReturnMessage { Result = Result.Error, Error = ex };
            }
        }

        private async Task ExceptionReceivedHandlerAsync(ExceptionReceivedEventArgs arg)
        {
            LogError(arg.Exception, "Error in Shopify Queue");
        }

        private async Task ProcessListNewOrdersTaskAsync(int sleep)
        {
            var beginDate = DateTime.UtcNow.AddHours(-1);

            while (!_taskCancellationTokenSource.Token.IsCancellationRequested)
            {
                var endDate = DateTime.UtcNow;

                var result = await ListRecentOrders(new ShopifyListRecentOrdersMessage
                {
                    BeginDate = beginDate,
                    EndDate = endDate
                });

                if (result.Result == Result.OK)
                {
                    beginDate = endDate;
                }
                else
                {
                    LogError("Error in ProcessListNewOrdersTaskAsync", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(),
                        new ShopifyListRecentOrdersMessage
                        {
                            BeginDate = beginDate,
                            EndDate = endDate
                        }, null));
                }

                await Task.Delay(sleep, _taskCancellationTokenSource.Token).ContinueWith(tsk => { }); //ignore exception
            }
        }

        private async Task ProcessListLostOrdersTaskAsync(int sleep)
        {
            var beginDate = DateTime.UtcNow.AddHours(-12);

            while (!_taskCancellationTokenSource.Token.IsCancellationRequested)
            {
                var endDate = DateTime.UtcNow.AddMinutes(-40);

                var auxDate = DateTime.UtcNow;

                var result = await ListLostOrders(new ShopifyListRecentOrdersMessage
                {
                    BeginDate = beginDate,
                    EndDate = endDate
                });

                if (result.Result != Result.OK)
                {
                    LogError("Error in ProcessListLostOrdersTaskAsync {0}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(),
                        new ShopifyListRecentOrdersMessage
                        {
                            BeginDate = beginDate,
                            EndDate = endDate
                        }, null));
                }

                beginDate = auxDate;
                await Task.Delay(sleep, _taskCancellationTokenSource.Token).ContinueWith(tsk => { }); //ignore exception
            }
        }

        public async Task<ReturnMessage> ListLostOrders(ShopifyListRecentOrdersMessage message)
        {
            var queryByDateResult = await _orderActor.Ask<ReturnMessage<OrderByDateQueryOutput>>(
                new OrderByDateQuery(message.BeginDate, message.EndDate, null, filters: "NOT (IsIntg-True-Intg)"),
                _webJobCancellationToken
            );

            if (queryByDateResult.Result == Result.Error)
                return new ReturnMessage { Result = Result.Error, Error = queryByDateResult.Error };

            var data = queryByDateResult.Data;

            while (queryByDateResult.Data.orders.pageInfo.hasNextPage == true)
            {
                queryByDateResult = await _orderActor.Ask<ReturnMessage<OrderByDateQueryOutput>>(
                    new OrderByDateQuery(message.BeginDate, message.EndDate, queryByDateResult.Data.orders.edges.Last().cursor,
                    filters: "NOT (IsIntg-True-Intg)"),
                    _webJobCancellationToken
                );

                if (queryByDateResult.Result == Result.Error)
                    return new ReturnMessage { Result = Result.Error, Error = queryByDateResult.Error };

                data.orders.edges.AddRange(queryByDateResult.Data.orders.edges);
            }

            if (data.orders.edges.Count > 0)
                LogWarning("O TenantId: {0} teve {1} pedidos não integrados as {2}", _shopifyData.Id, data.orders.edges.Count, DateTime.Now);

            foreach (var ids in data.orders.edges.Select(o => long.Parse(o.node.legacyResourceId)).Chunk(10))
            {
                var enqueueResult = await _orderActor.Ask<ReturnMessage>(
                    new ShopifyEnqueueListOrderMessage { OrderIds = ids.ToList() },
                _webJobCancellationToken);

                if (enqueueResult.Result == Result.Error)
                    return new ReturnMessage { Result = Result.Error, Error = enqueueResult.Error };
            }

            return new ReturnMessage { Result = Result.OK };
        }

        public async Task<ReturnMessage> ListRecentOrders(ShopifyListRecentOrdersMessage message)
        {
            var queryByDateResult = await _orderActor.Ask<ReturnMessage<OrderByDateQueryOutput>>(
                new OrderByDateQuery(message.BeginDate, message.EndDate), _webJobCancellationToken
            );

            if (queryByDateResult.Result == Result.Error)
                return new ReturnMessage { Result = Result.Error, Error = queryByDateResult.Error };

            var data = queryByDateResult.Data;

            while (queryByDateResult.Data.orders.pageInfo.hasNextPage == true)
            {
                queryByDateResult = await _orderActor.Ask<ReturnMessage<OrderByDateQueryOutput>>(
                    new OrderByDateQuery(message.BeginDate, message.EndDate, queryByDateResult.Data.orders.edges.Last().cursor), _webJobCancellationToken
                );

                if (queryByDateResult.Result == Result.Error)
                    return new ReturnMessage { Result = Result.Error, Error = queryByDateResult.Error };

                data.orders.edges.AddRange(queryByDateResult.Data.orders.edges);
            }

            foreach (var ids in data.orders.edges.Select(o => long.Parse(o.node.legacyResourceId)).Chunk(10))
            {
                var enqueueResult = await _orderActor.Ask<ReturnMessage>(
                    new ShopifyEnqueueListOrderMessage { OrderIds = ids.ToList() },
                _webJobCancellationToken);
                if (enqueueResult.Result == Result.Error)
                    return new ReturnMessage { Result = Result.Error, Error = enqueueResult.Error };
            }

            return new ReturnMessage { Result = Result.OK };
        }

        protected override void PostStop()
        {
            if (_taskCancellationTokenSource != null)
                _taskCancellationTokenSource.Dispose();

            ActorTaskScheduler.RunTask(async () =>
            {
                await Stop();
            });
        }

        private async Task AbandonMessageAsync(Message message, QueueClient queue, string method = "", string type = "", object request = null, object response = null, bool critical = false)
        {
            try
            {
                if (message.SystemProperties.DeliveryCount >= _maximumRetryCount)
                {
                    var logId = Guid.NewGuid();
                    try
                    {
                        var scope = _serviceProvider.CreateScope();
                        var logsAzureIdentityRepository = scope.ServiceProvider.GetService<LogsAbandonMessageRepository>();
                        var logs = new LogsAbandonMessage(logId, "WebJobShopify", _shopifyData.Id, method, type, Newtonsoft.Json.JsonConvert.SerializeObject(request));
                        logs.AddErrorInfo(response);

                        var exist = await logsAzureIdentityRepository.ExistAsync(logs);
                        if (!exist)
                        {
                            await logsAzureIdentityRepository.AddAsync(logs);
                            LogWarning("Error in AbandonMessageAsync | {0}", LoggerDescription.From(_shopifyData.Id.ToString(), type, method, request, response, logId), LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, "(Shopify) Error in AbandonMessageAsync/logsAzureIdentityRepository");
                    }

                    await queue.DeadLetterAsync(message.SystemProperties.LockToken, new Dictionary<string, object> { { "LogId", logId } }).ConfigureAwait(false);
                }
                else
                {
                    await queue.AbandonAsync(message.SystemProperties.LockToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                var msg = $"ShopifyTenantActor - Exception when abandon a event message from {message.SystemProperties.LockToken} of Azure Service Bus";
                LogError(ex, msg + " | {0}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(),
                    message, null, $"Exception when abandon a event message from { message.SystemProperties.LockToken } of Azure Service Bus"));
            }
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            return Akka.Actor.Props.Create(() => new ShopifyTenantActor(serviceProvider, cancellationToken));
        }
    }
}
