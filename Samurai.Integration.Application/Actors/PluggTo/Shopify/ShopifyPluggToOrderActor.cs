using Akka.Actor;
using Samurai.Integration.Domain.Messages.PluggTo;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.PluggTo.Shopify
{
    public class ShopifyPluggToOrderActor : BasePluggToTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IActorRef _apiActorGroup;
        private readonly CancellationToken _cancellationToken;
        public ShopifyPluggToOrderActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, PluggToData pluggToData, IActorRef apiActorGroup)
          : base("ShopifyPluggToOrderActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _pluggToData = pluggToData;
            _apiActorGroup = apiActorGroup;


            ReceiveAsync((Func<PluggToListAllProductsMessage, Task>)(async message =>
            {
                LogDebug("Chamar método para enviar os produtos para a shopify");
            }));
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, PluggToData pluggToData, IActorRef apiActorGroup)
        {
            return Akka.Actor.Props.Create(() => new ShopifyPluggToOrderActor(serviceProvider, cancellationToken, pluggToData, apiActorGroup));
        }
    }
}
