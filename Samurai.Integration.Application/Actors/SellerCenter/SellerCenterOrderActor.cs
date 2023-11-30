using Akka.Actor;
using Akka.Dispatch;

using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Extensions;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.SellerCenter;
using Samurai.Integration.Domain.Messages.SellerCenter.OrderActor;
using Samurai.Integration.Domain.Queues;

using System;
using System.Threading;
using System.Threading.Tasks;


namespace Samurai.Integration.Application.Actors.SellerCenter
{
    public class SellerCenterOrderActor : BaseSellerCenterTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly IActorRef _apiActorGroup;
        private readonly SellerCenterQueue.Queues _queues;

        public SellerCenterOrderActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, SellerCenterDataMessage sellerCenterData, IActorRef apiActorGroup)
            : base("SellerCenterOrderActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _sellerCenterData = sellerCenterData;
            _apiActorGroup = apiActorGroup;

            using (var scope = _serviceProvider.CreateScope())
            {
                var tenantService = scope.ServiceProvider.GetService<TenantService>();

                _queues = new SellerCenterQueue.Queues
                {

                    ProcessOrderQueue = tenantService.GetQueueClient(_sellerCenterData, SellerCenterQueue.ProcessOrderQueue),
                    CreateOrderQueueERP = tenantService.GetQueueClient(_sellerCenterData, SellerCenterQueue.GetCreateOrderQueuerErp(sellerCenterData)),
                    UpdatePartialOrderSeller = tenantService.GetQueueClient(_sellerCenterData, SellerCenterQueue.UpdatePartialOrderSeller),
                    UpdateOrderSellerDeliveryPackage = tenantService.GetQueueClient(_sellerCenterData, SellerCenterQueue.UpdateOrderSellerDeliveryPackage),
                    ListNewOrdersQueue = tenantService.GetQueueClient(_sellerCenterData, SellerCenterQueue.ListNewOrdersQueue)
                };

            }

            ReceiveAsync((Func<ListNewOrdersMessage, Task>)(async message =>
            {
                try
                {
                    LogDebug($"Starting ListNewOrdersTaskAsync");
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var sellerCenterService = scope.ServiceProvider.GetService<SellerCenterService>();
                        sellerCenterService.Init(_apiActorGroup, GetLog());
                        result = await sellerCenterService.Order.ListNewOrders(sellerCenterData, _queues, _cancellationToken);
                    }
                    LogDebug("Ending ListNewOrdersTaskAsync");
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterOrderActor - SellerCenterOrderActor - Error in ListNewOrdersTaskAsync", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in ListNewOrdersTaskAsync | {ex.Message}"));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ProcessOrderMessage, Task>)(async message =>
            {
                try
                {
                    LogDebug($"Starting ProcessOrderMessage id: {message.Id}");
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var serviceBusMessage = new ServiceBusMessage(message);
                        await _queues.CreateOrderQueueERP.SendAsync(serviceBusMessage.GetMessage(message.Id));
                        result = new ReturnMessage { Result = Result.OK };
                    }
                    LogDebug("Ending ProcessOrderMessage");
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterOrderActor - SellerCenterOrderActor - Error in ProcessOrderMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in ProcessOrderMessage | {ex.Message}"));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<UpdateOrderStatusMessage, Task>)(async message =>
            {
                try
                {
                    LogDebug($"Starting UpdateOrderStatusMessage id: {message.OrderExternalId}");
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var sellerCenterService = scope.ServiceProvider.GetService<SellerCenterService>();
                        sellerCenterService.Init(_apiActorGroup, GetLog());
                        result = await sellerCenterService.Order.UpdateOrderStatus(message, _cancellationToken);
                    }
                    LogDebug("Ending UpdateOrderStatusMessage");
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterOrderActor - SellerCenterOrderActor - Error in SellerCenterApiGetVariationByFilterRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in SellerCenterApiGetVariationByFilterRequest | {ex.Message}"));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<UpdatePartialOrderSellerMessage, Task>)(async message =>
            {
                try
                {
                    LogDebug($"Starting UpdatePartialOrderSellerMessage id: {message.OrderId}");
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var sellerCenterService = scope.ServiceProvider.GetService<SellerCenterService>();
                        sellerCenterService.Init(_apiActorGroup, GetLog());
                        result = await sellerCenterService.Order.UpdatePartialOrderSeller(_sellerCenterData, message, _cancellationToken);
                    }
                    LogDebug("Ending UpdatePartialOrderSellerMessage");
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterOrderActor - Error in SellerCenterApiGetVariationByFilterRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in SellerCenterApiGetVariationByFilterRequest | {ex.Message}"));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<UpdateOrderSellerDeliveryPackageMessage, Task>)(async message =>
            {
                try
                {
                    LogDebug($"Starting UpdatePartialOrderSellerDeliveryPackageMessage id: {message.OrderId}");
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var sellerCenterService = scope.ServiceProvider.GetService<SellerCenterService>();
                        sellerCenterService.Init(_apiActorGroup, GetLog());
                        result = await sellerCenterService.Order.UpdatePartialOrderSellerDeliveryPackage(_sellerCenterData, message, _cancellationToken);
                    }
                    LogDebug("Ending UpdatePartialOrderSellerDeliveryPackageMessage");
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterOrderActor - Error in SellerCenterApiGetVariationByFilterRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in SellerCenterApiGetVariationByFilterRequest | {ex.Message}"));

                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ListOrderMessage, Task>)(async message =>
            {
                try
                {
                    LogDebug($"Starting ListOrderMessage OrderNumber: {message.OrderNumber}");
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var sellerCenterService = scope.ServiceProvider.GetService<SellerCenterService>();
                        sellerCenterService.Init(_apiActorGroup, GetLog());
                        result = await sellerCenterService.Order.ListOrder(message, sellerCenterData, _queues, _cancellationToken);
                    }
                    LogDebug("Ending ListOrderMessage");
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterOrderActor - Error in SellerCenterApiGetVariationByFilterRequest", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Error in SellerCenterApiGetVariationByFilterRequest | {ex.Message}"));

                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));
        }

        protected override void PostStop()
        {
            base.PostStop();
            ActorTaskScheduler.RunTask(async () =>
            {
                await _queues.ProcessOrderQueue.CloseAsyncSafe();
                await _queues.CreateOrderQueueERP.CloseAsyncSafe();
            });
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, SellerCenterDataMessage millenniumData, IActorRef apiActorGroup)
        {
            return Akka.Actor.Props.Create(() => new SellerCenterOrderActor(serviceProvider, cancellationToken, millenniumData, apiActorGroup));
        }

    }
}

