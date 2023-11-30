using Akka.Actor;
using Akka.Dispatch;
using Akka.Event;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.APIClient.Omie.Models.Request.FamiliaCadastro;
using Samurai.Integration.APIClient.Omie.Models.Result.FamiliaCadastro;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Omie;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.Omie
{
    public class OmieProductActor : BaseOmieTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly IActorRef _apiActorGroup;
        private readonly QueueClient _omieFullProductQueueClient;
        private readonly QueueClient _shopifyFullProductQueueClient;
        private readonly QueueClient _shopifyPartialProductQueueClient;
        private readonly QueueClient _shopifyPartialSkuQueueClient;
        private readonly QueueClient _shopifyStockQueueClient;

        public OmieProductActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, OmieData omieData, IActorRef apiActorGroup)
            : base("OmieProductActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _omieData = omieData;
            _apiActorGroup = apiActorGroup;


            using (var scope = _serviceProvider.CreateScope())
            {
                var tenantService = scope.ServiceProvider.GetService<TenantService>();
                _omieFullProductQueueClient = tenantService.GetQueueClient(_omieData, OmieQueue.ListFullProductQueue);
                _shopifyFullProductQueueClient = tenantService.GetQueueClient(_omieData, ShopifyQueue.UpdateFullProductQueue);
                _shopifyPartialProductQueueClient = tenantService.GetQueueClient(_omieData, ShopifyQueue.UpdatePartialProductQueue);
                _shopifyPartialSkuQueueClient = tenantService.GetQueueClient(_omieData, ShopifyQueue.UpdatePartialSkuQueue);
                _shopifyStockQueueClient = tenantService.GetQueueClient(_omieData, ShopifyQueue.UpdateStockQueue);
            }

            ReceiveAsync((Func<PesquisarFamiliasOmieRequest, Task>)(async message =>
            {
                try
                {

                    var response = await _apiActorGroup.Ask<ReturnMessage<PesquisarFamiliasOmieRequestOutput>>(message, _cancellationToken);

                    Sender.Tell(response);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in PesquisarFamiliasOmieRequest");
                    Sender.Tell(new ReturnMessage<PesquisarFamiliasOmieRequestOutput> { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<OmieEnqueueFullProductsMessage, Task>)(async message =>
            {
                try
                {

                    await Task.WhenAll(message.ProductsIds.Select(p =>
                                                                _omieFullProductQueueClient.SendAsync(
                                                                    new ServiceBusMessage(
                                                                        new ShopifyListERPFullProductMessage
                                                                        {
                                                                            ExternalId = p.ToString()
                                                                        })
                                                                    .GetMessage(p))));
                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in NexaasEnqueueFullProductsMessage");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ShopifyListERPFullProductMessage, Task>)(async message =>
            {
                try
                {

                    ReturnMessage result = null;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var omieService = scope.ServiceProvider.GetService<OmieService>();
                        omieService.Init(_apiActorGroup, GetLog());
                        result = await omieService.ListProduct(long.Parse(message.ExternalId), _omieData, _shopifyFullProductQueueClient, _cancellationToken);
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in OmieListPartialProductMessage");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<OmieListPartialProductMessage, Task>)(async message =>
            {
                try
                {
                    ReturnMessage result = null;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var omieService = scope.ServiceProvider.GetService<OmieService>();
                        omieService.Init(_apiActorGroup, GetLog());
                        result = await omieService.ListPartialProduct(message, _omieData, _shopifyPartialProductQueueClient, _shopifyPartialSkuQueueClient, _cancellationToken);
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in OmieListPartialProductMessage");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<OmieListStockSkuMessage, Task>)(async message =>
            {
                try
                {

                    ReturnMessage result = null;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var omieService = scope.ServiceProvider.GetService<OmieService>();
                        omieService.Init(_apiActorGroup, GetLog());
                        result = await omieService.ListStockSku(message, _omieData, _shopifyStockQueueClient, _cancellationToken);
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in OmieListStockSkuMessage");
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));
        }

        protected override void PostStop()
        {
            base.PostStop();
            ActorTaskScheduler.RunTask(async () =>
            {
                if (_omieFullProductQueueClient != null && !_omieFullProductQueueClient.IsClosedOrClosing)
                    await _omieFullProductQueueClient.CloseAsync();
                if (_shopifyFullProductQueueClient != null && !_shopifyFullProductQueueClient.IsClosedOrClosing)
                    await _shopifyFullProductQueueClient.CloseAsync();
                if (_shopifyPartialProductQueueClient != null && !_shopifyPartialProductQueueClient.IsClosedOrClosing)
                    await _shopifyPartialProductQueueClient.CloseAsync();
                if (_shopifyPartialSkuQueueClient != null && !_shopifyPartialSkuQueueClient.IsClosedOrClosing)
                    await _shopifyPartialSkuQueueClient.CloseAsync();
                if (_shopifyStockQueueClient != null && !_shopifyStockQueueClient.IsClosedOrClosing)
                    await _shopifyStockQueueClient.CloseAsync();
            });
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, OmieData omieData, IActorRef apiActorGroup)
        {
            return Akka.Actor.Props.Create(() => new OmieProductActor(serviceProvider, cancellationToken, omieData, apiActorGroup));
        }
    }
}
