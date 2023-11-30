using Akka.Actor;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.APIClient.PluggTo.Models.Requests;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.PluggTo;
using Samurai.Integration.Domain.Queues;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.PluggTo.Shopify
{
    public class ShopifyPluggToProductActor : BasePluggToTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IActorRef _apiActorGroup;
        private readonly CancellationToken _cancellationToken;

        public ShopifyPluggToProductActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, PluggToData pluggToData, IActorRef apiActorGroup)
           : base("ShopifyPluggToProductActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _pluggToData = pluggToData;
            _apiActorGroup = apiActorGroup;

          
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, PluggToData pluggToData, IActorRef apiActorGroup)
        {
            return Akka.Actor.Props.Create(() => new ShopifyPluggToProductActor(serviceProvider, cancellationToken, pluggToData, apiActorGroup));
        }
    }
}
