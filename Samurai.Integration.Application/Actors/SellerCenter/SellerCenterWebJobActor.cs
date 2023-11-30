using Akka.Actor;
using Akka.Event;
using Akka.Logger.Serilog;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.SellerCenter;
using Samurai.Integration.Domain.Messages.Webjob;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Samurai.Integration.Application.Actors.SellerCenter
{
    public class SellerCenterWebJobActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log;
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private Dictionary<long, IActorRef> _tenantActors;

        public SellerCenterWebJobActor(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            _log = Context.GetLogger<SerilogLoggingAdapter>();
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _tenantActors = new Dictionary<long, IActorRef>();

            ReceiveAsync<OrchestrateSellerCenterMessage>(async message =>
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var tenantRepository = scope.ServiceProvider.GetService<TenantRepository>();
                        var tenants = await tenantRepository.GetActiveByIntegrationType(IntegrationType.SellerCenter, _cancellationToken);

                        foreach (var tenant in tenants)
                        {
                            if (_tenantActors.ContainsKey(tenant.Id))
                            {
                                var currentActor = _tenantActors[tenant.Id];
                                var updateMessage = new UpdateSellerCenterTenantMessage { Data = new SellerCenterDataMessage(tenant) };
                                var result = await currentActor.Ask<ReturnMessage>(updateMessage, _cancellationToken);

                                if (result.Result == Result.Error)
                                {
                                    _log.Warning($"SellerCenterWebJobActor - Error in OrchestrateSellerCenterMessage | {result.Error.Message}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                                    await currentActor.Ask(new StopSellerCenterTenantMessage(), _cancellationToken); //error in update, stop actor
                                    _tenantActors.Remove(tenant.Id);
                                }

                            }
                            else
                            {
                                var newActor = Context.ActorOf(SellerCenterTenantActor.Props(_serviceProvider, _cancellationToken));
                                var createMessage = new InitializeSellerCenterTenantMessage { Data = new SellerCenterDataMessage(tenant) };
                                var result = await newActor.Ask<ReturnMessage>(createMessage, _cancellationToken);
                                if (result.Result == Result.OK)
                                {
                                    _tenantActors.Add(tenant.Id, newActor);
                                }
                                else
                                {
                                    _log.Warning($"SellerCenterWebJobActor - Error in OrchestrateSellerCenterMessage | {result.Error.Message}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"SellerCenterWebJobActor - Error creating actor {tenant.Id}"));
                                    await newActor.Ask(new StopSellerCenterTenantMessage(), _cancellationToken); //could not create, stop it
                                }
                            }
                        }

                        foreach (var currentActor in _tenantActors)
                        {
                            if (tenants.All(x => x.Id != currentActor.Key))
                            {                              
                                await currentActor.Value.Ask(new StopSellerCenterTenantMessage(), _cancellationToken);
                                _tenantActors.Remove(currentActor.Key);                             
                            }
                        }                        
                    }
                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    _log.Error(ex, $"SellerCenterWebJobActor - Error in OrchestrateSellerCenterMessage, shutting down actors | {ex}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"SellerCenterWebJobActor - Error in OrchestrateSellerCenterMessage, shutting down actors | {ex.Message}"));
                    
                    foreach (var currentActor in _tenantActors)
                    {
                        await currentActor.Value.Ask(new StopSellerCenterTenantMessage(), _cancellationToken);
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

                            if (tenant != null)
                            {
                                if (_tenantActors.ContainsKey(tenant.Id))
                                {                                    
                                    var currentActor = _tenantActors[tenant.Id];
                                    var updateMessage = new UpdateSellerCenterTenantMessage { Data = new SellerCenterDataMessage(tenant) };
                                    var result = await currentActor.Ask<ReturnMessage>(updateMessage, _cancellationToken);
                                    if (result.Result == Result.Error)
                                    {
                                        _log.Warning($"SellerCenterWebJobActor - Error updating actor {tenant.Id} | {result.Error.Message}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"SellerCenterWebJobActor - Error updating actor {tenant.Id}"));


                                        await currentActor.Ask(new StopSellerCenterTenantMessage(), _cancellationToken); //error in update, stop actor
                                        _tenantActors.Remove(tenant.Id);
                                    }                                   
                                }
                                else
                                {
                                    _log.Info($"SellerCenterWebJobActor - Creating actor {tenant.Id}");
                                    var newActor = Context.ActorOf(SellerCenterTenantActor.Props(_serviceProvider, _cancellationToken));
                                    var createMessage = new InitializeSellerCenterTenantMessage { Data = new SellerCenterDataMessage(tenant) };
                                    var result = await newActor.Ask<ReturnMessage>(createMessage, _cancellationToken);
                                    if (result.Result == Result.OK)
                                    {
                                        _log.Info($"SellerCenterWebJobActor - Actor {tenant.Id} created successfully");
                                        _tenantActors.Add(tenant.Id, newActor);
                                    }
                                    else
                                    {
                                        _log.Warning($"SellerCenterWebJobActor - Error updating actor {tenant.Id} | {result.Error.Message}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"SellerCenterWebJobActor - Error updating actor {tenant.Id}"));                                        
                                        await newActor.Ask(new StopSellerCenterTenantMessage(), _cancellationToken); //could not create, stop it
                                    }
                                }
                            }
                            else
                            {
                                if (_tenantActors.ContainsKey(message.TenantId))
                                {
                                    var currentActor = _tenantActors[message.TenantId];                                    
                                    await currentActor.Ask(new StopSellerCenterTenantMessage(), _cancellationToken);
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
            return Akka.Actor.Props.Create(() => new SellerCenterWebJobActor(serviceProvider, cancellationToken));
        }
    }
}
