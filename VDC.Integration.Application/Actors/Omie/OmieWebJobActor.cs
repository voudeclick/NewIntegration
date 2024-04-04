using Akka.Actor;
using Akka.Event;
using Akka.Logger.Serilog;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VDC.Integration.Domain.Enums;
using VDC.Integration.Domain.Messages;
using VDC.Integration.Domain.Messages.Omie;
using VDC.Integration.Domain.Messages.Webjob;
using VDC.Integration.EntityFramework.Repositories;

namespace VDC.Integration.Application.Actors.Omie
{
    public class OmieWebJobActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log;
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly Dictionary<long, IActorRef> _tenantActors;

        public OmieWebJobActor(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            _log = Context.GetLogger<SerilogLoggingAdapter>();
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _tenantActors = new Dictionary<long, IActorRef>();

            ReceiveAsync<OrchestrateOmieMessage>(async message =>
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        _log.Info($"OmieWebJobActor - Received OrchestrateOmieMessage");

                        var tenantRepository = scope.ServiceProvider.GetService<TenantRepository>();
                        var tenants = await tenantRepository.GetActiveByTenantType(TenantType.Omie, _cancellationToken);
                        _log.Info($"OmieWebJobActor - Found {tenants.Count()} tenants");

                        foreach (var tenant in tenants)
                        {
                            if (_tenantActors.ContainsKey(tenant.Id))
                            {
                                _log.Info($"OmieWebJobActor - Updating actor {tenant.Id}");
                                var currentActor = _tenantActors[tenant.Id];
                                var updateMessage = new UpdateOmieTenantMessage { Data = new OmieData(tenant) };
                                var result = await currentActor.Ask<ReturnMessage>(updateMessage);
                                if (result.Result == Result.OK)
                                {
                                    _log.Info($"OmieWebJobActor - Actor {tenant.Id} updated successfully");
                                }
                                else
                                {
                                    _log.Info($"OmieWebJobActor - Error updating actor {tenant.Id}");
                                    await currentActor.Ask(new StopOmieTenantMessage()); //error in update, stop actor
                                    _tenantActors.Remove(tenant.Id);
                                }
                            }
                            else
                            {
                                _log.Info($"OmieWebJobActor - Creating actor {tenant.Id}");
                                var newActor = Context.ActorOf(OmieTenantActor.Props(_serviceProvider, _cancellationToken));
                                var createMessage = new InitializeOmieTenantMessage { Data = new OmieData(tenant) };
                                var result = await newActor.Ask<ReturnMessage>(createMessage);
                                if (result.Result == Result.OK)
                                {
                                    _log.Info($"OmieWebJobActor - Actor {tenant.Id} created successfully");
                                    _tenantActors.Add(tenant.Id, newActor);
                                }
                                else
                                {
                                    _log.Info($"OmieWebJobActor - Error creating actor {tenant.Id}");
                                    await newActor.Ask(new StopOmieTenantMessage()); //could not create, stop it
                                }
                            }
                        }

                        foreach (var currentActor in _tenantActors)
                        {
                            if (tenants.All(x => x.Id != currentActor.Key))
                            {
                                _log.Info($"OmieWebJobActor - Stoping actor {currentActor.Key}");
                                await currentActor.Value.Ask(new StopOmieTenantMessage());
                                _tenantActors.Remove(currentActor.Key);
                                _log.Info($"OmieWebJobActor - Actor {currentActor.Key} stoped");
                            }
                        }

                        _log.Info($"OmieWebJobActor - End OrchestrateOmieMessage");
                    }
                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "OmieWebJobActor - Error in OrchestrateOmieMessage, shutting down actors");
                    foreach (var currentActor in _tenantActors)
                    {
                        await currentActor.Value.Ask(new StopOmieTenantMessage());
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
                            _log.Info($"OmieWebJobActor - Received UpdateTenantMessage");

                            var tenantRepository = scope.ServiceProvider.GetService<TenantRepository>();
                            var tenant = await tenantRepository.GetActiveById(message.TenantId, _cancellationToken);

                            if (tenant != null && tenant.Type == TenantType.Omie)
                            {
                                if (_tenantActors.ContainsKey(tenant.Id))
                                {
                                    _log.Info($"OmieWebJobActor - Updating actor {tenant.Id}");
                                    var currentActor = _tenantActors[tenant.Id];
                                    var updateMessage = new UpdateOmieTenantMessage { Data = new OmieData(tenant) };
                                    var result = await currentActor.Ask<ReturnMessage>(updateMessage, _cancellationToken);
                                    if (result.Result == Result.OK)
                                    {
                                        _log.Info($"OmieWebJobActor - Actor {tenant.Id} updated successfully");
                                    }
                                    else
                                    {
                                        _log.Info($"OmieWebJobActor - Error updating actor {tenant.Id}");
                                        await currentActor.Ask(new StopOmieTenantMessage(), _cancellationToken); //error in update, stop actor
                                        _tenantActors.Remove(tenant.Id);
                                    }
                                }
                                else
                                {
                                    _log.Info($"OmieWebJobActor - Creating actor {tenant.Id}");
                                    var newActor = Context.ActorOf(OmieTenantActor.Props(_serviceProvider, _cancellationToken));
                                    var createMessage = new InitializeOmieTenantMessage { Data = new OmieData(tenant) };
                                    var result = await newActor.Ask<ReturnMessage>(createMessage, _cancellationToken);
                                    if (result.Result == Result.OK)
                                    {
                                        _log.Info($"OmieWebJobActor - Actor {tenant.Id} created successfully");
                                        _tenantActors.Add(tenant.Id, newActor);
                                    }
                                    else
                                    {
                                        _log.Info($"OmieWebJobActor - Error creating actor {tenant.Id}");
                                        await newActor.Ask(new StopOmieTenantMessage(), _cancellationToken); //could not create, stop it
                                    }
                                }
                            }
                            else
                            {
                                if (_tenantActors.ContainsKey(message.TenantId))
                                {
                                    var currentActor = _tenantActors[message.TenantId];
                                    _log.Info($"OmieWebJobActor - Stoping actor {message.TenantId}");
                                    await currentActor.Ask(new StopOmieTenantMessage(), _cancellationToken);
                                    _tenantActors.Remove(message.TenantId);
                                    _log.Info($"OmieWebJobActor - Actor {message.TenantId} stoped");
                                }
                            }

                            _log.Info($"OmieWebJobActor - End UpdateTenantMessage");
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
            return Akka.Actor.Props.Create(() => new OmieWebJobActor(serviceProvider, cancellationToken));
        }
    }
}
