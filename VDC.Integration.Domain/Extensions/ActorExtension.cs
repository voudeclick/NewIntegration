using Akka.Actor;
using System.Collections.Generic;
using System.Linq;
using VDC.Integration.Domain.Enums;

namespace VDC.Integration.Domain.Extensions
{
    public static class ActorExtension
    {
        public static IActorRef Route(this IDictionary<EnumActorType, IActorRef> actors, EnumActorType type)
            => actors.Where(x => x.Key == type).FirstOrDefault().Value;


    }
}
