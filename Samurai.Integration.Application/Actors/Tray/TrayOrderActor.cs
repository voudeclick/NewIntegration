using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Tray;
using Samurai.Integration.Domain.Messages.Tray.OrderActor;
using Samurai.Integration.Domain.Queues;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.Tray
{
    public class TrayOrderActor : BaseTrayTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly IActorRef _apiActorGroup;
        private TrayQueue.Queues _queues;


        public TrayOrderActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, TenantDataMessage tenantData, IActorRef apiActorGroup)
            : base("TrayOrderActor")
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

            ReceiveAsync((Func<TrayUpdateOrderStatusMessage, Task>)(async message =>
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var trayService = scope.ServiceProvider.GetService<TrayService>();
                        trayService.Init(_apiActorGroup, GetLog());

                        var result = await trayService.Order.UpdateOrderStatus(message, _cancellationToken);

                        foreach (var returnmessage in result.Data)
                        {
                            if (result.Data != null)
                                await _queues.SendEncryptedMessage((_queues.TrayAppReturnMessage, returnmessage, true));
                        }
                    }

                    Sender.Tell(new ReturnMessage { Result = Result.OK });
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
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, TenantDataMessage tenantData, IActorRef apiActorGroup)
        {
            return Akka.Actor.Props.Create(() => new TrayOrderActor(serviceProvider, cancellationToken, tenantData, apiActorGroup));
        }
    }
}
