using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Millennium;
using Samurai.Integration.Domain.Messages.Webjob;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Samurai.Integration.Application.Actors.Millennium
{
    public class MillenniumWebJobActor : BaseMillenniumWebjobActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private Dictionary<long, IActorRef> _tenantActors;
        private readonly MillenniumSessionToken _millenniumSessionToken;

        public MillenniumWebJobActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, MillenniumSessionToken millenniumSessionToken) : base("MillenniumWebJobActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _tenantActors = new Dictionary<long, IActorRef>();
            _millenniumSessionToken = millenniumSessionToken;

            ReceiveAsync<OrchestrateMillenniumMessage>(async message =>
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var tenantRepository = scope.ServiceProvider.GetService<TenantRepository>();
                        var tenants = await tenantRepository.GetActiveByTenantType(TenantType.Millennium, _cancellationToken);
                        
                        foreach (var tenant in tenants)
                        {
                            try
                            {
                                storeHandle = tenant.StoreHandle;
                                if (_tenantActors.ContainsKey(tenant.Id))
                                {
                                    LogWarning("(Millennium) Atualizando atores tenantId: {0}", tenant.Id);
                                    var currentActor = _tenantActors[tenant.Id];
                                    var updateMessage = new UpdateMillenniumTenantMessage { Data = new MillenniumData(tenant) };
                                    var result = await currentActor.Ask<ReturnMessage>(updateMessage, _cancellationToken);
                                    if (result.Result == Result.Error)
                                    {
                                        await currentActor.Ask(new StopMillenniumTenantMessage(), _cancellationToken); //error in update, stop actor
                                        _tenantActors.Remove(tenant.Id);
                                    }
                                }
                                else
                                {
                                    LogWarning("(Millennium) Atualizando atores tenantId: {0}", tenant.Id);
                                    var newActor = Context.ActorOf(MillenniumTenantActor.Props(_serviceProvider, _cancellationToken, _millenniumSessionToken));
                                    var createMessage = new InitializeMillenniumTenantMessage { Data = new MillenniumData(tenant) };
                                    var result = await newActor.Ask<ReturnMessage>(createMessage, _cancellationToken);
                                    if (result.Result == Result.OK)
                                    {
                                        _tenantActors.Add(tenant.Id, newActor);
                                    }
                                    else
                                    {
                                        await newActor.Ask(new StopMillenniumTenantMessage(), _cancellationToken); //could not create, stop it
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogError(ex, "(Millennium) Erro ao inicializar atores - tenantId {0}", tenant.Id);
                            }
                        }

                        foreach (var currentActor in _tenantActors)
                        {
                            if (tenants.All(x => x.Id != currentActor.Key))
                            {
                                await currentActor.Value.Ask(new StopMillenniumTenantMessage(), _cancellationToken);
                                _tenantActors.Remove(currentActor.Key);
                            }
                        }
                    }

                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    storeHandle = "MillenniumWebJobActor";
                    LogError(ex, "MillenniumWebJobActor - Error in OrchestrateMillenniumMessage, shutting down actors");

                    foreach (var currentActor in _tenantActors)
                    {
                        await currentActor.Value.Ask(new StopMillenniumTenantMessage(), _cancellationToken);
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

                            if (tenant != null && tenant.Type == TenantType.Millennium)
                            {
                                if (_tenantActors.ContainsKey(tenant.Id))
                                {
                                    var currentActor = _tenantActors[tenant.Id];
                                    var updateMessage = new UpdateMillenniumTenantMessage { Data = new MillenniumData(tenant) };
                                    var result = await currentActor.Ask<ReturnMessage>(updateMessage, _cancellationToken);
                                    if (result.Result == Result.Error)
                                    {
                                        await currentActor.Ask(new StopMillenniumTenantMessage(), _cancellationToken); //error in update, stop actor
                                        _tenantActors.Remove(tenant.Id);
                                    }
                                }
                                else
                                {
                                    var newActor = Context.ActorOf(MillenniumTenantActor.Props(_serviceProvider, _cancellationToken, _millenniumSessionToken));
                                    var createMessage = new InitializeMillenniumTenantMessage { Data = new MillenniumData(tenant) };
                                    var result = await newActor.Ask<ReturnMessage>(createMessage, _cancellationToken);
                                    if (result.Result == Result.OK)
                                    {
                                        _tenantActors.Add(tenant.Id, newActor);
                                    }
                                    else
                                    {
                                        await newActor.Ask(new StopMillenniumTenantMessage(), _cancellationToken); //could not create, stop it
                                    }
                                }
                            }
                            else
                            {
                                if (_tenantActors.ContainsKey(message.TenantId))
                                {
                                    var currentActor = _tenantActors[message.TenantId];

                                    await currentActor.Ask(new StopMillenniumTenantMessage(), _cancellationToken);
                                    _tenantActors.Remove(message.TenantId);
                                }
                            }
                        }
                    }

                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    storeHandle = "MillenniumWebJobActor";
                    LogError(ex, "MillenniumWebJobActor - Error in UpdateTenantMessage");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            });
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, MillenniumSessionToken millenniumSessionToken)
        {
            return Akka.Actor.Props.Create(() => new MillenniumWebJobActor(serviceProvider, cancellationToken, millenniumSessionToken));
        }
    }
}
