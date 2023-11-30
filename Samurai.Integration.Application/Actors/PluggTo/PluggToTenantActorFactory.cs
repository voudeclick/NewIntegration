using System;
using System.Threading;

using Akka.Actor;

using Samurai.Integration.Domain.Entities.Database;

namespace Samurai.Integration.Application.Actors.PluggTo
{
    public static class PluggToTenantActorFactory
    {
        public static IActorRef GetInstance(Tenant tenant, IUntypedActorContext context, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            switch (tenant.IntegrationType)
            {
                case Domain.Enums.IntegrationType.Shopify:
                    return context.ActorOf(ShopifyPluggToTenantActor.Props(serviceProvider, cancellationToken));
                case Domain.Enums.IntegrationType.SellerCenter:
                    return context.ActorOf(SellerCenterPluggToTenantActor.Props(serviceProvider, cancellationToken));
                default:
                    throw new Exception("Invalid tenant integration type");
            }
        }
    }
}
