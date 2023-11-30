using Akka.Actor;
using Akka.Dispatch;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Extensions;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Millennium;
using Samurai.Integration.Domain.Messages.Shared;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Queues;
using Samurai.Integration.Domain.Results.Logger;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.Millennium
{
    public class MillenniumProductActor : BaseMillenniumTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly IActorRef _apiActorGroup;
        private readonly SellerCenterQueue.Queues _sellerQueues;
        private readonly QueueClient _millenniumFullProductQueueClient;
        private readonly QueueClient _erpFullProductQueueClient;
        private readonly QueueClient _erpPartialProductQueueClient;
        private readonly QueueClient _ecommerceUpdateProductImageQueueClient;
        private readonly QueueClient _erpPriceQueueClient;
        private readonly QueueClient _erpStockQueueClient;
        private readonly QueueClient _millenniumProcessProductImageQueueClient;


        public MillenniumProductActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, MillenniumData millenniumData, IActorRef apiActorGroup)
            : base("MillenniumProductActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _millenniumData = millenniumData;
            _apiActorGroup = apiActorGroup;

            using (var scope = _serviceProvider.CreateScope())
            {
                var tenantService = scope.ServiceProvider.GetService<TenantService>();
                if (_millenniumData.IntegrationType == IntegrationType.Shopify)
                {
                    _erpFullProductQueueClient = tenantService.GetQueueClient(_millenniumData, ShopifyQueue.UpdateFullProductQueue);
                    _erpPartialProductQueueClient = tenantService.GetQueueClient(_millenniumData, ShopifyQueue.UpdatePartialProductQueue);
                    _erpPriceQueueClient = tenantService.GetQueueClient(_millenniumData, ShopifyQueue.UpdatePriceQueue);
                    _erpStockQueueClient = tenantService.GetQueueClient(_millenniumData, ShopifyQueue.UpdateStockQueue);
                    _ecommerceUpdateProductImageQueueClient = tenantService.GetQueueClient(_millenniumData, ShopifyQueue.UpdateProductImagesQueue);
                }
                else
                {
                    _sellerQueues = new SellerCenterQueue.Queues
                    {
                        ProcessVariationOptionsProductQueue = tenantService.GetQueueClient(_millenniumData, SellerCenterQueue.ProcessVariationOptionsProductQueue),
                        ProcessCategoriesProductQueue = tenantService.GetQueueClient(_millenniumData, SellerCenterQueue.ProcessCategoriesProductQueue),
                        ProcessManufacturersProductQueue = tenantService.GetQueueClient(_millenniumData, SellerCenterQueue.ProcessManufacturersProductQueue),
                        UpdatePriceQueue = tenantService.GetQueueClient(_millenniumData, SellerCenterQueue.UpdatePriceQueue),
                        UpdateStockProductQueue = tenantService.GetQueueClient(_millenniumData, SellerCenterQueue.UpdateStockProductQueue)

                    };

                    _erpPartialProductQueueClient = tenantService.GetQueueClient(_millenniumData, SellerCenterQueue.CreateProductQueue);

                }
                _millenniumFullProductQueueClient = tenantService.GetQueueClient(_millenniumData, MillenniumQueue.ListFullProductQueue);
                _millenniumProcessProductImageQueueClient = tenantService.GetQueueClient(_millenniumData, MillenniumQueue.ProcessProductImageQueue);

            }

            ReceiveAsync((Func<MillenniumListNewProductsMessage, Task>)(async message =>
            {
                try
                {
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var millenniumService = scope.ServiceProvider.GetService<MillenniumService>();
                        millenniumService.Init(new ActorRefWrapper(_apiActorGroup), GetLog());
                        result = await millenniumService.ListNewProducts(_millenniumData, _sellerQueues, _erpPartialProductQueueClient, _millenniumProcessProductImageQueueClient, _millenniumFullProductQueueClient, _cancellationToken);
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<MillenniumListNewStocksMessage, Task>)(async message =>
            {
                try
                {
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var millenniumService = scope.ServiceProvider.GetService<MillenniumService>();
                        millenniumService.Init(new ActorRefWrapper(_apiActorGroup), GetLog());
                        result = await millenniumService.ListNewStocks(_millenniumData, _sellerQueues, _erpStockQueueClient, _cancellationToken);
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<MillenniumListNewPricesMessage, Task>)(async message =>
            {
                try
                {
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var millenniumService = scope.ServiceProvider.GetService<MillenniumService>();
                        millenniumService.Init(new ActorRefWrapper(_apiActorGroup), GetLog());

                        result = await millenniumService.ListNewPrices(_millenniumData, _millenniumFullProductQueueClient, _erpPriceQueueClient, _sellerQueues, _cancellationToken);
                    }

                    if (_millenniumData.HasZeroedPriceCase)
                        LogInfo(LoggerDescription.FromProduct(_millenniumData.Id.ToString(), "HasZroedPriceCase", "MillenniumListNewPricesMessage", message, result));

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ShopifyListERPFullProductMessage, Task>)(async message =>
            {
                try
                {
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var millenniumService = scope.ServiceProvider.GetService<MillenniumService>();
                        millenniumService.Init(new ActorRefWrapper(_apiActorGroup), GetLog());
                        result = await millenniumService.ListProduct(message.ExternalId, _millenniumData, _erpFullProductQueueClient, _millenniumProcessProductImageQueueClient, _cancellationToken);
                    }

                    if (_millenniumData.HasTenantLogging)
                        LogInfo(LoggerDescription.FromProduct(_millenniumData.Id.ToString(), message.ExternalId, "ShopifyListERPFullProductMessage", message, result));

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(LoggerDescription.FromProduct(_millenniumData.Id.ToString(), message.ExternalId, "ShopifyListERPFullProductMessage", message, ex));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<GetPriceProductMessage, Task>)(async message =>
            {
                try
                {
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var millenniumService = scope.ServiceProvider.GetService<MillenniumService>();
                        millenniumService.Init(new ActorRefWrapper(_apiActorGroup), GetLog());
                        result = await millenniumService.GetPriceProduct(message.CodProduto, _millenniumData, _sellerQueues, _cancellationToken);
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ProcessProductImageMessage, Task>)(async message =>
            {
                try
                {
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var millenniumService = scope.ServiceProvider.GetService<MillenniumService>();
                        millenniumService.Init(new ActorRefWrapper(_apiActorGroup), GetLog());
                        result = await millenniumService.ProcessProductImage(message, _millenniumData, _ecommerceUpdateProductImageQueueClient, _cancellationToken);
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<MillenniumListNewStockMtoMessage, Task>)(async message =>
            {
                try
                {
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var millenniumService = scope.ServiceProvider.GetService<MillenniumService>();
                        millenniumService.Init(new ActorRefWrapper(_apiActorGroup), GetLog());
                        result = await millenniumService.ListNewStockMto(_millenniumData, _sellerQueues, _erpStockQueueClient, _cancellationToken);
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));
        }

        protected override void PostStop()
        {
            base.PostStop();
            ActorTaskScheduler.RunTask(async () =>
            {
                await _millenniumFullProductQueueClient.CloseAsyncSafe();
                await _millenniumProcessProductImageQueueClient.CloseAsyncSafe();
                await _erpFullProductQueueClient.CloseAsyncSafe();
                await _erpPartialProductQueueClient.CloseAsyncSafe();
                await _erpPriceQueueClient.CloseAsyncSafe();
                await _erpStockQueueClient.CloseAsyncSafe();
                await _ecommerceUpdateProductImageQueueClient.CloseAsyncSafe();
                await _sellerQueues.ProcessVariationOptionsProductQueue.CloseAsyncSafe();
                await _sellerQueues.ProcessCategoriesProductQueue.CloseAsyncSafe();
                await _sellerQueues.ProcessManufacturersProductQueue.CloseAsyncSafe();
                await _sellerQueues.UpdatePriceQueue.CloseAsyncSafe();
                await _sellerQueues.UpdateStockProductQueue.CloseAsyncSafe();

            });
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, MillenniumData millenniumData, IActorRef apiActorGroup)
        {
            return Akka.Actor.Props.Create(() => new MillenniumProductActor(serviceProvider, cancellationToken, millenniumData, apiActorGroup));
        }
    }
}
