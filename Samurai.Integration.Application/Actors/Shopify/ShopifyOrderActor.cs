﻿using Akka.Actor;
using Akka.Dispatch;
using Akka.Event;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.APIClient.Shopify.Models.Request;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Extensions;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Messages.Shopify.OrderActor;
using Samurai.Integration.Domain.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.Shopify
{
    public class ShopifyOrderActor : BaseShopifyTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly IActorRef _apiActorGroup;
        private readonly QueueClient _listOrderQueueClient;
        private readonly QueueClient _updateOrderNumberTagQueueClient;
        private readonly QueueClient _updateOrderQueueClient;
        private readonly QueueClient _updateStockQueueClient;


        public ShopifyOrderActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, ShopifyDataMessage shopifyData, IActorRef apiActorGroup)
            : base("ShopifyOrderActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _shopifyData = shopifyData;
            _apiActorGroup = apiActorGroup;

            using (var scope = _serviceProvider.CreateScope())
            {
                var tenantService = scope.ServiceProvider.GetService<TenantService>();

                _listOrderQueueClient = tenantService.GetQueueClient(_shopifyData, ShopifyQueue.ListOrderQueue);

                _updateOrderNumberTagQueueClient = tenantService.GetQueueClient(_shopifyData, ShopifyQueue.UpdateOrderNumberTagQueue);
                _updateStockQueueClient = tenantService.GetQueueClient(_shopifyData, ShopifyQueue.UpdateStockQueue);


                _updateOrderQueueClient = shopifyData.Type switch
                {
                    TenantType.Millennium => tenantService.GetQueueClient(_shopifyData, MillenniumQueue.UpdateOrderQueue),
                    TenantType.Nexaas => tenantService.GetQueueClient(_shopifyData, NexaasQueue.UpdateOrderQueue),
                    TenantType.Omie => tenantService.GetQueueClient(_shopifyData, OmieQueue.UpdateOrderQueue),
                    TenantType.Bling => tenantService.GetQueueClient(_shopifyData, BlingQueue.CreateOrderQueue),
                    _ => null
                };
            }

            ReceiveAsync((Func<OrderByDateQuery, Task>)(async message =>
            {
                try
                {
                    var response = await _apiActorGroup.Ask<ReturnMessage<OrderByDateQueryOutput>>(message, _cancellationToken);

                    Sender.Tell(response);
                }
                catch (Exception ex)
                {
                    LogError(ex, "ShopifyOrderActor -  Error in OrderByDateQuery", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ShopifyEnqueueListOrderMessage, Task>)(async message =>
            {
                try
                {
                    await Task.WhenAll(message.OrderIds.Select(o =>
                                                                _listOrderQueueClient.SendAsync(
                                                                    new ServiceBusMessage(
                                                                        new ShopifyListOrderMessage
                                                                        {
                                                                            ShopifyId = o
                                                                        })
                                                                    .GetMessage(o))));

                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    LogError(ex, "ShopifyOrderActor -  Error in ShopifyEnqueueListOrderMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ShopifyListOrderMessage, Task>)(async message =>
            {
                try
                {
                    ReturnMessage result = null;

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var shopifyService = scope.ServiceProvider.GetService<ShopifyService>();
                        shopifyService.Init(_apiActorGroup, GetLog(), _shopifyData);
                        result = await shopifyService.ListOrder(message, _shopifyData, _updateOrderQueueClient, _updateOrderNumberTagQueueClient, _updateStockQueueClient, _cancellationToken);
                    }

                    Sender.Tell(new ReturnMessage { Result = Result.Error});
                }
                catch (Exception ex)
                {
                    LogError(ex, "ShopifyOrderActor -  Error in ShopifyListOrderMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ShopifyUpdateOrderTagNumberMessage, Task>)(async message =>
            {
                try
                {
                    ReturnMessage result;                    
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var shopifyService = scope.ServiceProvider.GetService<ShopifyService>();
                        shopifyService.Init(_apiActorGroup, GetLog(), _shopifyData);
                        result = await shopifyService.UpdateOrderTagNumber(message, _shopifyData, _cancellationToken);
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "ShopifyOrderActor -  Error in ShopifyUpdateOrderTagNumberMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ShopifyUpdateOrderStatusMessage, Task>)(async message =>
            {
                try
                {
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var shopifyService = scope.ServiceProvider.GetService<ShopifyService>();
                        shopifyService.Init(_apiActorGroup, GetLog(), _shopifyData);
                        result = await shopifyService.UpdateOrderStatus(message, _shopifyData, _cancellationToken);
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "ShopifyOrderActor -  Error in ShopifyUpdateOrderStatusMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ShopifyUpdateTrackingOrder, Task>)(async message =>
            {
                try
                {
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var shopifyService = scope.ServiceProvider.GetService<ShopifyService>();
                        shopifyService.Init(_apiActorGroup, GetLog(), _shopifyData);
                        result = await shopifyService.UpdateTrackingOrder(message, _shopifyData, _cancellationToken);
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "ShopifyOrderActor -  Error in ShopifyUpdateTrackingOrder", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));
        }

        protected override void PostStop()
        {
            base.PostStop();
            ActorTaskScheduler.RunTask(async () =>
            {
                await _updateStockQueueClient.CloseAsyncSafe();
                await _updateOrderNumberTagQueueClient.CloseAsyncSafe();
                await _updateOrderQueueClient.CloseAsyncSafe();

            });
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, ShopifyDataMessage shopifyData, IActorRef apiActorGroup)
        {
            return Akka.Actor.Props.Create(() => new ShopifyOrderActor(serviceProvider, cancellationToken, shopifyData, apiActorGroup));
        }
    }
}
