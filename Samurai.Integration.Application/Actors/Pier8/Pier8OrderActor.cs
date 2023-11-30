using Akka.Actor;
using Akka.Dispatch;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Extensions;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Pier8;
using Samurai.Integration.Domain.Messages.Pier8.OrderActor;
using Samurai.Integration.Domain.Queues;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace Samurai.Integration.Application.Actors.Pier8
{
    public class Pier8OrderActor : BasePier8TenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly IDictionary<EnumActorType, IActorRef> _apiActorGroup;
        private readonly Pier8Queue.Queues _queues;
        private QueueClient _updateTrackingOrderQueue;
        private QueueClient _updateOrderTagNumberQueue;



        public Pier8OrderActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, Pier8DataMessage pier8DataMessage, IDictionary<EnumActorType, IActorRef> apiActorGroup)
            : base("Pier8OrderActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _pier8DataMessage = pier8DataMessage;
            _apiActorGroup = apiActorGroup;

            using (var scope = _serviceProvider.CreateScope())
            {
                var tenantService = scope.ServiceProvider.GetService<TenantService>();

                _queues = new Pier8Queue.Queues {

                    WebHookProcessQueue = tenantService.GetQueueClient(_pier8DataMessage, Pier8Queue.WebHookProcessQueue),
                    ProcessUpdateTrackingQueue = tenantService.GetQueueClient(_pier8DataMessage, Pier8Queue.ProcessUpdateTrackingQueue)

                };

                _updateTrackingOrderQueue = tenantService.GetQueueClient(_pier8DataMessage, ShopifyQueue.UpdateTrackingOrderQueue);
                _updateOrderTagNumberQueue = tenantService.GetQueueClient(_pier8DataMessage, ShopifyQueue.UpdateOrderNumberTagQueue);

            }


            ReceiveAsync((Func<WebhookOrderProcessMessage, Task>)(async message => {
                try
                {
                    LogDebug($"Starting WebhookOrderProcessMessage id: {message.idPier}");
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var pier8Service = scope.ServiceProvider.GetService<Pier8Service>();
                        pier8Service.Init(_apiActorGroup, GetLog());
                        result = await pier8Service.WebhookOrderProcess(message, _pier8DataMessage, _queues, _updateTrackingOrderQueue, _updateOrderTagNumberQueue, _cancellationToken);
                    }
                    LogDebug("Ending WebhookOrderProcessMessage");
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {                    
                    LogError(ex, "Pier8OrderActor - Error in WebhookOrderProcessMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));


            ReceiveAsync((Func<Pier8UpdateTrackingMessage, Task>)(async message => {
                try
                {
                    LogDebug($"Starting Pier8UpdateTrackingMessage id: {message.ExternalOrderId ?? message.ExternalRefOrderId}");
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var pier8Service = scope.ServiceProvider.GetService<Pier8Service>();
                        pier8Service.Init(_apiActorGroup, GetLog());
                        result = await pier8Service.ProcessUpdateTracking(message, _pier8DataMessage, _queues, _updateTrackingOrderQueue, _updateOrderTagNumberQueue, _cancellationToken);
                    }
                    LogDebug("Ending Pier8UpdateTrackingMessage");
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Pier8OrderActor - Error in Pier8UpdateTrackingMessage {0}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));



        }

        protected override void PostStop()
        {
            base.PostStop();
            ActorTaskScheduler.RunTask(async () =>
            {
                await _queues.WebHookProcessQueue.CloseAsyncSafe();
            });
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, Pier8DataMessage pier8DataMessage, IDictionary<EnumActorType, IActorRef> apiActorGroup)
        {
            return Akka.Actor.Props.Create(() => new Pier8OrderActor(serviceProvider, cancellationToken, pier8DataMessage, apiActorGroup));
        }

    }
}
