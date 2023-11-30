using Akka.Actor;
using Akka.Dispatch;
using Akka.Event;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Omie;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Queues;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.Omie
{
    public class OmieOrderActor : BaseOmieTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly IActorRef _apiActorGroup;
        private readonly QueueClient _shopifyUpdateOrderStatusQueueClient;

        public OmieOrderActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, OmieData omieData, IActorRef apiActorGroup)
            : base("OmieOrderActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _omieData = omieData;
            _apiActorGroup = apiActorGroup;


            using (var scope = _serviceProvider.CreateScope())
            {
                var tenantService = scope.ServiceProvider.GetService<TenantService>();
                _shopifyUpdateOrderStatusQueueClient = tenantService.GetQueueClient(_omieData, ShopifyQueue.UpdateOrderStatusQueue);
            }

            ReceiveAsync((Func<OmieListOrderMessage, Task>)(async message => {
                try
                {                 
                    ReturnMessage result = null;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var omieService = scope.ServiceProvider.GetService<OmieService>();
                        omieService.Init(_apiActorGroup, GetLog());
                        result = await omieService.ListOrder(message.ExternalOrderId, _omieData, _shopifyUpdateOrderStatusQueueClient, _cancellationToken);
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in OmieListOrderMessage");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ShopifySendOrderToERPMessage, Task>)(async message => {
                try
                {                   
                    ReturnMessage result = null;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var omieService = scope.ServiceProvider.GetService<OmieService>();
                        omieService.Init(_apiActorGroup, GetLog());
                        result = await omieService.UpdateOrder(_omieData, message, _cancellationToken);
                    }                   
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {                    
                    LogError(ex, "Error in OmieSendOrderMessage | ErrorMessage: {0} | InnerException?: {1}", ex.Message, ex.InnerException?.Message);
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

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, OmieData omieData, IActorRef apiActorGroup)
        {
            return Akka.Actor.Props.Create(() => new OmieOrderActor(serviceProvider, cancellationToken, omieData, apiActorGroup));
        }
    }
}
