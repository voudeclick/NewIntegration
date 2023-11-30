
using Akka.Actor;
using Akka.Routing;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Samurai.Integration.APIClient.PluggTo.Models.Results;
using Samurai.Integration.Application.Actors.PluggTo.SellerCenter;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Application.Tools;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.PluggTo;
using Samurai.Integration.Domain.Messages.SellerCenter.OrderActor;
using Samurai.Integration.Domain.Queues;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.PluggTo
{
    public class SellerCenterPluggToTenantActor : BasePluggToTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _webJobCancellationToken;


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

        public SellerCenterPluggToTenantActor(IServiceProvider serviceProvider, CancellationToken cancellationToken) : base("SellerCenterPluggToTenantActor")
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

            LogInfo($"Initializing Actors");

            #region Actors

            _apiActorGroup = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                    .Props(PluggToApiActor.Props(_serviceProvider, _webJobCancellationToken, _pluggToData)));

            if (_pluggToData.ProductIntegrationStatus == true)
            {
                _productActor = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                                    .Props(SellerCenterPluggToProductActor.Props(_serviceProvider, _webJobCancellationToken, _pluggToData, _apiActorGroup)));
            }

            if (_pluggToData.OrderIntegrationStatus == true)
            {
                _orderActor = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                                .Props(SellerCenterPluggToOrderActor.Props(_serviceProvider, _webJobCancellationToken, _pluggToData, _apiActorGroup)));
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

                _listFullProductQueue = _tenantService.GetQueueClient(_pluggToData, PluggToQueue.ListFullProductQueue);
                _listFullProductQueue.RegisterMessageHandler(ProcessListProductMessageAsync, messageHandlerOptions);

                _listAllProductsQueue = _tenantService.GetQueueClient(_pluggToData, PluggToQueue.ListAllProductsQueue);
                _listAllProductsQueue.RegisterMessageHandler(ProcessListAllProductsMessageAsync, new MessageHandlerOptions(ExceptionReceivedHandlerAsync)
                {
                    MaxConcurrentCalls = maxConcurrency,
                    MaxAutoRenewDuration = TimeSpan.FromMinutes(20),
                    AutoComplete = false
                });
            }

            if (_pluggToData.OrderIntegrationStatus == true)
            {
                _listOrderQueue = _tenantService.GetQueueClient(_pluggToData, PluggToQueue.ListOrderQueue);
                _listOrderQueue.RegisterMessageHandler(ListOrderMessageAsync, messageHandlerOptions);

                _createOrderQueue = _tenantService.GetQueueClient(_pluggToData, PluggToQueue.CreateOrderQueue);
                _createOrderQueue.RegisterMessageHandler(CreateOrderMessageAsync, messageHandlerOptions);
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
            if (_pluggToData != null)
            {
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

                if (_pluggToData.OrderIntegrationStatus == true)
                {
                    if (_listOrderQueue != null && !_listOrderQueue.IsClosedOrClosing)
                        await _listOrderQueue.CloseAsync();

                    if (_createOrderQueue != null && !_createOrderQueue.IsClosedOrClosing)
                        await _createOrderQueue.CloseAsync();

                    _listOrderQueue = null;
                    _createOrderQueue = null;
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

                if (_pluggToData.OrderIntegrationStatus == true)
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
            }
        }

        private async Task ProcessListProductMessageAsync(Message message, CancellationToken cancellationToken)
        {
            LogInfo($"PluggToProcessListProductMessageAsync Received message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");

            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<PluggToListProductMessage>();

                var result = await _productActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    LogInfo($"PluggToProcessListProductMessageAsync Finished message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listFullProductQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, $"PluggToProcessListProductMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
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

        private async Task ListOrderMessageAsync(Message message, CancellationToken cancellationToken)
        {
            LogInfo($"PluggToListOrderMessageAsync Received message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");

            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<PluggToListOrderMessage>();

                var result = await _orderActor.Ask<ReturnMessage<PluggToApiOrderResult>>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    LogInfo($"PluggToListOrderMessageAsync Finished message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listOrderQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, $"PluggToListOrderMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _listOrderQueue.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }

        private async Task CreateOrderMessageAsync(Message message, CancellationToken cancellationToken)
        {
            LogInfo($"PluggToCreateOrderMessageAsync Received message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<ProcessOrderMessage>();

                var result = await _orderActor.Ask<ReturnMessage<PluggToApiOrderResult>>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    LogInfo($"PluggToCreateOrderMessageAsync Finished message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _createOrderQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, $"PluggToCreateOrderMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}");
                    await _createOrderQueue.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            return Akka.Actor.Props.Create(() => new SellerCenterPluggToTenantActor(serviceProvider, cancellationToken));
        }
    }
}
