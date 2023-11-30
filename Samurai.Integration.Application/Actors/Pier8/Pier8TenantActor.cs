using Akka.Actor;
using Akka.Dispatch;
using Akka.Routing;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.APIClient.Pier8.Models;
using Samurai.Integration.APIClient.Shopify.Models.Request;
using Samurai.Integration.Application.Actors.Omie;
using Samurai.Integration.Application.Actors.Shopify;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Extensions;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Pier8;
using Samurai.Integration.Domain.Messages.Pier8.OrderActor;
using Samurai.Integration.Domain.Queues;
using Samurai.Integration.Domain.Results.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.Pier8
{
    public class Pier8TenantActor : BasePier8TenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _webJobCancellationToken;
        private CancellationTokenSource _taskCancellationTokenSource;

        #region Actors
        private IDictionary<EnumActorType, IActorRef> _apiActors = new Dictionary<EnumActorType, IActorRef>();
        private IActorRef _apiActorGroup;
        private IActorRef _orderActor;
        #endregion

        #region QueueClients
        private Pier8Queue.Queues _queues;
        private Task _listNewOrdersTask;
        private IConfiguration _configuration;

        #endregion

        public Pier8TenantActor(IServiceProvider serviceProvider, CancellationToken cancellationToken)
            : base("Pier8TenantActor")
        {
            _serviceProvider = serviceProvider;
            _webJobCancellationToken = cancellationToken;
                        
            ReceiveAsync((Func<InitializePier8TenantMessage, Task>)(async message =>
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
                    LogError(ex, "Pier8OrderActor - Error in InitializePier8TenantMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync<UpdatePier8TenantMessage>(async message =>
            {
                try
                {
                    //if there are any changes, stop and restart//
                    if (_pier8DataMessage.EqualsTo(message.Data) == false)
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
                    LogError(ex, "Pier8OrderActor - Error in UpdatePier8TenantMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            });

            ReceiveAsync((Func<StopPier8TenantMessage, Task>)(async message =>
            {
                try
                {
                    await Stop();
                    Context.Stop(Self);
                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    LogError(ex, "Pier8OrderActor - Error in StopPier8TenantMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));
        }

        private async Task Stop()
        {
            if (_pier8DataMessage != null && _taskCancellationTokenSource != null)
            {
                #region Tasks
                LogInfo($"Stoping Tasks");

                _taskCancellationTokenSource.Cancel();

                if (_pier8DataMessage.EnableIntegration == true)
                {
                    await _listNewOrdersTask.CloseAsyncSafe();
                }

                #endregion

                #region QueueClients
                LogInfo($"Stoping QueueClients");


                if (_pier8DataMessage.EnableIntegration == true)
                {
                    await _queues.WebHookProcessQueue.CloseAsyncSafe();
                }

                #endregion

                #region Actors
                LogInfo($"Stoping Actors");

                if (_pier8DataMessage.EnableIntegration == true)
                {
                    if (_orderActor != null)
                        await _orderActor.GracefulStop(TimeSpan.FromSeconds(30));
                    _orderActor = null;
                }

                if (_apiActors != null)
                {
                    foreach (var apiActor in _apiActors)
                    {
                        await apiActor.Value.GracefulStop(TimeSpan.FromSeconds(30));
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

        private void Initialize(Pier8DataMessage data, IServiceScope scope)
        {
            _pier8DataMessage = data;
            var maxConcurrency = 1;
            var _tenantService = scope.ServiceProvider.GetService<TenantService>();
            _configuration = scope.ServiceProvider.GetService<IConfiguration>();
            var url = _configuration.GetSection("Pier8.Url").Value;
            _taskCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_webJobCancellationToken);

            #region Actors


            _apiActors.Add(EnumActorType.Pier8, Context.ActorOf(Pier8ApiActor.Props(_serviceProvider, _webJobCancellationToken, _pier8DataMessage,
                                    url, new Credentials { ApiKey = data.ApiKey, Token = data.Token }), nameof(EnumActorType.Pier8)));

            var apiActorShopify = new List<IActorRef>();
            foreach (var app in _pier8DataMessage.ShopifyDataMessage.ShopifyApps)
            {
                apiActorShopify.Add(Context.ActorOf(ShopifyApiActor.Props(_serviceProvider, _webJobCancellationToken, _pier8DataMessage.ShopifyDataMessage, app), nameof(EnumActorType.Shopify)));
            }

            _apiActors.Add(EnumActorType.Shopify, Context.ActorOf(Akka.Actor.Props.Empty.WithRouter(new RoundRobinGroup(apiActorShopify))));

            _apiActors.Add(EnumActorType.Omie, Context.ActorOf(OmieApiActor.Props(_serviceProvider, _webJobCancellationToken, _pier8DataMessage.OmieData), nameof(EnumActorType.Omie)));


            if (_pier8DataMessage.EnableIntegration)
            {
                _orderActor = Context.ActorOf(new RoundRobinPool(maxConcurrency)
                                                    .Props(Pier8OrderActor.Props(_serviceProvider, _webJobCancellationToken, _pier8DataMessage, _apiActors)));
            }


            #endregion

            #region QueueClients

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandlerAsync)
            {
                MaxConcurrentCalls = maxConcurrency,
                AutoComplete = false
            };

            if (_pier8DataMessage.EnableIntegration == true)
            {
                _queues.WebHookProcessQueue = _tenantService.GetQueueClient(_pier8DataMessage, Pier8Queue.WebHookProcessQueue);
                _queues.WebHookProcessQueue.RegisterMessageHandler(WebHookProcessMessageAsync, messageHandlerOptions);

                _queues.ProcessUpdateTrackingQueue = _tenantService.GetQueueClient(_pier8DataMessage, Pier8Queue.ProcessUpdateTrackingQueue);
                _queues.ProcessUpdateTrackingQueue.RegisterMessageHandler(ProcessUpdateTrackingMessageAsync, messageHandlerOptions);
            }

            _listNewOrdersTask = ListNewOrdersTask(_tenantService, _pier8DataMessage, int.Parse(_configuration.GetSection("Schedulle")["MaxOrderSleep"]));


            #endregion

        }
        #region Task
        private async Task ListNewOrdersTask(TenantService tenantService, Pier8DataMessage pier8Data, int maxSleep)
        {
            while (!_taskCancellationTokenSource.Token.IsCancellationRequested)
            {
                var result = await ListRecentOrders(tenantService, pier8Data);

                if (result.Result == Result.Error)
                    LogError(result.Error, "Pier8OrderActor - Error in ListNewOrdersTaskAsync", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), tenantService, pier8Data));

                await Task.Delay(maxSleep, _taskCancellationTokenSource.Token).ContinueWith(tsk => { }); //ignore exception
            }

            LogInfo("terminating ListNewOrdersTaskAsync");
        }
        #endregion
        public async Task<ReturnMessage> ListRecentOrders(TenantService tenantService, Pier8DataMessage pier8Data)
        {
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            var beginDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow.AddDays(-7), cstZone);
            var endDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cstZone);

            var queryByDateResult = await _apiActors.Route(EnumActorType.Shopify).Ask<ReturnMessage<OrderByDateQueryOutput>>(
                new OrderByDateAndStatusQuery(beginDate, endDate), _webJobCancellationToken
            );

            if (queryByDateResult.Result == Result.Error)
                return new ReturnMessage { Result = Result.Error, Error = queryByDateResult.Error };                      

            var data = queryByDateResult.Data;

            while (queryByDateResult.Data.orders.pageInfo.hasNextPage == true)
            {
                queryByDateResult = await _apiActors.Route(EnumActorType.Shopify).Ask<ReturnMessage<OrderByDateQueryOutput>>(
                    new OrderByDateAndStatusQuery(beginDate, endDate, queryByDateResult.Data.orders.edges.Last().cursor), _webJobCancellationToken
                );

                if (queryByDateResult.Result == Result.Error)
                    return new ReturnMessage { Result = Result.Error, Error = queryByDateResult.Error };

                data.orders.edges.AddRange(queryByDateResult.Data.orders.edges);
            }

            var queue = tenantService.GetQueueClient(pier8Data, Pier8Queue.ProcessUpdateTrackingQueue);

            foreach (var edge in data.orders.edges)
            {
                var serviceBusMessage = new ServiceBusMessage(new Pier8UpdateTrackingMessage { ExternalOrderId = edge.node.legacyResourceId });
                await queue.SendAsync(serviceBusMessage.GetMessage(edge.node.legacyResourceId));
            }

            return new ReturnMessage { Result = Result.OK };
        }
        private async Task WebHookProcessMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<WebhookOrderProcessMessage>();

                var result = await _orderActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    await _queues.WebHookProcessQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, "Pier8OrderActor - WebHookProcessMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}",
                        LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null,
                        $"WebHookProcessMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}"));
                    await AbandonMessageAsync(message, _queues.WebHookProcessQueue, "WebHookProcessMessageAsync", "webhook", messageValue, result);
                }
            }
        }

        private async Task ProcessUpdateTrackingMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (!_webJobCancellationToken.IsCancellationRequested)
            {
                var messageValue = new ServiceBusMessage(message.Body).GetValue<Pier8UpdateTrackingMessage>();

                var result = await _orderActor.Ask<ReturnMessage>(messageValue, _webJobCancellationToken);

                if (result.Result == Result.OK)
                {
                    await _queues.ProcessUpdateTrackingQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    LogError(result.Error, "Pier8OrderActor - ProcessUpdateTrackingMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}",
                        LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null,
                        $"ProcessUpdateTrackingMessageAsync error during message: {message.MessageId}, SequenceNumber:{message.SystemProperties.SequenceNumber}"));
                    await AbandonMessageAsync(message, _queues.ProcessUpdateTrackingQueue, "ProcessUpdateTrackingMessageAsync", "processUpdate", messageValue, result);
                }
            }
        }
        private async Task AbandonMessageAsync(Message message, QueueClient queue, string method = "", string type = "", object request = null, object response = null)
        {
            try
            {
                if (message.SystemProperties.DeliveryCount >= 3)
                {
                    var logId = Guid.NewGuid();

                    var log = LoggerDescription.From(_pier8DataMessage.Id.ToString(), type, method, request, response, logId);

                    LogError(log, "PierTetantActor - Error in AbandonMessageAsync  |  {0}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));

                    await queue.DeadLetterAsync(message.SystemProperties.LockToken, new Dictionary<string, object> { { "LogId", logId } }).ConfigureAwait(false);
                }
                else
                {
                    await queue.AbandonAsync(message.SystemProperties.LockToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, $"Exception when abandon a event message from {message.SystemProperties.LockToken} of Azure Service Bus.",
                    LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
            }
        }


        private async Task ExceptionReceivedHandlerAsync(ExceptionReceivedEventArgs arg)
        {
            LogError($"Pier8OrderActor - Received exception in SellerCenter Queue, action: {arg.ExceptionReceivedContext.Action}, message: {arg.Exception.Message}.");
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
            => Akka.Actor.Props.Create(() => new Pier8TenantActor(serviceProvider, cancellationToken));
    }
}
