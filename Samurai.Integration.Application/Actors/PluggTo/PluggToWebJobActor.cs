using Akka.Actor;
using Akka.Event;
using Akka.Logger.Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.PluggTo;
using Samurai.Integration.Domain.Messages.Webjob;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Samurai.Integration.Application.Actors.PluggTo
{
    public class PluggToWebJobActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log;
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;

        private Dictionary<long, IActorRef> _tenantActors;

        public PluggToWebJobActor(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;

            _log = Context.GetLogger<SerilogLoggingAdapter>();          

            _tenantActors = new Dictionary<long, IActorRef>();

            ReceiveAsync<OrchestratePluggToMessage>(async message =>
            {

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        _log.Info($"PluggToWebJobActor - Received OrchestratePluggToMessage");

                        var tenantRepository = scope.ServiceProvider.GetService<TenantRepository>();
                        var tenants = await tenantRepository.GetActiveByTenantType(TenantType.PluggTo, _cancellationToken);

                        _log.Info($"PluggToWebJobActor - Found {tenants.Count()} tenants");

                        foreach (var tenant in tenants)
                        {
                            if (_tenantActors.ContainsKey(tenant.Id))
                            {
                                _log.Info($"PluggToWebJobActor - Updating actor {tenant.Id}");

                                var currentActor = _tenantActors[tenant.Id];

                                var updateMessage = new UpdatePluggToTenantMessage { Data = new PluggToData(tenant) };

                                var result = await currentActor.Ask<ReturnMessage>(updateMessage, _cancellationToken);
                                if (result.Result == Result.OK)
                                {
                                    _log.Info($"PluggToWebJobActor - Actor {tenant.Id} updated successfully");
                                }
                                else
                                {
                                    _log.Info($"PluggToWebJobActor - Error updating actor {tenant.Id}");

                                    await currentActor.Ask(new StopPluggToTenantMessage(), _cancellationToken); //error in update, stop actor

                                    _tenantActors.Remove(tenant.Id);
                                }
                            }
                            else
                            {
                                _log.Info($"PluggToWebJobActor - Creating actor {tenant.Id}");

                                var newActor = PluggToTenantActorFactory.GetInstance(tenant, Context, serviceProvider, cancellationToken);

                                var createMessage = new InitializePluggToTenantMessage { Data = new PluggToData(tenant) };

                                var result = await newActor.Ask<ReturnMessage>(createMessage, _cancellationToken);
                                if (result.Result == Result.OK)
                                {
                                    _log.Info($"PluggToWebJobActor - Actor {tenant.Id} created successfully");

                                    _tenantActors.Add(tenant.Id, newActor);
                                }
                                else
                                {
                                    _log.Info($"PluggToWebJobActor - Error creating actor {tenant.Id}");

                                    await newActor.Ask(new StopPluggToTenantMessage(), _cancellationToken);
                                }
                            }
                        }


                        foreach (var currentActor in _tenantActors)
                        {
                            if (tenants.All(x => x.Id != currentActor.Key))
                            {
                                _log.Info($"PluggToWebJobActor - Stoping actor {currentActor.Key}");

                                await currentActor.Value.Ask(new StopPluggToTenantMessage(), _cancellationToken);

                                _tenantActors.Remove(currentActor.Key);

                                _log.Info($"PluggToWebJobActor - Actor {currentActor.Key} stoped");
                            }
                        }

                        _log.Info($"PluggToWebJobActor - End OrchestratePluggToMessage");
                    }

                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "PluggToWebJobActor - Error in OrchestratePluggToMessage, shutting down actors",
                        LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));

                    foreach (var currentActor in _tenantActors)
                    {
                        await currentActor.Value.Ask(new StopPluggToTenantMessage(), _cancellationToken);

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
                            _log.Info($"PluggToWebJobActor - Received UpdateTenantMessage");

                            var tenantRepository = scope.ServiceProvider.GetService<TenantRepository>();
                            var tenant = await tenantRepository.GetActiveById(message.TenantId, _cancellationToken);

                            if (tenant != null && tenant.Type == TenantType.PluggTo)
                            {
                                if (_tenantActors.ContainsKey(tenant.Id))
                                {
                                    _log.Info($"PluggToWebJobActor - Updating actor {tenant.Id}");

                                    var currentActor = _tenantActors[tenant.Id];

                                    var updateMessage = new UpdatePluggToTenantMessage { Data = new PluggToData(tenant) };

                                    var result = await currentActor.Ask<ReturnMessage>(updateMessage, _cancellationToken);
                                    if (result.Result == Result.OK)
                                    {
                                        _log.Info($"PluggToWebJobActor - Actor {tenant.Id} updated successfully");
                                    }
                                    else
                                    {
                                        _log.Info($"PluggToWebJobActor - Error updating actor {tenant.Id}");

                                        await currentActor.Ask(new StopPluggToTenantMessage(), _cancellationToken); //error in update, stop actor

                                        _tenantActors.Remove(tenant.Id);
                                    }
                                }
                                else
                                {
                                    _log.Info($"PluggToWebJobActor - Creating actor {tenant.Id}");

                                    var newActor = PluggToTenantActorFactory.GetInstance(tenant, Context, serviceProvider, cancellationToken);

                                    var createMessage = new InitializePluggToTenantMessage { Data = new PluggToData(tenant) };

                                    var result = await newActor.Ask<ReturnMessage>(createMessage, _cancellationToken);
                                    if (result.Result == Result.OK)
                                    {
                                        _log.Info($"PluggToWebJobActor - Actor {tenant.Id} created successfully");

                                        _tenantActors.Add(tenant.Id, newActor);
                                    }
                                    else
                                    {
                                        _log.Info($"PluggToWebJobActor - Error creating actor {tenant.Id}");

                                        await newActor.Ask(new StopPluggToTenantMessage(), _cancellationToken);
                                    }
                                }
                            }
                            else
                            {
                                if (_tenantActors.ContainsKey(message.TenantId))
                                {
                                    var currentActor = _tenantActors[message.TenantId];

                                    _log.Info($"PluggToWebJobActor - Stoping actor {message.TenantId}");

                                    await currentActor.Ask(new StopPluggToTenantMessage(), _cancellationToken);

                                    _tenantActors.Remove(message.TenantId);

                                    _log.Info($"PluggToWebJobActor - Actor {message.TenantId} stoped");
                                }
                            }

                            _log.Info($"PluggToWebJobActor - End UpdateTenantMessage");
                        }
                    }

                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "PluggToWebJobActor - Error in UpdateTenantMessage",
                        LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }


            });
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            return Akka.Actor.Props.Create(() => new PluggToWebJobActor(serviceProvider, cancellationToken));
        }
    }
}
