using Akka.Actor;
using Akka.Event;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VDC.Integration.APIClient.API;
using VDC.Integration.APIClient.Shopify;
using VDC.Integration.APIClient.Shopify.Models;
using VDC.Integration.APIClient.Shopify.Models.Request;
using VDC.Integration.APIClient.Shopify.Models.Request.Inputs;
using VDC.Integration.APIClient.Shopify.Models.Request.REST;
using VDC.Integration.Application.Strategy.Interfaces;
using VDC.Integration.Application.Strategy.NoteAttributes;
using VDC.Integration.Application.Tools;
using VDC.Integration.Domain.Consts;
using VDC.Integration.Domain.Dtos;
using VDC.Integration.Domain.Entities.Database;
using VDC.Integration.Domain.Entities.Database.Integrations.Shopify;
using VDC.Integration.Domain.Enums;
using VDC.Integration.Domain.Enums.Millennium;
using VDC.Integration.Domain.Messages;
using VDC.Integration.Domain.Messages.ServiceBus;
using VDC.Integration.Domain.Messages.Shopify;
using VDC.Integration.Domain.Messages.Shopify.OrderActor;
using VDC.Integration.Domain.Models;
using VDC.Integration.Domain.Models.GatewayNoteAttributes;
using VDC.Integration.Domain.Shopify.Models.Request;
using VDC.Integration.Domain.Shopify.Models.Results;
using VDC.Integration.EntityFramework.Repositories;
using static VDC.Integration.Domain.Shopify.Models.Results.REST.OrderResult;
using Order = VDC.Integration.APIClient.Shopify.Models.Request.Inputs.Order;
using OrderResult = VDC.Integration.Domain.Shopify.Models.Results.REST.OrderResult;
using Product = VDC.Integration.APIClient.Shopify.Models.Request.Inputs.Product;
using Result = VDC.Integration.Domain.Messages.Result;

namespace VDC.Integration.Application.Services
{
    public class ShopifyService
    {
        private ILoggingAdapter _logger;
        private IActorRef _apiActorGroup;
        private ShopifyDataMessage _shopifyData;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private ShopifyListOrderProcessRepository _shopifyListOrderProcessRepository;
        private ShopifyListOrderIntegrationRepository _shopifyListOrderIntegrationRepository;
        private ShopifyUpdateOrderTagNumberProcessRepository _shopifyUpdateOrderTagNumberProcessRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ParamRepository _paramRepository;
        private string _firstVariants;

        public ShopifyService() { }

        public ShopifyService(IConfiguration configuration,
                              IServiceProvider service,
                              IHttpClientFactory httpClientFactory,
                              ParamRepository paramRepository)
        {
            _configuration = configuration;
            _serviceProvider = service;
            _httpClientFactory = httpClientFactory;
            _paramRepository = paramRepository;
        }

        public void Init(IActorRef apiActorGroup, ILoggingAdapter logger, ShopifyDataMessage shopifyData)
        {
            _apiActorGroup = apiActorGroup;
            _logger = logger;
            _shopifyData = shopifyData;
            _shopifyListOrderProcessRepository = _serviceProvider.GetService<ShopifyListOrderProcessRepository>();
            _shopifyListOrderIntegrationRepository = _serviceProvider.GetService<ShopifyListOrderIntegrationRepository>();
            _shopifyUpdateOrderTagNumberProcessRepository = _serviceProvider.GetService<ShopifyUpdateOrderTagNumberProcessRepository>();
            _firstVariants = "25";
        }

        public async Task<ReturnMessage> UpdateFullProduct(ShopifyUpdateFullProductMessage message,
                                                           ShopifyDataMessage shopifyData,
                                                           QueueClient updateProductExternalIdQueueClient,
                                                           QueueClient updateProductGroupingQueueClient,
                                                           QueueClient updateProductImagesQueueClient,
                                                           CancellationToken cancellationToken)
        {
            var logIdentify = Guid.NewGuid();
            var momentOfTheProcess = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            ProductResult currentData = null;
            var activeSkus = message.ProductInfo.Variants?.Where(v => v.Status).ToList();

            _logger.Warning($"UpdateFullProduct - TenantId: {shopifyData.Id} (logIdentify: {logIdentify} - Time: {momentOfTheProcess}) | Message:{Newtonsoft.Json.JsonConvert.SerializeObject(message)}");

            if (message.ProductInfo.ShopifyId.HasValue)
            {
                var queryByIdResult = await GetProductById(message.ProductInfo.ShopifyId.Value, cancellationToken);

                if (queryByIdResult.Result == Result.Error)
                {
                    _logger.Warning($"ShopifyService - Error in UpdateFullProduct | {queryByIdResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                    return new ReturnMessage { Result = Result.Error, Error = queryByIdResult.Error };
                }

                if (queryByIdResult.Data.product != null)
                    currentData = queryByIdResult.Data.product;

            }

            if (currentData == null && !string.IsNullOrWhiteSpace(message.ProductInfo.ExternalId))
            {
                var queryByTagResult = await GetProductByTag(SetTagValue(Tags.ProductExternalId, message.ProductInfo.ExternalId), cancellationToken);

                if (queryByTagResult.Result == Result.Error)
                    return new ReturnMessage { Result = Result.Error, Error = queryByTagResult.Error };

                if (queryByTagResult.Data.products.edges.Any() == true)
                    currentData = queryByTagResult.Data.products.edges[0].node;
            }

            if (message.ProductInfo.OptionsName?.Count == 0)
                message.ProductInfo.OptionsName.Add("Title");

            List<string> productGroupingRefs = null;
            if (shopifyData.ProductGroupingEnabled && message.ProductInfo.GroupingReference != null)
            {
                productGroupingRefs = new List<string>();
                if (currentData != null)
                {
                    var oldValue = SearchTagValue(currentData.tags, Tags.ProductGroupingReference).FirstOrDefault();
                    if (oldValue != null && oldValue != message.ProductInfo.GroupingReference)
                        productGroupingRefs.Add(oldValue);
                }

                if (!string.IsNullOrWhiteSpace(message.ProductInfo.GroupingReference))
                    productGroupingRefs.Add(message.ProductInfo.GroupingReference);
            }

            _logger.Warning($"UpdateFullProduct(currentData) - TenantId: {shopifyData.Id} (logIdentify: {logIdentify} - Time: {momentOfTheProcess}) | currentData:{Newtonsoft.Json.JsonConvert.SerializeObject(currentData)}");

            ProductResult productUpdateResult = null;
            if (currentData == null)
            {
                //new product
                if (message.ProductInfo.Status == true)
                {
                    var locationResult = await _apiActorGroup.Ask<ReturnMessage<LocationQueryOutput>>(
                        new LocationQuery(25), cancellationToken
                    );

                    if (locationResult.Result == Result.Error)
                    {
                        _logger.Warning($"ShopifyService - Error in UpdateFullProduct | {locationResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                        return new ReturnMessage { Result = Result.Error, Error = locationResult.Error };
                    }

                    List<VariantCreateVariantsInput> variantes = null;

                    foreach (var activeSku in activeSkus)
                    {
                        if (shopifyData.NotConsiderProductIfPriceIsZero)
                        {
                            if (activeSku.Price?.Price <= 0)
                            {
                                _logger.Info($"Tenantid: {shopifyData.Id} - Product {message.ProductInfo.ExternalId} - Sku {activeSku.Sku} - Price is zero.");
                                continue;
                            }
                        }

                        var variante = new VariantCreateVariantsInput
                        {
                            barcode = activeSku.Barcode,
                            price = activeSku.Price.Price,
                            inventoryItem = new VariantUpdateVariantsInventoryItem
                            {
                                sku = activeSku.Sku,
                                measurement = new VariantUpdateVariantsInventoryItemMeasurement
                                {
                                    weight = new VariantUpdateVariantsInventoryItemMeasurementWeight
                                    {
                                        unit = "KILOGRAMS",
                                        value = activeSku.WeightInKG
                                    }
                                }
                            },
                            optionValues = new List<VariantUpdateVariantsOptionValue>
                            {
                                new VariantUpdateVariantsOptionValue
                                {
                                    linkedMetafieldValue = activeSku.Options.Count == 0 ? $"Default Title {activeSku.Sku.ToHashMD5().Truncate(6)}" : activeSku.Options[0],
                                    name = activeSku.Options.Count == 0 ? $"Default Title {activeSku.Sku.ToHashMD5().Truncate(6)}" : activeSku.Options[0],
                                    optionName = message.ProductInfo.OptionsName[0]
                                }
                            }
                        };

                        if (variantes == null)
                            variantes = new List<VariantCreateVariantsInput>();

                        variantes.Add(variante);
                    }

                    var createProductInput = new ProductCreateMutationInput
                    {
                        input = new Product
                        {
                            //published = message.ProductInfo.Status == null ? null : message.ProductInfo.Status.Value && !shopifyData.SetProductsAsUnpublished,
                            title = message.ProductInfo.Title,
                            descriptionHtml = shopifyData.BodyIntegrationType == BodyIntegrationType.Never ? null : GetBody(message.ProductInfo, shopifyData),
                            vendor = message.ProductInfo.Vendor,
                            //options = message.ProductInfo.OptionsName,
                            tags = FillProductTags(shopifyData, message.ProductInfo),
                            metafields = GetProductMetafields(shopifyData, message.ProductInfo)
                            //variants = variantes
                        }
                    };

                    var json = JsonSerializer.Serialize(createProductInput);

                    var process = new ShopifyListOrderProcess()
                    {
                        Id = Guid.NewGuid(),
                        TenantId = shopifyData.Id,
                        ProcessDate = DateTime.Now,
                        OrderId = 00000,
                        Exception = $"Produto - {message.ProductInfo.ExternalId}",
                        ShopifyResult = json
                    };

                    try
                    {
                        await _shopifyListOrderProcessRepository.Save(process);
                    }
                    catch (Exception ex)
                    {
                        var tt = ex;
                    }

                    var createResult = await _apiActorGroup.Ask<ReturnMessage<ProductCreateMutationOutput>>(
                        new ProductCreateMutation(createProductInput), cancellationToken);

                    if (createResult.Result == Result.Error)
                    {
                        _logger.Warning($"ShopifyService - Error in UpdateFullProduct | {createResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                        return new ReturnMessage { Result = Result.Error, Error = createResult.Error };
                    }

                    if (createResult.Data.productCreate.userErrors?.Any() == true)
                        throw new Exception($"Error in create shopify product: {JsonSerializer.Serialize(createResult.Data.productCreate.userErrors)}");

                    productUpdateResult = createResult.Data.productCreate.product;


                    ReturnMessage<VariantCreateMutationOutput> createVariantResult = await _apiActorGroup.Ask<ReturnMessage<VariantCreateMutationOutput>>(
                        new VariantCreateMutation(new VariantCreateMutationInput
                        {
                            productId = productUpdateResult.id,
                            variants = variantes
                        }), cancellationToken);

                    if (createVariantResult.Result == Result.Error)
                    {
                        _logger.Warning($"ShopifyService - Error in UpdateFullProduct | {createVariantResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                        return new ReturnMessage { Result = Result.Error, Error = createVariantResult.Error };
                    }

                    if (createVariantResult.Data.productVariantsBulkCreate.userErrors?.Any() == true)
                        throw new Exception($"Error in create shopify sku: {JsonSerializer.Serialize(createVariantResult.Data.productVariantsBulkCreate.userErrors)}");

                    _logger.Warning($"UpdateFullProduct(CreateProductInput) - TenantId: {shopifyData.Id} (logIdentify: {logIdentify} - Time: {momentOfTheProcess}) | createProductInput:{Newtonsoft.Json.JsonConvert.SerializeObject(createProductInput)}");
                }
            }
            else
            {
                var optionsName = message.ProductInfo.OptionsName.ToList();
                bool? optionPositionChanged = false;
                if (activeSkus.Any())
                {
                    for (int i = 0; i < optionsName.Count; i++)
                    {
                        var currentIndex = currentData.options.FindIndex(o => o.name == optionsName[i]);
                        if (currentIndex > -1 && currentIndex != i)
                        {
                            optionsName[i] = $"{optionsName[i]}#";
                            optionPositionChanged = true;
                        }
                    }
                }

                //update product
                var locationId = currentData.variants.edges.FirstOrDefault()?.node.inventoryItem?.inventoryLevels?.edges?.FirstOrDefault()?.node?.location?.id;
                if (string.IsNullOrWhiteSpace(locationId))
                    throw new Exception($"empty locationId metafield in product {currentData.legacyResourceId}");

                var hasInventoryToUpdate = currentData.variants.edges.Any(vc => activeSkus.Any(vu => vu.Sku == vc.node.sku));

                if (hasInventoryToUpdate)
                {
                    var variants = new List<Variant>();
                    var inventoryItemAdjustments = new List<InventoryAdjustment>();

                    foreach (var sku in activeSkus)
                    {
                        if (shopifyData.NotConsiderProductIfPriceIsZero)
                        {
                            if (sku.Price?.Price <= 0)
                            {
                                _logger.Info($"Tenantid: {shopifyData.Id} - Product {message.ProductInfo.ExternalId} - Sku {sku.Sku} - Price is zero.");
                                continue;
                            }
                        }

                        var shopifyVariant = currentData.variants.edges.Where(v => v.node.sku == sku.Sku).FirstOrDefault()?.node;
                        if (shopifyVariant == null)
                        {
                            //new sku
                            variants.Add(new Variant
                            {
                              /*  sku = sku.Sku,
                                weight = sku.WeightInKG,
                                barcode = sku.Barcode,
                                compareAtPrice = new Optional<decimal?>(sku.Price.CompareAtPrice),
                                price = sku.Price.Price,
                                options = sku.Options.Count == 0 ? new List<string> { $"Default Title {sku.Sku.ToHashMD5().Truncate(6)}" } : sku.Options,
                                inventoryPolicy = sku.SellWithoutStock ? "CONTINUE" : "DENY",
                                inventoryQuantities = new List<InventoryQuantity>
                                    {
                                        new InventoryQuantity
                                        {
                                            availableQuantity = sku.Stock.Quantity,
                                            locationId = FillLocation(sku, locationId)
                                        }
                                },*/
                            });
                        }
                        else
                        {
                            //update sku
                            variants.Add(new Variant
                            {
                              /*  inventoryPolicy = sku.SellWithoutStock ? "CONTINUE" : "DENY",
                                id = shopifyVariant.id,
                                sku = sku.Sku,
                                weight = sku.WeightInKG,
                                barcode = sku.Barcode,
                                compareAtPrice = new Optional<decimal?>(sku.Price.CompareAtPrice),
                                price = sku.Price.Price,
                                options = sku.Options.Count == 0 ? new List<string> { $"Default Title {sku.Sku.ToHashMD5().Truncate(6)}" } : sku.Options*/
                            });

                            InventoryLevelResult inventoryItem = default;

                            if (_shopifyData.EnabledMultiLocation)
                            {
                                var location = _shopifyData.LocationMap.GetLocationByIdErp(sku.Stock.Locations.FirstOrDefault().ErpLocationId);

                                if (location is null)
                                    throw new Exception($"Not found mapping to LocationId {locationId} for client: {_shopifyData.StoreName}");

                                inventoryItem = shopifyVariant.inventoryItem?.inventoryLevels?.edges?
                                    .Where(x => x.node.location.legacyResourceId == location.EcommerceLocation)
                                    .FirstOrDefault()?.node;
                            }
                            else
                            {
                                inventoryItem = shopifyVariant.inventoryItem?.inventoryLevels?.edges?.FirstOrDefault()?.node;
                            }

                            if (inventoryItem == null)
                                throw new Exception($"no inventoryitem in variant {shopifyVariant.legacyResourceId} - sku:{sku.Sku}");

                            inventoryItemAdjustments.Add(new InventoryAdjustment
                            {
                                availableDelta = sku.Stock.Quantity - inventoryItem.quantities[0].quantity,
                                inventoryItemId = shopifyVariant.inventoryItem.id
                            });
                        }
                    }

                    ReturnMessage<ProductAndInventoryUpdateMutationOutput> updateResult;
                    while (optionPositionChanged != null)
                    {
                        updateResult = await _apiActorGroup.Ask<ReturnMessage<ProductAndInventoryUpdateMutationOutput>>(
                        new ProductAndInventoryUpdateMutation(new ProductAndInventoryUpdateMutationInput
                        {
                            input = InputProduct(shopifyData, currentData, message.ProductInfo, variants, optionPositionChanged, optionsName: optionsName),
                            inventoryItemAdjustments = inventoryItemAdjustments,
                            locationId = locationId
                        }), cancellationToken);


                        if (updateResult.Result == Result.Error)
                        {
                            _logger.Warning($"ShopifyService - Error in UpdateFullProduct | {updateResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                            return new ReturnMessage { Result = Result.Error, Error = updateResult.Error };
                        }

                        if (updateResult.Data.productUpdate.userErrors?.Any() == true)
                            throw new Exception($"Error in update shopify product: {JsonSerializer.Serialize(updateResult.Data.productUpdate.userErrors)}");
                        if (updateResult.Data.inventoryBulkAdjustQuantityAtLocation.userErrors?.Any() == true)
                            throw new Exception($"Error in update shopify product: {JsonSerializer.Serialize(updateResult.Data.inventoryBulkAdjustQuantityAtLocation.userErrors)}");

                        productUpdateResult = updateResult.Data.productUpdate.product;

                        optionPositionChanged = optionPositionChanged == true ? false : null;
                    }
                }
                else
                {
                    List<Variant> variants = null;

                    if (activeSkus.Any())
                    {
                        foreach (var activeSku in activeSkus)
                        {
                            if (shopifyData.NotConsiderProductIfPriceIsZero)
                            {
                                if (activeSku.Price?.Price <= 0)
                                {
                                    _logger.Info($"Tenantid: {shopifyData.Id} - Product {message.ProductInfo.ExternalId} - Sku {activeSku.Sku} - Price is zero.");
                                    continue;
                                }
                            }

                            var variante = new Variant
                            {
                               /* sku = activeSku.Sku,
                                weight = activeSku.WeightInKG,
                                barcode = activeSku.Barcode,
                                compareAtPrice = new Optional<decimal?>(activeSku.Price.CompareAtPrice),
                                price = activeSku.Price.Price,
                                inventoryPolicy = activeSku.SellWithoutStock ? "CONTINUE" : "DENY",
                                options = activeSku.Options.Count == 0 ? new List<string> { $"Default Title {activeSku.Sku.ToHashMD5().Truncate(6)}" } : activeSku.Options,
                                inventoryQuantities = new List<InventoryQuantity>
                                    {
                                        new InventoryQuantity
                                        {
                                            availableQuantity = activeSku.Stock.Quantity,
                                            locationId = FillLocation(activeSku, locationId)
                                        }
                                    }*/
                            };

                            if (variants == null)
                                variants = new List<Variant>();

                            variants.Add(variante);
                        }
                    }
                    else
                    {
                        //if there are no skus, mantain current data but unpublish the product
                        message.ProductInfo.Status = false;
                        message.ProductInfo.OptionsName = null;
                        variants = null;
                    }

                    ReturnMessage<ProductUpdateMutationOutput> updateResult;
                    while (optionPositionChanged != null)
                    {
                        updateResult = await _apiActorGroup.Ask<ReturnMessage<ProductUpdateMutationOutput>>(
                                                new ProductUpdateMutation(new ProductUpdateMutationInput
                                                {
                                                    input = InputProduct(shopifyData, currentData, message.ProductInfo, variants, optionPositionChanged, optionsName: optionsName),
                                                }), cancellationToken);

                        if (updateResult.Result == Result.Error)
                        {
                            _logger.Warning($"ShopifyService - Error in UpdateFullProduct | {updateResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                            return new ReturnMessage { Result = Result.Error, Error = updateResult.Error };
                        }

                        if (updateResult.Data.productUpdate.userErrors?.Any() == true)
                            throw new Exception($"Error in update shopify product: {JsonSerializer.Serialize(updateResult.Data.productUpdate.userErrors)}");

                        productUpdateResult = updateResult.Data.productUpdate.product;

                        optionPositionChanged = optionPositionChanged == true ? false : null;
                    }
                }
            }

            if (productUpdateResult != null)
            {
                if (shopifyData.ProductGroupingEnabled && message.ProductInfo.GroupingReference != null)
                {
                    await SendUpdateProductGroupingMessage(productUpdateResult.legacyResourceId, productGroupingRefs, shopifyData, updateProductGroupingQueueClient);
                }
            }

            if (productUpdateResult != null)
            {
                if (shopifyData.ImageIntegrationEnabled && message.ProductInfo.Images != null)
                {
                    await SendUpdateProductImagesMessage(productUpdateResult.legacyResourceId, message.ProductInfo.Images, shopifyData, updateProductImagesQueueClient);
                }
            }

            return new ReturnMessage { Result = Result.OK };
        }

        private Product InputProduct(ShopifyDataMessage shopifyData,
                                            ProductResult currentData,
                                            Domain.Models.Product.Info ProductInfo,
                                            List<Variant> variants,
                                            bool? optionPositionChanged = false,
                                            List<string> optionsName = null)
        {
            try
            {
                if (shopifyData.DisableUpdateProduct)
                {
                    return new Product
                    {
                        id = currentData?.id,
                        title = ProductInfo.Title
                    };
                }

                return new Product
                {
                    id = currentData.id,
                    title = ProductInfo.Title,
                    descriptionHtml = shopifyData.BodyIntegrationType == BodyIntegrationType.Always ? GetBody(ProductInfo, shopifyData) : null,
                    vendor = ProductInfo.Vendor,
                    options = optionPositionChanged == true ? optionsName : ProductInfo.OptionsName,
                    tags = FillProductTags(shopifyData, ProductInfo, currentData.tags),
                    metafields = GetProductMetafields(shopifyData, ProductInfo, currentData)
                };
            }
            catch (Exception ex)
            {
                var ff = ex;
                return null;
            }
        }

        public async Task<ReturnMessage> UpdatePartialProduct(ShopifyUpdatePartialProductMessage message,
                                                              ShopifyDataMessage shopifyData,
                                                              QueueClient fullProductQueueClient,
                                                              QueueClient updateProductExternalIdQueueClient,
                                                              QueueClient updateProductGroupingQueueClient,
                                                              CancellationToken cancellationToken)
        {
            ProductResult currentData = null;
            var activeSkus = message.ProductInfo.Variants?.Where(v => v.Status).ToList();


            if (message.ProductInfo.ShopifyId.HasValue)
            {
                var queryByIdResult = await GetProductById(message.ProductInfo.ShopifyId.Value, cancellationToken);

                if (queryByIdResult.Result == Result.Error)
                {
                    _logger.Warning($"ShopifyService - Error in UpdatePartialProduct | {queryByIdResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                    return new ReturnMessage { Result = Result.Error, Error = queryByIdResult.Error };
                }

                if (queryByIdResult.Data.product != null)
                    currentData = queryByIdResult.Data.product;

            }

            if (currentData == null && !string.IsNullOrWhiteSpace(message.ProductInfo.ExternalId))
            {
                var queryByTagResult = await GetProductByTag(SetTagValue(Tags.ProductExternalId, message.ProductInfo.ExternalId), cancellationToken);

                if (queryByTagResult.Result == Result.Error)
                {
                    _logger.Warning($"ShopifyService - Error in UpdatePartialProduct | {queryByTagResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                    return new ReturnMessage { Result = Result.Error, Error = queryByTagResult.Error };
                }

                if (queryByTagResult.Data.products.edges.Any() == true)
                    currentData = queryByTagResult.Data.products.edges[0].node;
            }

            if (message.ProductInfo.OptionsName?.Count == 0)
                message.ProductInfo.OptionsName.Add("Title");

            if (currentData == null)
            {
                //new product
                if (message.ProductInfo.Status == true)
                    await SendListFullProductMessage(message.ProductInfo.ExternalId, fullProductQueueClient);

            }
            else if (ProductVendorChanged(message.ProductInfo, currentData))
            {
                await SendListFullProductMessage(message.ProductInfo.ExternalId, fullProductQueueClient);
            }
            else if (ProductCategoriesChanged(shopifyData, message.ProductInfo, currentData))
            {
                await SendListFullProductMessage(message.ProductInfo.ExternalId, fullProductQueueClient);
            }
            else
            {
                //update product
                var locationId = currentData.variants.edges.FirstOrDefault()?.node.inventoryItem?.inventoryLevels?.edges?.FirstOrDefault()?.node?.location?.id;
                if (string.IsNullOrWhiteSpace(locationId))
                    throw new Exception($"empty locationId in product {currentData.legacyResourceId}");

                var hasNewSkus = activeSkus?.Any(vu => currentData.variants.edges.All(vc => vc.node.sku != vu.Sku)) == true;

                if (hasNewSkus)
                {
                    await SendListFullProductMessage(message.ProductInfo.ExternalId, fullProductQueueClient);
                }
                else
                {
                    List<string> productGroupingRefs = null;
                    if (shopifyData.ProductGroupingEnabled && message.ProductInfo.GroupingReference != null)
                    {
                        productGroupingRefs = new List<string>();
                        if (currentData != null)
                        {
                            var oldValue = SearchTagValue(currentData.tags, Tags.ProductGroupingReference).FirstOrDefault();
                            if (oldValue != null && oldValue != message.ProductInfo.GroupingReference)
                                productGroupingRefs.Add(oldValue);
                        }

                        if (!string.IsNullOrWhiteSpace(message.ProductInfo.GroupingReference))
                            productGroupingRefs.Add(message.ProductInfo.GroupingReference);
                    }

                    ProductResult productUpdateResult = null;
                    List<Variant> variants = null;

                    if (activeSkus != null)
                    {
                        variants = new List<Variant>();
                        if (activeSkus.Any())
                        {
                            foreach (var sku in activeSkus)
                            {
                                if (shopifyData.NotConsiderProductIfPriceIsZero)
                                {
                                    if (sku.Price?.Price <= 0)
                                    {
                                        throw new Exception($"Tenantid: {shopifyData.Id} - Product {message.ProductInfo.ExternalId} - Sku {sku.Sku} - Price is zero.");
                                    }
                                }

                                var shopifyVariant = currentData.variants.edges.Where(v => v.node.sku == sku.Sku).FirstOrDefault().node;

                                //update sku
                                variants.Add(new Variant
                                {
                                  /*  id = shopifyVariant.id,
                                    sku = sku.Sku,
                                    weight = sku.WeightInKG,
                                    barcode = sku.Barcode,
                                    price = sku.Price?.Price,
                                    compareAtPrice = new Optional<decimal?>(sku.Price?.CompareAtPrice),
                                    inventoryPolicy = sku.SellWithoutStock ? "CONTINUE" : "DENY",
                                    options = sku.Options.Count == 0 ? new List<string> { $"Default Title {sku.Sku.ToHashMD5().Truncate(6)}" } : sku.Options*/
                                });
                            }
                        }
                        else
                        {
                            //if there are no skus, mantain current data but unpublish the product
                            message.ProductInfo.Status = false;
                            message.ProductInfo.OptionsName = null;
                            variants = null;
                        }
                    }

                    var updateResult = await _apiActorGroup.Ask<ReturnMessage<ProductUpdateMutationOutput>>(
                        new ProductUpdateMutation(new ProductUpdateMutationInput
                        {
                            input = InputProduct(shopifyData, currentData, message.ProductInfo, variants, optionsName: null),
                        }), cancellationToken);

                    if (updateResult.Result == Result.Error)
                    {
                        _logger.Warning($"ShopifyService - Error in UpdatePartialProduct | {updateResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                        return new ReturnMessage { Result = Result.Error, Error = updateResult.Error };
                    }

                    if (updateResult.Data.productUpdate.userErrors?.Any() == true)
                        throw new Exception($"Error in update shopify product: {JsonSerializer.Serialize(updateResult.Data.productUpdate.userErrors)}");

                    productUpdateResult = updateResult.Data.productUpdate.product;

                    if (shopifyData.ProductGroupingEnabled && message.ProductInfo.GroupingReference != null)
                    {
                        await SendUpdateProductGroupingMessage(productUpdateResult.legacyResourceId, productGroupingRefs, shopifyData, updateProductGroupingQueueClient);
                    }
                }
            }

            return new ReturnMessage { Result = Result.OK };
        }
        private bool ProductVendorChanged(Domain.Models.Product.Info productInfo, ProductResult currentData)
        {
            if (productInfo.VendorId != null)
            {
                var currentVendorId = SearchTagValue(currentData.tags, Tags.ProductVendorId).FirstOrDefault();
                var newVendorId = productInfo.VendorId;
                if (currentVendorId != newVendorId)
                {
                    //if I have the new Vendor name there is no need for a full update
                    if (productInfo.Vendor == null)
                        return true;
                }
            }
            return false;
        }

        private bool ProductCategoriesChanged(ShopifyDataMessage shopifyData, Domain.Models.Product.Info productInfo, ProductResult currentData)
        {
            if (productInfo.Categories != null)
            {
                var currentCategoriesIds = SearchTagValue(currentData.tags, Tags.ProductCollectionId);
                var newCategoryIds = productInfo.Categories.Where(c => c.Id != null).ToList();

                foreach (var newCategory in newCategoryIds)
                {
                    if (!currentCategoriesIds.Contains(newCategory.Id))
                    {
                        //if I have the new Category name there is no need for a full update
                        if (newCategory.Name == null)
                            return true;
                    }
                }
            }

            return false;
        }

        private List<Metafield> GetProductMetafields(ShopifyDataMessage shopifyData, Domain.Models.Product.Info productInfo, ProductResult currentData = null)
        {
            List<Metafield> result = new List<Metafield>();
            if (shopifyData.ProductGroupingEnabled
                && productInfo.GroupingReference != null
                && string.IsNullOrWhiteSpace(productInfo.GroupingReference)) //empty grouping
            {
                Metafield productGroupingMetafield = null;
                var currentGroupingMetafield = currentData?.metafields.edges.Select(x => x.node).FirstOrDefault(x => x.key == "ProductGroupingHandles");
                if (currentGroupingMetafield == null)
                    //productGroupingMetafield = new Metafield { key = "ProductGroupingHandles", valueType = "STRING" };
                    productGroupingMetafield = new Metafield { key = "ProductGroupingHandles" };
                else
                    //productGroupingMetafield = new Metafield { id = currentGroupingMetafield.id, key = "ProductGroupingHandles", value = currentGroupingMetafield.value, valueType = "STRING" };
                    productGroupingMetafield = new Metafield { id = currentGroupingMetafield.id, key = "ProductGroupingHandles", value = currentGroupingMetafield.value };

                var handles = "|";
                if (productGroupingMetafield.id == null || productGroupingMetafield.value != handles)
                {
                    productGroupingMetafield.value = handles;
                    result.Add(productGroupingMetafield);
                }
            }

            if (productInfo.Metafields?.Any() == true)
            {
                foreach (var metafield in productInfo.Metafields)
                {
                    var shopifyMetafield = currentData?.metafields.edges
                                                    .Select(x => x.node)
                                                    .Where(x => x.key == metafield.Key)
                                                    .Select(x => new Metafield { id = x.id }).FirstOrDefault() ?? new Metafield();

                    shopifyMetafield.key = metafield.Key;
                    shopifyMetafield.value = metafield.Value;
                    //shopifyMetafield.valueType = metafield.ValueType;
                    result.Add(shopifyMetafield);
                }
            }

            if (result.Any())
                return result;
            else
                return null;
        }

        private string GetBody(Domain.Models.Product.Info productInfo, ShopifyDataMessage shopifyData)
        {
            if (shopifyData.ProductDescriptionIsHTML)
                return productInfo.BodyHtml;
            else
                return productInfo.BodyHtml?.Replace("\r\n", "<br/>").Replace("\n", "<br/>");

        }

        public async Task<ReturnMessage> UpdatePartialSku(ShopifyUpdatePartialSkuMessage message, ShopifyDataMessage shopifyData, QueueClient fullProductQueueClient, CancellationToken cancellationToken)
        {
            VariantResult currentData = null;

            if (message.SkuInfo.ShopifyId.HasValue)
            {
                var queryByIdResult = await _apiActorGroup.Ask<ReturnMessage<VariantByIdQueryOutput>>(
                    new VariantByIdQuery(message.SkuInfo.ShopifyId.Value), cancellationToken
                );

                if (queryByIdResult.Result == Result.Error)
                {
                    _logger.Warning($"ShopifyService - Error in UpdatePartialSku | {queryByIdResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                    return new ReturnMessage { Result = Result.Error, Error = queryByIdResult.Error };
                }

                if (queryByIdResult.Data.productVariant != null)
                    currentData = queryByIdResult.Data.productVariant;
            }

            if (currentData == null && !string.IsNullOrWhiteSpace(message.SkuInfo.Sku))
            {
                if (!shopifyData.CanMoveSku)
                {
                    var queryBySkuResult = await _apiActorGroup.Ask<ReturnMessage<VariantBySkuQueryOutput>>(
                        new VariantBySkuQuery(message.SkuInfo.Sku), cancellationToken
                    );

                    if (queryBySkuResult.Result == Result.Error)
                    {
                        _logger.Warning($"ShopifyService - Error in UpdatePartialSku | {queryBySkuResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                        return new ReturnMessage { Result = Result.Error, Error = queryBySkuResult.Error };
                    }

                    if (queryBySkuResult.Data.productVariants.edges.Any() == true)
                        currentData = queryBySkuResult.Data.productVariants.edges[0].node;
                }
                else
                {
                    var queryProductsBySkuResult = await _apiActorGroup.Ask<ReturnMessage<VariantParentProductsBySkuQueryOutput>>(
                        new VariantParentProductsBySkuQuery(message.SkuInfo.Sku), cancellationToken
                    );

                    if (queryProductsBySkuResult.Result == Result.Error)
                    {
                        _logger.Warning($"ShopifyService - Error in UpdatePartialSku | {queryProductsBySkuResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                        return new ReturnMessage { Result = Result.Error, Error = queryProductsBySkuResult.Error };
                    }

                    foreach (var sku in queryProductsBySkuResult.Data.productVariants.edges)
                    {
                        var externalId = SearchTagValue(sku.node.product.tags, Tags.ProductExternalId).FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(message.ExternalProductId) && externalId == message.ExternalProductId)
                        {
                            var queryByIdResult = await _apiActorGroup.Ask<ReturnMessage<VariantByIdQueryOutput>>(
                                new VariantByIdQuery(long.Parse(sku.node.legacyResourceId)), cancellationToken
                            );

                            if (queryByIdResult.Result == Result.Error)
                            {
                                _logger.Warning($"ShopifyService - Error in UpdatePartialSku | {queryByIdResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                                return new ReturnMessage { Result = Result.Error, Error = queryByIdResult.Error };
                            }

                            if (queryByIdResult.Data.productVariant != null)
                                currentData = queryByIdResult.Data.productVariant;
                        }
                        else
                        {
                            //sku migrated or removed
                            await SendListFullProductMessage(externalId, fullProductQueueClient);
                        }
                    }
                }
            }

            if (currentData == null)
            {
                if (message.SkuInfo.Status == true)
                {
                    //new sku
                    if (!string.IsNullOrWhiteSpace(message.ExternalProductId))
                        await SendListFullProductMessage(message.ExternalProductId, fullProductQueueClient);
                }
            }
            else
            {
                if (message.SkuInfo.Status == true)
                {
                    if (message.SkuInfo.OptionsName != null)
                    {
                        if (message.SkuInfo.OptionsName.Count == 0)
                            message.SkuInfo.OptionsName.Add("Title");

                        if (message.SkuInfo.OptionsName.Count != currentData.selectedOptions.Count ||
                            message.SkuInfo.OptionsName.Any(x =>
                            {
                                var option = currentData.selectedOptions[message.SkuInfo.OptionsName.IndexOf(x)];
                                return x != option.name;
                            }))
                        {
                            //changed options
                            await SendListFullProductMessage(message.ExternalProductId, fullProductQueueClient);
                            return new ReturnMessage { Result = Result.OK };
                        }
                    }

                    ReturnMessage<VariantUpdateMutationOutput> createResult = await _apiActorGroup.Ask<ReturnMessage<VariantUpdateMutationOutput>>(
                           new VariantUpdateMutation(new VariantUpdateMutationInput
                           {
                                productId = currentData.product.id,
                                variants = new List<VariantUpdateVariantsInput>
                                {
                                    new VariantUpdateVariantsInput {
                                        id = currentData.id,
                                        barcode = message.SkuInfo.Barcode,
                                        price = message.SkuInfo.Price?.Price,
                                        inventoryItem = new VariantUpdateVariantsInventoryItem
                                        {
                                            sku = message.SkuInfo.Sku,
                                            measurement = new VariantUpdateVariantsInventoryItemMeasurement
                                            {
                                                weight = new VariantUpdateVariantsInventoryItemMeasurementWeight
                                                {
                                                    unit = "KILOGRAMS",
                                                    value = message.SkuInfo.WeightInKG
                                                }
                                            }
                                        }/*,
                                        optionValues = new List<VariantUpdateVariantsOptionValue>
                                        {
                                            new VariantUpdateVariantsOptionValue
                                            {
                                                linkedMetafieldValue = "",
                                                name = activeSku.Options.Count == 0 ? new List<string> { $"Default Title {activeSku.Sku.ToHashMD5().Truncate(6)}" } : activeSku.Options,,

                                                optionName = ""
                                            }
                                        }*/
                                    }
                                }
                           }), cancellationToken);

                    if (createResult.Result == Result.Error)
                    {
                        _logger.Warning($"ShopifyService - Error in UpdatePartialSku | {createResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                        return new ReturnMessage { Result = Result.Error, Error = createResult.Error };
                    }

                    if (createResult.Data.productVariantsBulkUpdate.userErrors?.Any() == true)
                        throw new Exception($"Error in update shopify sku: {JsonSerializer.Serialize(createResult.Data.productVariantsBulkUpdate.userErrors)}");
                }
                else
                {
                    //sku removed
                    if (!string.IsNullOrWhiteSpace(message.ExternalProductId))
                        await SendListFullProductMessage(message.ExternalProductId, fullProductQueueClient);
                }
            }

            return new ReturnMessage { Result = Result.OK };
        }

        public async Task<ReturnMessage> UpdatePrice(ShopifyUpdatePriceMessage message, ShopifyDataMessage shopifyData, QueueClient fullProductQueueClient, CancellationToken cancellationToken)
        {
            VariantResult currentData = null;

            if (message.Value.ShopifyId.HasValue)
            {
                var queryByIdResult = await _apiActorGroup.Ask<ReturnMessage<VariantByIdQueryOutput>>(
                    new VariantByIdQuery(message.Value.ShopifyId.Value), cancellationToken
                );

                if (queryByIdResult.Result == Result.Error)
                {
                    _logger.Warning($"ShopifyService - Error in UpdatePrice | {queryByIdResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                    return new ReturnMessage { Result = Result.Error, Error = queryByIdResult.Error };
                }

                if (queryByIdResult.Data.productVariant != null)
                    currentData = queryByIdResult.Data.productVariant;
            }

            if (currentData == null && !string.IsNullOrWhiteSpace(message.Value.Sku))
            {
                var queryBySkuResult = await _apiActorGroup.Ask<ReturnMessage<VariantBySkuQueryOutput>>(
                    new VariantBySkuQuery(message.Value.Sku), cancellationToken
                );

                if (queryBySkuResult.Result == Result.Error)
                {
                    _logger.Warning($"ShopifyService - Error in UpdatePrice | {queryBySkuResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                    return new ReturnMessage { Result = Result.Error, Error = queryBySkuResult.Error };
                }

                if (queryBySkuResult.Data.productVariants.edges.Any() == true)
                    currentData = queryBySkuResult.Data.productVariants.edges[0].node;
            }

            if (currentData == null)
            {
                //new sku
                if (!string.IsNullOrWhiteSpace(message.ExternalProductId))
                    await SendListFullProductMessage(message.ExternalProductId, fullProductQueueClient);
            }
            else
            {
                var createResult = await _apiActorGroup.Ask<ReturnMessage<VariantUpdateMutationOutput>>(
                       new VariantUpdateMutation(new VariantUpdateMutationInput
                       {
                            productId = currentData.product.id,
                            variants = new List<VariantUpdateVariantsInput>
                            {
                                new VariantUpdateVariantsInput
                                {
                                    id = currentData.id,
                                    price = message.Value.Price,
                                }

                            }
                       }), cancellationToken);

                if (createResult.Result == Result.Error)
                {
                    _logger.Warning($"ShopifyService - Error in UpdatePrice | {createResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                    return new ReturnMessage { Result = Result.Error, Error = createResult.Error };
                }

                if (createResult.Data.productVariantsBulkUpdate.userErrors?.Any() == true)
                    throw new Exception($"Error in update shopify price: {JsonSerializer.Serialize(createResult.Data.productVariantsBulkUpdate.userErrors)}");
            }
            return new ReturnMessage { Result = Result.OK };
        }

        public async Task<ReturnMessage> UpdateStock(ShopifyUpdateStockMessage message, ShopifyDataMessage shopifyData, QueueClient fullProductQueueClient, QueueClient updateStockKitQueue, CancellationToken cancellationToken)
        {
            VariantResult currentData = null;

            if (message.Value.ShopifyId.HasValue)
            {
                var queryByIdResult = await _apiActorGroup.Ask<ReturnMessage<VariantByIdQueryOutput>>(
                    new VariantByIdQuery(message.Value.ShopifyId.Value), cancellationToken
                );

                if (queryByIdResult.Result == Result.Error)
                {
                    var errorMessage = new ReturnMessage { Result = Result.Error, Error = queryByIdResult.Error };
                    _logger.Warning($"ShopifyService - Error in UpdateStock | {errorMessage.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData,
                       $"TenantId: {shopifyData.Id} - Error when get variant by query at shopify - {message.Value.ShopifyId.Value}"));
                    return errorMessage;
                }

                if (queryByIdResult.Data.productVariant != null)
                    currentData = queryByIdResult.Data.productVariant;
            }

            if (currentData == null && !string.IsNullOrWhiteSpace(message.Value.Sku))
            {
                var queryBySkuResult = await _apiActorGroup.Ask<ReturnMessage<VariantBySkuQueryOutput>>(
                    new VariantBySkuQuery(message.Value.Sku), cancellationToken
                );

                if (queryBySkuResult.Result == Result.Error)
                {
                    var errorMessage = new ReturnMessage { Result = Result.Error, Error = queryBySkuResult.Error };
                    _logger.Warning($"ShopifyService - Error in UpdateStock | {errorMessage.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData,
                        $"TenantId: {shopifyData.Id} - Error when get sku by query at shopify - {message.Value.Sku}"));
                    return errorMessage;
                }

                if (queryBySkuResult.Data.productVariants.edges.Any() == true)
                    currentData = queryBySkuResult.Data.productVariants.edges[0].node;
            }

            if (currentData == null)
            {
                //new sku
                if (!string.IsNullOrWhiteSpace(message.ExternalProductId))
                    await SendListFullProductMessage(message.ExternalProductId, fullProductQueueClient);
            }
            else
            {

                InventoryLevelResult inventoryItem = currentData.inventoryItem?.inventoryLevels?.edges?.FirstOrDefault()?.node;

                if (_shopifyData.EnabledMultiLocation)
                {
                    var multiLocationId = _shopifyData.LocationMap.GetLocationByIdErp(message.Value.Locations.FirstOrDefault().ErpLocationId);

                    if (multiLocationId is null)
                    {
                        _logger.Warning($"ShopifyService - Error in UpdateStock", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData,
                            $"TenantId: {shopifyData.Id} - Not found mapping to LocationId {multiLocationId} for client: {_shopifyData.StoreName}"));
                        throw new Exception($"Not found mapping to LocationId {multiLocationId} for client: {_shopifyData.StoreName}");
                    }

                    var locationsShop = await _apiActorGroup.Ask<ReturnMessage<LocationQueryOutput>>(
                       new LocationQuery(25), cancellationToken
                   );

                    if (locationsShop.Result == Result.Error)
                    {
                        var errorMessage = new ReturnMessage { Result = Result.Error, Error = locationsShop.Error };
                        _logger.Warning($"ShopifyService - Error in UpdateStock | {locationsShop.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData,
                            $"TenantId: {shopifyData.Id} - Location result is null"));
                        return errorMessage;
                    }

                    var location = locationsShop.Data.locations.edges.Where(x => x.node.legacyResourceId == multiLocationId.EcommerceLocation).FirstOrDefault();
                    var inventories = currentData.inventoryItem?.inventoryLevels?.edges.Select(y => y.node.location.id).ToList();

                    var createNewLocation = !inventories.Contains(location.node.id);

                    if (createNewLocation)
                    {
                        var createResult = await _apiActorGroup.Ask<ReturnMessage<InventoryActivateMutationOutput>>(
                           new InventoryActivateMutation(new InventoryActivateMutationInput
                           {
                               inventoryItemId = currentData.inventoryItem.id,
                               locationId = location.node.id,
                               available = message.Value.Quantity
                           }), cancellationToken);

                        if (createResult.Result == Result.Error)
                        {
                            var errorMessage = new ReturnMessage { Result = Result.Error, Error = createResult.Error };
                            _logger.Warning($"ShopifyService - Error in UpdateStock | {createResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData,
                                $"TenantId: {shopifyData.Id} - Error when create inventory"));
                            return errorMessage;
                        }

                        return new ReturnMessage { Result = Result.OK };

                    }
                    else
                    {
                        inventoryItem = currentData.inventoryItem?.inventoryLevels?.edges?
                            .Where(x => x.node.location.legacyResourceId == multiLocationId.EcommerceLocation).FirstOrDefault()?.node;

                    }

                }
                else
                {
                    inventoryItem = currentData.inventoryItem?.inventoryLevels?.edges?.FirstOrDefault()?.node;
                }

                if (inventoryItem == null)
                    throw new Exception($"TenantId: {shopifyData.Id} - No inventory item in variant {currentData.legacyResourceId} sku:{message.Value.Sku}");

                if (message.Value.Quantity != inventoryItem.quantities[0].quantity)
                {
                    var createResult = await _apiActorGroup.Ask<ReturnMessage<InventoryUpdateMutationOutput>>(
                           new InventorySetQuantitiesMutation(new InventorySetQuantitiesInput
                           {
                               input = new InventoryLevel
                               {
                                   ignoreCompareQuantity = true,
                                   reason = "correction",
                                   name = "available",
                                   quantities = new List<inventoryAdjustChange>
                                    {
                                        new inventoryAdjustChange
                                        {
                                            inventoryItemId = currentData.inventoryItem?.id,
                                            locationId = inventoryItem.location?.id,
                                            //quantity = message.Value.DecreaseStock
                                            //        ? -message.Value.Quantity
                                            //        : message.Value.Quantity - inventoryItem.quantities[0].quantity
                                            quantity = message.Value.Quantity
                                        }
                                    }
                               }
                           }), cancellationToken);

                    if (createResult.Result == Result.Error)
                    {
                        var errorMessage = new ReturnMessage { Result = Result.Error, Error = createResult.Error };
                        _logger.Warning($"ShopifyService - Error in UpdateStock | {createResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData,
                                    $"Error when updating inventory for sku - {message.Value.Sku}"));
                        return errorMessage;
                    }

                    if (createResult.Data.inventoryAdjustQuantity.userErrors?.Any() == true)
                    {
                        var errormessage = $"TenantId: {shopifyData.Id} - Error in update shopify stock: {JsonSerializer.Serialize(createResult.Data.inventoryAdjustQuantity.userErrors)}";
                        _logger.Warning(errormessage);
                        throw new Exception(errormessage);
                    }

                }
            }
            return new ReturnMessage { Result = Result.OK };
        }

        public async Task<ReturnMessage> UpdateStockKit(ShopifyUpdateStockKitMessage message,
                                                        ShopifyDataMessage shopifyData,
                                                        QueueClient fullProductQueueClient,
                                                        CancellationToken cancellationToken)
        {

            ProductResult currentData = null;

            var queryByIdResult = await GetProductById(message.ExternalProductId, cancellationToken);

            if (queryByIdResult.Result == Result.Error)
            {
                _logger.Warning($"ShopifyService - Error in UpdateStockKit | {queryByIdResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                return new ReturnMessage { Result = Result.Error, Error = queryByIdResult.Error };
            }


            if (queryByIdResult.Data.product != null)
                currentData = queryByIdResult.Data.product;

            var inventoryItem = currentData.variants.edges.FirstOrDefault()?.node?.inventoryItem?.inventoryLevels?.edges?.FirstOrDefault()?.node;

            if (inventoryItem == null)
                throw new Exception($"no inventoryitem in variant {currentData.legacyResourceId} sku:{message.Sku}");



            var tagsWithStock = currentData.tags.Where(x => x.StartsWith(Tags.GetProductKitSku(message.Sku))).ToList()
                .Select(x => Tags.ProductKitSkuWithStock(message.Sku, message.Quantity)).ToList();

            var tags = currentData.tags.Where(x => !x.StartsWith(Tags.GetProductKitSku(message.Sku)))
                .ToList()
                .Union(tagsWithStock);

            int qtdStock;
            var availableStock = tags
                .Where(x => !x.StartsWith(Tags.ProductKit))
                .Select(x => int.TryParse(x.Split("|").LastOrDefault(), out qtdStock) ? (int?)qtdStock : null).ToList();

            var updateResult = await _apiActorGroup.Ask<ReturnMessage<ProductAndInventoryUpdateMutationOutput>>(
                    new ProductAndInventoryUpdateMutation(new ProductAndInventoryUpdateMutationInput
                    {
                        input = new Product
                        {
                            id = currentData.id,
                            tags = tags.ToList()
                        },
                        inventoryItemAdjustments = new List<InventoryAdjustment> {
                            new InventoryAdjustment {
                                availableDelta = -inventoryItem.quantities[0].quantity,
                                inventoryItemId = currentData.variants.edges.FirstOrDefault()?.node?.inventoryItem?.id
                            },
                            new InventoryAdjustment {
                                availableDelta = availableStock.Min().Value,
                                inventoryItemId = currentData.variants.edges.FirstOrDefault()?.node?.inventoryItem?.id
                            }
                        },

                        locationId = inventoryItem.location?.id
                    }), cancellationToken);

            if (updateResult.Result == Result.Error)
            {
                _logger.Warning($"ShopifyService - Error in UpdateStockKit | {updateResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), new ProductAndInventoryUpdateMutationInput
                {
                    input = new Product
                    {
                        id = currentData.id,
                        tags = tags.ToList()
                    },
                    inventoryItemAdjustments = new List<InventoryAdjustment> {
                            new InventoryAdjustment {
                                availableDelta = -inventoryItem.quantities[0].quantity,
                                inventoryItemId = currentData.variants.edges.FirstOrDefault()?.node?.inventoryItem?.id
                            },
                            new InventoryAdjustment {
                                availableDelta = availableStock.Min().Value,
                                inventoryItemId = currentData.variants.edges.FirstOrDefault()?.node?.inventoryItem?.id
                            }
                        },
                    locationId = inventoryItem.location?.id
                }, shopifyData));
                return new ReturnMessage { Result = Result.Error, Error = updateResult.Error };
            }


            if (updateResult.Data.productUpdate.userErrors?.Any() == true)
                throw new Exception($"Error in update shopify product: {JsonSerializer.Serialize(updateResult.Data.productUpdate.userErrors)}");
            if (updateResult.Data.inventoryBulkAdjustQuantityAtLocation.userErrors?.Any() == true)
                throw new Exception($"Error in update shopify product: {JsonSerializer.Serialize(updateResult.Data.inventoryBulkAdjustQuantityAtLocation.userErrors)}");


            return new ReturnMessage { Result = Result.OK };
        }

        public async Task<ReturnMessage> UpdateProductImages(ShopifyUpdateProductImagesMessage message,
                                                             ShopifyDataMessage shopifyData,
                                                             CancellationToken cancellationToken)
        {
            ProductResult currentData = null;

            if (!string.IsNullOrEmpty(message.ExternalProductId))
            {
                var queryByTagResult = await GetProductByTag(SetTagValue(Tags.ProductExternalId, message.ExternalProductId), cancellationToken);

                if (queryByTagResult.Result == Result.Error)
                {
                    _logger.Warning($"ShopifyService - Error in UpdateProductImages | {queryByTagResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                    return new ReturnMessage { Result = Result.Error, Error = queryByTagResult.Error };
                }


                if (queryByTagResult.Data.products.edges.Any() == true)
                    currentData = queryByTagResult.Data.products.edges[0].node;
            }

            if (currentData == null && message.ShopifyProductId.HasValue)
            {
                var queryByIdResult = await GetProductById(message.ShopifyProductId.Value, cancellationToken);

                if (queryByIdResult.Result == Result.Error)
                {
                    _logger.Warning($"ShopifyService - Error in UpdateProductImages | {queryByIdResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                    return new ReturnMessage { Result = Result.Error, Error = queryByIdResult.Error };
                }


                if (queryByIdResult.Data.product != null)
                    currentData = queryByIdResult.Data.product;
            }


            if (currentData == null)
                throw new Exception("Product not found");

            var clearResult = await _apiActorGroup.Ask<ReturnMessage<ProductUpdateMutationOutput>>(
                        new ProductUpdateMutation(new ProductUpdateMutationInput
                        {
                            input = new Product
                            {
                                id = currentData.id//,
                                //images = new List<Image>() //clean all images
                            }
                        }), cancellationToken);

            if (clearResult.Result == Result.Error)
            {
                _logger.Warning($"ShopifyService - Error in UpdateProductImages | {clearResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), new ProductUpdateMutationInput
                {
                    input = new Product
                    {
                        id = currentData.id//,
                        //images = new List<Image>() //clean all images
                    }
                }, shopifyData));
                return new ReturnMessage { Result = Result.Error, Error = clearResult.Error };
            }


            if (clearResult.Data.productUpdate.userErrors?.Any() == true)
                throw new Exception($"Error in update shopify product (clean images): {JsonSerializer.Serialize(clearResult.Data.productUpdate.userErrors)}");

            Dictionary<string, string> imagesIds = new Dictionary<string, string>();

            foreach (var images in message.Images.ImageUrls.Chunks(5))
            {
                var appendResult = await _apiActorGroup.Ask<ReturnMessage<ProductAppendImagesMutationOutput>>(
                           new ProductAppendImagesMutation(new ProductAppendImagesMutationInput
                           {
                               input = new ProductAppendImage
                               {
                                   id = currentData.id,
                                   images = images.Select(i => new Image
                                   {
                                       src = i
                                   }).ToList()
                               }
                           }), cancellationToken);

                if (appendResult.Result == Result.Error)
                {
                    _logger.Warning($"ShopifyService - Error in UpdateProductImages | {appendResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), new ProductAppendImagesMutationInput
                    {
                        input = new ProductAppendImage
                        {
                            id = currentData.id,
                            images = images.Select(i => new Image
                            {
                                src = i
                            }).ToList()
                        }
                    }, shopifyData));
                    return new ReturnMessage { Result = Result.Error, Error = appendResult.Error };
                }

                if (appendResult.Data.productAppendImages.userErrors?.Any() == true)
                    throw new Exception($"Error in update shopify product (clean images): {JsonSerializer.Serialize(appendResult.Data.productAppendImages.userErrors)}");

                for (int i = 0; i < images.Count; i++)
                {
                    foreach (var sku in message.Images.SkuImages.Where(x => x.SkuImageUrl == images[i]).Select(x => x.Sku))
                    {
                        imagesIds.Add(sku, appendResult.Data.productAppendImages.newImages[i].id);
                    }
                }
            }

            var updateResult = await _apiActorGroup.Ask<ReturnMessage<ProductUpdateMutationOutput>>(
                        new ProductUpdateMutation(new ProductUpdateMutationInput
                        {
                            input = new Product
                            {
                                id = currentData.id/*,
                                variants = currentData.variants.edges.Select(v => new Variant
                                {
                                    compareAtPrice = new Optional<decimal?>(v.node.compareAtPrice),
                                    id = v.node.id,
                                    imageId = imagesIds.ContainsKey(v.node.sku) ? imagesIds[v.node.sku] : null
                                }).ToList()*/
                            }
                        }), cancellationToken);

            if (updateResult.Result == Result.Error)
            {
                _logger.Warning($"ShopifyService - Error in UpdateProductImages | {updateResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), new ProductUpdateMutationInput
                {
                    input = new Product
                    {
                        id = currentData.id/*,
                        variants = currentData.variants.edges.Select(v => new Variant
                        {
                            compareAtPrice = new Optional<decimal?>(v.node.compareAtPrice),
                            id = v.node.id,
                            imageId = imagesIds.ContainsKey(v.node.sku) ? imagesIds[v.node.sku] : null
                        }).ToList()*/
                    }
                }, shopifyData));
                return new ReturnMessage { Result = Result.Error, Error = updateResult.Error };
            }


            if (updateResult.Data.productUpdate.userErrors?.Any() == true)
                throw new Exception($"Error in update shopify product (update sku image): {JsonSerializer.Serialize(updateResult.Data.productUpdate.userErrors)}");


            return new ReturnMessage { Result = Result.OK };
        }

        public async Task<ReturnMessage> ListOrder(ShopifyListOrderMessage message,
                                                   ShopifyDataMessage shopifyData,
                                                   QueueClient updateOrderQueueClient,
                                                   QueueClient updateOrderNumberTagQueueClient,
                                                   QueueClient updateStockQueueClient,
                                                   CancellationToken cancellationToken)
        {

            var process = new ShopifyListOrderProcess()
            {
                Id = Guid.NewGuid(),
                TenantId = shopifyData.Id,
                ProcessDate = DateTime.Now,
                OrderId = message.ShopifyId
            };

            if (shopifyData.EnableSaveIntegrationInformations)
            {
                try
                {
                    await _shopifyListOrderProcessRepository.Save(process);
                }
                catch (Exception ex)
                {
                    _logger.Warning("Problemas ao salvar processo de integração de pedido (1) | {0}",
                    Newtonsoft.Json.JsonConvert.SerializeObject(ex, new Newtonsoft.Json.JsonSerializerSettings
                    {
                        NullValueHandling = Newtonsoft.Json.NullValueHandling.Include,
                        ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                        Error = (serializer, error) => error.ErrorContext.Handled = true
                    }));
                }
            }

            try
            {
                var queryByIdResult = await _apiActorGroup.Ask<ReturnMessage<OrderResult>>(
                    new GetOrderRequest { OrderId = message.ShopifyId }, cancellationToken
                );

                if (queryByIdResult.Result == Result.Error)
                {
                    if (shopifyData.EnableSaveIntegrationInformations)
                    {
                        try
                        {
                            process.Exception = Newtonsoft.Json.JsonConvert.SerializeObject(queryByIdResult.Error);

                            await _shopifyListOrderProcessRepository.Save(process);
                        }
                        catch (Exception ex)
                        {
                            _logger.Warning("Problemas ao salvar processo de integração de pedido (2) | {0}",
                                Newtonsoft.Json.JsonConvert.SerializeObject(ex, new Newtonsoft.Json.JsonSerializerSettings
                                {
                                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Include,
                                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                                    Error = (serializer, error) => error.ErrorContext.Handled = true
                                }));
                        }
                    }

                    _logger.Warning($"ShopifyService - Error in ListOrder | {queryByIdResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                    return new ReturnMessage { Result = Result.Error, Error = queryByIdResult.Error };
                }

                if (queryByIdResult.Data?.order == null)
                    throw new Exception("Order not found");

                if (queryByIdResult.Data?.order.id < shopifyData.MinOrderId)
                    return new ReturnMessage { Result = Result.OK };

                if (!queryByIdResult.Data.order.payment_gateway_names.Contains("mercado_pago") && (queryByIdResult.Data.order.note_attributes is null || queryByIdResult.Data.order.note_attributes.Count <= 0))
                {
                    try
                    {
                        queryByIdResult.Data.order.note_attributes = await GetNoteAttributes(queryByIdResult.Data.order.payment_gateway_names.FirstOrDefault(), queryByIdResult.Data.order.checkout_token);

                        if (queryByIdResult.Data.order.note_attributes is null || queryByIdResult.Data.order.note_attributes.Count <= 0)
                            throw new Exception("Não foram encontrados note_attributes na base de dados");

                        await UpdateOrderNoteAttributesInShopify(queryByIdResult.Data.order.id, queryByIdResult.Data.order.note_attributes);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Erro ao buscar note_attributes", ex);
                    }
                }

                var shopifySendOrderToERPMessage = await BuildSendOrderToErp(message, queryByIdResult.Data.order, shopifyData, cancellationToken);

                if (updateOrderQueueClient != null)
                {
                    if (shopifyData.EnableSaveIntegrationInformations)
                        shopifySendOrderToERPMessage.ShopifyListOrderProcessId = process.Id;

                    var serviceBusMessage = new ServiceBusMessage(shopifySendOrderToERPMessage);
                    await updateOrderQueueClient.SendAsync(serviceBusMessage.GetMessage(shopifySendOrderToERPMessage.ID, EnableScheduledNextHour(shopifyData)));

                    if (shopifyData.EnableSaveIntegrationInformations)
                    {
                        try
                        {
                            var shopifyOrderIntegration = new ShopifyListOrderIntegration
                            {
                                Id = Guid.NewGuid(),
                                TenantId = shopifyData.Id,
                                IntegrationDate = DateTime.Now,
                                Payload = Newtonsoft.Json.JsonConvert.SerializeObject(shopifySendOrderToERPMessage),
                                OrderId = message.ShopifyId,
                                Action = "ShopifySendOrderToERPMessage",
                                ShopifyListOrderProcessId = process.Id
                            };

                            _shopifyListOrderIntegrationRepository.Save(shopifyOrderIntegration);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"Problemas ao salvar integração ShopifySendOrderToERPMessage de pedido");
                        }
                    }
                }
                return new ReturnMessage { Result = Result.OK };
            }
            catch (Exception ex)
            {
                if (shopifyData.EnableSaveIntegrationInformations)
                {
                    try
                    {
                        process.Exception = ex.Message;

                        await _shopifyListOrderProcessRepository.Save(process);
                    }
                    catch (Exception exx)
                    {
                        _logger.Error(exx, $"Problemas ao salvar catch do processo de integração de pedido (3)");
                    }
                }
                throw;
            }
        }

        private static int EnableScheduledNextHour(ShopifyDataMessage shopifyData)
        {
            var scheduled = 0;
            if (shopifyData.EnableScheduledNextHour)
            {
                var dateInitialForIntegration = DateTime.UtcNow.AddHours(1).AddMinutes(-DateTime.UtcNow.Minute).AddSeconds(-DateTime.UtcNow.Second);
                var dateNow = DateTime.UtcNow.AddSeconds(-DateTime.UtcNow.Second);
                TimeSpan ts = dateInitialForIntegration - dateNow;

                scheduled = (int)Math.Floor(ts.TotalMilliseconds);
            }

            return scheduled;
        }

        public async Task<ShopifySendOrderToERPMessage> BuildSendOrderToErp(ShopifyListOrderMessage message,
                                                                            OrderResult.Order currentData,
                                                                            ShopifyDataMessage shopifyData,
                                                                            CancellationToken cancellationToken)
        {
            var pt = new CultureInfo("pt-BR");

            var discountItens = currentData.line_items.Sum(soma => soma.discount_allocations.Sum(s => s.amount));
            var discountShipping = currentData.shipping_lines.Sum(soma => soma.discount_allocations.Sum(s => s.amount));
            var totalItemsPrice = currentData.total_line_items_price - discountItens;
            var totalShippingPrice = currentData.total_shipping_price_set.shop_money.amount - discountShipping;

            /*nova implementação*/
            /*IGatewayService service = GatewayServiceFactory.GetService(currentData.payment_gateway_names);

            var paymentData = service.GetPaymentData(currentData);*/

            if (currentData.payment_gateway_names.Any(x => x.ToLower().Contains("mercado_pago")))
            {
                var transactions = await _apiActorGroup.Ask<ReturnMessage<GetOrderTransactionResult>>(new GetOrderTransactionRequest { OrderId = currentData.id }, cancellationToken);
                if (transactions.Result == Result.Error)
                    _logger.Error(transactions.Error, "Erro ao buscar transação do pedido na Shopify");

                currentData.note_attributes.Add(new NoteAttribute { name = "Gateway", value = "mercado_pago" });
                currentData.note_attributes.Add(new NoteAttribute { name = "mercadoPagoNSU", value = transactions?.Data?.transactions?.FirstOrDefault().authorization });

                var noteAttributesMercadoPago = GetNoteAttributesMercadoPago(currentData);

                var externalOrderTag = SearchTagValue(currentData.tags?.Split(",").ToList(), Tags.OrderExternalId);
                string externalID = null;
                if (externalOrderTag?.Any() == true)
                    externalID = externalOrderTag[0];
                else
                    externalID = GetExternalOrderNumber(shopifyData, currentData.order_number.ToString());

                bool isPickup = false;
                List<string> pickupData = null;
                if (currentData.shipping_lines.FirstOrDefault()?.code.StartsWith("pickup") == true)
                {
                    isPickup = true;
                    pickupData = currentData.shipping_lines.FirstOrDefault()?.code.Replace("pickup$", "").Split('|').ToList();
                }

                await FillMultiLocation(message.ShopifyId, currentData, cancellationToken);

                (string ddd, string phone) = ParsePhone(currentData?.customer?.phone, shopifyData.ParsePhoneDDD);

                var items = new List<ShopifySendOrderItemToERPMessage>();

                currentData.line_items.ForEach(async data =>
                {
                    items.Add(new ShopifySendOrderItemToERPMessage()
                    {
                        LocationId = shopifyData.EnabledMultiLocation ? data.location_id : currentData?.location_id,
                        Id = data.id,
                        Sku = await GetExtenalIdOrSKU(shopifyData, data),
                        Name = data.name,
                        ProductGift = IsGift(data),
                        Quantity = data.quantity,
                        Price = data.price,
                        DiscountValue = data.discount_allocations?.Sum(d => d.amount) ?? 0

                    });
                });

                var billingAddress = GetAddressDataMercadoPago(shopifyData, currentData.billing_address, noteAttributesMercadoPago, true);
                var deliveryAddress = !string.IsNullOrWhiteSpace(currentData.shipping_address?.address1) ?
                                GetAddressDataMercadoPago(shopifyData, currentData.shipping_address, noteAttributesMercadoPago, false)
                                : GetAddressDataMercadoPago(shopifyData, currentData.customer.default_address, noteAttributesMercadoPago, false);

                return new ShopifySendOrderToERPMessage
                {
                    ID = currentData.id,
                    ExternalID = currentData.name,
                    Name = currentData.name,
                    Number = currentData.order_number,
                    Approved = currentData.financial_status == OrderResult.FinancialStatus.authorized || currentData.financial_status == OrderResult.FinancialStatus.paid,
                    Shipped = currentData.fulfillment_status == OrderResult.OrderFulfillmentStatus.fulfilled,
                    TrackingNumber = currentData.fulfillments?.FirstOrDefault()?.tracking_number,
                    Delivered = currentData.fulfillments?.FirstOrDefault()?.shipment_status == OrderResult.ShipmentStatus.delivered,
                    Cancelled = currentData.cancelled_at != null,
                    CreatedAt = currentData.created_at,
                    DaysToDelivery = currentData.GetPrazoEntrega() ?? shopifyData.DaysToDelivery,
                    Subtotal = currentData.total_line_items_price,
                    Total = Math.Round(totalItemsPrice + totalShippingPrice + currentData.total_tax, 2),
                    DiscountsValues = discountItens,
                    ShippingValue = totalShippingPrice,
                    InterestValue = 0,
                    TaxValue = currentData.total_tax,
                    CarrierName = currentData.shipping_lines.FirstOrDefault()?.title,
                    IsPickup = isPickup,
                    PickupAdditionalData = pickupData,
                    Note = currentData.note,
                    Items = items,
                    Checkout_Token = currentData.checkout_token,
                    SourceName = currentData.source_name,
                    Customer = new ShopifySendOrderCustomerToERPMessage
                    {
                        FirstName = currentData.customer.first_name,
                        LastName = currentData.customer.last_name,
                        DDD = ddd,
                        Phone = phone,
                        Email = currentData.customer.email,
                        Note = currentData.customer.note,
                        Company = currentData.billing_address.company,
                        BillingAddress = billingAddress,
                        DeliveryAddress = deliveryAddress
                    },
                    PaymentData = new ShopifySendOrderPaymentDataToERPMessage
                    {
                        Issuer = noteAttributesMercadoPago.PaymentMethod,
                        InstallmentQuantity = noteAttributesMercadoPago.GetPaymentInstallment(),
                        PaymentType = noteAttributesMercadoPago.PaymentType
                    },
                    NoteAttributes = currentData.note_attributes.Select(n => new ShopifySendOrderNoteAttributeToERPMessage
                    {
                        Name = n.name,
                        Value = n.value
                    }).ToList(),
                    DeliveryCount = message.DeliveryCount,
                    Refunds = currentData.refunds,
                };
            }
            else
            {
                var noteAttributes = GetNoteAttributes(currentData);

                var interestValue = (totalItemsPrice + totalShippingPrice + currentData.total_tax) * (noteAttributes.Multiplo / 100);

                var externalOrderTag = SearchTagValue(currentData.tags?.Split(",").ToList(), Tags.OrderExternalId);

                string externalID = null;

                if (externalOrderTag?.Any() == true)
                    externalID = externalOrderTag[0];
                else
                    externalID = GetExternalOrderNumber(shopifyData, currentData.order_number.ToString());

                bool isPickup = false;
                List<string> pickupData = null;
                if (currentData.shipping_lines.FirstOrDefault()?.code.StartsWith("pickup") == true)
                {
                    isPickup = true;
                    pickupData = currentData.shipping_lines.FirstOrDefault()?.code.Replace("pickup$", "").Split('|').ToList();
                }

                var birthDate = DateTime.TryParse(noteAttributes.NoteBirthDate, out DateTime birthDate2) ? birthDate2 : default(DateTime?);

                await FillMultiLocation(message.ShopifyId, currentData, cancellationToken);

                (string ddd, string phone) = ParsePhone(currentData?.customer?.phone, shopifyData.ParsePhoneDDD);

                var items = new List<ShopifySendOrderItemToERPMessage>();

                currentData.line_items.ForEach(async data =>
                {
                    items.Add(new ShopifySendOrderItemToERPMessage()
                    {
                        LocationId = shopifyData.EnabledMultiLocation ? data.location_id : currentData?.location_id,
                        Id = data.id,
                        Sku = await GetExtenalIdOrSKU(shopifyData, data),
                        Name = data.name,
                        ProductGift = IsGift(data),
                        Quantity = data.quantity,
                        Price = data.price,
                        DiscountValue = data.discount_allocations?.Sum(d => d.amount) ?? 0

                    });
                });

                return new ShopifySendOrderToERPMessage
                {
                    ID = currentData.id,
                    ExternalID = externalID,
                    Name = currentData.name,
                    Number = currentData.order_number,
                    Approved = currentData.financial_status == OrderResult.FinancialStatus.authorized || currentData.financial_status == OrderResult.FinancialStatus.paid,
                    Shipped = currentData.fulfillment_status == OrderResult.OrderFulfillmentStatus.fulfilled,
                    TrackingNumber = currentData.fulfillments?.FirstOrDefault()?.tracking_number,
                    Delivered = currentData.fulfillments?.FirstOrDefault()?.shipment_status == OrderResult.ShipmentStatus.delivered,
                    Cancelled = currentData.cancelled_at != null,
                    CreatedAt = currentData.created_at,
                    DaysToDelivery = currentData.GetPrazoEntrega() ?? shopifyData.DaysToDelivery,
                    Subtotal = currentData.total_line_items_price,
                    Total = Math.Round(totalItemsPrice + totalShippingPrice + currentData.total_tax + interestValue, 2),
                    DiscountsValues = discountItens,
                    ShippingValue = totalShippingPrice,
                    InterestValue = interestValue,
                    TaxValue = currentData.total_tax,
                    CarrierName = currentData.shipping_lines.FirstOrDefault()?.title,
                    IsPickup = isPickup,
                    PickupAdditionalData = pickupData,
                    Note = currentData.note,
                    Items = items,
                    vendor = noteAttributes.VendorShopify,
                    Checkout_Token = currentData.checkout_token,
                    SourceName = currentData.source_name,
                    Customer = new ShopifySendOrderCustomerToERPMessage
                    {
                        BirthDate = birthDate,
                        FirstName = currentData.customer.first_name,
                        LastName = currentData.customer.last_name,
                        DDD = ddd,
                        Phone = phone,
                        Email = currentData.customer.email,
                        Note = currentData.customer.note,
                        Company = currentData.billing_address.company,
                        BillingAddress = GetAddressData(shopifyData, currentData.billing_address, noteAttributes.NotesBillingAddress),
                        DeliveryAddress = !string.IsNullOrWhiteSpace(currentData.shipping_address?.address1) ?
                                GetAddressData(shopifyData, currentData.shipping_address, noteAttributes.NotesShippingAddress) : GetAddressData(shopifyData, currentData.customer.default_address, noteAttributes.NotesShippingAddress)
                    },
                    PaymentData = new ShopifySendOrderPaymentDataToERPMessage
                    {
                        Issuer = noteAttributes.Issuer,
                        InstallmentQuantity = noteAttributes.InstallmentQuantity,
                        InstallmentValue = noteAttributes.InstallmentValue,
                        PaymentType = noteAttributes.GetPayment
                    },
                    NoteAttributes = currentData.note_attributes.Select(n => new ShopifySendOrderNoteAttributeToERPMessage
                    {
                        Name = n.name,
                        Value = n.value
                    }).ToList(),
                    DeliveryCount = message.DeliveryCount,
                    Refunds = currentData.refunds,
                };
            }
        }

        private OrderNoteAttributesModelMercadoPagoInfo GetNoteAttributesMercadoPago(OrderResult.Order currentData)
        {
            return new OrderNoteAttributesModelMercadoPagoInfo
            {
                PaymentMethod = currentData?.note_attributes?.FirstOrDefault(w => w.name == "payment_method")?.value,
                PaymentType = currentData?.note_attributes?.FirstOrDefault(w => w.name == "payment_type")?.value,
                PaymentInstallment = currentData?.note_attributes?.FirstOrDefault(w => w.name == "payment_installment")?.value,
                ShippingFullAddress = currentData?.note_attributes?.FirstOrDefault(w => w.name == "shipping_full_address")?.value,
                ShippingStreetName = currentData?.note_attributes?.FirstOrDefault(w => w.name == "shipping_street_name")?.value,
                ShippingStreetNumber = currentData?.note_attributes?.FirstOrDefault(w => w.name == "shipping_street_number")?.value,
                ShippingNeighborhood = currentData?.note_attributes?.FirstOrDefault(w => w.name == "shipping_neighborhood")?.value,
                ShippingStreetComplement = currentData?.note_attributes?.FirstOrDefault(w => w.name == "shipping_street_complement")?.value
            };
        }

        public async Task<string> GetExtenalIdOrSKU(ShopifyDataMessage shopifyData, LineItem lineItem)
        {
            var externalId = lineItem.sku;

            if (string.IsNullOrEmpty(shopifyData.SkuFieldType.ToString()))
                return externalId;

            if (!string.IsNullOrEmpty(shopifyData.SkuFieldType.ToString())
            && shopifyData.SkuFieldType.ToString() == SkuFieldType.cod_produto.ToString())
            {
                var product = (await GetProductById((long)lineItem.product_id)).Data?.product;
                externalId = product?.tags != null ? product.tags.Where(x => x.StartsWith(Tags.SkuOriginal))
                                                                     .Select(x => GetTagValue(x, Tags.SkuOriginal)).FirstOrDefault() : lineItem.sku;
            }

            return externalId;
        }


        /*private (string, string) GetCustomerName(OrderResult.Order currentOrder)
        {
            var firstName = string.IsNullOrWhiteSpace(currentOrder?.customer?.first_name) ? currentOrder?.customer?.default_address?.first_name : currentOrder?.customer?.first_name;
            var lastName = string.IsNullOrWhiteSpace(currentOrder?.customer?.last_name) ? currentOrder?.customer?.default_address?.last_name : currentOrder?.customer?.last_name;

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                firstName = currentOrder?.billing_address?.first_name;
                lastName = currentOrder?.billing_address?.last_name;
            }

            return (string.IsNullOrWhiteSpace(firstName) ? currentOrder.shipping_address.first_name : firstName, string.IsNullOrWhiteSpace(lastName) ? currentOrder.shipping_address.last_name : lastName);
        }*/

        private async Task<List<NoteAttribute>> GetNoteAttributes(string gateway, string checkoutId)
        {
            var validationGateway = new List<IValidationGateway>
            {
                new Braspag(_serviceProvider),
                new Moip(_serviceProvider),
            };

            var validatesIntegrationTypes = new ValidateGateway(validationGateway);

            return await validatesIntegrationTypes.ReturnNoteAttributes(gateway, checkoutId);
        }

        private OrderNoteAttributesModelInfo GetNoteAttributes(OrderResult.Order currentData)
        {
            var pt = new System.Globalization.CultureInfo("pt-BR");

            return new OrderNoteAttributesModelInfo
            {
                VendorShopify = currentData.note_attributes.FirstOrDefault(x => x.name == "aditional_info_extra_vendedor")?.value ?? "",
                Multiplo = decimal.Parse(currentData.note_attributes.FirstOrDefault(n => n.name == "aditional_info_parcels_juros")?.value ?? "0", pt),

                NotesBillingAddress = new ShopifySendOrderAddressToERPMessage
                {
                    District = currentData.note_attributes.FirstOrDefault(n => n.name == "aditional_info_extra_billing_address_bairro")?.value,
                    City = currentData.note_attributes.FirstOrDefault(n => n.name == "aditional_info_extra_billing_address_cidade")?.value,
                    Complement = currentData.note_attributes.FirstOrDefault(n => n.name == "aditional_info_extra_billing_address_complemento")?.value,
                    Address = currentData.note_attributes.FirstOrDefault(n => n.name == "aditional_info_extra_billing_endereco")?.value,
                    Number = currentData.note_attributes.FirstOrDefault(n => n.name == "aditional_info_extra_billing_address_number")?.value,
                    State = currentData.note_attributes.FirstOrDefault(n => n.name == "aditional_info_extra_billing_address_state")?.value
                },

                NotesShippingAddress = new ShopifySendOrderAddressToERPMessage
                {
                    District = currentData.note_attributes.FirstOrDefault(n => n.name == "aditional_info_extra_bairro")?.value,
                    City = currentData.note_attributes.FirstOrDefault(n => n.name == "aditional_info_extra_cidade")?.value,
                    Complement = currentData.note_attributes.FirstOrDefault(n => n.name == "aditional_info_extra_complemento")?.value,
                    Address = currentData.note_attributes.FirstOrDefault(n => n.name == "aditional_info_extra_endereco")?.value,
                    Number = currentData.note_attributes.FirstOrDefault(n => n.name == "aditional_info_extra_numero")?.value,
                    State = currentData.note_attributes.FirstOrDefault(n => n.name == "aditional_info_extra_address_state")?.value
                },

                NoteBirthDate = currentData.note_attributes.FirstOrDefault(n => string.Equals(n.name, "Data de aniversário", StringComparison.OrdinalIgnoreCase))?.value,
                Issuer = currentData.note_attributes.FirstOrDefault(n => n.name == "aditional_info_brands")?.value,
                InstallmentQuantity = int.Parse(currentData.note_attributes.FirstOrDefault(n => (n.name == "aditional_info_parcels_number" || n.name == "aditional_info_extra_installment_count") && !string.IsNullOrWhiteSpace(n.value))?.value ?? "0"),
                InstallmentValue = decimal.Parse(currentData.note_attributes.FirstOrDefault(n => n.name == "aditional_info_parcels_valor" && !string.IsNullOrWhiteSpace(n.value))?.value ?? "0", pt),

                GetPayment = GetPaymentType(currentData),
            };
        }

        private bool IsGift(OrderResult.LineItem item)
        {
            return item.properties?.Any(w => w.name == "Brinde" && w.value == "true") ?? false;
        }

        private string GetPaymentType(OrderResult.Order currentData)
        {
            var payment = string.Empty;
            var MP = currentData.payment_gateway_names.FirstOrDefault(x => x.ToLower() == "mercado_pago");

            if (MP != null)
            {
                return currentData.note_attributes.FirstOrDefault(n => n.name == "payment_type")?.value ?? MP;
            }

            payment = currentData.note_attributes.FirstOrDefault(n => n.name == "shipping_payment_type")?.value ?? MP;

            if (string.IsNullOrEmpty(payment))
            {
                payment = currentData.note_attributes.FirstOrDefault(n => n.name == "payment_additional_info")?.value ?? "OUTROS";
            }

            return payment;
        }

        private ShopifySendOrderAddressToERPMessage GetAddressDataMercadoPago(ShopifyDataMessage shopifyData,
                                                                              OrderResult.Address address,
                                                                              OrderNoteAttributesModelMercadoPagoInfo notesAddress,
                                                                              bool isBillingAddress)
        {
            var contactName = $"{address.first_name} {address.last_name}";
            if (!string.IsNullOrWhiteSpace(contactName) && contactName.Length > 50)
                contactName = contactName[0..50];

            (string ddd, string phone) = ParsePhone(address.phone, shopifyData.ParsePhoneDDD);

            if (string.IsNullOrWhiteSpace(address.zip))
                new ArgumentNullException("Zip code is null or empty");

            var addressParts = string.Empty;
            var number = string.Empty;
            var district = string.Empty;

            string[] parts = null;
            char[] delimiters = { ',', '-' };
            var virgulaHifen = @"^[\w\s.,]+,\s*[\w\s.-]+$";
            var virgula = @"^[\w\s]+,\s*[\w\s]+,\s*[\w\s]+$";
            var regex = new Regex(virgulaHifen);

            if (regex.IsMatch(address?.address1) == true)
                parts = address?.address1?.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            regex = new Regex(virgula);
            if (regex.IsMatch(address?.address1) == true)
                parts = address?.address1?.Split(",", StringSplitOptions.RemoveEmptyEntries);

            if(parts != null && parts.Length >= 3)
            {
                addressParts = parts[0];
                number = parts[1];
                district = parts[2];
            }

            else
            {
                addressParts = notesAddress.ShippingStreetName;
                number = notesAddress.ShippingStreetNumber;
                district = notesAddress.ShippingNeighborhood;
            }

            return new ShopifySendOrderAddressToERPMessage
            {
                District = district,//getDistrict(notesAddress, isBillingAddress),
                ZipCode = ReplaceString(address.zip, new Dictionary<string, string> { { ".", "" }, { "-", "" } }, "ZipCode"),
                City = CleanText(address.city.ToUpper(), "address.city"),
                Complement = getComplement(address, notesAddress, isBillingAddress),
                Contact = contactName,
                State = ReturnState(shopifyData, address.country_code, address.province_code),
                DDD = ddd,
                Phone = phone,
                Address = addressParts,//getAddress(address, isBillingAddress, notesAddress, shopifyData),
                Number = number, //getNumber(shopifyData, address, notesAddress, isBillingAddress),
                Country = address.country,
                CountryCode = address.country_code
            };

            string getAddress(OrderResult.Address address, bool isBillingAddress, OrderNoteAttributesModelMercadoPagoInfo notesAddress, ShopifyDataMessage shopifyData)
            {
                if (isBillingAddress)
                    return shopifyData.DisableAddressParse || address.address1.IndexOf(",") == -1 ? address.address1 : address.address1.Substring(0, address.address1.IndexOf(","));

                if (string.IsNullOrWhiteSpace(notesAddress.ShippingStreetName))
                {
                    return address.address1.IndexOf(",") == -1 ? address.address1 : address.address1.Substring(0, address.address1.IndexOf(","));
                }

                return notesAddress.ShippingStreetName;
            }

            string getDistrict(OrderNoteAttributesModelMercadoPagoInfo notesAddress, bool isBillingAddress)
            {
                if (isBillingAddress || string.IsNullOrWhiteSpace(notesAddress.ShippingNeighborhood))
                    return "";

                return CleanText(notesAddress.ShippingNeighborhood, "ShippingNeighborhood").ToUpper();
            }

            string getComplement(OrderResult.Address address, OrderNoteAttributesModelMercadoPagoInfo notesAddress, bool isBillingAddress)
            {
                if (isBillingAddress)
                    return !string.IsNullOrWhiteSpace(address.address2) ? CleanText(address.address2?.ToUpper(), "address.address2") : "";

                if (string.IsNullOrWhiteSpace(notesAddress.ShippingStreetComplement))
                {
                    return !string.IsNullOrWhiteSpace(address.address2) ? CleanText(address.address2?.ToUpper(), "address.address2") : "";
                }

                return CleanText(notesAddress.ShippingStreetComplement, "notesAddress.Complement").ToUpper();
            }

            string getNumber(ShopifyDataMessage shopifyData, OrderResult.Address address, OrderNoteAttributesModelMercadoPagoInfo notesAddress, bool isBillingAddress)
            {
                if (isBillingAddress)
                    return shopifyData.DisableAddressParse ? "" : GetAddressNumber(address?.address1);

                if (string.IsNullOrWhiteSpace(notesAddress.ShippingStreetNumber))
                {
                    return shopifyData.DisableAddressParse ? "" : GetAddressNumber(address?.address1);
                }

                return notesAddress.ShippingStreetNumber;
            }
        }

        private ShopifySendOrderAddressToERPMessage GetAddressData(ShopifyDataMessage shopifyData, OrderResult.Address address, ShopifySendOrderAddressToERPMessage notesAddress)
        {
            var contactName = $"{address.first_name} {address.last_name}";
            if (!string.IsNullOrWhiteSpace(contactName) && contactName.Length > 50)
                contactName = contactName[0..50];

            (string ddd, string phone) = ParsePhone(address.phone, shopifyData.ParsePhoneDDD);

            if (string.IsNullOrWhiteSpace(address.zip))
                new ArgumentNullException("Zip code is null or empty");

            return new ShopifySendOrderAddressToERPMessage
            {
                District = !string.IsNullOrWhiteSpace(notesAddress.District) ? CleanText(notesAddress.District, "District").ToUpper() : (shopifyData.DisableAddressParse || address.address1.IndexOf(",") == -1 ? "" : CleanText(address.address1.Substring(address.address1.LastIndexOf(",") + 1).ToUpper(), "address.address1")),
                ZipCode = ReplaceString(address.zip, new Dictionary<string, string> { { ".", "" }, { "-", "" } }, "ZipCode"),
                City = !string.IsNullOrWhiteSpace(notesAddress.City) ? CleanText(notesAddress.City, "notesAddress.City").ToUpper() : CleanText(address.city.ToUpper(), "address.city"),
                Complement = !string.IsNullOrWhiteSpace(notesAddress.Complement) ? CleanText(notesAddress.Complement, "notesAddress.Complement").ToUpper() : !string.IsNullOrWhiteSpace(address.address2) ? CleanText(address.address2?.ToUpper(), "address.address2") : "",
                Contact = contactName,
                State = ReturnState(shopifyData, address.country_code, !string.IsNullOrWhiteSpace(notesAddress.State) ? notesAddress.State : address.province_code),
                DDD = ddd,
                Phone = phone,
                Address = !string.IsNullOrWhiteSpace(notesAddress.Address) ?
                                notesAddress.Address
                                : (shopifyData.DisableAddressParse || address.address1.IndexOf(",") == -1 ? address.address1 : address.address1.Substring(0, address.address1.IndexOf(","))),
                Number = !string.IsNullOrWhiteSpace(notesAddress.Number) ?
                                notesAddress.Number
                                : (shopifyData.DisableAddressParse ? "" : GetAddressNumber(address?.address1)),
                Country = address.country,
                CountryCode = address.country_code
            };
        }

        private string ReplaceString(string text, Dictionary<string, string> parameters, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(text) || parameters.Count <= 0)
                throw new ArgumentNullException($"(ShopifyService) - ReplaceString Problem | Text and parameters must have a value to replace | field :{fieldName} | TenantId: {_shopifyData.Id}");

            foreach (var param in parameters)
            {
                var key = param.Key;
                var value = param.Value;

                text = text.Replace(param.Key, param.Value);
            }

            return text;
        }

        private string ReturnState(ShopifyDataMessage shopifyData, string countryCode, string state)
        {
            if (shopifyData.EnableAuxiliaryCountry)
                return countryCode?.ToLower() != "br" ? "EX" : state;

            return state;
        }

        private static readonly List<Regex> _phoneFormats = new List<Regex>
        {
            new Regex("^(?<ddd>\\d{2})\\s+(?<phone>.+)$"), //11 94857-3847
            //new Regex("^\\+\\d{2}(?<ddd>\\d{2})(?<phone>.+)$"), //+5511948573847
            new Regex("^\\((?<ddd>\\d{2})\\)\\s*(?<phone>.+)$"), // (11) 94857-3847 
            new Regex("^\\+\\d{2}\\s*(?<ddd>\\d{2})\\s*(?<phone>\\d*)") //+5511948573847 -ALL
        };

        public (string ddd, string phone) ParsePhone(string phone, bool parsePhoneDDD)
        {
            if (parsePhoneDDD && !string.IsNullOrWhiteSpace(phone))
            {
                foreach (var format in _phoneFormats)
                {
                    if (format.IsMatch(phone))
                    {
                        var result = format.Match(phone);
                        return (result.Groups["ddd"].Value, result.Groups["phone"].Value);
                    }
                }
            }
            return (null, phone);
        }

        public async Task<ReturnMessage> UpdateOrderTagNumber(ShopifyUpdateOrderTagNumberMessage message, ShopifyDataMessage shopifyData, CancellationToken cancellationToken)
        {
            var process = new ShopifyUpdateOrderTagNumberProcess()
            {
                Id = Guid.NewGuid(),
                TenantId = shopifyData.Id,
                ProcessDate = DateTime.Now,
                OrderId = message.ShopifyId,
                OrderExternalId = message.OrderExternalId,
                OrderNumber = message.OrderNumber,
                ShopifyListOrderProcessReferenceId = message.ShopifyListOrderProcessId
            };

            if (shopifyData.EnableSaveIntegrationInformations)
            {
                try
                {
                    _shopifyUpdateOrderTagNumberProcessRepository.Save(process);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Problemas ao salvar processo de integração de atualização das tags do pedido (1)");
                }
            }

            try
            {
                var queryByIdResult = await _apiActorGroup.Ask<ReturnMessage<OrderByIdQueryOutput>>(
                new OrderByIdQuery(message.ShopifyId, Domain.Shopify.Models.Results.OrderResult.Tags), cancellationToken);

                if (queryByIdResult.Result == Result.Error)
                {
                    if (shopifyData.EnableSaveIntegrationInformations)
                    {
                        try
                        {
                            process.Exception = Newtonsoft.Json.JsonConvert.SerializeObject(queryByIdResult.Error);

                            _shopifyUpdateOrderTagNumberProcessRepository.Save(process);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"Problemas ao salvar processo de integração de atualização das tags do pedido (2)");
                        }
                    }
                    _logger.Warning($"ShopifyService - Error in UpdateOrderTagNumber | {queryByIdResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                    return new ReturnMessage { Result = Result.Error, Error = queryByIdResult.Error };
                }

                var currentData = queryByIdResult.Data.order;

                if (currentData == null)
                    throw new Exception($"order {message.ShopifyId} not found");

                var tags = new List<string>();

                if (message.OrderExternalId != null)
                {
                    tags.Add(SetTagValue(Tags.OrderExternalId, message.OrderExternalId));
                }

                if (message.OrderNumber != null)
                {
                    tags.Add(SetTagValue(Tags.OrderNumber, message.OrderNumber));
                }

                if (message.IntegrationStatus != null)
                {
                    tags.Add(SetTagValue(Tags.OrderIntegrationStatus, message.IntegrationStatus.ToString()));
                }

                var orderIsIntegratedTag = SetTagValue(Tags.OrderIsIntegrated, "True");

                tags.Add(orderIsIntegratedTag);

                if (message.CustomTags.Any())
                {
                    var preffix = message.CustomTags.Select(x => { return x.Split("-")[0]; }).ToList();

                    currentData.tags = currentData.tags?
                           .Select(t => t.Trim())
                           .Where(t => !preffix.Contains(t.Split("-")[0], StringComparer.InvariantCultureIgnoreCase))
                           .Select(t => { return t; })
                           .ToList();

                    currentData.tags = currentData.tags.Concat(message.CustomTags).ToList();
                    if (currentData.tags.Any(x => x.Split("-")[0].Contains("Error") || x.Split("-")[0].Contains("Erro", StringComparison.InvariantCultureIgnoreCase)))
                        tags = new List<string>();

                    _logger.Warning("Tags: {0}", Newtonsoft.Json.JsonConvert.SerializeObject(currentData.tags));
                }

                var preffixTags = tags.Select(x => { return x.Split("-")[0]; }).ToList();

                currentData.tags = currentData.tags.Select(t => t.Trim()).Where(x => !preffixTags.Contains(x.Split("-")[0], StringComparer.InvariantCultureIgnoreCase)).ToList();

                tags.AddRange(currentData.tags);

                if (tags.Any(t => t == orderIsIntegratedTag))
                {
                    tags.RemoveAll(t => t.StartsWith("Error", StringComparison.InvariantCultureIgnoreCase) || t.StartsWith("Erro", StringComparison.InvariantCultureIgnoreCase));
                }

                if (tags.Any())
                {
                    var orderUpdateMutationInput = new OrderUpdateMutationInput
                    {
                        input = new Order
                        {
                            id = currentData.id,
                            tags = tags
                        }
                    };

                    if (shopifyData.EnableSaveIntegrationInformations)
                    {
                        try
                        {
                            process.OrderUpdateMutationInput = Newtonsoft.Json.JsonConvert.SerializeObject(orderUpdateMutationInput);

                            _shopifyUpdateOrderTagNumberProcessRepository.Save(process);
                        }
                        catch (Exception exx)
                        {
                            _logger.Error(exx, $"Problemas ao salvar catch do processo de integração de pedido (3)");
                        }
                    }

                    var updateResult = await _apiActorGroup.Ask<ReturnMessage<OrderUpdateMutationOutput>>(
                                  new OrderUpdateMutation(orderUpdateMutationInput), cancellationToken);

                    if (shopifyData.EnableSaveIntegrationInformations)
                    {
                        try
                        {
                            process.ShopifyResult = Newtonsoft.Json.JsonConvert.SerializeObject(updateResult);

                            _shopifyUpdateOrderTagNumberProcessRepository.Save(process);
                        }
                        catch (Exception exx)
                        {
                            _logger.Error(exx, $"Problemas ao salvar catch do processo de integração de pedido (4)");
                        }
                    }

                    if (updateResult.Result == Result.Error)
                        return new ReturnMessage { Result = Result.Error, Error = updateResult.Error };

                    if (updateResult.Data.orderUpdate.userErrors?.Any() == true)
                        throw new Exception($"Error in update shopify order tag: {JsonSerializer.Serialize(updateResult.Data.orderUpdate.userErrors)}");
                }

                return new ReturnMessage { Result = Result.OK };
            }
            catch (Exception ex)
            {
                if (shopifyData.EnableSaveIntegrationInformations)
                {
                    try
                    {
                        process.Exception = ex.Message;

                        _shopifyUpdateOrderTagNumberProcessRepository.Save(process);
                    }
                    catch (Exception exx)
                    {
                        _logger.Error(exx, $"Problemas ao salvar catch do processo de integração de pedido (5)");
                    }
                }
                throw ex;
            }
        }

        public async Task<ReturnMessage> UpdateOrderStatus(ShopifyUpdateOrderStatusMessage message, ShopifyDataMessage shopifyData, CancellationToken cancellationToken)
        {
            Domain.Shopify.Models.Results.OrderResult currentData = null;
            if (message.ShopifyId.HasValue)
            {
                var queryByIdResult = await _apiActorGroup.Ask<ReturnMessage<OrderByIdQueryOutput>>(
                       new OrderByIdQuery(message.ShopifyId.Value, Domain.Shopify.Models.Results.OrderResult.Status), cancellationToken
                   );

                if (queryByIdResult.Result == Result.Error)
                {
                    _logger.Warning($"ShopifyService - Error in UpdateOrderStatus | {queryByIdResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                    return new ReturnMessage { Result = Result.Error, Error = queryByIdResult.Error };
                }

                if (queryByIdResult.Data.order != null)
                    currentData = queryByIdResult.Data.order;
            }

            if (currentData == null && !string.IsNullOrWhiteSpace(message.OrderExternalId))
            {
                var queryByTagResult = await _apiActorGroup.Ask<ReturnMessage<OrderByTagQueryOutput>>(
                    new OrderByTagQuery(SetTagValue(Tags.OrderExternalId, message.OrderExternalId), Domain.Shopify.Models.Results.OrderResult.Status), cancellationToken
                );

                if (queryByTagResult.Result == Result.Error)
                {
                    _logger.Warning($"ShopifyService - Error in UpdateOrderStatus | {queryByTagResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                    return new ReturnMessage { Result = Result.Error, Error = queryByTagResult.Error };
                }


                if (queryByTagResult.Data.orders.edges.Any() == true)
                    currentData = queryByTagResult.Data.orders.edges[0].node;
            }

            if (currentData == null)
            {
                //_logger.Info($"order {message.ShopifyId?.ToString() ?? message.OrderExternalId} not found");
            }
            else
            {
                if (message.Cancellation.IsCancelled)
                {
                    if (currentData.cancelledAt == null)
                    {
                        if (currentData.fulfillments?.Any() == true)
                        {
                            throw new Exception($"can't delete because order {currentData.id} has fulfillments");
                        }
                        else
                        {
                            var orderCancelResult = await _apiActorGroup.Ask<ReturnMessage>(
                                new CancelOrderRequest
                                {
                                    OrderId = currentData.legacyResourceId,
                                    SendEmail = true
                                }, cancellationToken
                            );

                            if (orderCancelResult.Result == Result.Error)
                            {
                                _logger.Warning($"ShopifyService - Error in UpdateOrderStatus | {orderCancelResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                                return new ReturnMessage { Result = Result.Error, Error = orderCancelResult.Error };
                            }
                        }
                    }
                }
                else
                {
                    if (currentData.cancelledAt != null)
                    {
                        //_logger.Info($"can't update because order {currentData.id} is cancelled");
                    }
                    else
                    {
                        if (message.Payment.IsPaid)
                        {
                            if (currentData.displayFinancialStatus == "PENDING")
                            {
                                var markAsPaidResult = await _apiActorGroup.Ask<ReturnMessage<OrderMarkAsPaidMutationOutput>>(
                                    new OrderMarkAsPaidMutation(new OrderMarkAsPaidMutationInput
                                    {
                                        input = new OrderMarkAsPaidMutationInput.MarkAsPaidInput
                                        {
                                            id = currentData.id
                                        }
                                    }), cancellationToken
                                );

                                if (markAsPaidResult.Result == Result.Error)
                                {
                                    _logger.Warning($"ShopifyService - Error in UpdateOrderStatus | {markAsPaidResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                                    return new ReturnMessage { Result = Result.Error, Error = markAsPaidResult.Error };
                                }

                                if (markAsPaidResult.Data.orderMarkAsPaid.userErrors?.Any() == true)
                                    throw new Exception($"Error in update shopify stock: {JsonSerializer.Serialize(markAsPaidResult.Data.orderMarkAsPaid.userErrors)}");
                            }
                        }

                        FulfillmentResult currentFulfillment = currentData.fulfillments?.FirstOrDefault();
                        if (message.Shipping.IsShipped)
                        {
                            if (currentData.displayFulfillmentStatus == "UNFULFILLED")
                            {
                                var locationId = currentData.draftFulfillments?.FirstOrDefault()?.service?.location?.id;
                                if (locationId == null)
                                    throw new Exception($"No location found for order {currentData.id}");

                                var fulfillmentCreateResult = await _apiActorGroup.Ask<ReturnMessage<FulfillmentCreateMutationOutput>>(
                                    new FulfillmentCreateMutation(new FulfillmentCreateMutationInput
                                    {
                                        input = new FulfillmentCreateMutationInput.FullfillmentInput
                                        {
                                            orderId = currentData.id,
                                            locationId = locationId,
                                            notifyCustomer = HasToNotifyFulfillment(currentData, shopifyData),
                                            trackingCompany = "Other",
                                            trackingUrls = !string.IsNullOrWhiteSpace(message.Shipping.TrackingObject) || !string.IsNullOrWhiteSpace(message.Shipping.TrackingUrl) ?
                                                            new List<string> { GetTrackingUrl(message.Shipping) } : null,
                                            trackingNumbers = !string.IsNullOrWhiteSpace(message.Shipping.TrackingObject) ?
                                                            new List<string> { message.Shipping.TrackingObject } : null
                                        }
                                    }), cancellationToken
                                );

                                if (fulfillmentCreateResult.Result == Result.Error)
                                {
                                    _logger.Warning($"ShopifyService - Error in UpdateOrderStatus | {fulfillmentCreateResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                                    return new ReturnMessage { Result = Result.Error, Error = fulfillmentCreateResult.Error };
                                }

                                if (fulfillmentCreateResult.Data.fulfillmentCreate.userErrors?.Any() == true)
                                    throw new Exception($"Error in update shopify stock: {JsonSerializer.Serialize(fulfillmentCreateResult.Data.fulfillmentCreate.userErrors)}");

                                currentFulfillment = fulfillmentCreateResult.Data.fulfillmentCreate.fulfillment;
                            }
                            else
                            {
                                if (!message.Shipping.IsDelivered)
                                {
                                    var currentTracking = currentFulfillment?.trackingInfo?.FirstOrDefault();
                                    if (currentTracking == null || currentTracking.number != message.Shipping.TrackingObject)
                                    {
                                        var fulfillmentTrackingInfoUpdateResult = await _apiActorGroup.Ask<ReturnMessage<FulfillmentTrackingInfoUpdateMutationOutput>>(
                                             new FulfillmentTrackingInfoUpdateMutation(new FulfillmentTrackingInfoUpdateMutationInput
                                             {

                                                 fulfillmentId = currentFulfillment.id,
                                                 trackingInfoUpdateInput = new FulfillmentTrackingInfoUpdateMutationInput.TrackingInfoUpdateInput
                                                 {
                                                     notifyCustomer = HasToNotifyFulfillment(currentData, shopifyData),
                                                     trackingCompany = "Other",
                                                     trackingDetails = new FulfillmentTrackingInfoUpdateMutationInput.TrackingInfoInput
                                                     {
                                                         url = GetTrackingUrl(message.Shipping),
                                                         number = message.Shipping.TrackingObject ?? string.Empty
                                                     }
                                                 }

                                             }), cancellationToken
                                        );

                                        if (fulfillmentTrackingInfoUpdateResult.Result == Result.Error)
                                        {
                                            _logger.Warning($"ShopifyService - Error in UpdateOrderStatus | {fulfillmentTrackingInfoUpdateResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                                            return new ReturnMessage { Result = Result.Error, Error = fulfillmentTrackingInfoUpdateResult.Error };
                                        }


                                        if (fulfillmentTrackingInfoUpdateResult.Data.fulfillmentTrackingInfoUpdate.userErrors?.Any() == true)
                                            throw new Exception($"Error in update shopify stock: {JsonSerializer.Serialize(fulfillmentTrackingInfoUpdateResult.Data.fulfillmentTrackingInfoUpdate.userErrors)}");
                                    }
                                }
                            }
                        }

                        if (message.Shipping.IsDelivered)
                        {
                            if (currentFulfillment?.id != null && currentFulfillment?.deliveredAt == null)
                            {
                                var fulfillmentEventCreateResult = await _apiActorGroup.Ask<ReturnMessage>(
                                    new CreateFulfillmentEventRequest
                                    {
                                        OrderId = currentData.legacyResourceId,
                                        FulfillmentId = currentFulfillment.legacyResourceId,
                                        Status = "delivered"
                                    }, cancellationToken
                                );

                                if (fulfillmentEventCreateResult.Result == Result.Error)
                                {
                                    _logger.Warning($"ShopifyService - Error in UpdateOrderStatus | {fulfillmentEventCreateResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                                    return new ReturnMessage { Result = Result.Error, Error = fulfillmentEventCreateResult.Error };
                                }
                            }
                        }
                    }
                }
            }
            return new ReturnMessage { Result = Result.OK };
        }

        public async Task<ReturnMessage> UpdateTrackingOrder(ShopifyUpdateTrackingOrder message, ShopifyDataMessage shopifyData, CancellationToken cancellationToken)
        {
            Domain.Shopify.Models.Results.OrderResult currentData = null;
            if (message.ShopifyId.HasValue)
            {
                var queryByIdResult = await _apiActorGroup.Ask<ReturnMessage<OrderByIdQueryOutput>>(
                       new OrderByIdQuery(message.ShopifyId.Value, Domain.Shopify.Models.Results.OrderResult.Status), cancellationToken
                   );

                if (queryByIdResult.Result == Result.Error)
                {
                    _logger.Warning($"ShopifyService - Error in UpdateTrackingOrder | {queryByIdResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                    return new ReturnMessage { Result = Result.Error, Error = queryByIdResult.Error };
                }

                if (queryByIdResult.Data.order != null)
                    currentData = queryByIdResult.Data.order;
            }

            if (currentData == null && !string.IsNullOrWhiteSpace(message.OrderExternalId))
            {
                var queryByTagResult = await _apiActorGroup.Ask<ReturnMessage<OrderByTagQueryOutput>>(
                    new OrderByTagQuery(SetTagValue(Tags.OrderExternalId, message.OrderExternalId), Domain.Shopify.Models.Results.OrderResult.Status), cancellationToken
                );

                if (queryByTagResult.Result == Result.Error)
                {
                    _logger.Warning($"ShopifyService - Error in UpdateTrackingOrder | {queryByTagResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                    return new ReturnMessage { Result = Result.Error, Error = queryByTagResult.Error };
                }

                if (queryByTagResult.Data.orders.edges.Any() == true)
                    currentData = queryByTagResult.Data.orders.edges[0].node;
            }

            if (currentData == null)
            {
                //_logger.Info($"order {message.ShopifyId?.ToString() ?? message.OrderExternalId} not found");
            }
            else
            {
                if (currentData.cancelledAt != null)
                {
                    //_logger.Info($"can't update because order {currentData.id} is cancelled");
                }
                else
                {
                    FulfillmentResult currentFulfillment = currentData.fulfillments?.FirstOrDefault();
                    if (message.Shipping.IsShipped)
                    {
                        if (currentData.displayFulfillmentStatus == "UNFULFILLED")
                        {
                            var locationId = currentData.draftFulfillments?.FirstOrDefault()?.service?.location?.id;
                            if (locationId == null)
                                throw new Exception($"No location found for order {currentData.id}");

                            var fulfillmentCreateResult = await _apiActorGroup.Ask<ReturnMessage<FulfillmentCreateMutationOutput>>(
                                new FulfillmentCreateMutation(new FulfillmentCreateMutationInput
                                {
                                    input = new FulfillmentCreateMutationInput.FullfillmentInput
                                    {
                                        orderId = currentData.id,
                                        locationId = locationId,
                                        notifyCustomer = HasToNotifyFulfillment(currentData, shopifyData),
                                        trackingCompany = "Other",
                                        trackingUrls = !string.IsNullOrWhiteSpace(message.Shipping.TrackingCode) || !string.IsNullOrWhiteSpace(message.Shipping.TrackingUrl) ?
                                                        new List<string> { message.Shipping.TrackingUrl } : null,
                                        trackingNumbers = !string.IsNullOrWhiteSpace(message.Shipping.TrackingCode) ?
                                                        new List<string> { message.Shipping.TrackingCode } : null
                                    }
                                }), cancellationToken
                            );

                            if (fulfillmentCreateResult.Result == Result.Error)
                            {
                                _logger.Warning($"ShopifyService - Error in UpdateTrackingOrder | {fulfillmentCreateResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                                return new ReturnMessage { Result = Result.Error, Error = fulfillmentCreateResult.Error };
                            }


                            if (fulfillmentCreateResult.Data.fulfillmentCreate.userErrors?.Any() == true)
                                throw new Exception($"Error in update shopify stock: {JsonSerializer.Serialize(fulfillmentCreateResult.Data.fulfillmentCreate.userErrors)}");

                            currentFulfillment = fulfillmentCreateResult.Data.fulfillmentCreate.fulfillment;
                        }
                        else
                        {
                            if (!message.Shipping.IsDelivered || (message.Shipping.IsDelivered && currentFulfillment?.trackingInfo?.FirstOrDefault() == null))
                            {
                                var currentTracking = currentFulfillment?.trackingInfo?.FirstOrDefault();
                                if (currentTracking == null || currentTracking.number != message.Shipping.TrackingCode)
                                {
                                    var fulfillmentTrackingInfoUpdateResult = await _apiActorGroup.Ask<ReturnMessage<FulfillmentTrackingInfoUpdateMutationOutput>>(
                                         new FulfillmentTrackingInfoUpdateMutation(new FulfillmentTrackingInfoUpdateMutationInput
                                         {

                                             fulfillmentId = currentFulfillment.id,
                                             trackingInfoUpdateInput = new FulfillmentTrackingInfoUpdateMutationInput.TrackingInfoUpdateInput
                                             {
                                                 notifyCustomer = HasToNotifyFulfillment(currentData, shopifyData),
                                                 trackingCompany = "Other",
                                                 trackingDetails = new FulfillmentTrackingInfoUpdateMutationInput.TrackingInfoInput
                                                 {
                                                     url = message.Shipping.TrackingUrl,
                                                     number = message.Shipping.TrackingCode ?? string.Empty
                                                 }
                                             }

                                         }), cancellationToken
                                    );

                                    if (fulfillmentTrackingInfoUpdateResult.Result == Result.Error)
                                    {
                                        _logger.Warning($"ShopifyService - Error in UpdateTrackingOrder | {fulfillmentTrackingInfoUpdateResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                                        return new ReturnMessage { Result = Result.Error, Error = fulfillmentTrackingInfoUpdateResult.Error };
                                    }

                                    if (fulfillmentTrackingInfoUpdateResult.Data.fulfillmentTrackingInfoUpdate.userErrors?.Any() == true)
                                        throw new Exception($"Error in update shopify stock: {JsonSerializer.Serialize(fulfillmentTrackingInfoUpdateResult.Data.fulfillmentTrackingInfoUpdate.userErrors)}");
                                }
                            }
                        }
                    }

                    if (message.Shipping.IsDelivered)
                    {
                        if (currentFulfillment?.id != null && currentFulfillment?.deliveredAt == null)
                        {
                            var fulfillmentEventCreateResult = await _apiActorGroup.Ask<ReturnMessage>(
                                new CreateFulfillmentEventRequest
                                {
                                    OrderId = currentData.legacyResourceId,
                                    FulfillmentId = currentFulfillment.legacyResourceId,
                                    Status = "delivered"
                                }, cancellationToken
                            );

                            if (fulfillmentEventCreateResult.Result == Result.Error)
                            {
                                _logger.Warning($"ShopifyService - Error in UpdateTrackingOrder | {fulfillmentEventCreateResult.Error.Message}", Extensions.LoggingExtensions.FromService(Extensions.LoggingExtensions.GetCurrentMethod(), message, shopifyData));
                                return new ReturnMessage { Result = Result.Error, Error = fulfillmentEventCreateResult.Error };
                            }
                        }
                    }
                }
            }
            return new ReturnMessage { Result = Result.OK };
        }

        public bool HasToNotifyFulfillment(Domain.Shopify.Models.Results.OrderResult order, ShopifyDataMessage shopifyData)
        {
            if (shopifyData.BlockFulfillmentNotificationPerShipmentService)
            {
                var services = shopifyData.ShipmentServicesForFulfillmentNotification?.Split(";").Select(x => { return x.ToLower(); }).ToList();

                if (services == null || (services != null && services.Count == 0))
                    return false;

                return !services.Contains(order?.shippingLine?.title?.ToLower());
            }

            return true;
        }

        private string GetTrackingUrl(ShopifyUpdateOrderStatusMessage.ShippingStatus shipping)
        {
            if (!string.IsNullOrWhiteSpace(shipping.TrackingUrl))
                return shipping.TrackingUrl;
            else if (!string.IsNullOrWhiteSpace(shipping.TrackingObject))
                return string.Format(_configuration.GetSection("Shopify")["TrackingUrl"], shipping.TrackingObject);
            else
                return string.Empty;
        }

        private async Task SendListFullProductMessage(string externalId, QueueClient fullProductQueueClient, int scheduled = 0)
        {
            var listProductMessage = new ShopifyListERPFullProductMessage
            {
                ExternalId = externalId
            };
            var serviceBusMessage = new ServiceBusMessage(listProductMessage);
            await fullProductQueueClient.SendAsync(serviceBusMessage.GetMessage(listProductMessage.ExternalId, scheduled));
        }

        private async Task SendUpdateProductGroupingMessage(string productId, List<string> groupRefs, ShopifyDataMessage shopifyData, QueueClient updateProductGroupingQueueClient)
        {
            if (shopifyData.ProductGroupingEnabled)
            {
                foreach (var groupRef in groupRefs)
                {
                    var listProductMessage = new ShopifyUpdateProductGroupingMessage
                    {
                        GroupingReference = groupRef
                    };
                    var serviceBusMessage = new ServiceBusMessage(listProductMessage);
                    await updateProductGroupingQueueClient.SendAsync(serviceBusMessage.GetMessage($"{productId}-{groupRef}"));
                }
            }
        }

        private async Task SendUpdateProductImagesMessage(string productId, ProductImages images, ShopifyDataMessage shopifyData, QueueClient updateProductImagesQueueClient)
        {
            if (shopifyData.ImageIntegrationEnabled)
            {
                var listProductMessage = new ShopifyUpdateProductImagesMessage
                {
                    ShopifyProductId = long.Parse(productId),
                    Images = images
                };
                var serviceBusMessage = new ServiceBusMessage(listProductMessage);
                await updateProductImagesQueueClient.SendAsync(serviceBusMessage.GetMessage(productId));
            }
        }

        public string SetTagKitValue(string key, string value)
        {
            return $"{key}{value}-Intg";
        }

        public string SetTagValue(string key, string value)
        {
            return $"{key}-{value}-Intg";
        }

        public List<string> SearchTagValue(List<string> allTags, string key)
        {
            return allTags?
                    .Select(t => t.Trim())
                    .Where(t => IsTag(t, key))
                    .Select(t =>
                    {
                        return GetTagValue(t, key);
                    })
                    .ToList();
        }

        public string GetTagValue(string tag, string key)
        {
            tag = tag.Remove(0, $"{key}-".Length);
            tag = tag.Remove(tag.Length - "-Intg".Length, "-Intg".Length);
            return tag;
        }

        public bool IsTag(string tag, string key)
        {
            return tag.StartsWith($"{key}") && tag.EndsWith("-Intg");
        }

        public bool IsAnyTag(string tag, List<string> keys)
        {
            return keys.Any(key => IsTag(tag, key));
        }

        public string GetExternalOrderNumber(Tenant tenant, string orderNumber)
        {
            return tenant.Type switch
            {
                TenantType.Millennium => string.Concat(tenant.MillenniumData.OrderPrefix, orderNumber.TrimStart('#')),
                TenantType.Omie => string.Concat(tenant.OmieData.OrderPrefix, orderNumber.TrimStart('#')),
                _ => orderNumber.TrimStart('#')
            };
        }

        public string GetExternalOrderNumber(ShopifyDataMessage shopifyData, string orderNumber)
        {
            return shopifyData.Type switch
            {
                TenantType.Millennium => string.Concat(shopifyData.OrderPrefix[TenantType.Millennium], orderNumber.TrimStart('#')),
                _ => orderNumber.TrimStart('#')
            };
        }

        /*private List<Metafield> MergeMetafields(List<Metafield> update, Connection<MetafieldResult> current)
        {
            foreach (var metaField in update)
            {
                metaField.id = current.edges?.Where(m => m.node.key == metaField.key).Select(m => m.node.id).FirstOrDefault();
            }
            return update;
        }*/

        /*private List<InventoryQuantity> FillInventory(Domain.Models.Product.SkuInfo sku, List<Edges<LocationResult>> shopifyLocations)
        {
            var inventories = new List<InventoryQuantity>();

            if (_shopifyData.EnabledMultiLocation && sku.Stock.Locations.Any())
            {
                foreach (var warehouse in sku.Stock.Locations)
                {
                    var location = _shopifyData.LocationMap.GetLocationByIdErp(warehouse.ErpLocationId);

                    if (location is null)
                        throw new Exception($"Not found locationMap to LocationErpId {warehouse.ErpLocationId} for client: {_shopifyData.StoreName}");

                    var hasLocationShopify = shopifyLocations.Any(x => x.node.legacyResourceId == location.EcommerceLocation);

                    if (!hasLocationShopify)
                        throw new Exception($"Not found locationId: {location.EcommerceLocation} on Shopify for client: {_shopifyData.StoreName}");

                    var shopifyLocation = shopifyLocations.Where(x => x.node.legacyResourceId == location.EcommerceLocation).FirstOrDefault();

                    inventories.Add(new InventoryQuantity
                    {
                        availableQuantity = warehouse.Quantity,
                        locationId = shopifyLocation.node.id
                    });
                }

                return inventories;

            }

            inventories.Add(new InventoryQuantity
            {
                availableQuantity = sku.Stock.Quantity,
                locationId = shopifyLocations[0].node.id
            });

            return inventories;
        }*/

        /*private string FillLocation(Domain.Models.Product.SkuInfo sku, string locationId)
        {
            var newLocationId = locationId;

            string replaceLocation(string location) => $"{locationId.Substring(0, locationId.LastIndexOf("/"))}/{location}";

            if (_shopifyData.EnabledMultiLocation)
            {
                var location = _shopifyData.LocationMap.GetLocationByIdErp(sku.Stock.Locations.FirstOrDefault().ErpLocationId);

                if (location is null) throw new Exception($"Not found mapping to LocationId {sku.Stock.Locations.FirstOrDefault().ErpLocationId} for client: {_shopifyData.StoreName}");

                newLocationId = replaceLocation(location.EcommerceLocation);
            }

            return newLocationId;

        }*/

        public async Task<ReturnMessage> SendProductKitToUpdate(ShopifyUpdateProductKitMessage message,
                                                                 QueueClient fullProductQueueClient,
                                                                 CancellationToken cancellationToken)
        {
            _logger.Warning("Chegou no update de produtokit: {0}", message.ExternalProductId);
            var tagFilter = $"{Tags.GetProductKitSku(message.ExternalProductId.ToString())}|";
            _logger.Warning("Montou a tag: {0}", tagFilter);
            var response = await _apiActorGroup.Ask<ReturnMessage<ProductIdsByTagQueryOutput>>(
                new ProductIdsByTagQuery(tagFilter), cancellationToken);

            if (response.Result == Result.Error)
                return new ReturnMessage { Result = Result.Error, Error = response.Error };

            var productKit = response.Data.products.edges.Where(x => x.node.tags.Contains(Tags.ProductKit) && !string.IsNullOrWhiteSpace(x.node.onlineStoreUrl)).ToList();
            response.Data.products.edges = productKit;
            var data = response.Data;

            do
            {
                response = await _apiActorGroup.Ask<ReturnMessage<ProductIdsByTagQueryOutput>>(
                    new ProductIdsByTagQuery(tagFilter, response.Data.products.edges.Last().cursor), cancellationToken);

                if (response.Result == Result.Error)
                    throw response.Error;
                productKit = response.Data.products.edges.Where(x => x.node.tags.Contains(Tags.ProductKit) && !string.IsNullOrWhiteSpace(x.node.onlineStoreUrl)).ToList();
                data.products.edges.AddRange(productKit);

            } while (response.Data.products.pageInfo.hasNextPage == true);

            _logger.Warning("Retornou da Shopify : {0}", Newtonsoft.Json.JsonConvert.SerializeObject(data));

            data.products.edges.ForEach(async item =>
            {
                var externalID = Regex.Match(item.node.tags.FirstOrDefault(tag => tag.StartsWith(Tags.ProductExternalId)), @"\d+").Value;
                _logger.Warning("Regex externalID : {0}", externalID);
                await SendListFullProductMessage(externalID, fullProductQueueClient, 2);
            });

            return new ReturnMessage { Result = Result.OK };
        }

       /* private async Task SendProductToUpdateStockKit(ShopifyUpdateStockMessage message,
                                                       QueueClient updateStockKitQueue,
                                                       CancellationToken cancellationToken)
        {
            var response = await _apiActorGroup.Ask<ReturnMessage<ProductIdsByTagQueryOutput>>(
                new ProductIdsByTagQuery(Tags.GetProductKitSku(message.Value.Sku)), cancellationToken);

            if (response.Result == Result.Error)
                throw response.Error;

            var productKit = response.Data.products.edges.Where(x => x.node.tags.Contains(Tags.ProductKit)).ToList();
            response.Data.products.edges = productKit;
            var data = response.Data;

            do
            {
                response = await _apiActorGroup.Ask<ReturnMessage<ProductIdsByTagQueryOutput>>(
                    new ProductIdsByTagQuery(Tags.GetProductKitSku(message.Value.Sku), response.Data.products.edges.Last().cursor), cancellationToken);

                if (response.Result == Result.Error)
                    throw response.Error;
                productKit = response.Data.products.edges.Where(x => x.node.tags.Contains(Tags.ProductKit)).ToList();
                data.products.edges.AddRange(productKit);

            } while (response.Data.products.pageInfo.hasNextPage == true);

            List<Task> sendMessages = new List<Task>();

            foreach (var item in data.products.edges)
            {
                var serviceBusMessage = new ServiceBusMessage(new ShopifyUpdateStockKitMessage
                {
                    ExternalProductId = Convert.ToInt64(item.node.legacyResourceId),
                    Sku = message.Value.Sku,
                    Quantity = message.Value.Quantity
                });

                sendMessages.Add(updateStockKitQueue.ScheduleMessageAsync(serviceBusMessage.GetMessage(Guid.NewGuid()), DateTime.UtcNow.AddMinutes(2)));

            }

            await Task.WhenAll(sendMessages);

        }*/
        /*public async Task<bool> ValidAndReturnProductKit(OrderResult.Order order, ShopifyDataMessage shopifyData, QueueClient updateOrderTagNumber, QueueClient updateStockQueueClient, CancellationToken cancellationToken = default)
        {
            var itemsToRemove = new List<long>();
            var itemsToInsert = new List<OrderResult.LineItem>();

            (OrderResult.Order order, ProductResult product) orderItem = (order, null);
            foreach (var item in order.line_items)
            {

                var response = await GetProductById(item.product_id.Value, cancellationToken);

                if (response.Result == Result.Error)
                    throw response.Error;

                if (response.Data.product == null)
                    throw new Exception($"not found product {item.product_id}");


                var product = response.Data.product;

                if (product.tags.Select(x => { return x.Trim(); }).Contains("Assinatura", StringComparer.OrdinalIgnoreCase))
                    return true;

                orderItem.product = product;

                var isProductKit = product.tags.Any(x => string.Equals(x, Tags.ProductKit, StringComparison.OrdinalIgnoreCase));

                if (isProductKit)
                {
                    itemsToRemove.Add(item.product_id.Value);

                    var skus = product.tags.Where(x => x.StartsWith(Tags.ProductKitSKU))
                        .Select(x => new { sku = Tags.GetSkuInTagKit(x), qtdItems = x.Split("|").LastOrDefault() ?? "1" })
                        .ToList();

                    await ProcessStockProductKit(shopifyData, orderItem, updateOrderTagNumber, updateStockQueueClient, cancellationToken);

                    var totalItems = skus.Count > 0 ? skus.Count : 1;
                    var newValueOrder = item.price / totalItems;
                    var newDiscount = item.discount_allocations?.Sum(d => d.amount) ?? 0;
                    if (newDiscount != 0) newDiscount = newDiscount / totalItems;

                    foreach (var sku in skus)
                    {
                        var queryBySkuResult = await _apiActorGroup.Ask<ReturnMessage<VariantBySkuQueryOutput>>(
                            new VariantBySkuQuery(sku.sku), cancellationToken);

                        if (queryBySkuResult.Result == Result.Error)
                            throw response.Error;

                        if (queryBySkuResult.Data.productVariants.edges.Any() == false)
                            throw new Exception($"not found variant: {sku} for productId:{item.product_id}");


                        var variants = queryBySkuResult.Data.productVariants.edges[0].node;

                        itemsToInsert.Add(new OrderResult.LineItem
                        {
                            location_id = Convert.ToInt64(variants.inventoryItem.inventoryLevels.edges.FirstOrDefault().node.location.legacyResourceId),
                            id = Convert.ToInt64(variants.product.legacyResourceId),
                            product_id = item.product_id.Value,
                            sku = sku.sku,
                            name = variants.product.title,
                            quantity = Convert.ToInt32(sku.qtdItems),
                            price = newValueOrder / Convert.ToInt32(sku.qtdItems),
                            discount_allocations = new List<OrderResult.DiscountAllocation> { new OrderResult.DiscountAllocation { amount = newDiscount } }
                        });

                    }
                }

            }

            if (itemsToRemove.Count > 0)
            {
                var newItems = order.line_items.Where(x => !itemsToRemove.Contains(x.product_id.Value)).ToList();
                order.line_items = newItems.Union(itemsToInsert).ToList();

            }

            return false;
        }*/

        /*public async Task ProcessStockProductKit(ShopifyDataMessage shopifyData, (OrderResult.Order order, ProductResult product) order, QueueClient updateOrderTagNumber, QueueClient updateStockQueueClient, CancellationToken cancellationToken = default)
        {

            if (shopifyData.EnableStockProductKit)
            {
                var skus = order.product.tags.Where(x => x.StartsWith(Tags.ProductKitSKU))
                        .Select(x => new { sku = Tags.GetSkuInTagKit(x), qtdItems = x.Split("|").LastOrDefault() ?? "1" })
                        .ToList();

                var orderTags = order.order.tags.Split(",").Select(x => { return x.Trim(); }).ToList();
                var hasTagProcessedStock = orderTags.Contains(Tags.OrderProcessedStockKit);

                if (!hasTagProcessedStock)
                {
                    var updateOrderTagBusMessage = new ServiceBusMessage(new ShopifyUpdateOrderTagNumberMessage
                    {
                        ShopifyId = order.order.id,
                        CustomTags = new List<string> { Tags.OrderProcessedStockKit }
                    });
                    await updateOrderTagNumber.SendAsync(updateOrderTagBusMessage.GetMessage(Guid.NewGuid()));

                    await Task.WhenAll(skus.Select(o =>
                        updateStockQueueClient.SendAsync(new ServiceBusMessage(
                            new ShopifyUpdateStockMessage
                            {
                                Value = new Domain.Models.Product.SkuStock
                                {
                                    DecreaseStock = true,
                                    Sku = o.sku,
                                    Quantity = Convert.ToInt32(o.qtdItems)
                                }
                            })
                        .GetMessage(o))));
                }
            }
        }*/

        private List<string> FillProductTags(ShopifyDataMessage shopifyData, Domain.Models.Product.Info productInfo, List<string> currentTags = null)
        {
            var result = new List<string>();

            var tagIds = new List<string>();

            if (productInfo.ExternalId != null)
            {
                if (!string.IsNullOrWhiteSpace(productInfo.ExternalId))
                    result.Add(SetTagValue(Tags.ProductExternalId, productInfo.ExternalId));

                tagIds.Add(Tags.ProductExternalId);
            }

            if (productInfo.ExternalCode != null)
            {
                if (!string.IsNullOrWhiteSpace(productInfo.ExternalCode))
                    result.Add(SetTagValue(Tags.ProductExternalCode, productInfo.ExternalCode));

                tagIds.Add(Tags.ProductExternalCode);
            }

            if (productInfo.SkuOriginal != null)
            {
                if (!string.IsNullOrWhiteSpace(productInfo.SkuOriginal))
                    result.Add(SetTagValue(Tags.SkuOriginal, productInfo.SkuOriginal));

                tagIds.Add(Tags.SkuOriginal);
            }

            if (productInfo.GroupingReference != null)
            {
                if (!string.IsNullOrWhiteSpace(productInfo.GroupingReference))
                    result.Add(SetTagValue(Tags.ProductGroupingReference, productInfo.GroupingReference));

                tagIds.Add(Tags.ProductGroupingReference);
            }

            if (productInfo.VendorId != null)
            {
                if (!string.IsNullOrWhiteSpace(productInfo.VendorId))
                    result.Add(SetTagValue(Tags.ProductVendorId, productInfo.VendorId.ToString()));

                tagIds.Add(Tags.ProductVendorId);
            }

            if (productInfo.Status != null)
            {
                result.Add(SetTagValue(Tags.ProductStatus, productInfo.Status == true ? "Ativo" : "Inativo"));
                tagIds.Add(Tags.ProductStatus);
            }

            if (productInfo.kit == true && shopifyData.HasProductKit)
            {
                result.Add(Tags.ProductKit);
                tagIds.Add(Tags.ProductKit);

                foreach (var sku in productInfo.Variants)
                    FillKitTags(sku.SkuKits.ToList(), ref result, ref tagIds);
            }

            var categoryNames = new List<string>();
            Action<List<Domain.Models.Product.Category>, string> fillCategoryTags = null;

            fillCategoryTags = (categories, path) =>
            {
                foreach (var category in categories)
                {
                    if (!string.IsNullOrWhiteSpace(category.Id))
                        result.Add(SetTagValue(Tags.ProductCollectionId, category.Id.ToString()));

                    if (!categoryNames.Contains(category.Name))
                        categoryNames.Add(category.Name);

                    var handle = Regex.Replace(Regex.Replace(category.Name, @"[\W]+", "-"), @"[-]+", "-").ToLowerInvariant();
                    if (path != null)
                        handle = string.Concat(path, ">>", handle);

                    result.Add(SetTagValue(Tags.ProductCollection, handle));

                    if (category.ChildCategories?.Any() == true)
                        fillCategoryTags(category.ChildCategories, handle);
                }
            };

            if (productInfo.Categories != null
                && (productInfo.Categories.Count == 0
                || productInfo.Categories.Any(c => c.Name != null))) //if only Ids (to check if any changed) no need to change the tags
            {
                fillCategoryTags(productInfo.Categories, null);
                tagIds.AddRange(new List<string> { Tags.ProductCollectionId,
                                                    Tags.ProductCollection });

                if (shopifyData.WriteCategoryNameTags)
                {
                    if (currentTags != null)
                    {
                        var currentCategoryNameTags = currentTags.Where(t => IsTag(t, Tags.ProductCategoryName)).Select(t => GetTagValue(t, Tags.ProductCategoryName)).ToList();
                        currentTags.RemoveAll(t => currentCategoryNameTags.Contains(t));
                    }

                    foreach (var name in categoryNames)
                    {
                        result.Add(SetTagValue(Tags.ProductCategoryName, name));
                        result.Add(name);
                    }
                    tagIds.Add(Tags.ProductCategoryName);
                }
            }

            if (currentTags != null)
            {
                result.AddRange(currentTags.Where(t => !IsAnyTag(t, tagIds)));
            }

            return result;
        }

        private void FillKitTags(List<Domain.Models.Product.SkuKit> skuKits, ref List<string> result, ref List<string> tagIds)
        {
            tagIds.Add(Tags.ProductKitSKU);
            foreach (var kit in skuKits)
                result.Add(SetTagKitValue(Tags.ProductKitSKU, $"{kit.ChildProduct}|{kit.Quantity}"));
        }

        public async Task<ReturnMessage<ProductByIdQueryOutput>> GetProductById(long shopifyId, CancellationToken cancellationToken = default)
        {
            if (_shopifyData.EnableMaxVariantsQueryGraphQL)
                _firstVariants = _shopifyData.MaxVariantsQueryGraphQL;

            var queryByIdResult = await _apiActorGroup.Ask<ReturnMessage<ProductByIdQueryOutput>>(
                new ProductByIdQuery(shopifyId, _firstVariants), cancellationToken
            );

            if (queryByIdResult.Result == Result.Error)
                return queryByIdResult;

            var data = queryByIdResult.Data;

            while (queryByIdResult.Data.product?.variants?.pageInfo?.hasNextPage == true)
            {
                queryByIdResult = await _apiActorGroup.Ask<ReturnMessage<ProductByIdQueryOutput>>(
                    new ProductByIdQuery(shopifyId, _firstVariants, queryByIdResult.Data.product.variants.edges.Last().cursor), cancellationToken
                );

                if (queryByIdResult.Result == Result.Error)
                    return queryByIdResult;

                data.product.variants.edges.AddRange(queryByIdResult.Data.product.variants.edges);
            }

            return new ReturnMessage<ProductByIdQueryOutput> { Result = Result.OK, Data = data };
        }

        private async Task<ReturnMessage<ProductByTagQueryOutput>> GetProductByTag(string tag, CancellationToken cancellationToken)
        {
            if (_shopifyData.EnableMaxVariantsQueryGraphQL)
                _firstVariants = _shopifyData.MaxVariantsQueryGraphQL;

            var queryByTagResult = await _apiActorGroup.Ask<ReturnMessage<ProductByTagQueryOutput>>(
                             new ProductByTagQuery(tag, _firstVariants), cancellationToken);

            if (queryByTagResult.Result == Result.Error)
                return queryByTagResult;

            var data = queryByTagResult.Data;

            while (queryByTagResult.Data.products.edges.FirstOrDefault()?.node.variants?.pageInfo?.hasNextPage == true)
            {
                queryByTagResult = await _apiActorGroup.Ask<ReturnMessage<ProductByTagQueryOutput>>(
                    new ProductByTagQuery(tag, _firstVariants, queryByTagResult.Data.products.edges.First().node.variants.edges.Last().cursor), cancellationToken
                );

                if (queryByTagResult.Result == Result.Error)
                    return queryByTagResult;

                data.products.edges.First().node.variants.edges.AddRange(queryByTagResult.Data.products.edges.First().node.variants.edges);
            }

            return new ReturnMessage<ProductByTagQueryOutput> { Result = Result.OK, Data = data };
        }

        public string CleanText(string text, string fieldName)
        {
            var parameters = new Dictionary<string, string> { { "'", "" } };
            text = ReplaceString(text, parameters, fieldName).Trim();

            return new string(text
                     .Normalize(NormalizationForm.FormD)
                     .Where(ch => char.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                     .ToArray());
        }

        private async Task FillMultiLocation(long orderId, OrderResult.Order orderToUpdate, CancellationToken cancellationToken)
        {
            if (_shopifyData.EnabledMultiLocation)
            {
                var orderFulfillment = await _apiActorGroup.Ask<ReturnMessage<OrderResult>>(
                    new GetOrderFulfillmentRequest { OrderId = orderId }, cancellationToken
                );

                if (orderFulfillment.Result == Result.Error)
                    throw orderFulfillment.Error;

                if (orderFulfillment.Data?.fulfillment_orders == null)
                    throw new Exception($"Order no has fulfillment {orderId} not found");

                var order = orderFulfillment.Data?.fulfillment_orders;

                foreach (var item in orderToUpdate.line_items)
                {
                    var location = order.SelectMany(x => x.line_items.Where(y => y.variant_id == item.variant_id), (x, c) => x.assigned_location_id).FirstOrDefault();

                    item.location_id = location;
                }
            }

        }

        public string GetAddressNumber(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var split = text.Split(',');

            return split.Length > 1 ? split[1] : string.Empty;
        }

        public async Task<List<StoreLostedOrdersDto>> GetLostOrders(IEnumerable<Tenant> tenants, CancellationToken cancellationToken = default, double? fromLastHours = null)
        {
            var toleranceMinutes = _paramRepository
                .GetByKeyAsync(ParamConsts.SeachLostedOrders)
                .Result
                ?.GetValueBykey(SeachLostedOrdersConsts.ToleranceMinutes)
                ?.GetDoubleOrDefault() ?? 0.0;

            var lostedOrders = new List<StoreLostedOrdersDto>();

            foreach (var tenant in tenants)
            {

                var queryOutput = await GetOrders(tenant,
                    fromLastHours,
                    "NOT (IsIntg-True-Intg)",
                    toleranceMinutes,
                    cancellationToken);

                var edgersOrdersResult = queryOutput.orders.edges;

                if (edgersOrdersResult.Count <= 0) continue;

                lostedOrders.Add(new StoreLostedOrdersDto()
                {
                    StoreName = tenant.StoreName,
                    ShopifyStoreDomain = tenant.ShopifyData.ShopifyStoreDomain,
                    Orders = edgersOrdersResult.Select(x =>
                    new OrderDto()
                    {
                        Id = string.Join("", x.node.id.ToCharArray().Where(char.IsDigit)),
                        CreatedAt = x.node.createdAt
                    }).ToList()
                });
            }

            return lostedOrders;

        }

        public async Task<OrderByDateQueryOutput> GetOrders(Tenant tenant,
            double? fromLastHours = null,
            string additionalFilters = null,
            double toleranceMinutes = 0,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var apps = tenant.ShopifyData.GetShopifyApps();

                var versionShopify = _configuration.GetSection("Shopify")["Version"];

                var client = new ShopifyApiClient(_httpClientFactory, tenant.Id.ToString(), tenant.ShopifyData.ShopifyStoreDomain, versionShopify, apps.FirstOrDefault().ShopifyPassword);

                var paramValue = _paramRepository.GetByKeyAsync(ParamConsts.SeachLostedOrders)
                    .Result?.GetValueBykey(SeachLostedOrdersConsts.FromLastHours);

                var utcNow = DateTime.UtcNow.AddMinutes(-toleranceMinutes);

                var beginDate = utcNow.AddTicks(-TimeSpan.TicksPerDay);

                if (fromLastHours.HasValue)
                {
                    beginDate = utcNow.AddHours(-fromLastHours.Value);
                }
                else if (paramValue != null)
                {
                    beginDate = utcNow.AddHours(-Convert.ToDouble(paramValue.Value));
                }

                var queryByDateResult = await client.Post(
                    new OrderByDateQuery(beginDate, utcNow, null, Domain.Shopify.Models.Results.OrderResult.Tags, filters: additionalFilters),
                    cancellationToken);

                var orders = queryByDateResult.orders.edges;

                while (queryByDateResult.orders.pageInfo.hasNextPage == true && !cancellationToken.IsCancellationRequested)
                {
                    queryByDateResult = await client.Post(
                        new OrderByDateQuery(beginDate, DateTime.UtcNow, queryByDateResult.orders.edges.Last().cursor, Domain.Shopify.Models.Results.OrderResult.Tags, filters: additionalFilters),
                    cancellationToken);

                    if (queryByDateResult.orders.edges.Count > 0)
                    {
                        orders.AddRange(queryByDateResult.orders.edges);
                    }
                }

                queryByDateResult.orders.edges = orders;

                return queryByDateResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<NoteAttributeDto>> ReturnNoteAttributes(string urlBase, string method, Dictionary<string, string> param)
        {
            method = QueryHelpers.AddQueryString(method, param);

            var apiClient = new APIClientGeneric(_httpClientFactory, "", urlBase);

            var result = await apiClient.Get<List<ResultNotAttributesDto>>(method);

            return result.First().BuildNoteAttribute();
        }

        public async Task UpdateOrderNoteAttributesInShopify(long orderId, List<NoteAttribute> noteAttributes)
        {
            try
            {
                var loggerShopifyRESTClient = _serviceProvider.GetService<ILogger<ShopifyRESTClient>>();
                string versionShopify = _configuration.GetSection("Shopify")["Version"];

                var app = _shopifyData.ShopifyApps.First();

                var client = new ShopifyRESTClient(loggerShopifyRESTClient,
                                                   _httpClientFactory,
                                                   _shopifyData.Id.ToString(),
                                                   _shopifyData.ShopifyStoreDomain,
                                                   versionShopify,
                                                   app.ShopifyPassword);

                var shopifyOrderNoteAttributesWrapper = new ShopifyOrderWrapper<ShopifyOrderAttributesDTO>();
                shopifyOrderNoteAttributesWrapper.order = new ShopifyOrderAttributesDTO();
                shopifyOrderNoteAttributesWrapper.order.id = orderId;

                shopifyOrderNoteAttributesWrapper.order.note_attributes.AddRange(noteAttributes.Select(s => new ShopifyNoteAttributesDTO(s.name, s.value)));

                await client.Put($"orders/{orderId}", shopifyOrderNoteAttributesWrapper);

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Erro ao atualizar note_attributes");
            }
        }
    }
}
