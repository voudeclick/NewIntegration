using Akka.Actor;
using Akka.Dispatch;

using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Extensions;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.SellerCenter;
using Samurai.Integration.Domain.Messages.SellerCenter.ProductActor;
using Samurai.Integration.Domain.Queues;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.SellerCenter
{
    public class SellerCenterProductActor : BaseSellerCenterTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly IActorRef _apiActorGroup;
        private readonly SellerCenterQueue.Queues _queues;

        public SellerCenterProductActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, SellerCenterDataMessage sellerCenterData, IActorRef apiActorGroup)
            : base("SellerCenterProductActor")
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
                    CreateProductQueue = tenantService.GetQueueClient(_sellerCenterData, SellerCenterQueue.CreateProductQueue),
                    GetPriceProduct = tenantService.GetQueueClient(_sellerCenterData, SellerCenterQueue.GetPriceProductQueueErp(sellerCenterData)),
                    UpdatePriceQueue = tenantService.GetQueueClient(_sellerCenterData, SellerCenterQueue.UpdatePriceQueue),
                    UpdateStockProductQueue = tenantService.GetQueueClient(_sellerCenterData, SellerCenterQueue.UpdateStockProductQueue),
                };
            }

            ReceiveAsync((Func<SellerCenterCreateProductMessage, Task>)(async message =>
            {
                try
                {
                    LogDebug($"Starting SellerCenterProductMessage id: {message.ProductInfo.ShopifyId?.ToString() ?? message.ProductInfo.ExternalId}");
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var sellerCenterService = scope.ServiceProvider.GetService<SellerCenterService>();
                        sellerCenterService.Init(_apiActorGroup, GetLog());
                        result = await sellerCenterService.CreateProduct(message, _sellerCenterData, _queues, _cancellationToken);
                    }
                    LogDebug("Ending SellerCenterProductMessage");
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterProductActor - Error in SellerCenterCreateProductMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            #region VariationOptions
            ReceiveAsync((Func<ProcessVariationOptionsMessage, Task>)(async message =>
            {
                try
                {
                    LogDebug($"Starting ProcessVariationOptionsMessage");
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var sellerCenterService = scope.ServiceProvider.GetService<SellerCenterService>();
                        sellerCenterService.Init(_apiActorGroup, GetLog());
                        result = await sellerCenterService.ProcessVariationOptionsProduct(message, _cancellationToken);
                    }
                    LogDebug("Ending ProcessVariationOptionsMessage");
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterProductActor - Error in ProcessVariationOptionsMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));
            #endregion

            #region Categories
            ReceiveAsync((Func<ProcessCategoriesProductMessage, Task>)(async message =>
            {
                try
                {
                    LogDebug($"Starting ProcessCategoriesProductMessage");
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var sellerCenterService = scope.ServiceProvider.GetService<SellerCenterService>();
                        sellerCenterService.Init(_apiActorGroup, GetLog());
                        result = await sellerCenterService.ProcessCategories(message, _cancellationToken);
                    }
                    LogDebug("Ending ProcessCategoriesProductMessage");
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterProductActor - Error in ProcessCategoriesProductMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));
            #endregion

            #region Manufacturers
            ReceiveAsync((Func<ProcessManufacturersMessage, Task>)(async message =>
            {
                try
                {
                    LogDebug($"Starting ProcessManufacturersMessage");
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var sellerCenterService = scope.ServiceProvider.GetService<SellerCenterService>();
                        sellerCenterService.Init(_apiActorGroup, GetLog());
                        result = await sellerCenterService.ProcessManufacturers(message, _cancellationToken);
                    }
                    LogDebug("Ending ProcessManufacturersMessage");
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterProductActor - Error in ProcessManufacturersMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));
            #endregion

            ReceiveAsync((Func<SellerCenterUpdatePriceAndStockMessage, Task>)(async message =>
            {
                try
                {
                    LogDebug($"Starting SellerCenterUpdatePriceMessage");
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var sellerCenterService = scope.ServiceProvider.GetService<SellerCenterService>();
                        sellerCenterService.Init(_apiActorGroup, GetLog());
                        result = await sellerCenterService.UpdatePriceAndStockProduct(message, sellerCenterData, _cancellationToken);
                    }
                    LogDebug("Ending SellerCenterUpdatePriceMessage");
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterProductActor - Error in SellerCenterUpdatePriceMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<SellerCenterUpdateStockProductMessage, Task>)(async message =>
            {
                try
                {
                    LogDebug($"Starting SellerCenterUpdateStockProductMessage");
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var sellerCenterService = scope.ServiceProvider.GetService<SellerCenterService>();
                        sellerCenterService.Init(_apiActorGroup, GetLog());
                        result = await sellerCenterService.UpdateStockProduct(message, sellerCenterData, _cancellationToken);
                    }
                    LogDebug("Ending SellerCenterUpdateStockProductMessage");
                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, "SellerCenterProductActor - Error in SellerCenterUpdateStockProductMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));
        }

        protected override void PostStop()
        {
            base.PostStop();
            ActorTaskScheduler.RunTask(async () =>
            {
                await _queues.CreateProductQueue.CloseAsyncSafe();
                await _queues.GetPriceProduct.CloseAsyncSafe();
            });
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, SellerCenterDataMessage millenniumData, IActorRef apiActorGroup)
        {
            return Akka.Actor.Props.Create(() => new SellerCenterProductActor(serviceProvider, cancellationToken, millenniumData, apiActorGroup));
        }
    }
}
