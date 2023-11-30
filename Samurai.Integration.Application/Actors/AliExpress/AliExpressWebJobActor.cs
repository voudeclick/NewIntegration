using Akka.Actor;
using Akka.Event;
using Akka.Logger.Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.APIClient.AliExpress;
using Samurai.Integration.APIClient.AliExpress.Models.Response;
using Samurai.Integration.Application.Actors.AliExpress.Tray;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.AliExpress;
using Samurai.Integration.Domain.Messages.Webjob;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace Samurai.Integration.Application.Actors.AliExpress
{
    public class AliExpressWebJobActor : ReceiveActor
    {

        private readonly ILoggingAdapter _log;
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly AliExpressSdkClient _client;

        private Dictionary<long, IActorRef> _tenantActors;

        public AliExpressWebJobActor(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            _log = Context.GetLogger<SerilogLoggingAdapter>();
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;

            _tenantActors = new Dictionary<long, IActorRef>();


            ReceiveAsync<OrchestratorAliExpressMessage>(async message =>
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var tenantRepository = scope.ServiceProvider.GetService<TenantRepository>();
                        var tenants = await tenantRepository.GetActiveByTenantType(TenantType.AliExpress, _cancellationToken);

                        var _configuration = scope.ServiceProvider.GetService<IConfiguration>();

                        var httpClientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();

                        //tenants = tenants.Where(x => x.StoreHandle == "1021540").ToList();

                        foreach (var tenant in tenants)
                        {
                            if (_tenantActors.ContainsKey(tenant.Id))
                            {
                                var currentActor = _tenantActors[tenant.Id];

                                var updateMessage = new UpdateAliExpressTenantMessage { Data = new TenantDataMessage(tenant) };

                                var result = await currentActor.Ask<ReturnMessage>(updateMessage, _cancellationToken);
                                if (result.Result == Result.Error)
                                {
                                    await currentActor.Ask(new StopAliExpressTenantMessage(), _cancellationToken); //error in update, stop actor

                                    _tenantActors.Remove(tenant.Id);
                                }
                            }
                            else
                            {
                                var newActor = Context.ActorOf(TrayAliExpressTenantActor.Props(_serviceProvider, _cancellationToken));

                                var createMessage = new InitializeAliExpressTenantMessage { Data = new TenantDataMessage(tenant) };

                                var result = await newActor.Ask<ReturnMessage>(createMessage, _cancellationToken);
                                if (result.Result == Result.OK)
                                {
                                    _tenantActors.Add(tenant.Id, newActor);
                                }
                                else
                                {
                                    await newActor.Ask(new StopAliExpressTenantMessage(), _cancellationToken);
                                }
                            }
                        }


                        foreach (var currentActor in _tenantActors)
                        {
                            if (tenants.All(x => x.Id != currentActor.Key))
                            {
                                await currentActor.Value.Ask(new StopAliExpressTenantMessage(), _cancellationToken);

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
                        await currentActor.Value.Ask(new StopAliExpressTenantMessage(), _cancellationToken);

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

                            if (tenant != null && tenant.Type == TenantType.AliExpress)
                            {
                                if (_tenantActors.ContainsKey(tenant.Id))
                                {
                                    var currentActor = _tenantActors[tenant.Id];

                                    var updateMessage = new UpdateAliExpressTenantMessage { Data = new TenantDataMessage(tenant) };

                                    var result = await currentActor.Ask<ReturnMessage>(updateMessage, _cancellationToken);
                                    if (result.Result == Result.Error)
                                    {
                                        await currentActor.Ask(new StopAliExpressTenantMessage(), _cancellationToken); //error in update, stop actor

                                        _tenantActors.Remove(tenant.Id);
                                    }
                                }
                                else
                                {
                                    var newActor = Context.ActorOf(TrayAliExpressTenantActor.Props(_serviceProvider, _cancellationToken));

                                    var createMessage = new InitializeAliExpressTenantMessage { Data = new TenantDataMessage(tenant) };

                                    var result = await newActor.Ask<ReturnMessage>(createMessage, _cancellationToken);
                                    if (result.Result == Result.OK)
                                    {
                                        _tenantActors.Add(tenant.Id, newActor);
                                    }
                                    else
                                    {
                                        await newActor.Ask(new StopAliExpressTenantMessage(), _cancellationToken);
                                    }
                                }
                            }
                            else
                            {
                                if (_tenantActors.ContainsKey(message.TenantId))
                                {
                                    var currentActor = _tenantActors[message.TenantId];

                                    await currentActor.Ask(new StopAliExpressTenantMessage(), _cancellationToken);

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
            return Akka.Actor.Props.Create(() => new AliExpressWebJobActor(serviceProvider, cancellationToken));
        }
    }
}
