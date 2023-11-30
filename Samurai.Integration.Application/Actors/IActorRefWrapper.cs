using Akka.Actor;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors
{
    public interface IActorRefWrapper
    {
        Task<object> Ask(object message, TimeSpan? timeout = null);
        Task<object> Ask(object message, CancellationToken cancellationToken);
        Task<object> Ask(object message, TimeSpan? timeout, CancellationToken cancellationToken);
        Task<T> Ask<T>(object message, TimeSpan? timeout = null);
        Task<T> Ask<T>(object message, CancellationToken cancellationToken);
        Task<T> Ask<T>(object message, TimeSpan? timeout, CancellationToken cancellationToken);
    }
}