using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.AliExpress.Order;
using Samurai.Integration.Domain.Queues;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.AliExpress.Tray
{
    public class TrayAliExpressOrderActor : BaseAliExpressTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IActorRef _apiActorGroup;
        private readonly CancellationToken _cancellationToken;

        private readonly TrayQueue.Queues _queues;

        public TrayAliExpressOrderActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, TenantDataMessage tenantData, IActorRef apiActorGroup)
          : base("TrayAliExpressOrderActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _tenantData = tenantData;
            _apiActorGroup = apiActorGroup;

            using (var scope = _serviceProvider.CreateScope())
            {
                var tenantService = scope.ServiceProvider.GetService<TenantService>();
                _queues = new TrayQueue.Queues
                {
                    TrayAppReturnMessage = tenantService.GetQueueClient(_tenantData, TrayQueue.TrayAppReturnMessage, false),
                    UpdateStatusOrderQueue = tenantService.GetQueueClient(_tenantData, TrayQueue.UpdateStatusOrderQueue, false)
                };
            }

            ReceiveAsync((Func<AliExpressGetOrderMessage, Task>)(async message =>
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var service = scope.ServiceProvider.GetService<AliExpressService>();
                        service.Init(_apiActorGroup, GetLog());

                        var result = await service.ProcessOrder(message, _queues, _cancellationToken);
                    }

                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, TenantDataMessage tenantData, IActorRef apiActorGroup)
        {
            return Akka.Actor.Props.Create(() => new TrayAliExpressOrderActor(serviceProvider, cancellationToken, tenantData, apiActorGroup));
        }
    }
}
