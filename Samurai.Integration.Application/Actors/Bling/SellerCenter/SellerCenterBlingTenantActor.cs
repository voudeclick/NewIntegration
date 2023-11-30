using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Akka.Actor;
using Akka.Dispatch;
using Akka.Routing;

using AutoMapper.Configuration;

using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Samurai.Integration.APIClient.Bling.Models.Results;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Bling;
using Samurai.Integration.Domain.Messages.SellerCenter.OrderActor;
using Samurai.Integration.Domain.Queues;
using Samurai.Integration.Domain.Results.Logger;
using Samurai.Integration.EntityFramework.Repositories;

namespace Samurai.Integration.Application.Actors.Bling.SellerCenter
{
    public class SellerCenterBlingTenantActor : BaseBlingTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _webJobCancellationToken;
        private CancellationTokenSource _taskCancellationTokenSource;
        private int _maxDeliveryCount = 3;

        #region Actors
        private List<IActorRef> _apiActors;
        private IActorRef _apiActorGroup;
        private IActorRef _productActor;
        private IActorRef _orderActor;
        #endregion

        #region QueueClients
        private QueueClient _listFullProductQueue;
        private QueueClient _listAllProductsQueue;
        private QueueClient _listOrderQueue;
        private QueueClient _createOrderQueue;
        #endregion

        #region Tasks
        private Task _listUpdatedProductsTask;
        #endregion

        public SellerCenterBlingTenantActor(IServiceProvider serviceProvider, CancellationToken cancellationToken)
         : base("SellerCenterBlingTenantActor")
        {
            _serviceProvider = serviceProvider;
            _webJobCancellationToken = cancellationToken;

            ReceiveAsync((Func<InitializeBlingTenantMessage, Task>)(async message =>
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
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync<UpdateBlingTenantMessage>(async message =>
            {
                try
                {
                    //if there are any changes, stop and restart//
                    if (_blingData.EqualsTo(message.Data) == false)
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
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            });

            ReceiveAsync((Func<StopBlingTenantMessage, Task>)(async message =>
            {
                try
                {
                    await Stop();
                    Context.Stop(Self);
                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));
        }

        private async Task Stop()
        {
            if (_blingData != null && _taskCancellationTokenSource != null)
            {
                #region Tasks

                _taskCancellationTokenSource.Cancel();

                if (_blingData.ProductIntegrationStatus == true)
                {
                    if (_listUpdatedProductsTask != null)
                        await _listUpdatedProductsTask;

                    _listUpdatedProductsTask = null;
                }

                #endregion

                #region QueueClients

                if (_blingData.ProductIntegrationStatus == true)
                {
                    if (_listFullProductQueue != null && !_listFullProductQueue.IsClosedOrClosing)
                        await _listFullProductQueue.CloseAsync();
                    _listFullProductQueue = null;

                    if (_listAllProductsQueue != null && !_listAllProductsQueue.IsClosedOrClosing)
                        await _listAllProductsQueue.CloseAsync();
                    _listAllProductsQueue = null;
                }

                if (_blingData.OrderIntegrationStatus == true)
                {
                    if (_listOrderQueue != null && !_listOrderQueue.IsClosedOrClosing)
                        await _listOrderQueue.CloseAsync();

                    if (_createOrderQueue != null && !_createOrderQueue.IsClosedOrClosing)
                        await _createOrderQueue.CloseAsync();

                    _listOrderQueue = null;
                    _createOrderQueue = null;
                }

                #endregion

                #region Actors

                if (_blingData.ProductIntegrationStatus == true)
                {
                    if (_productActor != null)
                        await _productActor.GracefulStop(TimeSpan.FromSeconds(30));
                    _productActor = null;
                }

                if (_blingData.OrderIntegrationStatus == true)
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

        private void Initialize(BlingData data, IServiceScope scope)
        {
            var maxConcurrency = 1;
            _blingData = data;

            var _tenantService = scope.ServiceProvider.GetService<TenantService>();
            var _configuration = scope.ServiceProvider.GetService<IConfiguration>();

            _taskCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_webJobCancellationToken);

            #region Actors

            _apiActorGroup = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                    .Props(BlingApiActor.Props(_serviceProvider, _webJobCancellationToken, _blingData)));

            if (_blingData.ProductIntegrationStatus == true)
            {
                _productActor = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                                    .Props(SellerCenterBlingProductActor.Props(_serviceProvider, _webJobCancellationToken, _blingData, _apiActorGroup)));
            }

            if (_blingData.OrderIntegrationStatus == true)
            {
                _orderActor = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                                .Props(SellerCenterBlingOrderActor.Props(_serviceProvider, _webJobCancellationToken, _blingData, _apiActorGroup)));
            }

            #endregion

            #region QueueClients

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandlerAsync)
            {
                MaxConcurrentCalls = maxConcurrency,
                AutoComplete = false
            };

            if (_blingData.ProductIntegrationStatus == true)
            {
                _listFullProductQueue = _tenantService.GetQueueClient(_blingData, BlingQueue.ListFullProductQueue);
                _listFullProductQueue.RegisterMessageHandler(ProcessBlingListProductMessageAsync, messageHandlerOptions);

                _listAllProductsQueue = _tenantService.GetQueueClient(_blingData, BlingQueue.ListAllProductsQueue);
                _listAllProductsQueue.RegisterMessageHandler(ProcessBlingListAllProductsMessageAsync, new MessageHandlerOptions(ExceptionReceivedHandlerAsync)
                {
                    MaxConcurrentCalls = maxConcurrency,
                    MaxAutoRenewDuration = TimeSpan.FromMinutes(20),
                    AutoComplete = false
                });
            }

            if (_blingData.OrderIntegrationStatus == true)
            {
                _listOrderQueue = _tenantService.GetQueueClient(_blingData, BlingQueue.ListOrderQueue);
                _listOrderQueue.RegisterMessageHandler(ListOrderMessageAsync, messageHandlerOptions);

                _createOrderQueue = _tenantService.GetQueueClient(_blingData, BlingQueue.CreateOrderQueue);
                _createOrderQueue.RegisterMessageHandler(CreateOrderMessageAsync, messageHandlerOptions);
            }

            #endregion

            #region Tasks

            if (_blingData.ProductIntegrationStatus == true)
            {
                _listUpdatedProductsTask = ProcessListUpdatedProductsTaskAsync();
            }

            #endregion
        }

        private async Task ProcessBlingListProductMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<BlingListProductMessage>();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                    await _listFullProductQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _listFullProductQueue, "ProcessBlingListProductMessageAsync", "product", messageValue, result);
            }
        }

        private async Task ProcessBlingListAllProductsMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<BlingListAllProductsMessage>();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                    await _listAllProductsQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _listAllProductsQueue, "ProcessBlingListAllProductsMessageAsync", "product", messageValue, result);
            }
        }

        private async Task ListOrderMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<BlingListOrderMessage>();

                var result = await _orderActor.Ask<ReturnMessage<BlingApiOrderResult>>(messageValue, _webJobCancellationToken);
               
                if (result.Result == Result.OK)
                    await _listOrderQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _listOrderQueue, "ListOrderMessageAsync", "order", messageValue, result);
            }
        }

        private async Task CreateOrderMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<ProcessOrderMessage>();

                var result = await _orderActor.Ask<ReturnMessage<BlingApiOrderResult>>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                    await _createOrderQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _createOrderQueue, "CreateOrderMessageAsync", "order", messageValue, result);
            }
        }

        private Task ExceptionReceivedHandlerAsync(ExceptionReceivedEventArgs arg)
        {
            LogError(arg.Exception, "Error in Bling Queue");
            return Task.CompletedTask;
        }

        private async Task ProcessListUpdatedProductsTaskAsync()
        {
            var tenantRepository = _serviceProvider.GetService<TenantRepository>();
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");

            var tenant = await tenantRepository.GetById(_blingData.Id, _webJobCancellationToken);

            while (!_taskCancellationTokenSource.Token.IsCancellationRequested)
            {
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cstZone);
                var lastProcessedDate = tenant.BlingData.LastProductUpdateDate ?? now.Date.AddDays(-2);

                if ((now - lastProcessedDate).TotalSeconds >= TimeSpan.FromSeconds(30).TotalSeconds)
                {
                    var result = await _productActor.Ask<ReturnMessage>(new BlingListAllProductsMessage { ProductUpdatedDate = lastProcessedDate }, _webJobCancellationToken);

                    if (result.Result == Result.OK)
                    {
                        tenant.BlingData.LastProductUpdateDate = now;
                        await tenantRepository.CommitAsync(_webJobCancellationToken);
                    }
                }
            }

        }

        private async Task AbandonMessageAsync(Message message, QueueClient queue, string method = "", string type = "", object request = null, object response = null, bool critical = false)
        {
            try
            {
                if (message.SystemProperties.DeliveryCount >= _maxDeliveryCount)
                {
                    var logId = Guid.NewGuid();

                    var log = LoggerDescription.From(_blingData.Id.ToString(), type, method, request, response, logId);

                    if (critical)
                        LogError(log);
                    else
                        LogWarning(log);

                    await queue.DeadLetterAsync(message.SystemProperties.LockToken, new Dictionary<string, object> { { "LogId", logId } }).ConfigureAwait(false);
                }
                else
                {
                    await queue.AbandonAsync(message.SystemProperties.LockToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
               LogError(ex, $"Exception when abandon a event message from {message.SystemProperties.LockToken} of Azure Service Bus.");
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

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            return Akka.Actor.Props.Create(() => new SellerCenterBlingTenantActor(serviceProvider, cancellationToken));
        }
    }
}
