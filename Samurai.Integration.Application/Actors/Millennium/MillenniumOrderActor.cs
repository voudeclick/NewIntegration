using Akka.Actor;
using Akka.Dispatch;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Extensions;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Millennium;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Queues;
using System;
using System.Threading;
using System.Threading.Tasks;
using Result = Samurai.Integration.Domain.Messages.Result;

namespace Samurai.Integration.Application.Actors.Millennium
{
    public class MillenniumOrderActor : BaseMillenniumTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly IActorRef _apiActorGroup;
        private readonly QueueClient _erpUpdateOrderStatusQueueClient;

        public MillenniumOrderActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, MillenniumData millenniumData, IActorRef apiActorGroup)
            : base("MillenniumOrderActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _millenniumData = millenniumData;
            _apiActorGroup = apiActorGroup;

            using (var scope = _serviceProvider.CreateScope())
            {
                var tenantService = scope.ServiceProvider.GetService<TenantService>();

                if (millenniumData.IntegrationType == Domain.Enums.IntegrationType.Shopify)
                    _erpUpdateOrderStatusQueueClient = tenantService.GetQueueClient(_millenniumData, ShopifyQueue.UpdateOrderStatusQueue);
                else
                    _erpUpdateOrderStatusQueueClient = tenantService.GetQueueClient(_millenniumData, SellerCenterQueue.UpdateStatusOrderQueue);
            }

            ReceiveAsync((Func<MillenniumListNewOrdersMessage, Task>)(async message =>
            {
                try
                {
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var millenniumService = scope.ServiceProvider.GetService<MillenniumService>();
                        millenniumService.Init(new ActorRefWrapper(_apiActorGroup), GetLog());
                        _millenniumData.Retry = message.Retry;
                        result = await millenniumService.ListNewOrders(_millenniumData, _erpUpdateOrderStatusQueueClient, _cancellationToken);
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<MillenniumListOrderMessage, Task>)(async message =>
            {
                try
                {
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var millenniumService = scope.ServiceProvider.GetService<MillenniumService>();
                        millenniumService.Init(new ActorRefWrapper(_apiActorGroup), GetLog());
                        result = await millenniumService.ListOrder(message.ExternalOrderId, _millenniumData, _erpUpdateOrderStatusQueueClient, _cancellationToken);
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ShopifySendOrderToERPMessage, Task>)(async message =>
            {
                try
                {
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var millenniumService = scope.ServiceProvider.GetService<MillenniumService>();
                        millenniumService.Init(new ActorRefWrapper(_apiActorGroup), GetLog());
                        result = await millenniumService.UpdateOrder(_millenniumData, message, _cancellationToken);
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<CreateOrderMessage, Task>)(async message =>
            {
                try
                {
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var millenniumService = scope.ServiceProvider.GetService<MillenniumService>();
                        millenniumService.Init(new ActorRefWrapper(_apiActorGroup), GetLog());
                        result = await millenniumService.UpdateOrderNew(_millenniumData, message, _cancellationToken);
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));
        }

        protected override void PostStop()
        {
            base.PostStop();
            ActorTaskScheduler.RunTask(async () =>
            {
                await _erpUpdateOrderStatusQueueClient.CloseAsyncSafe();
            });
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, MillenniumData millenniumData, IActorRef apiActorGroup)
        {
            return Akka.Actor.Props.Create(() => new MillenniumOrderActor(serviceProvider, cancellationToken, millenniumData, apiActorGroup));
        }
    }
}
