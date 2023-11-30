using Akka.Actor;
using Akka.Event;
using Akka.Logger.Serilog;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Pier8;
using Samurai.Integration.Domain.Messages.Webjob;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Samurai.Integration.Application.Actors.Pier8
{
    public class Pier8WebJobActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log;
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private Dictionary<long, IActorRef> _tenantActors;

        public Pier8WebJobActor(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            _log = Context.GetLogger<SerilogLoggingAdapter>();
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _tenantActors = new Dictionary<long, IActorRef>();

            ReceiveAsync<OrchestratePier8Message>(async message => {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        _log.Info($"Pier8WebJobActor - Received OrchestratePier8Message");

                        var tenantRepository = scope.ServiceProvider.GetService<TenantRepository>();
                        var tenants = await tenantRepository.GetActivePier8(_cancellationToken);

                        _log.Info($"Pier8WebJobActor - Found {tenants.Count()} tenants");

                        foreach (var tenant in tenants)
                        {
                            if (_tenantActors.ContainsKey(tenant.Id))
                            {
                                _log.Info($"Pier8WebJobActor - Updating actor {tenant.Id}");
                                var currentActor = _tenantActors[tenant.Id];
                                var updateMessage = new UpdatePier8TenantMessage { Data = new Pier8DataMessage(tenant) };
                                var result = await currentActor.Ask<ReturnMessage>(updateMessage, _cancellationToken);
                                if (result.Result == Result.OK)
                                {
                                    _log.Info($"Pier8WebJobActor - Actor {tenant.Id} updated successfully");
                                }
                                else
                                {
                                    _log.Info($"Pier8WebJobActor - Error updating actor {tenant.Id}");
                                    await currentActor.Ask(new StopPier8TenantMessage(), _cancellationToken); //error in update, stop actor
                                    _tenantActors.Remove(tenant.Id);
                                }
                            }
                            else
                            {
                                _log.Info($"Pier8WebJobActor - Creating actor {tenant.Id}");
                                var newActor = Context.ActorOf(Pier8TenantActor.Props(_serviceProvider, _cancellationToken));
                                var createMessage = new InitializePier8TenantMessage { Data = new Pier8DataMessage(tenant) };
                                var result = await newActor.Ask<ReturnMessage>(createMessage, _cancellationToken);
                                if (result.Result == Result.OK)
                                {
                                    _log.Info($"Pier8WebJobActor - Actor {tenant.Id} created successfully");
                                    _tenantActors.Add(tenant.Id, newActor);
                                }
                                else
                                {
                                    _log.Info($"Pier8WebJobActor - Error creating actor {tenant.Id}");
                                    await newActor.Ask(new StopPier8TenantMessage(), _cancellationToken); //could not create, stop it
                                }
                            }
                        }

                        foreach (var currentActor in _tenantActors)
                        {
                            if (tenants.All(x => x.Id != currentActor.Key))
                            {
                                _log.Info($"Pier8WebJobActor - Stoping actor {currentActor.Key}");
                                await currentActor.Value.Ask(new StopPier8TenantMessage(), _cancellationToken);
                                _tenantActors.Remove(currentActor.Key);
                                _log.Info($"Pier8WebJobActor - Actor {currentActor.Key} stoped");
                            }
                        }

                        _log.Info($"Pier8WebJobActor - End OrchestratePier8Message");
                    }
                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Pier8WebJobActor - Error in OrchestratePier8Message, shutting down actors");
                    foreach (var currentActor in _tenantActors)
                    {
                        await currentActor.Value.Ask(new StopPier8TenantMessage(), _cancellationToken);
                        _tenantActors.Remove(currentActor.Key);
                    }
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            });

            ReceiveAsync<UpdateTenantMessage>(async message => {
                try
                {
                    if (message.TimerData > DateTime.Now.AddMinutes(-5))
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            _log.Info($"Pier8WebJobActor - Received UpdateTenantMessage");

                            var tenantRepository = scope.ServiceProvider.GetService<TenantRepository>();
                            var tenant = await tenantRepository.GetActiveById(message.TenantId, _cancellationToken);

                            if (tenant != null)
                            {
                                if (_tenantActors.ContainsKey(tenant.Id))
                                {
                                    _log.Info($"Pier8WebJobActor - Updating actor {tenant.Id}");
                                    var currentActor = _tenantActors[tenant.Id];
                                    var updateMessage = new UpdatePier8TenantMessage { Data = new Pier8DataMessage(tenant) };
                                    var result = await currentActor.Ask<ReturnMessage>(updateMessage, _cancellationToken);
                                    if (result.Result == Result.OK)
                                    {
                                        _log.Info($"Pier8WebJobActor - Actor {tenant.Id} updated successfully");
                                    }
                                    else
                                    {
                                        _log.Info($"Pier8WebJobActor - Error updating actor {tenant.Id}");
                                        await currentActor.Ask(new StopPier8TenantMessage(), _cancellationToken); //error in update, stop actor
                                        _tenantActors.Remove(tenant.Id);
                                    }
                                }
                                else
                                {
                                    _log.Info($"Pier8WebJobActor - Creating actor {tenant.Id}");
                                    var newActor = Context.ActorOf(Pier8TenantActor.Props(_serviceProvider, _cancellationToken));
                                    var createMessage = new InitializePier8TenantMessage { Data = new Pier8DataMessage(tenant) };
                                    var result = await newActor.Ask<ReturnMessage>(createMessage, _cancellationToken);
                                    if (result.Result == Result.OK)
                                    {
                                        _log.Info($"Pier8WebJobActor - Actor {tenant.Id} created successfully");
                                        _tenantActors.Add(tenant.Id, newActor);
                                    }
                                    else
                                    {
                                        _log.Info($"Pier8WebJobActor - Error creating actor {tenant.Id}");
                                        await newActor.Ask(new StopPier8TenantMessage(), _cancellationToken); //could not create, stop it
                                    }
                                }
                            }
                            else
                            {
                                if (_tenantActors.ContainsKey(message.TenantId))
                                {
                                    var currentActor = _tenantActors[message.TenantId];
                                    _log.Info($"Pier8WebJobActor - Stoping actor {message.TenantId}");
                                    await currentActor.Ask(new StopPier8TenantMessage(), _cancellationToken);
                                    _tenantActors.Remove(message.TenantId);
                                    _log.Info($"Pier8WebJobActor - Actor {message.TenantId} stoped");
                                }
                            }

                            _log.Info($"Pier8WebJobActor - End UpdateTenantMessage");
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
            return Akka.Actor.Props.Create(() => new Pier8WebJobActor(serviceProvider, cancellationToken));
        }
    }
}
