
using Akka.Actor;
using Akka.Routing;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.Application.Actors.PluggTo.Shopify;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.PluggTo;
using Samurai.Integration.Domain.Queues;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.PluggTo
{
    public class ShopifyPluggToTenantActor : BasePluggToTenantActor
    {
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
        private QueueClient _listAllProductsQueue;
        private QueueClient _listFullProductQueue;
        #endregion

        public ShopifyPluggToTenantActor(IServiceProvider serviceProvider, CancellationToken cancellationToken) : base("ShopifyPluggToTenantActor")
        {
            _serviceProvider = serviceProvider;
            _webJobCancellationToken = cancellationToken;

            ReceiveAsync((Func<InitializePluggToTenantMessage, Task>)(async message =>
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
                    LogError(ex, "Error in InitializePluggToTenantMessage");

                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));


            ReceiveAsync<UpdatePluggToTenantMessage>(async message =>
            {
                try
                {
                    //if there are any changes, stop and restart//
                    if (_pluggToData.EqualsTo(message.Data) == false)
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
                    LogError(ex, "Error in UpdatePluggToTenantMessage");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            });

            ReceiveAsync((Func<StopPluggToTenantMessage, Task>)(async message =>
            {
                try
                {
                    await Stop();
                    Context.Stop(Self);
                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in StopPluggToTenantMessage");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));
        }

        private void Initialize(PluggToData data, IServiceScope scope)
        {
            var maxConcurrency = 1;
            _pluggToData = data;

            LogInfo($"Initializing");

            var _tenantService = scope.ServiceProvider.GetService<TenantService>();
            var _configuration = scope.ServiceProvider.GetService<IConfiguration>();

            _taskCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_webJobCancellationToken);

            LogInfo($"Initializing Actors");

            #region Actors

            _apiActorGroup = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                    .Props(PluggToApiActor.Props(_serviceProvider, _webJobCancellationToken, _pluggToData)));

            if (_pluggToData.ProductIntegrationStatus == true)
            {
                _productActor = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                                    .Props(ShopifyPluggToProductActor.Props(_serviceProvider, _webJobCancellationToken, _pluggToData, _apiActorGroup)));
            }

            if (_pluggToData.OrderIntegrationStatus == true)
            {
                _orderActor = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                                .Props(ShopifyPluggToOrderActor.Props(_serviceProvider, _webJobCancellationToken, _pluggToData, _apiActorGroup)));
            }

            #endregion

            LogInfo($"Initializing QueueClients");
            #region QueueClients

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandlerAsync)
            {
                MaxConcurrentCalls = maxConcurrency,
                AutoComplete = false
            };

            if (_pluggToData.ProductIntegrationStatus == true)
            {
                _listFullProductQueue = _tenantService.GetQueueClient(_pluggToData, BlingQueue.ListFullProductQueue);
                _listFullProductQueue.RegisterMessageHandler(ProcessProductMessageAsync, messageHandlerOptions);

                _listAllProductsQueue = _tenantService.GetQueueClient(_pluggToData, PluggToQueue.ListAllProductsQueue);
                _listAllProductsQueue.RegisterMessageHandler(ProcessListAllProductsMessageAsync, new MessageHandlerOptions(ExceptionReceivedHandlerAsync)
                {
                    MaxConcurrentCalls = maxConcurrency,
                    MaxAutoRenewDuration = TimeSpan.FromMinutes(20),
                    AutoComplete = false
                });
            }

            #endregion

            LogInfo($"Initializing Tasks");
            #region Tasks

            if (_pluggToData.ProductIntegrationStatus == true)
            {
                //_listUpdatedProductsTask = ProcessListUpdatedProductsTaskAsync();
            }

            #endregion
        }
        private Task ExceptionReceivedHandlerAsync(ExceptionReceivedEventArgs arg)
        {
            LogError(arg.Exception, "Error in PluggTo Queue");

            return Task.CompletedTask;
        }

        private async Task Stop()
        {
            if (_pluggToData != null && _taskCancellationTokenSource != null)
            {
                LogInfo($"Stoping Tasks");

                #region Tasks

                _taskCancellationTokenSource.Cancel();

                if (_pluggToData.ProductIntegrationStatus == true)
                {
                    //if (_listUpdatedProductsTask != null)
                    //    await _listUpdatedProductsTask;

                    //_listUpdatedProductsTask = null;
                }

                #endregion

                LogInfo($"Stoping QueueClients");

                #region QueueClients

                if (_pluggToData.ProductIntegrationStatus == true)
                {
                    if (_listFullProductQueue != null && !_listFullProductQueue.IsClosedOrClosing)
                        await _listFullProductQueue.CloseAsync();

                    _listFullProductQueue = null;

                    if (_listAllProductsQueue != null && !_listAllProductsQueue.IsClosedOrClosing)
                        await _listAllProductsQueue.CloseAsync();

                    _listAllProductsQueue = null;
                }

                #endregion

                LogInfo($"Stoping Actors");

                #region Actors

                if (_pluggToData.ProductIntegrationStatus == true)
                {
                    if (_productActor != null)
                        await _productActor.GracefulStop(TimeSpan.FromSeconds(30));
                    _productActor = null;
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


        private async Task ProcessProductMessageAsync(Message message, CancellationToken cancellationToken)
        {
            LogInfo($"PluggToProcessProductMessageAsync Received message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<PluggToListProductMessage>();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    LogInfo($"PluggToProcessProductMessageAsync Finished message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listFullProductQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, $"PluggToProcessProductMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listFullProductQueue.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }

        private async Task ProcessListAllProductsMessageAsync(Message message, CancellationToken cancellationToken)
        {
            LogInfo($"PluggToProcessListAllProductsMessageAsync Received message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");

            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<PluggToListAllProductsMessage>();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    LogInfo($"PluggToProcessListAllProductsMessageAsync Finished message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listAllProductsQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, $"PluggToProcessListAllProductsMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listAllProductsQueue.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            return Akka.Actor.Props.Create(() => new ShopifyPluggToTenantActor(serviceProvider, cancellationToken));
        }
    }
}
