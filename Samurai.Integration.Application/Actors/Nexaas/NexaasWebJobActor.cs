using Akka.Actor;
using Akka.Event;
using Akka.Logger.Serilog;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Nexaas;
using Samurai.Integration.Domain.Messages.Webjob;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Samurai.Integration.Application.Actors.Nexaas
{
    public class NexaasWebJobActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log;
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private Dictionary<long, IActorRef> _tenantActors;

        public NexaasWebJobActor(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            _log = Context.GetLogger<SerilogLoggingAdapter>();
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _tenantActors = new Dictionary<long, IActorRef>();

            ReceiveAsync<OrchestrateNexaasMessage>(async message =>
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        _log.Info($"NexaasWebJobActor - Received OrchestrateNexaasMessage");

                        var tenantRepository = scope.ServiceProvider.GetService<TenantRepository>();
                        var tenants = await tenantRepository.GetActiveByTenantType(TenantType.Nexaas, _cancellationToken);

                        _log.Info($"NexaasWebJobActor - Found {tenants.Count()} tenants");

                        foreach (var tenant in tenants)
                        {
                            if (_tenantActors.ContainsKey(tenant.Id))
                            {
                                _log.Info($"NexaasWebJobActor - Updating actor {tenant.Id}");
                                var currentActor = _tenantActors[tenant.Id];
                                var updateMessage = new UpdateNexaasTenantMessage { Data = new NexaasData(tenant) };
                                var result = await currentActor.Ask<ReturnMessage>(updateMessage);
                                if (result.Result == Result.OK)
                                {
                                    _log.Info($"NexaasWebJobActor - Actor {tenant.Id} updated successfully");
                                }
                                else
                                {
                                    _log.Info($"NexaasWebJobActor - Error updating actor {tenant.Id}");
                                    await currentActor.Ask(new StopNexaasTenantMessage()); //error in update, stop actor
                                    _tenantActors.Remove(tenant.Id);
                                }
                            }
                            else
                            {
                                _log.Info($"NexaasWebJobActor - Creating actor {tenant.Id}");
                                var newActor = Context.ActorOf(NexaasTenantActor.Props(_serviceProvider, _cancellationToken));
                                var createMessage = new InitializeNexaasTenantMessage { Data = new NexaasData(tenant) };
                                var result = await newActor.Ask<ReturnMessage>(createMessage);
                                if (result.Result == Result.OK)
                                {
                                    _log.Info($"NexaasWebJobActor - Actor {tenant.Id} created successfully");
                                    _tenantActors.Add(tenant.Id, newActor);
                                }
                                else
                                {
                                    _log.Info($"NexaasWebJobActor - Error creating actor {tenant.Id}");
                                    await newActor.Ask(new StopNexaasTenantMessage()); //could not create, stop it
                                }
                            }
                        }

                        foreach (var currentActor in _tenantActors)
                        {
                            if (tenants.All(x => x.Id != currentActor.Key))
                            {
                                _log.Info($"NexaasWebJobActor - Stoping actor {currentActor.Key}");
                                await currentActor.Value.Ask(new StopNexaasTenantMessage());
                                _tenantActors.Remove(currentActor.Key);
                                _log.Info($"NexaasWebJobActor - Actor {currentActor.Key} stoped");
                            }
                        }

                        _log.Info($"NexaasWebJobActor - End OrchestrateNexaasMessage");
                    }
                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "NexaasWebJobActor - Error in OrchestrateNexaasMessage, shutting down actors");
                    foreach (var currentActor in _tenantActors)
                    {
                        await currentActor.Value.Ask(new StopNexaasTenantMessage());
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
                            _log.Info($"NexaasWebJobActor - Received UpdateTenantMessage");

                            var tenantRepository = scope.ServiceProvider.GetService<TenantRepository>();
                            var tenant = await tenantRepository.GetActiveById(message.TenantId, _cancellationToken);

                            if (tenant != null && tenant.Type == TenantType.Nexaas)
                            {
                                if (_tenantActors.ContainsKey(tenant.Id))
                                {
                                    _log.Info($"NexaasWebJobActor - Updating actor {tenant.Id}");
                                    var currentActor = _tenantActors[tenant.Id];
                                    var updateMessage = new UpdateNexaasTenantMessage { Data = new NexaasData(tenant) };
                                    var result = await currentActor.Ask<ReturnMessage>(updateMessage, _cancellationToken);
                                    if (result.Result == Result.OK)
                                    {
                                        _log.Info($"NexaasWebJobActor - Actor {tenant.Id} updated successfully");
                                    }
                                    else
                                    {
                                        _log.Info($"NexaasWebJobActor - Error updating actor {tenant.Id}");
                                        await currentActor.Ask(new StopNexaasTenantMessage(), _cancellationToken); //error in update, stop actor
                                        _tenantActors.Remove(tenant.Id);
                                    }
                                }
                                else
                                {
                                    _log.Info($"NexaasWebJobActor - Creating actor {tenant.Id}");
                                    var newActor = Context.ActorOf(NexaasTenantActor.Props(_serviceProvider, _cancellationToken));
                                    var createMessage = new InitializeNexaasTenantMessage { Data = new NexaasData(tenant) };
                                    var result = await newActor.Ask<ReturnMessage>(createMessage, _cancellationToken);
                                    if (result.Result == Result.OK)
                                    {
                                        _log.Info($"NexaasWebJobActor - Actor {tenant.Id} created successfully");
                                        _tenantActors.Add(tenant.Id, newActor);
                                    }
                                    else
                                    {
                                        _log.Info($"NexaasWebJobActor - Error creating actor {tenant.Id}");
                                        await newActor.Ask(new StopNexaasTenantMessage(), _cancellationToken); //could not create, stop it
                                    }
                                }
                            }
                            else
                            {
                                if (_tenantActors.ContainsKey(message.TenantId))
                                {
                                    var currentActor = _tenantActors[message.TenantId];
                                    _log.Info($"NexaasWebJobActor - Stoping actor {message.TenantId}");
                                    await currentActor.Ask(new StopNexaasTenantMessage(), _cancellationToken);
                                    _tenantActors.Remove(message.TenantId);
                                    _log.Info($"NexaasWebJobActor - Actor {message.TenantId} stoped");
                                }
                            }

                            _log.Info($"NexaasWebJobActor - End UpdateTenantMessage");
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
            return Akka.Actor.Props.Create(() => new NexaasWebJobActor(serviceProvider, cancellationToken));
        }
    }
}
