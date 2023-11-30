using Akka.Actor;
using Akka.Dispatch;
using Akka.Routing;

using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Application.Tools;
using Samurai.Integration.Domain.Consts;
using Samurai.Integration.Domain.Entities.Database.Logs;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Extensions;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Millennium;
using Samurai.Integration.Domain.Messages.Shared;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Queues;
using Samurai.Integration.Domain.Results.Logger;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.Millennium
{
    public class MillenniumTenantActor : BaseMillenniumTenantActor
    {
        private readonly int _maximumRetryCount = 3;
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _webJobCancellationToken;
        private CancellationTokenSource _taskCancellationTokenSource;
        private readonly MillenniumSessionToken _millenniumSessionToken;

        #region Actors
        private List<IActorRef> _apiActors;
        private IActorRef _apiActorGroup;
        private IActorRef _productActor;
        private IActorRef _orderActor;
        #endregion

        #region QueueClients
        private QueueClient _listFullProductQueue;
        private QueueClient _getPriceProductQueue;
        private QueueClient _updateOrderQueue;
        private QueueClient _listOrderQueue;
        private QueueClient _createOrderQueue;
        private QueueClient _shopifyListOrderQueueClient;
        private QueueClient _shopifyUpdateOrderNumberTagQueueClient;
        private QueueClient _processProductImageQueue;

        #endregion

        #region Tasks
        private Task _listNewProductsTask;
        private Task _listNewStocksTask;
        private Task _listNewPricesTask;
        private Task _listNewOrdersTask;
        private Task _reprocessListNewOrdersTask;
        private Task _listNewStocksMtoTask;


        #endregion      

        public MillenniumTenantActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, MillenniumSessionToken millenniumSessionToken)
            : base("MillenniumTenantActor")
        {
            _serviceProvider = serviceProvider;
            _webJobCancellationToken = cancellationToken;
            _millenniumSessionToken = millenniumSessionToken;

            ReceiveAsync((Func<InitializeMillenniumTenantMessage, Task>)(async message =>
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
                    LogError(ex, "Error in InitializeMillenniumTenantMessage | Erro.Message: {0}", ex.Message);
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync<UpdateMillenniumTenantMessage>(async message =>
            {
                try
                {
                    //if there are any changes, stop and restart//
                    if (_millenniumData.EqualsTo(message.Data) == false)
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
                    LogError(ex, "Error in UpdateMillenniumTenantMessage | Erro.Message: {0}", ex.Message);
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            });

            ReceiveAsync((Func<StopMillenniumTenantMessage, Task>)(async message =>
            {
                try
                {
                    await Stop();
                    Context.Stop(Self);
                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in StopMillenniumTenantMessage | Erro.Message: {0}", ex.Message);
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));
        }

        private async Task Stop()
        {
            if (_millenniumData != null && _taskCancellationTokenSource != null)
            {
                #region Tasks

                _taskCancellationTokenSource.Cancel();

                if (_millenniumData.ProductIntegrationStatus == true)
                {
                    if (_listNewProductsTask != null)
                        await _listNewProductsTask;
                    if (_listNewStocksTask != null)
                        await _listNewStocksTask;
                    if (_listNewPricesTask != null)
                        await _listNewPricesTask;
                    _listNewProductsTask = _listNewStocksTask = _listNewPricesTask = null;
                }

                if (_millenniumData.OrderIntegrationStatus == true)
                {
                    await _listNewOrdersTask.CloseAsyncSafe();
                    await _reprocessListNewOrdersTask.CloseAsyncSafe();
                }

                #endregion

                #region QueueClients

                if (_millenniumData.ProductIntegrationStatus == true)
                {
                    await _listFullProductQueue.CloseAsyncSafe();
                    await _getPriceProductQueue.CloseAsyncSafe();
                    await _processProductImageQueue.CloseAsyncSafe();


                }

                if (_millenniumData.OrderIntegrationStatus == true)
                {
                    if (_updateOrderQueue != null && !_updateOrderQueue.IsClosedOrClosing)
                        await _updateOrderQueue.CloseAsync();
                    _updateOrderQueue = null;

                    if (_listOrderQueue != null && !_listOrderQueue.IsClosedOrClosing)
                        await _listOrderQueue.CloseAsync();
                    _listOrderQueue = null;

                    if (_shopifyListOrderQueueClient != null && !_shopifyListOrderQueueClient.IsClosedOrClosing)
                        await _shopifyListOrderQueueClient.CloseAsync();
                    _shopifyListOrderQueueClient = null;

                    if (_shopifyUpdateOrderNumberTagQueueClient != null && !_shopifyUpdateOrderNumberTagQueueClient.IsClosedOrClosing)
                        await _shopifyUpdateOrderNumberTagQueueClient.CloseAsync();
                    _shopifyUpdateOrderNumberTagQueueClient = null;
                }

                #endregion

                #region Actors

                if (_millenniumData.ProductIntegrationStatus == true)
                {
                    if (_productActor != null)
                        await _productActor.GracefulStop(TimeSpan.FromSeconds(30));
                    _productActor = null;
                }

                if (_millenniumData.OrderIntegrationStatus == true)
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

        private void Initialize(MillenniumData data, IServiceScope scope)
        {
            _millenniumData = data;

            var _tenantService = scope.ServiceProvider.GetService<TenantService>();
            var _configuration = scope.ServiceProvider.GetService<IConfiguration>();

            _taskCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_webJobCancellationToken);

            #region Actors
            _apiActors = new List<IActorRef>();
            foreach (var login in _millenniumData.Logins)
            {
                _apiActors.Add(Context.ActorOf(MillenniumApiActor.Props(_serviceProvider, _webJobCancellationToken, _millenniumData, login, _millenniumSessionToken)));
            }
            _apiActorGroup = Context.ActorOf(Akka.Actor.Props.Empty.WithRouter(new RoundRobinGroup(_apiActors)));

            if (_millenniumData.ProductIntegrationStatus == true)
            {
                _productActor = Context.ActorOf(new RoundRobinPool(_millenniumData.Logins.Count)
                                                    .Props(MillenniumProductActor.Props(_serviceProvider, _webJobCancellationToken, _millenniumData, _apiActorGroup)));
            }

            if (_millenniumData.OrderIntegrationStatus == true)
            {
                _orderActor = Context.ActorOf(new RoundRobinPool(_millenniumData.Logins.Count)
                                                .Props(MillenniumOrderActor.Props(_serviceProvider, _webJobCancellationToken, _millenniumData, _apiActorGroup)));
            }

            #endregion

            #region QueueClients

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandlerAsync)
            {
                MaxConcurrentCalls = _millenniumData.Logins.Count,
                AutoComplete = false
            };

            if (_millenniumData.ProductIntegrationStatus == true)
            {
                _listFullProductQueue = _tenantService.GetQueueClient(_millenniumData, MillenniumQueue.ListFullProductQueue);
                _listFullProductQueue.RegisterMessageHandler(ProcessListProductMessageAsync, messageHandlerOptions);

                _getPriceProductQueue = _tenantService.GetQueueClient(_millenniumData, MillenniumQueue.GetPriceProductQueue);
                _getPriceProductQueue.RegisterMessageHandler(GetPriceProductMessageAsync, messageHandlerOptions);

                _processProductImageQueue = _tenantService.GetQueueClient(_millenniumData, MillenniumQueue.ProcessProductImageQueue);
                _processProductImageQueue.RegisterMessageHandler(ProcessProductImageMessageAsync, new MessageHandlerOptions(ExceptionReceivedHandlerAsync)
                {
                    MaxConcurrentCalls = _millenniumData.Logins.Count,
                    AutoComplete = false,
                    MaxAutoRenewDuration = TimeSpan.FromSeconds(180)
                });
            }

            if (_millenniumData.OrderIntegrationStatus == true)
            {
                _updateOrderQueue = _tenantService.GetQueueClient(_millenniumData, MillenniumQueue.UpdateOrderQueue);
                _updateOrderQueue.RegisterMessageHandler(ProcessUpdateOrderMessageAsync, messageHandlerOptions);

                _listOrderQueue = _tenantService.GetQueueClient(_millenniumData, MillenniumQueue.ListOrderQueue);
                _listOrderQueue.RegisterMessageHandler(ProcessListOrderMessageAsync, messageHandlerOptions);

                _createOrderQueue = _tenantService.GetQueueClient(_millenniumData, MillenniumQueue.CreateOrderQueue);
                _createOrderQueue.RegisterMessageHandler(CreateOrderMessageAsync, messageHandlerOptions);

                if (_millenniumData.IntegrationType == Domain.Enums.IntegrationType.Shopify)
                {
                    _shopifyListOrderQueueClient = _tenantService.GetQueueClient(_millenniumData, ShopifyQueue.ListOrderQueue);

                    _shopifyUpdateOrderNumberTagQueueClient = _tenantService.GetQueueClient(_millenniumData, ShopifyQueue.UpdateOrderNumberTagQueue);
                }

            }

            #endregion

            #region Tasks
            if (_millenniumData.ProductIntegrationStatus == true)
            {
                _listNewProductsTask = ProcessListNewProductsTaskAsync(int.Parse(_configuration.GetSection("Schedulle")["MaxProductSleep"]), _millenniumData);
                _listNewStocksTask = ProcessListNewStocksTaskAsync(int.Parse(_configuration.GetSection("Schedulle")["MaxStockSleep"]), _millenniumData);
            }


            if (_millenniumData.ProductIntegrationPrice == true && _millenniumData.ProductIntegrationStatus == true)//price process
                _listNewPricesTask = ProcessListNewPricesTaskAsync(int.Parse(_configuration.GetSection("Schedulle")["MaxPriceSleep"]), _millenniumData);

            if (_millenniumData.EnabledStockMto)
            {
                _listNewStocksMtoTask = ProcessListNewStocksMtoTaskAsync(int.Parse(_configuration.GetSection("Schedulle")["MaxStockSleep"]));
            }

            if (_millenniumData.OrderIntegrationStatus == true)
            {
                _listNewOrdersTask = ProcessListNewOrdersTaskAsync(int.Parse(_configuration.GetSection("Schedulle")["MaxOrderSleep"]));
                //_reprocessListNewOrdersTask = ReprocessListNewOrdersTaskAsync();
            }

            #endregion
        }

        private async Task ProcessListProductMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                    await _listFullProductQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _listFullProductQueue, "ProcessListProductMessageAsync", "product", messageValue, result);
            }
        }

        private async Task GetPriceProductMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<GetPriceProductMessage>();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                    await _getPriceProductQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _getPriceProductQueue, "GetPriceProductMessageAsync", "product", messageValue, result);
            }
        }

        private async Task ProcessProductImageMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<ProcessProductImageMessage>();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                    await _processProductImageQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _processProductImageQueue, "ProcessProductImageMessageAsync", "product", messageValue, result);
            }
        }

        private async Task ProcessUpdateOrderMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<ShopifySendOrderToERPMessage>();

                var result = await _orderActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    await _updateOrderQueue.CompleteAsync(message.SystemProperties.LockToken);

                    var serviceBusMessage = new ServiceBusMessage(new ShopifyUpdateOrderTagNumberMessage
                    {
                        ShopifyId = messageValue.ID,
                        IntegrationStatus = messageValue.GetOrderStatus(),
                        OrderExternalId = messageValue.ExternalID,
                        OrderNumber = messageValue.Number.ToString()
                    });

                    await _shopifyUpdateOrderNumberTagQueueClient.SendAsync(serviceBusMessage.GetMessage(messageValue.ID));
                }
                else
                {
                    await AbandonMessageAsync(message, _updateOrderQueue, "ProcessUpdateOrderMessageAsync", "order", messageValue, result, true);
                }
                    
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
                    await AbandonMessageAsync(message, _listOrderQueue, "ProcessListOrderMessageAsync", "order", messageValue, result);
            }
        }

        private async Task CreateOrderMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<Domain.Models.SellerCenter.API.Orders.Order>();

                var result = await _orderActor.Ask<ReturnMessage>(new CreateOrderMessage(messageValue), _webJobCancellationToken);

                if (result.Result == Result.OK)
                    await _createOrderQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _createOrderQueue, "CreateOrderMessageAsync", "order", messageValue, result, true);
            }
        }

        private object returnError(ReturnMessage message)
        {
            return new { message.Error?.Message, message.Error?.StackTrace };
        }

        private async Task ExceptionReceivedHandlerAsync(ExceptionReceivedEventArgs arg)
        {
            LogError(arg.Exception, "Received exception in Millennium Queue, action: {0}, message: {1}.", arg.ExceptionReceivedContext.Action, arg.Exception.Message);
        }

        private async Task ProcessListNewProductsTaskAsync(int maxSleep, MillenniumData millenniumData)
        {
            var sleepScheduller = new SleepScheduller(maxSleep, 10000, 2);

            while (!_taskCancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var result = await _productActor.Ask<ReturnMessage>(new MillenniumListNewProductsMessage(), _webJobCancellationToken);

                    if (result.Result == Result.OK)
                    {
                        sleepScheduller.Reset();
                    }
                    else
                    {
                        var timeToSleep = sleepScheduller.GetNextTime();
                        await Task.Delay(timeToSleep, _taskCancellationTokenSource.Token).ContinueWith(tsk => { });
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, "Tenantid:{0} - ProcessListNewProductsTaskAsync error | Error.Message: {1}", millenniumData.Id, ex.Message);
                }
            }
        }

        private async Task ProcessListNewStocksTaskAsync(int maxSleep, MillenniumData millenniumData)
        {
            var sleepScheduller = new SleepScheduller(maxSleep, 10000, 2);

            while (!_taskCancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var result = await _productActor.Ask<ReturnMessage>(new MillenniumListNewStocksMessage(), _webJobCancellationToken);

                    if (result.Result == Result.OK)
                    {
                        sleepScheduller.Reset();
                    }
                    else
                    {
                        var timeToSleep = sleepScheduller.GetNextTime();
                        await Task.Delay(timeToSleep, _taskCancellationTokenSource.Token).ContinueWith(tsk => { });
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, "Tenantid:{0} - ProcessListNewStocksTaskAsync error | Error.Message: {1}", millenniumData.Id, ex.Message);
                }
            }
        }

        private async Task ProcessListNewStocksMtoTaskAsync(int maxSleep)
        {
            var sleepScheduller = new SleepScheduller(maxSleep, 10000, 2);

            while (!_taskCancellationTokenSource.Token.IsCancellationRequested)
            {
                var result = await _productActor.Ask<ReturnMessage>(new MillenniumListNewStockMtoMessage(), _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    sleepScheduller.Reset();
                }
                else
                {
                    var timeToSleep = sleepScheduller.GetNextTime();

                    await Task.Delay(timeToSleep, _taskCancellationTokenSource.Token).ContinueWith(tsk => { });
                }
            }

        }

        private async Task ProcessListNewPricesTaskAsync(int maxSleep, MillenniumData millenniumData)
        {
            var sleepScheduller = new SleepScheduller(maxSleep, 10000, 2);

            while (!_taskCancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var result = await _productActor.Ask<ReturnMessage>(new MillenniumListNewPricesMessage(), _webJobCancellationToken);

                    if (result.Result == Result.OK)
                    {
                        sleepScheduller.Reset();
                    }
                    else
                    {
                        var timeToSleep = sleepScheduller.GetNextTime();

                        await Task.Delay(timeToSleep, _taskCancellationTokenSource.Token).ContinueWith(tsk => { });
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, "Tenantid:{0} - ProcessListNewPricesTaskAsync error", millenniumData.Id);
                }
            }

        }

        private async Task ProcessListNewOrdersTaskAsync(int maxSleep)
        {
            var sleepScheduller = new SleepScheduller(maxSleep, 10000, 2);

            while (!_taskCancellationTokenSource.Token.IsCancellationRequested)
            {
                var result = await _orderActor.Ask<ReturnMessage>(new MillenniumListNewOrdersMessage(), _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    sleepScheduller.Reset();
                }
                else
                {
                    var timeToSleep = sleepScheduller.GetNextTime();

                    await Task.Delay(timeToSleep, _taskCancellationTokenSource.Token).ContinueWith(tsk => { }); //ignore exception
                }
            }

        }

        private async Task ReprocessListNewOrdersTaskAsync()
        {
            while (!_taskCancellationTokenSource.Token.IsCancellationRequested)
            {
                var result = await _orderActor.Ask<ReturnMessage>(new MillenniumListNewOrdersMessage { Retry = true }, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    await Task.Delay(32400000, _taskCancellationTokenSource.Token).ContinueWith(tsk => { }); //ignore exception
                }
                else
                {
                    LogError("Error: ReprocessListNewOrdersTaskAsync slepping {0} milliseconds: {1}", 3600, result.Error);
                    await Task.Delay(3600000, _taskCancellationTokenSource.Token).ContinueWith(tsk => { }); //ignore exception
                }
            }
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

        private async Task AbandonMessageAsync(Message message, 
            QueueClient queue,
            string method = "",
            string type = "",
            object request = null,
            ReturnMessage response = null,
            bool critical = false)
        {
            var logId = Guid.NewGuid();
            var log = LoggerDescription.From(_millenniumData.Id.ToString(), type, method, request, response, logId);

            try
            {
                if (message.SystemProperties.DeliveryCount >= _maximumRetryCount)
                {   
                    try
                    {
                        var scope = _serviceProvider.CreateScope();
                        var logsAzureIdentityRepository = scope.ServiceProvider.GetService<LogsAbandonMessageRepository>();
                        var logs = new LogsAbandonMessage(logId, "WebJobMillennium", _millenniumData.Id, method, type, Newtonsoft.Json.JsonConvert.SerializeObject(request));
                        logs.AddErrorInfo(response);

                        var exist = await logsAzureIdentityRepository.ExistAsync(logs);
                        if (!exist)
                        {
                            await logsAzureIdentityRepository.AddAsync(logs);
                            LogError(log);
                        }

                        var error = logs.Error;

                        await SendOrderErrorTagToShopify(request,response, error);                

                    }   
                    catch (Exception ex)
                    {
                        var error = "(Millennium )Error in AbandonMessageAsync/logsAzureIdentityRepository";
                        LogError(ex, error);
                        await SendOrderErrorTagToShopify(request, response, error);
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
                LogError(ex, "LogId: {1} - Exception when abandon a event message from {0} of Azure Service Bus.", message.SystemProperties.LockToken, logId);
            }
        }

        private async Task SendOrderErrorTagToShopify(object request,ReturnMessage response, string error)
        {
            if (request?.GetType() != typeof(ShopifySendOrderToERPMessage))
            {
                return;
            }

            var tag = GenerateTagError(response, error);

            await UpdateOrderTagOnShopify((ShopifySendOrderToERPMessage)request, tag);

        }

        private string GenerateTagError(ReturnMessage response, string error)
        {
            using var scope = _serviceProvider.CreateScope();

            var _integrationErrorService = scope.ServiceProvider.GetService<IntegrationErrorService>();

            return _integrationErrorService.GenerateTagError(error, IntegrationErrorSource.Millennium, () => IsMilleniumException(response)).Result;

            static bool IsMilleniumException(ReturnMessage response)
            {
                return response?.Error?.ToString()?.Contains(MillenniumConsts.MillenniumApiClient) ?? false;
            }
        }

        private async Task UpdateOrderTagOnShopify(ShopifySendOrderToERPMessage request, string tag)
        {
            var messageValue = request;
            var serviceBusMessage = new ServiceBusMessage(new ShopifyUpdateOrderTagNumberMessage
            {
                ShopifyId = messageValue.ID,
                CustomTags = new List<string>() { tag }
            });

            await _shopifyUpdateOrderNumberTagQueueClient.SendAsync(serviceBusMessage.GetMessage(messageValue.ID));

        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, MillenniumSessionToken millenniumSessionToken)
        {
            return Akka.Actor.Props.Create(() => new MillenniumTenantActor(serviceProvider, cancellationToken, millenniumSessionToken));
        }
    }
}
