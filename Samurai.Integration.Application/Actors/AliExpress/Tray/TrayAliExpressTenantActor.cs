using Akka.Actor;
using Akka.Routing;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.AliExpress;
using Samurai.Integration.Domain.Messages.AliExpress.Order;
using Samurai.Integration.Domain.Queues;
using Samurai.Integration.Domain.Results.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.AliExpress.Tray
{
    public class TrayAliExpressTenantActor : BaseAliExpressTenantActor
    {
        private readonly int _maximumRetryCount = 3;

        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _webJobCancellationToken;


        #region Actors
        private List<IActorRef> _apiActors;
        private IActorRef _apiActorGroup;
        private IActorRef _orderActor;
        #endregion

        #region QueueClients
        private QueueClient _listOrderQueue;
        #endregion

        public TrayAliExpressTenantActor(IServiceProvider serviceProvider, CancellationToken cancellationToken) : base("TrayAliExpressTenantActor")
        {
            _serviceProvider = serviceProvider;
            _webJobCancellationToken = cancellationToken;

            ReceiveAsync((Func<InitializeAliExpressTenantMessage, Task>)(async message =>
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


            ReceiveAsync<UpdateAliExpressTenantMessage>(async message =>
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

            ReceiveAsync((Func<StopAliExpressTenantMessage, Task>)(async message =>
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

        private void Initialize(TenantDataMessage data, IServiceScope scope)
        {
            var maxConcurrency = 1;
//#if DEBUG
//            maxConcurrency = 1;
//#endif
            _tenantData = data;

            var _tenantService = scope.ServiceProvider.GetService<TenantService>();
            var _configuration = scope.ServiceProvider.GetService<IConfiguration>();

            #region Actors

            _apiActorGroup = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                    .Props(AliExpressApiActor.Props(_serviceProvider, _webJobCancellationToken, _tenantData)));

            if (_tenantData.OrderIntegrationStatus == true)
            {
                _orderActor = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                                .Props(TrayAliExpressOrderActor.Props(_serviceProvider, _webJobCancellationToken, _tenantData, _apiActorGroup)));
            }

            #endregion

            #region QueueClients

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandlerAsync)
            {
                MaxConcurrentCalls = maxConcurrency,
                AutoComplete = false
            };
            if (_tenantData.OrderIntegrationStatus == true)
            {

                _listOrderQueue = _tenantService.GetQueueClient(_tenantData, AliExpressQueue.ListOrderQueue, false);
                _listOrderQueue.RegisterMessageHandler(GetOrderMessageAsync, messageHandlerOptions);
            }

            #endregion
        }
        private Task ExceptionReceivedHandlerAsync(ExceptionReceivedEventArgs arg)
        {
            try
            {
                if (arg.Exception?.InnerException.GetType().FullName != "System.Net.Sockets.SocketException")
                    LogError(arg.Exception, "Error in AliExpress Queue");
            }
            catch
            {
            }

            return Task.CompletedTask;
        }

        private async Task Stop()
        {
            if (_tenantData != null)
            {
                #region QueueClients


                if (_tenantData.OrderIntegrationStatus == true)
                {
                    if (_listOrderQueue != null && !_listOrderQueue.IsClosedOrClosing)
                        await _listOrderQueue.CloseAsync();

                    _listOrderQueue = null;
                }

                #endregion

                #region Actors

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
            }
        }
        private async Task GetOrderMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body, message.UserProperties.Any(x => x.Key == "Compressed")).GetValue<AliExpressGetOrderMessage>();

                var result = await _orderActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                    await _listOrderQueue.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await AbandonMessageAsync(message, _listOrderQueue, "GetOrderMessageAsync", "order", messageValue, result, false);
            }
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
        {
            return Akka.Actor.Props.Create(() => new TrayAliExpressTenantActor(serviceProvider, cancellationToken));
        }
    }
}
