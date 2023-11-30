using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Messages.Webjob;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Samurai.Integration.Application.Actors.Shopify
{
    public class ShopifyWebJobActor : BaseShopifyWebjobActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private Dictionary<long, IActorRef> _tenantActors;        

        public ShopifyWebJobActor(IServiceProvider serviceProvider, CancellationToken cancellationToken) : base("ShopifyWebJobActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _tenantActors = new Dictionary<long, IActorRef>();            

            ReceiveAsync<OrchestrateShopifyMessage>(async message =>
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var tenantRepository = scope.ServiceProvider.GetService<TenantRepository>();
                        var tenants = await tenantRepository.GetActiveByIntegrationType(IntegrationType.Shopify, _cancellationToken);
                        tenants = tenants.Where(x => x.Id == 16);
                        foreach (var tenant in tenants)
                        {
                            try
                            {                                
                                storeHandle = tenant.StoreHandle;
                                if (_tenantActors.ContainsKey(tenant.Id))
                                {
                                    LogWarning("(Shopify) Atualizando atores tenantId: {0}", tenant.Id);
                                    var currentActor = _tenantActors[tenant.Id];
                                    var updateMessage = new UpdateShopifyTenantMessage { Data = new ShopifyDataMessage(tenant) };
                                    var result = await currentActor.Ask<ReturnMessage>(updateMessage, _cancellationToken);
                                    if (result.Result == Result.Error)
                                    {
                                        await currentActor.Ask(new StopShopifyTenantMessage(), _cancellationToken); //error in update, stop actor
                                        _tenantActors.Remove(tenant.Id);
                                    }
                                }
                                else
                                {
                                    LogWarning("(Shopify) Inicializando atores tenantId: {0}", tenant.Id);
                                    var newActor = Context.ActorOf(ShopifyTenantActor.Props(_serviceProvider, _cancellationToken));
                                    var createMessage = new InitializeShopifyTenantMessage { Data = new ShopifyDataMessage(tenant) };
                                    var result = await newActor.Ask<ReturnMessage>(createMessage, _cancellationToken);
                                    if (result.Result == Result.OK)
                                        _tenantActors.Add(tenant.Id, newActor);
                                    else
                                        await newActor.Ask(new StopShopifyTenantMessage(), _cancellationToken); //could not create, stop it
                                }
                            }
                            catch (Exception ex)
                            {
                                LogError(ex, "(Shopify) Erro ao inicializar atores - tenantId {0}", tenant.Id);
                            }
                        }

                        foreach (var currentActor in _tenantActors)
                        {
                            if (tenants.All(x => x.Id != currentActor.Key))
                            {
                                await currentActor.Value.Ask(new StopShopifyTenantMessage(), _cancellationToken);
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
                        await currentActor.Value.Ask(new StopShopifyTenantMessage(), _cancellationToken);
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

                            if (tenant != null && tenant.Id != 70)
                            {
                                if (_tenantActors.ContainsKey(tenant.Id))
                                {
                                    var currentActor = _tenantActors[tenant.Id];
                                    var updateMessage = new UpdateShopifyTenantMessage { Data = new ShopifyDataMessage(tenant) };
                                    var result = await currentActor.Ask<ReturnMessage>(updateMessage, _cancellationToken);
                                    if (result.Result == Result.Error)
                                    {
                                        await currentActor.Ask(new StopShopifyTenantMessage(), _cancellationToken); //error in update, stop actor
                                        _tenantActors.Remove(tenant.Id);
                                    }
                                }
                                else
                                {
                                    var newActor = Context.ActorOf(ShopifyTenantActor.Props(_serviceProvider, _cancellationToken));
                                    var createMessage = new InitializeShopifyTenantMessage { Data = new ShopifyDataMessage(tenant) };
                                    var result = await newActor.Ask<ReturnMessage>(createMessage, _cancellationToken);
                                    if (result.Result == Result.OK)
                                    {
                                        _tenantActors.Add(tenant.Id, newActor);
                                    }
                                    else
                                    {
                                        await newActor.Ask(new StopShopifyTenantMessage(), _cancellationToken); //could not create, stop it
                                    }
                                }
                            }
                            else
                            {
                                if (_tenantActors.ContainsKey(message.TenantId))
                                {
                                    var currentActor = _tenantActors[message.TenantId];
                                    await currentActor.Ask(new StopShopifyTenantMessage(), _cancellationToken);
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
            return Akka.Actor.Props.Create(() => new ShopifyWebJobActor(serviceProvider, cancellationToken));
        }
    }
}
