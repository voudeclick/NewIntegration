using Akka.Actor;
using Akka.Dispatch;
using Akka.Routing;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.APIClient.Nexaas.Models.Requests;
using Samurai.Integration.APIClient.Nexaas.Models.Results;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Application.Tools;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Nexaas;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Models.Nexaas;
using Samurai.Integration.Domain.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.Nexaas
{
    public class NexaasTenantActor : BaseNexaasTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _webJobCancellationToken;
        private CancellationTokenSource _taskCancellationTokenSource;

        #region Actors
        private IActorRef _apiActorGroup;
        private IActorRef _productActor;
        private IActorRef _orderActor;
        #endregion

        #region QueueClients
        private QueueClient _listFullProductQueue;
        private QueueClient _listAllProductsQueue;
        private QueueClient _listPartialProductQueue;
        private QueueClient _listStockQueue;
        private QueueClient _listVendorQueue;
        private QueueClient _listProductCategoriesQueue;
        private QueueClient _listOrderQueue;
        private QueueClient _updateOrderQueue;
        private QueueClient _shopifyListOrderQueueClient;
        private QueueClient _shopifyUpdateOrderNumberTagQueueClient;
        #endregion

        #region Tasks

        #endregion

        public NexaasTenantActor(IServiceProvider serviceProvider, CancellationToken cancellationToken)
            : base("NexaasTenantActor")
        {
            _serviceProvider = serviceProvider;
            _webJobCancellationToken = cancellationToken;

            ReceiveAsync((Func<InitializeNexaasTenantMessage, Task>)(async message => {
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
                    LogError(ex, "Error in InitializeNexaasTenantMessage");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync<UpdateNexaasTenantMessage>(async message => {
                try
                {
                    //if there are any changes, stop and restart//
                    if (_nexaasData.EqualsTo(message.Data) == false)
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
                    LogError(ex, "Error in UpdateNexaasTenantMessage");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            });

            ReceiveAsync((Func<StopNexaasTenantMessage, Task>)(async message => {
                try
                {
                    await Stop();
                    Context.Stop(Self);
                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in StopNexaasTenantMessage");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));
        }

        private async Task Stop()
        {
            if (_nexaasData != null && _taskCancellationTokenSource != null)
            {
                LogInfo($"Stoping Tasks");
                #region Tasks

                _taskCancellationTokenSource.Cancel();

                if (_nexaasData.ProductIntegrationStatus == true)
                {

                }

                if (_nexaasData.OrderIntegrationStatus == true)
                {
         
                }

                #endregion

                LogInfo($"Stoping QueueClients");
                #region QueueClients

                if (_nexaasData.ProductIntegrationStatus == true)
                {
                    if (_listFullProductQueue != null && !_listFullProductQueue.IsClosedOrClosing)
                        await _listFullProductQueue.CloseAsync();
                    _listFullProductQueue = null;

                    if (_listAllProductsQueue != null && !_listAllProductsQueue.IsClosedOrClosing)
                        await _listAllProductsQueue.CloseAsync();
                    _listAllProductsQueue = null;

                    if (_listPartialProductQueue != null && !_listPartialProductQueue.IsClosedOrClosing)
                        await _listPartialProductQueue.CloseAsync();
                    _listPartialProductQueue = null;

                    if (_listStockQueue != null && !_listStockQueue.IsClosedOrClosing)
                        await _listStockQueue.CloseAsync();
                    _listStockQueue = null;

                    if (_listVendorQueue != null && !_listVendorQueue.IsClosedOrClosing)
                        await _listVendorQueue.CloseAsync();
                    _listVendorQueue = null;

                    if (_listProductCategoriesQueue != null && !_listProductCategoriesQueue.IsClosedOrClosing)
                        await _listProductCategoriesQueue.CloseAsync();
                    _listProductCategoriesQueue = null;
                }

                if (_nexaasData.OrderIntegrationStatus == true)
                {
                    if (_updateOrderQueue != null && !_updateOrderQueue.IsClosedOrClosing)
                        await _updateOrderQueue.CloseAsync();
                    _updateOrderQueue = null;

                    if (_listOrderQueue != null && !_listOrderQueue.IsClosedOrClosing)
                        await _listOrderQueue.CloseAsync();
                    _listOrderQueue = null;
                }

                #endregion

                LogInfo($"Stoping Actors");
                #region Actors

                if (_nexaasData.ProductIntegrationStatus == true)
                {
                    if (_productActor != null)
                        await _productActor.GracefulStop(TimeSpan.FromSeconds(30));
                    _productActor = null;
                }

                if (_nexaasData.OrderIntegrationStatus == true)
                {
                    if (_orderActor != null)
                        await _orderActor.GracefulStop(TimeSpan.FromSeconds(30));
                    _orderActor = null;
                }

                if (_apiActorGroup != null)
                    await _apiActorGroup.GracefulStop(TimeSpan.FromSeconds(30));
                _apiActorGroup = null;

                #endregion

                _taskCancellationTokenSource.Dispose();
                _taskCancellationTokenSource = null;
            }
        }

        private void Initialize(NexaasData data, IServiceScope scope)
        {
            var maxConcurrency = 1;
            _nexaasData = data;
            LogInfo($"Initializing");
            var _tenantService = scope.ServiceProvider.GetService<TenantService>();
            var _configuration = scope.ServiceProvider.GetService<IConfiguration>();

            _taskCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_webJobCancellationToken);

            LogInfo($"Initializing Actors");
            #region Actors
            _apiActorGroup = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                    .Props(NexaasApiActor.Props(_serviceProvider, _webJobCancellationToken, _nexaasData)));

            if (_nexaasData.ProductIntegrationStatus == true)
            {
                _productActor = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                                    .Props(NexaasProductActor.Props(_serviceProvider, _webJobCancellationToken, _nexaasData, _apiActorGroup)));
            }

            if (_nexaasData.OrderIntegrationStatus == true)
            {
                _orderActor = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                                .Props(NexaasOrderActor.Props(_serviceProvider, _webJobCancellationToken, _nexaasData, _apiActorGroup)));
            }

            #endregion

            LogInfo($"Initializing QueueClients");
            #region QueueClients

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandlerAsync)
            {
                MaxConcurrentCalls = maxConcurrency,
                AutoComplete = false
            };

            if (_nexaasData.ProductIntegrationStatus == true)
            {
                _listFullProductQueue = _tenantService.GetQueueClient(_nexaasData, NexaasQueue.ListFullProductQueue);
                _listFullProductQueue.RegisterMessageHandler(ProcessListFullProductMessageAsync, messageHandlerOptions);

                _listAllProductsQueue = _tenantService.GetQueueClient(_nexaasData, NexaasQueue.ListAllProductsQueue);
                _listAllProductsQueue.RegisterMessageHandler(ProcessListAllProductsMessageAsync, new MessageHandlerOptions(ExceptionReceivedHandlerAsync)
                {
                    MaxConcurrentCalls = maxConcurrency,
                    MaxAutoRenewDuration = TimeSpan.FromMinutes(20),
                    AutoComplete = false
                });

                _listPartialProductQueue = _tenantService.GetQueueClient(_nexaasData, NexaasQueue.ListPartialProductQueue);
                _listPartialProductQueue.RegisterMessageHandler(ProcessListPartialProductMessageAsync, messageHandlerOptions);

                _listStockQueue = _tenantService.GetQueueClient(_nexaasData, NexaasQueue.ListStockQueue);
                _listStockQueue.RegisterMessageHandler(ProcessListStockMessageAsync, messageHandlerOptions);

                _listVendorQueue = _tenantService.GetQueueClient(_nexaasData, NexaasQueue.ListVendorQueue);
                _listVendorQueue.RegisterMessageHandler(ProcessListVendorMessageAsync, messageHandlerOptions);

                _listProductCategoriesQueue = _tenantService.GetQueueClient(_nexaasData, NexaasQueue.ListProductCategoriesQueue);
                _listProductCategoriesQueue.RegisterMessageHandler(ProcessListProductCategoriesMessageAsync, messageHandlerOptions);                                                                   
            }

            if (_nexaasData.OrderIntegrationStatus == true)
            {
                _updateOrderQueue = _tenantService.GetQueueClient(_nexaasData, NexaasQueue.UpdateOrderQueue);
                _updateOrderQueue.RegisterMessageHandler(ProcessUpdateOrderMessageAsync, messageHandlerOptions);

                _listOrderQueue = _tenantService.GetQueueClient(_nexaasData, NexaasQueue.ListOrderQueue);
                _listOrderQueue.RegisterMessageHandler(ProcessListOrderMessageAsync, messageHandlerOptions);

                _shopifyListOrderQueueClient = _tenantService.GetQueueClient(_nexaasData, ShopifyQueue.ListOrderQueue);

                _shopifyUpdateOrderNumberTagQueueClient = _tenantService.GetQueueClient(_nexaasData, ShopifyQueue.UpdateOrderNumberTagQueue);
            }

            #endregion

            LogInfo($"Initializing Tasks");
            #region Tasks

            if (_nexaasData.ProductIntegrationStatus == true)
            {
            }

            if (_nexaasData.OrderIntegrationStatus == true)
            {
            }

            #endregion
        }
        
        private async Task ProcessListFullProductMessageAsync(Message message, CancellationToken cancellationToken)
        {
            LogInfo($"ProcessListFullProductMessageAsync Received message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                Func<Exception, Task> AbandonMessage = async e =>
                {
                    LogError(e, $"ProcessListFullProductMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listFullProductQueue.AbandonAsync(message.SystemProperties.LockToken);
                };

                try
                {
                    var messageValue = new ServiceBusMessage(message.Body).GetValue<ShopifyListERPFullProductMessage>();

                    var resultProduct = await _productActor.Ask<ReturnMessage<NexaasApiListProductResult>>(new NexaasApiListProductRequest { Id = long.Parse(messageValue.ExternalId) }, _webJobCancellationToken);

                    if (resultProduct.Result == Result.Error)
                    {
                        await AbandonMessage(resultProduct.Error);
                        return;
                    }
                    var product = resultProduct.Data.product;

                    var skus = new List<NexaasSku>();
                    var productsStockRequest = new NexaasApiListProductStocksRequest();

                    if (product.product_skus.Where(s => s.active).Count() > 100)
                        throw new Exception("product has more than 100 skus");

                    foreach (var sku in product.product_skus)
                    {
                        if (sku.active)
                        {
                            var resultSkuPrice = await _productActor.Ask<ReturnMessage<NexaasApiListProductPricesResult>>(new NexaasApiListProductPricesRequest { ProductSkuId = sku.id }, _webJobCancellationToken);
                            if (resultSkuPrice.Result == Result.Error)
                            {
                                await AbandonMessage(resultSkuPrice.Error);
                                return;
                            }
                            sku.SkuPrice = resultSkuPrice.Data?.sku_price;

                            productsStockRequest.search.stock_skus.Add(new NexaasApiListProductStocksRequest.SkuStockRequest
                            {
                                product_sku_id = sku.id
                            });
                        }

                        skus.Add(sku);
                    }

                    var stocksSkus = new List<NexaasStockSku>();
                    ReturnMessage<NexaasApiListProductStocksResult> resultStock = null;
                    productsStockRequest.Page = 1;
                    do
                    {
                        resultStock = await _productActor.Ask<ReturnMessage<NexaasApiListProductStocksResult>>(productsStockRequest, _webJobCancellationToken);
                        if (resultStock.Result == Result.Error)
                        {
                            await AbandonMessage(resultStock.Error);
                            return;
                        }
                        stocksSkus.AddRange(resultStock.Data.stock_skus);
                        productsStockRequest.Page++;
                    } while (resultStock.Data.stock_skus.Any());

                    var resultVendor = await _productActor.Ask<ReturnMessage<NexaasApiListVendorResult>>(new NexaasApiListVendorRequest { Id = product.product_brand_id }, _webJobCancellationToken);
                    if (resultVendor.Result == Result.Error)
                    {
                        await AbandonMessage(resultVendor.Error);
                        return;
                    }
                    var vendor = resultVendor.Data.product_brand;

                    long? categoryId = product.product_category_id;
                    var categories = new List<NexaasCategory>();
                    while (categoryId > 0)
                    {
                        var resultCategory = await _productActor.Ask<ReturnMessage<NexaasApiListCategoryResult>>(new NexaasApiListCategoryRequest { Id = (long)categoryId }, _webJobCancellationToken);
                        if (resultCategory.Result == Result.Error)
                        {
                            await AbandonMessage(resultCategory.Error);
                            return;
                        }
                        categories.Add(resultCategory.Data.product_category);
                        categoryId = resultCategory.Data.product_category.parent_id;
                    }

                    var resultSendToShopify = await _productActor.Ask<ReturnMessage>(new NexaasSendFullProductShopifyMessage
                    {
                        Categories = categories,
                        Product = product,
                        Skus = skus,
                        StocksSkus = stocksSkus,
                        Vendor = vendor
                    }, _webJobCancellationToken);

                    if (resultSendToShopify.Result == Result.Error)
                    {
                        await AbandonMessage(resultSendToShopify.Error);
                        return;
                    }

                    LogInfo($"ProcessListFullProductMessageAsync Finished message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listFullProductQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                catch(Exception ex)
                {
                    await AbandonMessage(ex);
                    return;
                }
            }
        }

        private async Task ProcessListAllProductsMessageAsync(Message message, CancellationToken cancellationToken)
        {
            LogInfo($"ProcessListAllProductsMessageAsync Received message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<NexaasListAllProductsMessage>();
                var allIds = new List<long>();
                var page = 1;
                ReturnMessage<NexaasApiListAllProductsResult> result = null;

                do
                {
                    result = await _productActor.Ask<ReturnMessage<NexaasApiListAllProductsResult>>(new NexaasApiListAllProductsRequest { Page = page }, _webJobCancellationToken);
                    if (result.Result == Result.Error)
                    {
                        LogError(result.Error, $"ProcessListAllProductsMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                        await _listAllProductsQueue.AbandonAsync(message.SystemProperties.LockToken);
                        return;
                    }
                    allIds.AddRange(result.Data.products.Select(p => p.id));
                    page++;
                } while (result.Data.products.Any());

                foreach (var ids in allIds.Chunk(10))
                {
                    var enqueueResult = await _productActor.Ask<ReturnMessage>(new NexaasEnqueueFullProductsMessage { ProductsIds = ids.ToList() }, _webJobCancellationToken);
                    if (enqueueResult.Result == Result.Error)
                    {
                        LogError(enqueueResult.Error, $"ProcessListAllProductsMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                        await _listAllProductsQueue.AbandonAsync(message.SystemProperties.LockToken);
                        return;
                    }
                }
                LogInfo($"ProcessListAllProductsMessageAsync Finished message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                await _listAllProductsQueue.CompleteAsync(message.SystemProperties.LockToken);
            }
        }

        private async Task ProcessListPartialProductMessageAsync(Message message, CancellationToken cancellationToken)
        {
            LogInfo($"ProcessListPartialProductMessageAsync Received message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    LogInfo($"ProcessListPartialProductMessageAsync Finished message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listPartialProductQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, $"ProcessListPartialProductMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listPartialProductQueue.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }

        private async Task ProcessListVendorMessageAsync(Message message, CancellationToken cancellationToken)
        {
            LogInfo($"ProcessListVendorMessageAsync Received message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    LogInfo($"ProcessListVendorMessageAsync Finished message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listVendorQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, $"ProcessListVendorMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listVendorQueue.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }

        private async Task ProcessListStockMessageAsync(Message message, CancellationToken cancellationToken)
        {
            LogInfo($"ProcessListStockMessageAsync Received message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    LogInfo($"ProcessListStockMessageAsync Finished message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listStockQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, $"ProcessListStockMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listStockQueue.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }

        private async Task ProcessListProductCategoriesMessageAsync(Message message, CancellationToken cancellationToken)
        {
            LogInfo($"ProcessLisProductCategoriesMessageAsync Received message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    LogInfo($"ProcessLisProductCategoriesMessageAsync Finished message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listProductCategoriesQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, $"ProcessLisProductCategoriesMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listProductCategoriesQueue.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }

        private async Task ProcessUpdateOrderMessageAsync(Message message, CancellationToken cancellationToken)
        {
            LogInfo($"ProcessUpdateOrderMessageAsync Received message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<ShopifySendOrderToERPMessage>();

                var result = await _orderActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    LogInfo($"ProcessUpdateOrderMessageAsync Finished message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _updateOrderQueue.CompleteAsync(message.SystemProperties.LockToken);

                    var serviceBusMessage = new ServiceBusMessage(new ShopifyUpdateOrderTagNumberMessage
                    {
                        ShopifyId = messageValue.ID,
                        IntegrationStatus = messageValue.GetOrderStatus()

                    });
                    await _shopifyUpdateOrderNumberTagQueueClient.SendAsync(serviceBusMessage.GetMessage(messageValue.ID));
                }
                else
                {
                    LogError(result.Error, $"ProcessOrderMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _updateOrderQueue.AbandonAsync(message.SystemProperties.LockToken);

                    if (message.SystemProperties.DeliveryCount == 3)
                    {
                        if (messageValue.DeliveryCount < 4)
                        {
                            var serviceBusMessage = new ServiceBusMessage(new ShopifyListOrderMessage { ShopifyId = messageValue.ID, DeliveryCount = messageValue.DeliveryCount + 1 });
                            await _shopifyListOrderQueueClient.ScheduleMessageAsync(serviceBusMessage.GetMessage(messageValue.ID), DateTime.UtcNow.AddHours(Math.Pow(4, messageValue.DeliveryCount)));
                        }
                    }
                }
            }
        }

        private async Task ProcessListOrderMessageAsync(Message message, CancellationToken cancellationToken)
        {
            LogInfo($"ProcessListOrderMessageAsync Received message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue();

                var result = await _orderActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    LogInfo($"ProcessListOrderMessageAsync Finished message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listOrderQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, $"ProcessListOrderMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listOrderQueue.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }

        private async Task ExceptionReceivedHandlerAsync(ExceptionReceivedEventArgs arg)
        {
            LogError(arg.Exception, "Error in Nexaas Queue");
        }


        protected override void PostStop()
        {
            if (_taskCancellationTokenSource != null)
                _taskCancellationTokenSource.Dispose();

            ActorTaskScheduler.RunTask(async () => {
                await Stop();
            });
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            return Akka.Actor.Props.Create(() => new NexaasTenantActor(serviceProvider, cancellationToken));
        }
    }
}
