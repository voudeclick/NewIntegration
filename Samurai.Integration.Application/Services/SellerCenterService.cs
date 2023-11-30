using Akka.Actor;
using Akka.Event;

using Microsoft.Extensions.Configuration;

using Samurai.Integration.APIClient.SellerCenter.Models.Requests;
using Samurai.Integration.APIClient.SellerCenter.Models.Requests.Inputs;
using Samurai.Integration.APIClient.SellerCenter.Models.Response;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.SellerCenter;
using Samurai.Integration.Domain.Messages.SellerCenter.ProductActor;
using Samurai.Integration.Domain.Messages.Shared;
using Samurai.Integration.Domain.Models.SellerCenter.API.Enums;
using Samurai.Integration.Domain.Queues;
using Samurai.Integration.EntityFramework.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Samurai.Integration.Domain.Extensions;

using static Samurai.Integration.Domain.Models.Product;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Application.Extensions;
using Newtonsoft.Json;
using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;

namespace Samurai.Integration.Application.Services
{
    public partial class SellerCenterService
    {
        private ILoggingAdapter _logger;
        private IActorRef _apiActorGroup;
        private readonly IConfiguration _configuration;
        private readonly TenantRepository _tenantRepository;

        public OrderService Order;
        public SellerCenterService(IConfiguration configuration, TenantRepository tenantRepository)
        {
            _configuration = configuration;
            _tenantRepository = tenantRepository;
            Order = new OrderService(configuration, _tenantRepository);
        }

        public void Init(IActorRef apiActorGroup, ILoggingAdapter logger)
        {
            Order.Init(apiActorGroup, logger);
            _apiActorGroup = apiActorGroup;
            _logger = logger;
        }

        public async Task<ReturnMessage> CreateProduct(SellerCenterCreateProductMessage message, SellerCenterDataMessage sellerCenterData,
            SellerCenterQueue.Queues queues, CancellationToken cancellationToken = default)
        {

            _logger.Warning("Innicio rotina de envio para o Seller Center");
            async Task<ReturnMessage<CreateProductResponse>> apiCreateProduct(CreateProductRequest request)
                => await _apiActorGroup.Ask<ReturnMessage<CreateProductResponse>>(request, cancellationToken);

            async Task<ReturnMessage<UpdateProductResponse>> apiUpdateProduct(UpdateProductRequest request)
                => await _apiActorGroup.Ask<ReturnMessage<UpdateProductResponse>>(request, cancellationToken);

            async Task<ReturnMessage<GetProductByFilterResponse>> apiGetProductByCode(string productId, Guid? sellerId)
                => await _apiActorGroup.Ask<ReturnMessage<GetProductByFilterResponse>>(new GetProductByFilterRequest { ProductCode = productId, SellerId = sellerId }, cancellationToken);

            var productInfo = message.ProductInfo;            

            var approvalStatus = sellerCenterData.StatusIntegration switch
            {
                OrderIntegrationStatusEnum.Aprovado => 0,
                OrderIntegrationStatusEnum.AguardandoAprovacao => 1,
                OrderIntegrationStatusEnum.Reprovado => 2,
                _ => throw new ArgumentException("Invalid StatusIntegration")
            };

            var sellerInfo = await GetSeller(sellerCenterData.SellerId, cancellationToken);
            var product = await apiGetProductByCode($"{sellerInfo.prefixId}_{productInfo.ExternalId}", new Guid(sellerCenterData.SellerId));

            if (product.Result == Result.Error) throw product.Error;

            var Manufacturers = new ProcessManufacturersMessage() { Manufacturers = new List<string> { sellerInfo.Name } };
            await ProcessManufacturers(Manufacturers, cancellationToken);

            if (product.Data?.Value != null)
            {

                if (!sellerCenterData.DisableUpdateProduct)
                {
                    var productSC = product.Data.Value;
                    var translation = productSC.Translations.FirstOrDefault(x => x.CultureName == Culture.ptBR);
                    if (translation != null)
                    {
                        translation.DisplayName = productInfo.Title;
                        translation.Description = productInfo.BodyHtml;
                        translation.Model = productInfo.Model;
                    }
                    else
                    {
                        productSC.Translations.Add(new TranslationDto { CultureName = Culture.ptBR, Description = productInfo.BodyHtml, DisplayName = productInfo.Title });
                    }

                    productSC.Images.Merge(productInfo.Images.ImageUrls.Select(x => new Image { Url = x }).ToList(),
                                           image => image.Url,
                                           updateImage => updateImage.Url,
                                           updateImage => updateImage,
                                           (image, updateImage) => image.Url = updateImage.Url);

                    productSC.Variations.Merge(await GetVariations(productInfo.DataSellerCenter.Variants, sellerInfo.prefixId.ToString(), cancellationToken),
                                               variation => variation.SKU,
                                               variationUpdate => variationUpdate.SKU,
                                               variationUpdate => variationUpdate,
                                               (variation, variationUpdate) => variation.UpdateFrom(variationUpdate));

                    productSC.Categories.Merge(await GetCategories(productInfo.Categories, cancellationToken: cancellationToken),
                                               category => category.CategoryId,
                                               categoryUpdate => categoryUpdate.CategoryId,
                                               categoryUpdate => categoryUpdate,
                                               (category, categoryUpdate) => { });
                    

                    var response = await apiUpdateProduct(new UpdateProductRequest
                    {
                        ClientCode = productInfo.ExternalId,
                        Id = productSC.Id,
                        SellerId = sellerCenterData.SellerId,
                        IsDigital = false,
                        Weight = productInfo.GetInfo?.Weight,
                        Height = productInfo.GetInfo?.Height,
                        HasVariations = productInfo.Variants.Count > 0,
                        Length = productInfo.GetInfo?.Length,
                        Width = productInfo.GetInfo?.Width,
                        Diameter = productInfo.GetInfo?.Diameter,
                        //ApprovalStatus = approvalStatus,
                        Variations = productSC.Variations,
                        Categories = productSC.Categories,
                        ManufacturerId = await GetManufacturer(sellerInfo.Name, cancellationToken),
                        Translations = productSC.Translations,
                        Images = productSC.Images
                    });

                    if (response.Result == Result.Error)
                        throw response.Error;
                }
            }
            else
            {
                var response = await apiCreateProduct(new CreateProductRequest
                {
                    ClientCode = productInfo.ExternalId,
                    SellerId = sellerCenterData.SellerId,
                    IsDigital = false,
                    Weight = productInfo.GetInfo?.Weight,
                    Height = productInfo.GetInfo?.Height,
                    HasVariations = productInfo.Variants.Count > 0,
                    Length = productInfo.GetInfo?.Length,
                    Width = productInfo.GetInfo?.Width,
                    Diameter = productInfo.GetInfo?.Diameter,
                    ApprovalStatus = approvalStatus,
                    Variations = await GetVariations(productInfo.DataSellerCenter.Variants, sellerInfo.prefixId.ToString(), cancellationToken),
                    Categories = await GetCategories(productInfo.Categories, cancellationToken: cancellationToken),
                    ManufacturerId = await GetManufacturer(sellerInfo.Name, cancellationToken),
                    Translations = new List<TranslationDto>() { new TranslationDto { CultureName = Culture.ptBR, Description = productInfo.BodyHtml, DisplayName = productInfo.Title, Model = productInfo.Model } },
                    Images = productInfo.Images?.ImageUrls?.Select(x => new Image() { Url = x })?.ToList()
                });

                if (response.Result == Result.Error)
                    throw response.Error;
            }

            _logger.Warning("Produto enviado para o Seller Center: {0}", JsonConvert.SerializeObject(product));
            _logger.Warning("Mensagem Seller Center: {0}", JsonConvert.SerializeObject(message));

            if (message.HasPriceStock)
            {
                var SkuPrices = new List<SkuPrice>();

                productInfo.DataSellerCenter?.Variants.Select(skuSellerCenter => skuSellerCenter).ToList().ForEach(delegate (SkuSellerCenter item)
                {
                    var sku = item.SkuPrice.Sku;
                    if (productInfo.DataSellerCenter.Variants.Count == 1)
                        sku = $"{sellerInfo.prefixId}_{item.Sku}";

                    SkuPrices.Add(new SkuPrice
                    {
                        Sku = sku,
                        CompareAtPrice = item.SkuPrice.CompareAtPrice,
                        Price = item.SkuPrice.Price,
                        ShopifyId = item.SkuPrice.ShopifyId,
                        Stock = item.SkuStock
                    });
                });                                

                #region price
                //productInfo.DataSellerCenter?.Variants?.Select(x => x.SkuPrice)?.ToList().ForEach(delegate (SkuPrice item)
                //{
                //    var sku = item.Sku;
                //    if (productInfo.DataSellerCenter.Variants.Count == 1)
                //        sku = $"{sellerInfo.prefixId}_{item.Sku}";

                //    SkuPrices.Add(new SkuPrice
                //    {
                //        Sku = sku,
                //        CompareAtPrice = item.CompareAtPrice,
                //        Price = item.Price,
                //        ShopifyId = item.ShopifyId                        
                //    });

                //});
                #endregion

                var priceMessage = new SellerCenterUpdatePriceAndStockMessage
                {
                    ProductId = $"{sellerInfo.prefixId}_{productInfo.ExternalId}",

                    Values = SkuPrices
                };

                _logger.Warning("priceMessage enviado para Seller Center: {0}", JsonConvert.SerializeObject(priceMessage));

                await queues.SendMessages((queues.UpdatePriceQueue, priceMessage, true));

                #region stock
                //foreach (var variant in productInfo.DataSellerCenter.Variants)
                //{
                //    var sku = variant.Sku;
                //    if (productInfo.DataSellerCenter.Variants.Count == 1)
                //        sku = $"{sellerInfo.prefixId}_{variant.Sku}";

                //    var stockMessage = new SellerCenterUpdateStockProductMessage
                //    {
                //        ExternalProductId = $"{sellerInfo.prefixId}_{productInfo.ExternalId}",
                //        Stock = new SellerCenterUpdateStockProductMessage.ValueItem
                //        {
                //            Sku = sku,
                //            Quantity = variant.SkuStock.Quantity
                //        }
                //    };

                //    _logger.Warning("stockMessage enviado para Seller Center: {0}", JsonConvert.SerializeObject(stockMessage));

                //    await queues.SendMessages((queues.UpdateStockProductQueue, stockMessage, true));
                //}
                #endregion
            }
            else
            {
                await queues.SendMessages((queues.GetPriceProduct, new GetPriceProductMessage { CodProduto = productInfo.ExternalId }, true));
            }

            return new ReturnMessage { Result = Result.OK };
        }

        #region VariationOption
        public async Task<ReturnMessage> ProcessVariationOptionsProduct(ProcessVariationOptionsMessage message, CancellationToken cancellationToken = default)
        {

            foreach (var variantion in message.Variants)
            {
                var response = await _apiActorGroup.Ask<ReturnMessage<GetVariationsByIdResponse>>(
                    new GetVariationByFilterRequest { Name = variantion.NomeVariacao }, cancellationToken);

                if (response.Result == Result.Error)
                {
                    await CreateVariation(variantion, cancellationToken);
                }
                else
                {
                    var result = CompareVariantions(variantion.Values, response.Data.Value);
                    if (result.CanUpdate)
                    {
                        result.Itens.AddRange(response.Data.Value.GetAvailableValues);
                        await UpdateVariation(response.Data.Value.Id.Value.ToString(), variantion.NomeVariacao, result.Itens);
                    }
                }

            }

            return new ReturnMessage { Result = Result.OK };

        }
        private async Task CreateVariation(ProcessVariationOptionsMessage.Variations variation, CancellationToken cancellationToken = default)
        {

            var request = new CreateVariationOptionRequest() { Name = variation.NomeVariacao };

            request.Translations.Add(new TranslationDto { CultureName = Culture.ptBR, DisplayName = variation.NomeVariacao });

            foreach (var value in variation.Values)
            {
                var item = new AvailableValues() { Value = value };
                item.Translations.Add(new TranslationDto { CultureName = Culture.ptBR, DisplayValue = value });
                request.AvailableValues.Add(item);
            }

            await _apiActorGroup.Ask<ReturnMessage<CreateVariationOptionResponse>>(request, cancellationToken);

        }
        private async Task UpdateVariation(string idVariation, string variationName, List<string> variationValues, CancellationToken cancellationToken = default)
        {
            var payload = new UpdateVariationOptionRequest { Name = variationName };
            payload.Translations.Add(new TranslationDto { CultureName = Culture.ptBR, DisplayName = variationName });

            foreach (var value in variationValues)
            {
                var availableValue = new AvailableValues { Value = value };
                availableValue.Translations.Add(new TranslationDto { CultureName = Culture.ptBR, DisplayValue = value });
                payload.AvailableValues.Add(availableValue);
            }

            payload.Id = idVariation;

            await _apiActorGroup.Ask<ReturnMessage<UpdateVariationOptionResponse>>(payload, cancellationToken);

        }
        #endregion

        #region Categories
        public async Task<ReturnMessage> ProcessCategories(ProcessCategoriesProductMessage message, CancellationToken cancellationToken = default)
        {

            await FindAndCreateCategoriesAsync(message.Categories, cancellationToken: cancellationToken);

            return new ReturnMessage { Result = Result.OK };

        }
        private async Task FindAndCreateCategoriesAsync(List<Domain.Models.Product.Category> categories, bool isParent = true, Guid? parentId = null,
            CancellationToken cancellationToken = default)
        {
            if (categories.Count <= 0) return;

            async Task<ReturnMessage<CreateCategoryResponse>> apiCreateCategory(CreateCategoryRequest request)
                => await _apiActorGroup.Ask<ReturnMessage<CreateCategoryResponse>>(request, cancellationToken);

            async Task<ReturnMessage<GetCategoriesByFilterResponse>> apiGetCategory(string category)
                => await _apiActorGroup.Ask<ReturnMessage<GetCategoriesByFilterResponse>>(new GetCategoriesByFilterRequest { Name = category }, cancellationToken);


            foreach (var category in categories)
            {
                var response = await apiGetCategory(category.Name);

                if (response.Result == Result.Error) throw response.Error;

                async Task validResponse(GetCategoriesByFilterResponse.Values response)
                {
                    var translations = new List<TranslationDto>() { new TranslationDto { CultureName = Culture.ptBR, DisplayName = category.Name } };
                    if (response != null)
                    {
                        await FindAndCreateCategoriesAsync(category.ChildCategories, false, response.Id, cancellationToken);
                    }
                    else
                    {
                        var responseCreate = await apiCreateCategory(new CreateCategoryRequest { Name = category.Name, Translations = translations, ParentId = parentId });
                        await FindAndCreateCategoriesAsync(category.ChildCategories, false, responseCreate.Data?.Value?.Id, cancellationToken);
                    }
                };

                var result = isParent ? response.Data.ParentCategories(category.Name) : response.Data.ChildCategories(category.Name, parentId);
                await validResponse(result);
            }

        }
        #endregion
        public async Task<ReturnMessage> ProcessManufacturers(ProcessManufacturersMessage message, CancellationToken cancellationToken = default)
        {

            async Task<ReturnMessage<GetManufacturersByFilterResponse>> apiGetManufacturer(string category)
                => await _apiActorGroup.Ask<ReturnMessage<GetManufacturersByFilterResponse>>(new GetManufacturersByFilterRequest { Name = category }, cancellationToken);

            async Task<ReturnMessage<CreateManufacturersResponse>> apiCreateManufacturer(CreateManufacturersRequest request)
              => await _apiActorGroup.Ask<ReturnMessage<CreateManufacturersResponse>>(request, cancellationToken);

            foreach (var manufacturer in message.Manufacturers)
            {
                var response = await apiGetManufacturer(manufacturer);

                if (response.Data.NotExist)
                {
                    var translations = new List<TranslationDto>() { new TranslationDto { CultureName = Culture.ptBR, DisplayName = manufacturer } };
                    await apiCreateManufacturer(new CreateManufacturersRequest { Name = manufacturer, Translations = translations });
                }
            }

            return new ReturnMessage { Result = Result.OK };

        }

        private (bool CanUpdate, List<string> Itens) CompareVariantions(List<string> variantsMessage, GetVariationsByIdResponse.Values variantsApi)
        {
            var listValuesNotAvailable = new List<string>();

            var valuesAvailable = variantsApi.AvailableValues.Where(x => variantsMessage.Select(y => y.ToUpper()).Contains(x.Value.ToUpper())).ToList();
            listValuesNotAvailable.AddRange(variantsMessage.Where(x => !variantsApi.GetAvailableValues.Select(y => y.ToUpper()).Contains(x.ToUpper())).ToList());

            return (listValuesNotAvailable.Count > 0, listValuesNotAvailable.Distinct().ToList());

        }

        public async Task<ReturnMessage> UpdatePriceAndStockProduct(SellerCenterUpdatePriceAndStockMessage message, SellerCenterDataMessage sellercenter, CancellationToken cancellationToken = default)
        {
            var request = new List<UpdatePriceProductRequest>();

            foreach (var item in message.Values)
            {
                request.Add(new UpdatePriceProductRequest
                {
                    SellerId = sellercenter.SellerId,
                    ProductClientCode = message.ProductId,
                    VariationSku = item.Sku
                }.AddStocksItem(new StockItem
                {
                    SellerWarehouseId = sellercenter.SellerWarehouseId,
                    FromPrice = item.CompareAtPrice,
                    ByPrice = item.Price,
                    Quantity = item.Stock.Quantity,
                    SellWithoutStock = sellercenter.SellWithoutStock
                })
                );
            }

            async Task<ReturnMessage<UpdatePriceProductResponse>> apiUpdatePriceAndStock()
                => await _apiActorGroup.Ask<ReturnMessage<UpdatePriceProductResponse>>(request, cancellationToken);

            async Task<ReturnMessage<GetProductByFilterResponse>> apiGetProductByCode(string productId, Guid? sellerId)
                => await _apiActorGroup.Ask<ReturnMessage<GetProductByFilterResponse>>(new GetProductByFilterRequest { ProductCode = productId, SellerId = sellerId }, cancellationToken);
            
            var product = await apiGetProductByCode(message.ProductId, new Guid(sellercenter.SellerId));
            if (product.Result == Result.Error) throw product.Error;

            if (product.Data.Value != null)
                await apiUpdatePriceAndStock();
            else
                return new ReturnMessage { Result = Result.Error, Error = new Exception($"Not found productId {message.ProductId} by SellerId: {sellercenter.SellerId}") };

            return new ReturnMessage { Result = Result.OK };
        }

        public async Task<ReturnMessage> UpdateStockProduct(SellerCenterUpdateStockProductMessage message, SellerCenterDataMessage sellercenter, CancellationToken cancellationToken = default)
        {
            var request = new List<UpdatePriceProductRequest>();

            async Task<ReturnMessage<GetProductVariationByClientCodeResponse>> apiGetProductVariationByCode(string productId)
               => await _apiActorGroup.Ask<ReturnMessage<GetProductVariationByClientCodeResponse>>(new GetProductVariationByClientCodeRequest { ClientCode = productId, SellerId = sellercenter.SellerId }, cancellationToken);

            var product = await apiGetProductVariationByCode(message.ExternalProductId);
            if (product.Result == Result.Error) throw product.Error;

            foreach (var item in product.Data.Value.ProductVariations)
            {
                var sku = product.Data.GetStockBySku(item.VariationSKU);

                var stockItem = new StockItem
                {
                    SellerWarehouseId = sellercenter.SellerWarehouseId,
                    ByPrice = sku.ByPrice,
                    FromPrice = sku.FromPrice,
                    Quantity = sku.Quantity,
                    SellWithoutStock = sellercenter.SellWithoutStock
                };

                if (string.Equals(item.VariationSKU, message.Stock.Sku, StringComparison.Ordinal))
                    stockItem.Quantity = message.Stock.Quantity;

                request.Add(new UpdatePriceProductRequest
                {
                    SellerId = item.SellerId,
                    ProductClientCode = item.ProductClientCode,
                    VariationSku = item.VariationSKU
                }.AddStocksItem(stockItem));
            }

            async Task<ReturnMessage<UpdatePriceProductResponse>> apiUpdatePrice()
                => await _apiActorGroup.Ask<ReturnMessage<UpdatePriceProductResponse>>(request, cancellationToken);

            if (product.Data.Exists)
                await apiUpdatePrice();
            else
            {
                _logger.Warning("SellerCenterService - Error in UpdateStockProduct", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, sellercenter));
                return new ReturnMessage { Result = Result.Error, Error = new Exception($"Not found productId {message.ExternalProductId} by SellerId: {sellercenter.SellerId}") };
            }

            return new ReturnMessage { Result = Result.OK };

        }

        private async Task<List<Variation>> GetVariations(List<SkuSellerCenter> productVariations, string prefixId, CancellationToken cancellationToken = default)
        {
            async Task<ReturnMessage<GetVariationsByIdResponse>> apiGetVariation(string name)
                => await _apiActorGroup.Ask<ReturnMessage<GetVariationsByIdResponse>>(new GetVariationByFilterRequest { Name = name }, cancellationToken);

            var variations = new List<Variation>();

            foreach (var productVariation in productVariations)
            {
                if (productVariation.InfoVariations.Any())
                {
                    var sku = productVariation.Sku;
                    if (productVariation.InfoVariations.Count == 1)
                        sku = $"{prefixId}_{productVariation.Sku}";

                    var product = new Variation
                    {
                        SKU = sku,
                        BarCode = productVariation.Barcode,
                        IsDigital = productVariation.IsDigital,
                        Weight = productVariation.Weight,
                        Height = productVariation.Height,
                        Length = productVariation.Length,
                        Width = productVariation.Width,
                        Diameter = productVariation.Diameter,
                    };

                    foreach (var item in productVariation.InfoVariations)
                    {
                        var response = await apiGetVariation(item.NomeVariacao);
                        if (response.Result == Result.OK)
                        {

                            product.Options.Add(new VariationOption
                            {
                                VariationOptionId = response.Data.Value.Id,
                                VariationOptionAvailableValueId = response.Data.Value.GetIdAvailableValuesByName(item.ValorVariacao)
                            });
                        }
                    }
                    variations.Add(product);
                }
                else
                {
                    var product = new Variation
                    {
                        SKU = productVariation.Sku,
                        BarCode = productVariation.Barcode,
                        IsDigital = productVariation.IsDigital,
                        Weight = productVariation.Weight,
                        Height = productVariation.Height,
                        Length = productVariation.Length,
                        Width = productVariation.Width,
                        Diameter = productVariation.Diameter,
                    };

                    variations.Add(product);
                }
            }
            return variations;

        }

        private async Task<List<APIClient.SellerCenter.Models.Requests.Inputs.Category>> GetCategories(List<Domain.Models.Product.Category> categories, Guid? parentId = null, bool isParent = true, CancellationToken cancellationToken = default)
        {
            if (categories.Count <= 0) return new List<APIClient.SellerCenter.Models.Requests.Inputs.Category>() { new APIClient.SellerCenter.Models.Requests.Inputs.Category { CategoryId = parentId.Value } };

            async Task<ReturnMessage<GetCategoriesByFilterResponse>> apiGetCategory(string category)
                => await _apiActorGroup.Ask<ReturnMessage<GetCategoriesByFilterResponse>>(new GetCategoriesByFilterRequest { Name = category }, cancellationToken);

            var productCategories = new List<APIClient.SellerCenter.Models.Requests.Inputs.Category>();
            foreach (var categorie in categories)
            {
                var response = await apiGetCategory(categorie.Name);

                if (response.Result == Result.Error) throw response.Error;

                if (isParent)
                {
                    var parentCategories = response.Data.ParentCategories(categorie.Name);
                    productCategories.AddRange(await GetCategories(categorie.ChildCategories, parentCategories.Id, false, cancellationToken));
                }
                else
                {
                    var child = response.Data.ChildCategories(categorie.Name, parentId);
                    productCategories.AddRange(await GetCategories(categorie.ChildCategories, child.Id, false, cancellationToken));

                }
            }

            return productCategories;

        }
        private async Task<Guid?> GetManufacturer(string manufacturer, CancellationToken cancellationToken = default)
        {
            async Task<ReturnMessage<GetManufacturersByFilterResponse>> apiGetManufacturer(string name)
                => await _apiActorGroup.Ask<ReturnMessage<GetManufacturersByFilterResponse>>(new GetManufacturersByFilterRequest { Name = name }, cancellationToken);

            Guid? id = Guid.Empty;
            var response = await apiGetManufacturer(manufacturer);
            _logger.Warning("GetManufacturer: {0}", JsonConvert.SerializeObject(response));
            if (response.Result == Result.OK && response.Data.Value.Count > 0)
                id = response.Data.Value.FirstOrDefault().Id;



            return id;
        }

        private async Task<GetSellerResponse.Values> GetSeller(string sellerId, CancellationToken cancellationToken = default)
        {
            var response = await _apiActorGroup.Ask<ReturnMessage<GetSellerResponse>>(new GetSellerRequest { SellerId = sellerId }, cancellationToken);           
            if (response.Result == Result.OK)
                return response.Data.Value;

            return null;
        }
    }
}
