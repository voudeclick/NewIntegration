using Akka.Actor;
using Akka.Dispatch;
using Akka.Routing;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Extensions;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Tray;
using Samurai.Integration.Domain.Messages.Tray.OrderActor;
using Samurai.Integration.Domain.Messages.Tray.ProductActor;
using Samurai.Integration.Domain.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.Tray
{
    public class TrayTenantActor : BaseTrayTenantActor
    {
        private readonly int _maximumRetryCount = 1;

        private readonly IServiceProvider _serviceProvider;
        private IConfiguration _configuration;
        private readonly CancellationToken _webJobCancellationToken;
        private CancellationTokenSource _taskCancellationTokenSource;

        #region Actors
        private List<IActorRef> _apiActors;
        private IActorRef _apiActorGroup;
        private IActorRef _productActor;
        private IActorRef _orderActor;
        #endregion

        #region QueueClients
        private TrayQueue.Queues _queues;

        #endregion

        public TrayTenantActor(IServiceProvider serviceProvider, CancellationToken cancellationToken) : base("TrayTenantActor")
        {
            _serviceProvider = serviceProvider;
            _webJobCancellationToken = cancellationToken;

            ReceiveAsync((Func<InitializeTrayTenantMessage, Task>)(async message =>
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

            ReceiveAsync<UpdateTrayTenantMessage>(async message =>
            {
                try
                {
                    //if there are any changes, stop and restart//
                    if (_tenantData.EqualsTo(message.Data) == false)
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

            ReceiveAsync((Func<StopTrayTenantMessage, Task>)(async message =>
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
            if (_tenantData != null && _taskCancellationTokenSource != null)
            {
                #region Tasks

                _taskCancellationTokenSource.Cancel();
                #endregion

                #region QueueClients

                //if (_tenantData.ProductIntegrationStatus == true)
                //{
                //    await _queues.ProcessProductQueue.CloseAsyncSafe();
                //}

                if (_tenantData.OrderIntegrationStatus == true)
                {
                    await _queues.UpdateStatusOrderQueue.CloseAsyncSafe();
                }
                #endregion

                #region Actors

                //if (_tenantData.ProductIntegrationStatus == true)
                //{
                //    if (_productActor != null)
                //        await _productActor.GracefulStop(TimeSpan.FromSeconds(30));

                //    _productActor = null;
                //}

                if (_tenantData.OrderIntegrationStatus == true)
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

        private void Initialize(TenantDataMessage data, IServiceScope scope)
        {
            _tenantData = data;

            var maxConcurrency = 10;
#if DEBUG
            maxConcurrency = 1;
#endif

            var _tenantService = scope.ServiceProvider.GetService<TenantService>();
            _configuration = scope.ServiceProvider.GetService<IConfiguration>();
            _taskCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_webJobCancellationToken);

            #region Actors

            _apiActorGroup = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                    .Props(TrayApiActor.Props(_serviceProvider, _webJobCancellationToken, _tenantData)));

            //if (_tenantData.ProductIntegrationStatus == true)
            //{
            //    _productActor = Context.ActorOf(new RoundRobinPool(maxConcurrency)
            //                                        .Props(TrayProductActor.Props(_serviceProvider, _webJobCancellationToken, _tenantData, _apiActorGroup))
            //                                        .WithRouter(new ConsistentHashingPool(10)));
            //}

            if (_tenantData.OrderIntegrationStatus == true)
            {
                _orderActor = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                                .Props(TrayOrderActor.Props(_serviceProvider, _webJobCancellationToken, _tenantData, _apiActorGroup)));
            }

            #endregion

            #region QueueClients

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandlerAsync)
            {
                MaxConcurrentCalls = maxConcurrency,
                AutoComplete = false
            };

  //          if (_tenantData.ProductIntegrationStatus == true)
  //          {
  //              _queues.ProcessProductQueue = _tenantService.GetQueueClient(_tenantData, TrayQueue.ProcessProductQueue, false);
  //              _queues.ProcessProductQueue.RegisterMessageHandler(ProcessProductMessageAsync, messageHandlerOptions);
  //}

            if (_tenantData.OrderIntegrationStatus == true)
            {
                _queues.UpdateStatusOrderQueue = _tenantService.GetQueueClient(_tenantData, TrayQueue.UpdateStatusOrderQueue, false);
                _queues.UpdateStatusOrderQueue.RegisterMessageHandler(UpdateStatusOrderMessageAsync, messageHandlerOptions);
            }

            #endregion
        }

        //private async Task ProcessProductMessageAsync(Message message, CancellationToken cancellationToken)
        //{
        //    if (!_webJobCancellationToken.IsCancellationRequested)
        //    {
        //        var messageValue = new ServiceBusMessage(message.Body, message.UserProperties.Any(x => x.Key == "Compressed")).GetValue<TrayProcessProductMessage>();

        //        var envelope = new ConsistentHashableEnvelope(messageValue, $"{messageValue.StoreId}_{messageValue.Product.AppTrayProductId}");

        //        var result = await _productActor.Ask<ReturnMessage>(envelope, _webJobCancellationToken);

        //        if (result.Result == Result.OK)
        //            await _queues.ProcessProductQueue.CompleteAsync(message.SystemProperties.LockToken);
        //        else
        //            await AbandonMessageAsync(message, _queues.ProcessProductQueue, "ProcessProductMessageAsync", "product", messageValue, result, true);
        //    }
        //}

        private async Task UpdateStatusOrderMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body, message.UserProperties.Any(x => x.Key == "Compressed")).GetValue<TrayUpdateOrderStatusMessage>();

                var result = await _orderActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                    await _queues.UpdateStatusOrderQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _queues.UpdateStatusOrderQueue, "UpdateStatusOrderMessageAsync", "order", messageValue, result, true);
            }
        }

        private Task ExceptionReceivedHandlerAsync(ExceptionReceivedEventArgs arg)
        {
            try
            {
                if (arg.Exception?.InnerException.GetType().FullName != "System.Net.Sockets.SocketException")
                    LogError(arg.Exception, "Error in Tray Queue");
            }
            catch 
            {
            }
        
            return Task.CompletedTask;
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
                    _log.Warning("AbandonMessageAsync | Contagem máxima de tentativas", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, request));
                    await queue.DeadLetterAsync(message.SystemProperties.LockToken).ConfigureAwait(false);

                    return;
                }

                await queue.AbandonAsync(message.SystemProperties.LockToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"Exception when abandon a event message from {message.SystemProperties.LockToken} of Azure Service Bus.", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, request), ex);
            }
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken)
            => Akka.Actor.Props.Create(() => new TrayTenantActor(serviceProvider, cancellationToken));
    }
}
