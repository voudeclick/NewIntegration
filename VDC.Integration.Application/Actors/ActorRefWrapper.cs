using Akka.Actor;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace VDC.Integration.Application.Actors
{
    public class ActorRefWrapper : IActorRefWrapper
    {
        private readonly IActorRef _apiActorGroup;

        public ActorRefWrapper(IActorRef apiActorGroup)
        {
            _apiActorGroup = apiActorGroup;
        }

        public Task<T> Ask<T>(object message, CancellationToken cancellationToken)
        {
            return _apiActorGroup.Ask<T>(message, cancellationToken);
        }

        public Task<object> Ask(object message, TimeSpan? timeout = null)
        {
            return _apiActorGroup.Ask<object>(message, timeout, CancellationToken.None);
        }

        public Task<object> Ask(object message, CancellationToken cancellationToken)
        {
            return _apiActorGroup.Ask<object>(message, null, cancellationToken);
        }

        public Task<object> Ask(object message, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            return _apiActorGroup.Ask<object>(message, timeout, cancellationToken);
        }

        public Task<T> Ask<T>(object message, TimeSpan? timeout = null)
        {
            return _apiActorGroup.Ask<T>(message, timeout, CancellationToken.None);
        }

        public Task<T> Ask<T>(object message, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            return _apiActorGroup.Ask<T>((IActorRef _) => message, timeout, cancellationToken);
        }
    }
}
