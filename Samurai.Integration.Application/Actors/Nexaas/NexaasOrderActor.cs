using Akka.Actor;
using Akka.Dispatch;
using Akka.Event;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Nexaas;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Queues;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.Nexaas
{
    public class NexaasOrderActor : BaseNexaasTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly IActorRef _apiActorGroup;
        private readonly QueueClient _shopifyUpdateOrderStatusQueueClient;

        public NexaasOrderActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, NexaasData nexaasData, IActorRef apiActorGroup)
            : base("NexaasOrderActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _nexaasData = nexaasData;
            _apiActorGroup = apiActorGroup;


            using (var scope = _serviceProvider.CreateScope())
            {
                var tenantService = scope.ServiceProvider.GetService<TenantService>();
                _shopifyUpdateOrderStatusQueueClient = tenantService.GetQueueClient(_nexaasData, ShopifyQueue.UpdateOrderStatusQueue);
            }

            ReceiveAsync((Func<NexaasListOrderMessage, Task>)(async message => {
                try
                {
                    LogDebug($"Starting NexaasListOrderMessage id: {message.NexaasOrderId?.ToString() ?? message.ExternalOrderId}");
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var nexaasService = scope.ServiceProvider.GetService<NexaasService>();
                        nexaasService.Init(_apiActorGroup, GetLog());
                        result = await nexaasService.ListOrder(message, _nexaasData, _shopifyUpdateOrderStatusQueueClient, _cancellationToken);
                    }
                    LogDebug("Ending NexaasListOrderMessage");
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in NexaasListOrderMessage");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ShopifySendOrderToERPMessage, Task>)(async message => {
                try
                {
                    LogDebug($"Starting NexaasSendOrderMessage id: {message.ID}");
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var nexaasService = scope.ServiceProvider.GetService<NexaasService>();
                        nexaasService.Init(_apiActorGroup, GetLog());
                        result = await nexaasService.UpdateOrder(nexaasData, message, _cancellationToken);
                    }
                    LogDebug("Ending NexaasSendOrderMessage");
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in NexaasSendOrderMessage");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));
        }

        protected override void PostStop()
        {
            base.PostStop();
            ActorTaskScheduler.RunTask(async () =>
            {
                if (_shopifyUpdateOrderStatusQueueClient != null && !_shopifyUpdateOrderStatusQueueClient.IsClosedOrClosing)
                    await _shopifyUpdateOrderStatusQueueClient.CloseAsync();
            });
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, NexaasData nexaasData, IActorRef apiActorGroup)
        {
            return Akka.Actor.Props.Create(() => new NexaasOrderActor(serviceProvider, cancellationToken, nexaasData, apiActorGroup));
        }
    }
}
