using Akka.Actor;
using Akka.Event;
using Akka.Logger.Serilog;

using Microsoft.Extensions.DependencyInjection;

using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Bling;
using Samurai.Integration.Domain.Messages.Webjob;
using Samurai.Integration.EntityFramework.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Samurai.Integration.Application.Actors.Bling
{
    public class BlingWebJobActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log;
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private Dictionary<long, IActorRef> _tenantActors;

        public BlingWebJobActor(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            _log = Context.GetLogger<SerilogLoggingAdapter>();
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _tenantActors = new Dictionary<long, IActorRef>();

            ReceiveAsync<OrchestrateBlingMessage>(async message =>
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var tenantRepository = scope.ServiceProvider.GetService<TenantRepository>();
                        var tenants = await tenantRepository.GetActiveByTenantType(TenantType.Bling, _cancellationToken);

                        foreach (var tenant in tenants)
                        {
                            if (_tenantActors.ContainsKey(tenant.Id))
                            {
                                var currentActor = _tenantActors[tenant.Id];
                                var updateMessage = new UpdateBlingTenantMessage { Data = new BlingData(tenant) };
                                var result = await currentActor.Ask<ReturnMessage>(updateMessage, _cancellationToken);
                                if (result.Result == Result.Error)
                                {
                                    await currentActor.Ask(new StopBlingTenantMessage(), _cancellationToken); //error in update, stop actor
                                    _tenantActors.Remove(tenant.Id);
                                }
                            }
                            else
                            {
                                var newActor = TenantActorFactory.GetInstance(tenant, Context, serviceProvider, cancellationToken);
                                var createMessage = new InitializeBlingTenantMessage { Data = new BlingData(tenant) };
                                var result = await newActor.Ask<ReturnMessage>(createMessage, _cancellationToken);
                                if (result.Result == Result.OK)
                                {
                                    _tenantActors.Add(tenant.Id, newActor);
                                }
                                else
                                {
                                    await newActor.Ask(new StopBlingTenantMessage(), _cancellationToken); //could not create, stop it
                                }
                            }
                        }

                        foreach (var currentActor in _tenantActors)
                        {
                            if (tenants.All(x => x.Id != currentActor.Key))
                            {
                                await currentActor.Value.Ask(new StopBlingTenantMessage(), _cancellationToken);
                                _tenantActors.Remove(currentActor.Key);
                            }
                        }
                    }

                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    foreach (var currentActor in _tenantActors)
                    {
                        await currentActor.Value.Ask(new StopBlingTenantMessage(), _cancellationToken);
                        _tenantActors.Remove(currentActor.Key);
                    }

                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            });

            ReceiveAsync<UpdateTenantMessage>(async message =>
            {
                try
                {
                    if (message.TimerData > DateTime.Now.AddMinutes(-5))
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var tenantRepository = scope.ServiceProvider.GetService<TenantRepository>();
                            var tenant = await tenantRepository.GetActiveById(message.TenantId, _cancellationToken);

                            if (tenant != null && tenant.Type == TenantType.Bling)
                            {
                                if (_tenantActors.ContainsKey(tenant.Id))
                                {
                                    var currentActor = _tenantActors[tenant.Id];
                                    var updateMessage = new UpdateBlingTenantMessage { Data = new BlingData(tenant) };
                                    var result = await currentActor.Ask<ReturnMessage>(updateMessage, _cancellationToken);
                                    if (result.Result == Result.Error)
                                    {
                                        await currentActor.Ask(new StopBlingTenantMessage(), _cancellationToken); //error in update, stop actor
                                        _tenantActors.Remove(tenant.Id);
                                    }
                                }
                                else
                                {
                                    var newActor = TenantActorFactory.GetInstance(tenant, Context, serviceProvider, cancellationToken);
                                    var createMessage = new InitializeBlingTenantMessage { Data = new BlingData(tenant) };
                                    var result = await newActor.Ask<ReturnMessage>(createMessage, _cancellationToken);
                                    if (result.Result == Result.OK)
                                    {
                                        _tenantActors.Add(tenant.Id, newActor);
                                    }
                                    else
                                    {
                                        await newActor.Ask(new StopBlingTenantMessage(), _cancellationToken); //could not create, stop it
                                    }
                                }
                            }
                            else
                            {
                                if (_tenantActors.ContainsKey(message.TenantId))
                                {
                                    var currentActor = _tenantActors[message.TenantId];
                                    await currentActor.Ask(new StopBlingTenantMessage(), _cancellationToken);
                                    _tenantActors.Remove(message.TenantId);
                                }
                            }
                        }
                    }
                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            });
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            return Akka.Actor.Props.Create(() => new BlingWebJobActor(serviceProvider, cancellationToken));
        }
    }
}
