using System;
using System.Threading;

using Akka.Actor;

using Samurai.Integration.Application.Actors.Bling.SellerCenter;
using Samurai.Integration.Application.Actors.Bling.Shopify;
using Samurai.Integration.Domain.Entities.Database;

namespace Samurai.Integration.Application.Actors.Bling
{
    public static class TenantActorFactory
    {
        public static IActorRef GetInstance(Tenant tenant, IUntypedActorContext context, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            switch (tenant.IntegrationType)
            {
                case Domain.Enums.IntegrationType.Shopify:
                    return context.ActorOf(ShopifyBlingTenantActor.Props(serviceProvider, cancellationToken));
                case Domain.Enums.IntegrationType.SellerCenter:
                    return context.ActorOf(SellerCenterBlingTenantActor.Props(serviceProvider, cancellationToken));
                default:
                    throw new Exception("Invalid tenant integration type");
            }
        }
    }
}
