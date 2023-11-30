using Akka.Actor;
using Samurai.Integration.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samurai.Integration.Domain.Extensions
{
    public static class ActorExtension
    {
        public static IActorRef Route(this IDictionary<EnumActorType, IActorRef> actors, EnumActorType type)
            => actors.Where(x => x.Key == type).FirstOrDefault().Value;

        
    }
}
