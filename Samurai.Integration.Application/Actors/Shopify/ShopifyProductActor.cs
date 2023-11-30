using Akka.Actor;
using Akka.Dispatch;

using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Samurai.Integration.APIClient.Shopify.Models.Request;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Entities.Database.Integrations.Shopify;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Queues;
using Samurai.Integration.Domain.Results.Logger;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.Shopify
{
    public class ShopifyProductActor : BaseShopifyTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly IActorRef _apiActorGroup;
        private readonly QueueClient _fullProductQueueClient;
        private readonly QueueClient _updateProductExternalIdQueueClient;
        private readonly QueueClient _updatePartialProductQueueClient;
        private readonly QueueClient _updateProductGroupingQueueClient;
        private readonly QueueClient _updateProductImagesQueueClient;
        private readonly QueueClient _listProductCategoriesQueueClient;
        private readonly QueueClient _updateStockKitQueue;
        private readonly ShopifyProductStockIntegrationRepository _shopifyProductStockIntegrationRepository;

        private readonly ShopifyProductIntegrationRepository _shopifyProductIntegrationRepository;
        private readonly ShopifyProductPriceIntegrationRepository _shopifyProductPriceIntegrationRepository;
        private readonly ShopifyProductImageIntegrationRepository _shopifyProductImageIntegrationRepository;

        public ShopifyProductActor(IServiceProvider serviceProvider,
            CancellationToken cancellationToken,
            ShopifyDataMessage shopifyData,
            IActorRef apiActorGroup)
            : base("ShopifyProductActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _shopifyData = shopifyData;
            _apiActorGroup = apiActorGroup;

            using (var scope = _serviceProvider.CreateScope())
            {
                _shopifyProductStockIntegrationRepository = _serviceProvider.GetService<ShopifyProductStockIntegrationRepository>();
                _shopifyProductIntegrationRepository = _serviceProvider.GetService<ShopifyProductIntegrationRepository>();
                _shopifyProductPriceIntegrationRepository = _serviceProvider.GetService<ShopifyProductPriceIntegrationRepository>();
                _shopifyProductImageIntegrationRepository = _serviceProvider.GetService<ShopifyProductImageIntegrationRepository>();

                var tenantService = scope.ServiceProvider.GetService<TenantService>();

                _fullProductQueueClient = shopifyData.Type switch
                {
                    TenantType.Millennium => tenantService.GetQueueClient(_shopifyData, MillenniumQueue.ListFullProductQueue),
                    TenantType.Nexaas => tenantService.GetQueueClient(_shopifyData, NexaasQueue.ListFullProductQueue),
                    TenantType.Omie => tenantService.GetQueueClient(_shopifyData, OmieQueue.ListFullProductQueue),
                    TenantType.Bling => tenantService.GetQueueClient(_shopifyData, BlingQueue.ListFullProductQueue),
                    _ => throw new NotImplementedException()
                };

                _updateProductExternalIdQueueClient = shopifyData.Type switch
                {
                    _ => null
                };

                _updateStockKitQueue = tenantService.GetQueueClient(_shopifyData, ShopifyQueue.UpdateStockKitQueue);

                _updatePartialProductQueueClient = tenantService.GetQueueClient(_shopifyData, ShopifyQueue.UpdatePartialProductQueue);

                _updateProductGroupingQueueClient = tenantService.GetQueueClient(_shopifyData, ShopifyQueue.UpdateProductGroupingQueue);

                if (_shopifyData.ImageIntegrationEnabled)
                    _updateProductImagesQueueClient = tenantService.GetQueueClient(_shopifyData, ShopifyQueue.UpdateProductImagesQueue);

                _listProductCategoriesQueueClient = shopifyData.Type switch
                {
                    TenantType.Nexaas => tenantService.GetQueueClient(_shopifyData, NexaasQueue.ListProductCategoriesQueue),
                    _ => null
                };
            }

            ReceiveAsync((Func<ShopifyUpdateFullProductMessage, Task>)(async message =>
            {
                try
                {
                    var integration = new ShopifyProductIntegration();

                    try
                    {
                        if (shopifyData.EnableSaveIntegrationInformations)
                        {
                            integration = new ShopifyProductIntegration()
                            {
                                Id = Guid.NewGuid(),
                                TenantId = shopifyData.Id,
                                ProductShopifyId = message.ProductInfo.ShopifyId,
                                Payload = JsonConvert.SerializeObject(message),
                                Status = IntegrationStatus.Received,
                                IntegrationDate = DateTime.Now,
                                ReferenceIntegrationId = message.IntegrationId
                            };
                            await _shopifyProductIntegrationRepository.Save(integration);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, "ShopifyProductActor (ShopifyUpdatePartialProductMessage) - Problemas ao salvar alteração na integração de ShopifyUpdateFullProductMessage | {0}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Problemas ao salvar alteração na integração de preço | {ex.Message}"));
                    }
                    ReturnMessage result;

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var shopifyService = scope.ServiceProvider.GetService<ShopifyService>();
                        shopifyService.Init(_apiActorGroup, GetLog(), _shopifyData);
                        result = await shopifyService.UpdateFullProduct(message, _shopifyData, _updateProductExternalIdQueueClient, _updateProductGroupingQueueClient, _updateProductImagesQueueClient, _cancellationToken);
                    }

                    if (shopifyData.EnableSaveIntegrationInformations)
                    {
                        try
                        {
                            integration.Status = IntegrationStatus.Processed;
                            integration.Result = JsonConvert.SerializeObject(result);

                            await _shopifyProductIntegrationRepository.Save(integration);
                        }
                        catch (Exception ex)
                        {
                            LogError(ex, $"ShopifyProductActor - Problemas ao salvar alteração na integração de ShopifyUpdateFullProductMessage | {ex.Message}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Problemas ao salvar alteração na integração de preço"));
                        }
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ShopifyUpdatePartialProductMessage, Task>)(async message =>
            {
                try
                {
                    var integration = new ShopifyProductIntegration();

                    try
                    {
                        if (shopifyData.EnableSaveIntegrationInformations)
                        {
                            integration = new ShopifyProductIntegration()
                            {
                                Id = Guid.NewGuid(),
                                TenantId = shopifyData.Id,
                                ProductShopifyId = message.ProductInfo.ShopifyId,
                                Payload = JsonConvert.SerializeObject(message),
                                Status = IntegrationStatus.Received,
                                IntegrationDate = DateTime.Now,
                                ReferenceIntegrationId = message.IntegrationId
                            };
                            await _shopifyProductIntegrationRepository.Save(integration);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, "ShopifyProductActor (ShopifyUpdatePartialProductMessage) - Problemas ao salvar alteração na integração de ShopifyUpdatePartialProductMessage | {0}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Problemas ao salvar alteração na integração de preço | {ex.Message}"));
                    }

                    ReturnMessage result;

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var shopifyService = scope.ServiceProvider.GetService<ShopifyService>();
                        shopifyService.Init(_apiActorGroup, GetLog(), _shopifyData);
                        result = await shopifyService.UpdatePartialProduct(message, _shopifyData, _fullProductQueueClient, _updateProductExternalIdQueueClient, _updateProductGroupingQueueClient, _cancellationToken);
                    }

                    if (shopifyData.EnableSaveIntegrationInformations)
                    {
                        try
                        {
                            integration.Status = IntegrationStatus.Processed;
                            integration.Result = JsonConvert.SerializeObject(result);

                            await _shopifyProductIntegrationRepository.Save(integration);
                        }
                        catch (Exception ex)
                        {
                            LogError(ex, $"ShopifyProductActor - Problemas ao salvar alteração na integração de ShopifyUpdatePartialProductMessage | {ex.Message}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Problemas ao salvar alteração na integração de preço"));
                        }
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ShopifyUpdatePartialSkuMessage, Task>)(async message =>
            {
                try
                {
                    ReturnMessage result;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var shopifyService = scope.ServiceProvider.GetService<ShopifyService>();
                        shopifyService.Init(_apiActorGroup, GetLog(), _shopifyData);
                        result = await shopifyService.UpdatePartialSku(message, _shopifyData, _fullProductQueueClient, _cancellationToken);
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ShopifyUpdatePriceMessage, Task>)(async message =>
            {
                try
                {
                    var integration = new ShopifyProductPriceIntegration();

                    try
                    {
                        if (shopifyData.EnableSaveIntegrationInformations)
                        {
                            integration = new ShopifyProductPriceIntegration()
                            {
                                Id = Guid.NewGuid(),
                                TenantId = shopifyData.Id,
                                ProductShopifyId = message.Value.ShopifyId,
                                ProductShopifySku = message.Value.Sku,
                                Payload = JsonConvert.SerializeObject(message),
                                Status = IntegrationStatus.Received,
                                IntegrationDate = DateTime.Now,
                                ReferenceIntegrationId = message.IntegrationId
                            };

                            _shopifyProductPriceIntegrationRepository.Save(integration);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, $"ShopifyProductActor - Problemas ao salvar alteração na integração de preço | {ex.Message}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Problemas ao salvar alteração na integração de preço"));
                    }

                    if (shopifyData.NotConsiderProductIfPriceIsZero)
                    {
                        if (message.Value.Price <= 0)
                        {
                            throw new Exception($"Tenantid: {shopifyData.Id} - Product {message.ExternalProductId} - Price is zero.");
                        }
                    }

                    ReturnMessage result;

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var shopifyService = scope.ServiceProvider.GetService<ShopifyService>();
                        shopifyService.Init(_apiActorGroup, GetLog(), _shopifyData);
                        result = await shopifyService.UpdatePrice(message, _shopifyData, _fullProductQueueClient, _cancellationToken);
                    }

                    if (shopifyData.EnableSaveIntegrationInformations)
                    {
                        try
                        {
                            integration.Status = IntegrationStatus.Processed;
                            integration.Result = JsonConvert.SerializeObject(result);

                            _shopifyProductPriceIntegrationRepository.Save(integration);
                        }
                        catch (Exception ex)
                        {
                            LogError(ex, $"ShopifyProductActor - Problemas ao salvar alteração na integração de preço | {ex.Message}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Problemas ao salvar alteração na integração de preço"));
                        }
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ShopifyUpdateStockMessage, Task>)(async message =>
            {
                try
                {
                    var integration = new ShopifyProductStockIntegration();

                    try
                    {
                        if (shopifyData.EnableSaveIntegrationInformations)
                        {
                            integration = new ShopifyProductStockIntegration()
                            {
                                Id = Guid.NewGuid(),
                                TenantId = shopifyData.Id,
                                ProductShopifyId = message.Value.ShopifyId,
                                ProductShopifySku = message.Value.Sku,
                                Payload = JsonConvert.SerializeObject(message),
                                Status = IntegrationStatus.Received,
                                IntegrationDate = DateTime.Now,
                                ReferenceIntegrationId = message.IntegrationId
                            };

                            _shopifyProductStockIntegrationRepository.Save(integration);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogWarning("ShopifyProductActor - Problemas ao salvar alteração na integração de stock | {0}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Problemas ao salvar alteração na integração de stock | {ex.Message}"));
                    }

                    ReturnMessage result;

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var shopifyService = scope.ServiceProvider.GetService<ShopifyService>();
                        shopifyService.Init(_apiActorGroup, GetLog(), _shopifyData);
                        result = await shopifyService.UpdateStock(message, _shopifyData, _fullProductQueueClient, _updateStockKitQueue, _cancellationToken);
                    }

                    if (shopifyData.EnableSaveIntegrationInformations)
                    {
                        try
                        {
                            integration.Status = IntegrationStatus.Processed;
                            integration.Result = JsonConvert.SerializeObject(result);

                            _shopifyProductStockIntegrationRepository.Save(integration);
                        }
                        catch (Exception ex)
                        {
                            LogWarning("ShopifyProductActor - Problemas ao salvar alteração na integração de stock | {0}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Problemas ao salvar alteração na integração de stock | {ex.Message}"));
                        }
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, $"ShopifyProductActor - Error in ShopifyUpdateStockMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ShopifyUpdateProductKitMessage, Task>)(async message =>
             {
                 try
                 {
                     ReturnMessage result;

                     using (var scope = _serviceProvider.CreateScope())
                     {
                         var shopifyService = scope.ServiceProvider.GetService<ShopifyService>();
                         shopifyService.Init(_apiActorGroup, GetLog(), _shopifyData);
                         result = await shopifyService.SendProductKitToUpdate(message, _fullProductQueueClient, _cancellationToken);
                     }

                     Sender.Tell(result);
                 }
                 catch (Exception ex)
                 {
                     LogError(ex, $"ShopifyProductActor - Error in ShopifyUpdateProductKitMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                     Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                 }
             }));

            ReceiveAsync((Func<ShopifyUpdateStockKitMessage, Task>)(async message =>
            {
                try
                {
                    ReturnMessage result;

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var shopifyService = scope.ServiceProvider.GetService<ShopifyService>();
                        shopifyService.Init(_apiActorGroup, GetLog(), _shopifyData);
                        result = await shopifyService.UpdateStockKit(message, _shopifyData, _fullProductQueueClient, _cancellationToken);
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    LogError(ex, $"ShopifyProductActor - Error in ShopifyUpdateStockKitMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ShopifyUpdateProductImagesMessage, Task>)(async message =>
            {
                var integration = new ShopifyProductImageIntegration();

                try
                {

                    try
                    {
                        if (shopifyData.EnableSaveIntegrationInformations)
                        {
                            integration = new ShopifyProductImageIntegration()
                            {
                                Id = Guid.NewGuid(),
                                TenantId = shopifyData.Id,
                                ProductShopifyId = message.ShopifyProductId,
                                ExternalProductId = message.ExternalProductId,
                                Payload = JsonConvert.SerializeObject(message),
                                Status = IntegrationStatus.Received,
                                IntegrationDate = DateTime.Now,
                                ReferenceIntegrationId = message.ReferenceIntegrationId
                            };

                            _shopifyProductImageIntegrationRepository.Save(integration);
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"Problemas ao salvar alteração na integração de imagem | {ex.Message}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Problemas ao salvar alteração na integração de imagem"));
                    }

                    ReturnMessage result;

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var shopifyService = scope.ServiceProvider.GetService<ShopifyService>();
                        shopifyService.Init(_apiActorGroup, GetLog(), _shopifyData);
                        result = await shopifyService.UpdateProductImages(message, _shopifyData, _cancellationToken);
                    }

                    if (shopifyData.EnableSaveIntegrationInformations)
                    {
                        try
                        {
                            integration.Status = IntegrationStatus.Processed;
                            integration.Result = JsonConvert.SerializeObject(result);

                            _shopifyProductImageIntegrationRepository.Save(integration);
                        }
                        catch (Exception ex)
                        {
                            _log.Error($"Problemas ao salvar alteração na integração de imagem | {ex.Message}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Problemas ao salvar alteração na integração de imagem"));
                        }
                    }

                    Sender.Tell(result);
                }
                catch (Exception ex)
                {
                    try
                    {
                        if (shopifyData.EnableSaveIntegrationInformations)
                        {
                            integration.Exception = JsonConvert.SerializeObject(ex);

                            _shopifyProductImageIntegrationRepository.Save(integration);
                        }
                    }
                    catch (Exception exx)
                    {
                        _log.Error($"Problemas ao salvar exception na integração de imagem | {exx.Message}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null, $"Problemas ao salvar alteração na integração de imagem"));
                    }

                    LogError(ex, $"ShopifyProductActor - Error in ShopifyUpdateProductImagesMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ProductIdsByTagQuery, Task>)(async message =>
            {
                try
                {
                    var response = await _apiActorGroup.Ask<ReturnMessage<ProductIdsByTagQueryOutput>>(message, _cancellationToken);

                    Sender.Tell(response);
                }
                catch (Exception ex)
                {
                    LogError(ex, $"ShopifyProductActor - Error in ProductIdsByTagQuery", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ProductMetafieldsByTagQuery, Task>)(async message =>
            {
                try
                {
                    var response = await _apiActorGroup.Ask<ReturnMessage<ProductMetafieldsByTagQueryOutput>>(message, _cancellationToken);

                    Sender.Tell(response);
                }
                catch (Exception ex)
                {
                    LogError(ex, $"ShopifyProductActor - Error in ProductMetafieldsByTagQuery", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<AllProductsTagsQuery, Task>)(async message =>
            {
                try
                {
                    var response = await _apiActorGroup.Ask<ReturnMessage<AllProductsTagsQueryOutput>>(message, _cancellationToken);

                    Sender.Tell(response);
                }
                catch (Exception ex)
                {
                    LogError(ex, $"ShopifyProductActor - Error in AllProductsTagsQuery", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<AllCollectionsQuery, Task>)(async message =>
            {
                try
                {
                    var response = await _apiActorGroup.Ask<ReturnMessage<AllCollectionsQueryOutput>>(message, _cancellationToken);

                    Sender.Tell(response);
                }
                catch (Exception ex)
                {
                    LogError(ex, $"ShopifyProductActor - Error in AllCollectionsQuery", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<CollectionCreateMutation, Task>)(async message =>
            {
                try
                {
                    var response = await _apiActorGroup.Ask<ReturnMessage<CollectionCreateMutationOutput>>(message, _cancellationToken);

                    Sender.Tell(response);
                }
                catch (Exception ex)
                {
                    LogError(ex, $"ShopifyProductActor - Error in CollectionCreateMutation", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ProductUpdateMutation, Task>)(async message =>
            {
                try
                {
                    var response = await _apiActorGroup.Ask<ReturnMessage<ProductUpdateMutationOutput>>(message, _cancellationToken);

                    if (_shopifyData.Id == 36 || _shopifyData.Id == 13) //Camys e Pit bull                        
                        _log.Info(LoggerDescription.FromProduct(_shopifyData.Id.ToString(), message.Variables.input.id, "ProductUpdateMutation", message, response));

                    Sender.Tell(response);
                }
                catch (Exception ex)
                {
                    LogError(ex, $"ShopifyProductActor - Error in ProductUpdateMutation", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ShopifyEnqueueListERPProductCategoriesMessage, Task>)(async message =>
            {
                try
                {
                    await Task.WhenAll(message.ExternalIds.Select(p =>
                                                                _listProductCategoriesQueueClient.SendAsync(
                                                                    new ServiceBusMessage(
                                                                        new ShopifyListERPProductCategoriesMessage
                                                                        {
                                                                            ExternalId = p
                                                                        })
                                                                    .GetMessage(p))));

                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    LogError(ex, $"ShopifyProductActor - Error in ShopifyEnqueueListERPProductCategoriesMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

            ReceiveAsync((Func<ShopifyEnqueueUpdatePartialProductMessage, Task>)(async message =>
            {
                try
                {
                    await Task.WhenAll(message.ProductInfos.Select(p =>
                                                                _updatePartialProductQueueClient.SendAsync(
                                                                    new ServiceBusMessage(
                                                                        new ShopifyUpdatePartialProductMessage
                                                                        {
                                                                            ProductInfo = p
                                                                        })
                                                                    .GetMessage(p.ShopifyId?.ToString() ?? p.ExternalId))));

                    Sender.Tell(new ReturnMessage { Result = Result.OK });
                }
                catch (Exception ex)
                {
                    LogError(ex, $"ShopifyProductActor - Error in ShopifyEnqueueUpdatePartialProductMessage", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));
                    Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
                }
            }));

        }
        protected override void PostStop()
        {
            base.PostStop();
            ActorTaskScheduler.RunTask(async () =>
            {
                if (_fullProductQueueClient != null && !_fullProductQueueClient.IsClosedOrClosing)
                    await _fullProductQueueClient.CloseAsync();

                if (_updateProductExternalIdQueueClient != null && !_updateProductExternalIdQueueClient.IsClosedOrClosing)
                    await _updateProductExternalIdQueueClient.CloseAsync();

                if (_updatePartialProductQueueClient != null && !_updatePartialProductQueueClient.IsClosedOrClosing)
                    await _updatePartialProductQueueClient.CloseAsync();

                if (_updateProductGroupingQueueClient != null && !_updateProductGroupingQueueClient.IsClosedOrClosing)
                    await _updateProductGroupingQueueClient.CloseAsync();

                if (_updateProductImagesQueueClient != null && !_updateProductImagesQueueClient.IsClosedOrClosing)
                    await _updateProductImagesQueueClient.CloseAsync();
            });
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, ShopifyDataMessage shopifyData, IActorRef apiActorGroup)
        {
            return Akka.Actor.Props.Create(() => new ShopifyProductActor(serviceProvider, cancellationToken, shopifyData, apiActorGroup));
        }
    }
}
