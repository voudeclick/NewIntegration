using Akka.Actor;
using Akka.Event;
using Microsoft.Extensions.Configuration;
using Samurai.Integration.APIClient.Tray.Models.Requests.Category;
using Samurai.Integration.APIClient.Tray.Models.Requests.Inputs;
using Samurai.Integration.APIClient.Tray.Models.Requests.Manufacture;
using Samurai.Integration.APIClient.Tray.Models.Requests.Product;
using Samurai.Integration.APIClient.Tray.Models.Requests.Variant;
using Samurai.Integration.APIClient.Tray.Models.Requests.Variation;
using Samurai.Integration.APIClient.Tray.Models.Response.Category;
using Samurai.Integration.APIClient.Tray.Models.Response.Inputs;
using Samurai.Integration.APIClient.Tray.Models.Response.Manufacture;
using Samurai.Integration.APIClient.Tray.Models.Response.Product;
using Samurai.Integration.APIClient.Tray.Models.Response.Variant;
using Samurai.Integration.Application.Extensions;
using Samurai.Integration.Domain.Enums.Tray;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Tray;
using Samurai.Integration.Domain.Messages.Tray.ProductActor;
using Samurai.Integration.Domain.Queues;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static Samurai.Integration.APIClient.Tray.Models.Requests.Category.CreateCategoryRequest;
using static Samurai.Integration.APIClient.Tray.Models.Requests.Inputs.Product;
using static Samurai.Integration.APIClient.Tray.Models.Response.Inputs.VariationModel;
using static Samurai.Integration.Domain.Messages.Tray.Models.Product;
using ProductInfo = Samurai.Integration.Domain.Messages.Tray.Models.Product.Info;

namespace Samurai.Integration.Application.Services
{
    public partial class TrayService
    {
        private ILoggingAdapter _log;
        private IActorRef _apiActorGroup;
        private readonly IConfiguration _configuration;
        private readonly TenantRepository _tenantRepository;

        public TrayOrderService Order;


        public TrayService(IConfiguration configuration, TenantRepository tenantRepository)
        {
            _configuration = configuration;
            _tenantRepository = tenantRepository;
            Order = new TrayOrderService(configuration, _tenantRepository);
        }

        public void Init(IActorRef apiActorGroup, ILoggingAdapter log)
        {
            _apiActorGroup = apiActorGroup;
            _log = log;
            Order.Init(apiActorGroup, log);
        }

        //private async Task<ReturnMessage<GetProductsByFilterResponse>> GetProduct(long productId, string reference, CancellationToken cancellationToken = default)
        //{
        //    async Task<ReturnMessage<GetProductsByFilterResponse>> apiGetProductById(long productId)
        //     => await _apiActorGroup.Ask<ReturnMessage<GetProductsByFilterResponse>>(new GetProductsByFilterRequest { Id = productId }, cancellationToken);

        //    async Task<ReturnMessage<GetProductsByFilterResponse>> apiGetProductByReference(string reference)
        //        => await _apiActorGroup.Ask<ReturnMessage<GetProductsByFilterResponse>>(new GetProductsByFilterRequest { Reference = reference }, cancellationToken);

        //    if (productId > 0)
        //        return await apiGetProductById(productId);
        //    else
        //        return await apiGetProductByReference(reference);
        //}

        public async Task<ReturnMessage> ProcessProduct(TrayProcessProductMessage message, CancellationToken cancellationToken = default)
        {
            var result = new ReturnMessage() { Result = Result.OK };

            async Task<ReturnMessage<GetProductByIdResponse>> apiGetProductById(long productId)
             => await _apiActorGroup.Ask<ReturnMessage<GetProductByIdResponse>>(new GetProductByIdRequest { Id = productId }, cancellationToken);

            async Task<ReturnMessage> apiProcessProduct(UpdateProductProcessingRequest request)
             => await _apiActorGroup.Ask<ReturnMessage>(request, cancellationToken);

            async Task<ReturnMessage<GetProductProcessResponse>> apiGetProductProcessById(Guid productId)
             => await _apiActorGroup.Ask<ReturnMessage<GetProductProcessResponse>>(new GetProductProcessRequest { ProductId = productId }, cancellationToken);

            bool available = false;

            var returnMessage = new TrayAppReturnMessage()
            {
                Type = TrayAppReturnMessageTypeEnum.Product.ToString(),
                Success = true,
                StoreId = message.StoreId,
                Product = new TrayAppReturnMessage.ProductIntegration()
                {
                    Id = message.Product.AppTrayProductId,
                    TrayProductId = message.Product.Id,
                    Status = message.Status,
                    Available = available
                }
            };

            try
            {
                APIClient.Tray.Models.Response.Inputs.ProductModel productExist = null;

                if (message.Product != null && message?.Product?.AppTrayProductId != Guid.Empty && message?.Product?.AppTrayProductId != null)
                {
                    var product = await apiGetProductProcessById(message.Product.AppTrayProductId);

                    if (product?.Data != null && (product?.Data?.TrayProductId != null && product?.Data?.TrayProductId > 0))
                        message.Product.Id = (long)product?.Data?.TrayProductId;

                    if (message.Product.Id > 0)
                    {
                        var trayProduct = await apiGetProductById(message.Product.Id);
                        if (trayProduct.Result == Result.Error)
                        {
                            returnMessage.Success = false;
                            returnMessage.Message = trayProduct.Error?.Message;

                            await apiProcessProduct(new UpdateProductProcessingRequest() { TrayAppMessage = returnMessage });

                            //_log.Error($"Error in ProcessProduct (apiGetProductById) - Store = {message.StoreId}, ProductId = {message.Product.AppTrayProductId}, TrayProductId = {message.Product.Id}, Message = {trayProduct.Error?.Message}");

                            return result;
                        }

                        productExist = trayProduct?.Data?.Product;
                    }

                    if (productExist == null && message.Product.Id > 0)
                    {
                        returnMessage.Success = false;
                        returnMessage.Message = "This product does not exist.";

                        await apiProcessProduct(new UpdateProductProcessingRequest() { TrayAppMessage = returnMessage });
                        return result;
                    }

                    if (productExist == null && message.Product.Id == 0)
                    {
                        var responseCreated = await CreateProduct(message.Product, returnMessage, cancellationToken);
                        if (!responseCreated.Success)
                        {
                            //_log.Error($"Error in ProcessProduct (CreateProduct) - Store = {message.StoreId}, ProductId = {message.Product.AppTrayProductId}, TrayProductId = {message.Product.Id}, Message = {responseCreated.Message}");

                            await apiProcessProduct(new UpdateProductProcessingRequest() { TrayAppMessage = responseCreated });
                            return result;
                        }

                        message.Product.Id = responseCreated.Product.TrayProductId;

                        var responseProcessProduct = await apiProcessProduct(new UpdateProductProcessingRequest() { TrayAppMessage = responseCreated });
                        if (responseProcessProduct.Result == Result.Error)
                        {
                            //_log.Error($"Error in CreateProduct (apiProcessProduct) - Store = {message.StoreId}, ProductId = {message.Product.AppTrayProductId}, TrayProductId = {message.Product.Id}, Message = {responseProcessProduct.Error?.Message}");
                            return result;
                        }

                        available = true;
                    }
                    else if (productExist != null && message.Product.Id > 0)
                    {
                        message.Product.Id = long.Parse(productExist.Id);

                        if (message.Deleted)
                        {
                            var responseDeleted = await DeleteProduct(message.Product.Id, returnMessage, cancellationToken);

                            //_log.Info($"ProcessProduct (DeleteProduct) - Store = {message.StoreId}, ProductId = {message.Product.AppTrayProductId}, TrayProductId = {message.Product.Id}, Message = {responseDeleted.Message}");

                            await apiProcessProduct(new UpdateProductProcessingRequest() { TrayAppMessage = responseDeleted });
                            return result;
                        }

                        var responseUpdated = await UpdateProduct(message, productExist, returnMessage, cancellationToken);
                        if (!responseUpdated.Success)
                        {
                            //_log.Error($"Error in ProcessProduct (UpdateProduct) - Store = {message.StoreId}, ProductId = {message.Product.AppTrayProductId}, TrayProductId = {message.Product.Id}, Message = {responseUpdated.Message}");

                            await apiProcessProduct(new UpdateProductProcessingRequest() { TrayAppMessage = responseUpdated });
                            return result;
                        }

                        var responseProcessProduct = await apiProcessProduct(new UpdateProductProcessingRequest() { TrayAppMessage = responseUpdated });
                        if (responseProcessProduct.Result == Result.Error)
                        {
                            //_log.Error($"Error in UpdateProduct (apiProcessProduct) - Store = {message.StoreId}, ProductId = {message.Product.AppTrayProductId}, TrayProductId = {message.Product.Id}, Message = {responseProcessProduct.Error?.Message}");
                            return result;
                        }
                    }

                    if (message.Product.Id > 0 && message.Product.Variations != null && message.Product.Variations.Count() > 0)
                    {
                        var requestVariation = new TrayProcessVariationMessage()
                        {
                            StoreId = message.StoreId,
                            AppTrayProductId = message.Product.AppTrayProductId,
                            TrayProductId = message.Product.Id,
                            Variations = message.Product.Variations,
                            UpdatePrice = message.UpdatePrice,
                            UpdateStock = message.UpdateStock
                        };

                        var response = await ProcessVariationOptions(requestVariation, cancellationToken);

                        returnMessage.Product.Available = available;
                        returnMessage.Product.Variations = response.Product.Variations;

                        await apiProcessProduct(new UpdateProductProcessingRequest() { TrayAppMessage = returnMessage });
                        return result;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                //_log.Error(ex, $"Error in ProcessProduct - Store = {message.StoreId}, ProductId = {message.Product.AppTrayProductId}, TrayProductId = {message.Product.Id}");

                returnMessage.Success = false;
                returnMessage.Message = $"Error in ProcessProduct: {ex.Message}";

                await apiProcessProduct(new UpdateProductProcessingRequest() { TrayAppMessage = returnMessage });
                return result;
            }
        }


        public async Task<ReturnMessage> ProcessManufacturers(TrayProcessManufactureMessage message, CancellationToken cancellationToken = default)
        {
            async Task<ReturnMessage<CreateManufactureResponse>> apiCreateManufacture(CreateManufactureRequest request)
                => await _apiActorGroup.Ask<ReturnMessage<CreateManufactureResponse>>(request, cancellationToken);

            async Task<ReturnMessage<GetManufactureByFilterResponse>> apiGetManufactureByName(GetManufactureByFilterRequest request)
              => await _apiActorGroup.Ask<ReturnMessage<GetManufactureByFilterResponse>>(request, cancellationToken);

            foreach (var manufacture in message.Manufacturers)
            {
                var response = await apiGetManufactureByName(new GetManufactureByFilterRequest() { Brand = manufacture.Brand });

                if (response.Result == Result.Error)
                    return new ReturnMessage() { Result = Result.Error, Error = response.Error };

                if (!response.Data.Brands.Any())
                {
                    var responseCreate = await apiCreateManufacture(new CreateManufactureRequest()
                    {
                        Brand = manufacture.Brand,
                        Slug = manufacture.Slug
                    });


                    if (responseCreate.Result == Result.Error)
                        return new ReturnMessage() { Result = Result.Error, Error = responseCreate.Error };
                }
            }

            return new ReturnMessage() { Result = Result.OK };
        }


        private async Task<TrayAppReturnMessage> CreateProduct(ProductInfo product, TrayAppReturnMessage returnMessage, CancellationToken cancellationToken = default)
        {
            try
            {
                async Task<ReturnMessage<CreateProductResponse>> apiCreateProduct(CreateProductRequest request)
                  => await _apiActorGroup.Ask<ReturnMessage<CreateProductResponse>>(request, cancellationToken);

                var response = await apiCreateProduct(new CreateProductRequest()
                {
                    Ean = product.Ean,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    CostPrice = product.CostPrice,
                    IpiValue = product.IpiValue,
                    Brand = product.Brand?.Brand,
                    Model = product.Model,
                    Weight = product.Weight,
                    Length = product.Length,
                    Width = product.Width,
                    Height = product.Height,
                    Stock = product.Stock,
                    CategoryId = product.Category.Id,
                    Available = 0,
                    Reference = product.Reference,
                    RelatedCategories = product.RelatedCategories,
                    PictureSource1 = ImageExtensions.ValidateExtension(product.PictureSource1),
                    PictureSource2 = ImageExtensions.ValidateExtension(product.PictureSource2),
                    PictureSource3 = ImageExtensions.ValidateExtension(product.PictureSource3),
                    PictureSource4 = ImageExtensions.ValidateExtension(product.PictureSource4),
                    PictureSource5 = ImageExtensions.ValidateExtension(product.PictureSource5),
                    PictureSource6 = ImageExtensions.ValidateExtension(product.PictureSource6),
                    Metatag = product.Metatag?.Select(x => new MetatagModel() { Type = x.Type, Content = x.Content, Local = x.Local }).ToList(),
                    VirtualProduct = "0", //Produto Normal
                    Availability = product.Availability,
                    AvailabilityDays = product.AvailabilityDays
                });

                if (response.Result == Result.Error)
                {
                    returnMessage.Success = false;
                    returnMessage.Message = response.Error?.Message;
                }
                else
                {
                    var trayProductId = response?.Data != null ? long.Parse(response.Data.Id) : 0;
                    if (trayProductId > 0)
                    {
                        returnMessage.Product.TrayProductId = trayProductId;
                        returnMessage.Message = response?.Data?.Message;
                        returnMessage.Success = true;
                    }
                    else
                    {
                        returnMessage.Success = false;
                        returnMessage.Message = $"Produto não importado - {response.Data.Message}";
                    }
                }
            }
            catch (Exception ex)
            {
                returnMessage.Success = false;
                returnMessage.Message = $"Error in CreateProduct: {ex.Message}";
            }

            return returnMessage;
        }
        private async Task<TrayAppReturnMessage> UpdateProduct(TrayProcessProductMessage message, APIClient.Tray.Models.Response.Inputs.ProductModel productExist, TrayAppReturnMessage returnMessage, CancellationToken cancellationToken = default)
        {
            try
            {
                async Task<ReturnMessage<UpdateProductResponse>> apiUpdateProduct(UpdateProductRequest request)
                    => await _apiActorGroup.Ask<ReturnMessage<UpdateProductResponse>>(request, cancellationToken);

                async Task<ReturnMessage<UpdateProductResponse>> apiUpdateProductPrice(UpdateProductPriceRequest request)
                   => await _apiActorGroup.Ask<ReturnMessage<UpdateProductResponse>>(request, cancellationToken);

                async Task<ReturnMessage<UpdateProductResponse>> apiUpdateProductStock(UpdateProductStockRequest request)
                   => await _apiActorGroup.Ask<ReturnMessage<UpdateProductResponse>>(request, cancellationToken);

                async Task<ReturnMessage<UpdateProductResponse>> apiUpdateAvailable(UpdateProductAvailableRequest request)
                   => await _apiActorGroup.Ask<ReturnMessage<UpdateProductResponse>>(request, cancellationToken);

                var response = new ReturnMessage<UpdateProductResponse>();
                var product = message.Product;

                returnMessage.Product.TrayProductId = product.Id;

                if (message.UpdateAvailable)
                {
                    response = await apiUpdateAvailable(new UpdateProductAvailableRequest()
                    {
                        Id = product.Id.ToString(),
                        Available = (int)product.Available
                    });
                }
                else if (message.UpdatePrice)
                {
                    var updateProductPriceRequest = new UpdateProductPriceRequest()
                    {
                        Id = product.Id.ToString(),
                        CostPrice = (double)product.CostPrice,
                        Price = (double)product.Price,
                        Availability = product.Availability,
                        AvailabilityDays = (int)product.AvailabilityDays,
                        Stock = null
                    };

                    if (message.UpdateStock && product.Stock != null)
                    {
                        updateProductPriceRequest.Stock = (long)product.Stock;
                        updateProductPriceRequest.Available = message.Product.Available;
                    }

                    response = await apiUpdateProductPrice(updateProductPriceRequest);
                }
                else if (message.UpdateStock)
                {
                    response = await apiUpdateProductStock(new UpdateProductStockRequest()
                    {
                        Id = product.Id.ToString(),
                        Stock = (long)product.Stock,
                        Available = message.Product.Available
                    });
                }
                else
                {
                    var updateProductRequest = new UpdateProductRequest();
                    if (updateProductRequest.HasProductUpdate(productExist, product))
                    {
                        updateProductRequest.From(productExist, product);

                        response = await apiUpdateProduct(updateProductRequest);
                    }
                }

                returnMessage.Message = response.Data?.Message;

                if (response.Result == Result.Error)
                {
                    returnMessage.Success = false;
                    returnMessage.Message = response.Error?.Message;
                }
            }
            catch (Exception ex)
            {
                returnMessage.Success = false;
                returnMessage.Message = $"Error in UpdateProduct: {ex.Message}";
            }

            return returnMessage;
        }
        private async Task<TrayAppReturnMessage> DeleteProduct(long productId, TrayAppReturnMessage returnMessage, CancellationToken cancellationToken = default)
        {
            try
            {
                async Task<ReturnMessage<DeleteProductResponse>> apiDeleteProduct(DeleteProductRequest request)
                    => await _apiActorGroup.Ask<ReturnMessage<DeleteProductResponse>>(request, cancellationToken);

                var response = await apiDeleteProduct(new DeleteProductRequest()
                {
                    Id = productId
                });

                returnMessage.Product.TrayProductId = productId;
                returnMessage.Message = response.Data?.Message;

                if (response.Result == Result.Error)
                {
                    returnMessage.Success = false;
                    returnMessage.Message = response.Error?.Message;
                }

            }
            catch (Exception ex)
            {
                returnMessage.Success = false;
                returnMessage.Message = $"Error in DeleteProduct: {ex.Message}";

                _log.Error($"TrayService - Error in DeleteProduct | {ex}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), productId, ex.Message));

            }

            return returnMessage;
        }


        public async Task<TrayAppReturnMessage> ProcessVariationOptions(TrayProcessVariationMessage message, CancellationToken cancellationToken = default)
        {
            var returnMessage = new TrayAppReturnMessage()
            {
                Type = TrayAppReturnMessageTypeEnum.ProductVariation.ToString(),
                Success = true,
                Product = new TrayAppReturnMessage.ProductIntegration()
                {
                    Id = message.AppTrayProductId,
                    TrayProductId = message.TrayProductId,
                    Variations = new List<TrayAppReturnMessage.ProductIntegration.ProductVariationIntegration>()
                }
            };

            try
            {
                async Task<ReturnMessage<GetVariantsByFilterResponse>> apiGetVariationByProductId(GetVariantsByFilterRequest request)
                  => await _apiActorGroup.Ask<ReturnMessage<GetVariantsByFilterResponse>>(request, cancellationToken);

                async Task<ReturnMessage<UpdateVariantResponse>> apiUpdateVariationStock(UpdateVariantStockRequest request)
                  => await _apiActorGroup.Ask<ReturnMessage<UpdateVariantResponse>>(request, cancellationToken);

                async Task<ReturnMessage<UpdateVariantResponse>> apiUpdateVariationPrice(UpdateVariantPriceRequest request)
                  => await _apiActorGroup.Ask<ReturnMessage<UpdateVariantResponse>>(request, cancellationToken);

                async Task<ReturnMessage<DeleteVariantResponse>> apiDeleteVariation(DeleteVariantRequest request)
                  => await _apiActorGroup.Ask<ReturnMessage<DeleteVariantResponse>>(request, cancellationToken);

                var productId = message.TrayProductId;
                if (productId == 0)
                {
                    returnMessage.Success = false;
                    returnMessage.Message = "Product Id is required in Variation";

                    returnMessage.Type = TrayAppReturnMessageTypeEnum.Product.ToString();

                    return returnMessage;
                }

                var responseVariations = await apiGetVariationByProductId(new GetVariantsByFilterRequest() { ProductId = productId });
                if (responseVariations.Result == Result.Error)
                {
                    returnMessage.Success = false;
                    returnMessage.Message = responseVariations.Error?.Message;

                    returnMessage.Type = TrayAppReturnMessageTypeEnum.Product.ToString();

                    return returnMessage;
                }

                var variationsExists = responseVariations?.Data?.Variants.Select(x => x.Variation).ToList();

                var groupVariations = message.Variations.GroupBy(x => new { x.Type1, x.Value1, x.Type2, x.Value2 }).Select(x => x.First()).Distinct().ToList();

                var isDoubleVariation = groupVariations.Any(x => !string.IsNullOrEmpty(x.Type2) && !string.IsNullOrEmpty(x.Value2));

                if (isDoubleVariation)
                {
                    groupVariations = groupVariations.Where(x => !string.IsNullOrEmpty(x.Type1) && !string.IsNullOrEmpty(x.Value1) &&
                                                                 !string.IsNullOrEmpty(x.Type2) && !string.IsNullOrEmpty(x.Value2)).ToList();

                    var removeNotDoubleVariations = variationsExists.Where(x => x.Sku.Count() == 1).ToList();
                    if (removeNotDoubleVariations.Any())
                    {
                        foreach (var variation in removeNotDoubleVariations)
                        {
                            var response = await apiDeleteVariation(new DeleteVariantRequest() { Id = variation.Id.ToString() });
                            if (response.Result == Result.Error)
                                continue;

                            variationsExists.Remove(variation);
                        }
                    }
                }
                else
                {
                    groupVariations = groupVariations.Where(x => !string.IsNullOrEmpty(x.Type1) && !string.IsNullOrEmpty(x.Value1) &&
                                                                 string.IsNullOrEmpty(x.Type2) && string.IsNullOrEmpty(x.Value2)).ToList();

                    var removeNotSimpleVariations = variationsExists.Where(x => x.Sku.Count() > 1).ToList();
                    if (removeNotSimpleVariations.Any())
                    {
                        foreach (var variation in removeNotSimpleVariations)
                        {
                            var response = await apiDeleteVariation(new DeleteVariantRequest() { Id = variation.Id.ToString() });
                            if (response.Result == Result.Error)
                                continue;

                            variationsExists.Remove(variation);
                        }
                    }
                }

                var type1 = groupVariations.Select(x => x.Type1).FirstOrDefault();
                var type2 = groupVariations.Select(x => x.Type2).FirstOrDefault();

                if (variationsExists != null && variationsExists.Count() > 0)
                {
                    var updateType = !variationsExists.Where(x => !isDoubleVariation ? x.Sku.FirstOrDefault(y => y.Type == type1.Trim()) != null :
                                                                                      (x.Sku.FirstOrDefault(y => y.Type == type1.Trim()) != null &&
                                                                                       x.Sku.LastOrDefault(y => y.Type == type2.Trim()) != null)).Any();

                    if (updateType)
                    {
                        foreach (var variation in variationsExists.ToList())
                        {
                            var response = await apiDeleteVariation(new DeleteVariantRequest() { Id = variation.Id.ToString() });
                            if (response.Result == Result.Error)
                                continue;

                            variationsExists.Remove(variation);
                        }

                        groupVariations.ForEach(x => x.Id = 0);
                    }
                }

                foreach (var variation in groupVariations)
                {
                    var variationExist = variationsExists.FirstOrDefault(x => x.Id == variation.Id.ToString());

                    if (variationExist == null)
                        variationExist = variationsExists.FirstOrDefault(x => x.Reference.Trim() == variation.Reference);

                    if(variationExist == null)
                        variationExist = variationsExists.FirstOrDefault(x => (!isDoubleVariation ?
                                                                               x.Sku?[0]?.Type.ToUpper().Trim() == variation.Type1.ToUpper().Trim() && x.Sku?[0]?.Value.ToUpper().Trim() == variation.Value1.ToUpper().Trim()
                                                                               :
                                                                               x.Sku?[0]?.Type.ToUpper().Trim() == variation.Type1.ToUpper().Trim() && x.Sku?[0]?.Value.ToUpper().Trim() == variation.Value1.ToUpper().Trim() &&
                                                                               x.Sku?[1]?.Type.ToUpper().Trim() == variation.Type2.ToUpper().Trim() && x.Sku?[1]?.Value.ToUpper().Trim() == variation.Value2.ToUpper().Trim()));

                    if (!variation.Active && variation.Id > 0)
                    {
                        await apiDeleteVariation(new DeleteVariantRequest() { Id = variation.Id.ToString() });
                        continue;
                    }

                    var productVariation = new ProductVariation()
                    {
                        ProductId = productId,
                        Ean = variation.Ean,
                        Order = variation.Order,
                        Price = variation.Price,
                        CostPrice = variation.CostPrice,
                        Stock = variation.Stock,
                        MinimumStock = variation.MinimumStock,
                        Weight = variation.Weight,
                        Length = variation.Length,
                        Width = variation.Width,
                        Height = variation.Height,
                        Reference = variation.Reference
                    };

                    if (isDoubleVariation)
                    {
                        productVariation.Type1 = variation.Type1.Trim();
                        productVariation.Value1 = variation.Value1.Trim();
                        productVariation.Type2 = variation.Type2.Trim();
                        productVariation.Value2 = variation.Value2.Trim();
                    }
                    else
                    {
                        productVariation.Type1 = variation.Type1.Trim();
                        productVariation.Value1 = variation.Value1.Trim();
                        productVariation.Type2 = null;
                        productVariation.Value2 = null;
                    }

                    if (variationExist == null)
                    {
                        productVariation.PictureSource1 = !string.IsNullOrEmpty(variation.PictureSource1) ? ImageExtensions.ValidateExtension(variation.PictureSource1) : null;
                        productVariation.PictureSource2 = !string.IsNullOrEmpty(variation.PictureSource2) ? ImageExtensions.ValidateExtension(variation.PictureSource2) : null;
                        productVariation.PictureSource3 = !string.IsNullOrEmpty(variation.PictureSource3) ? ImageExtensions.ValidateExtension(variation.PictureSource3) : null;
                        productVariation.PictureSource4 = !string.IsNullOrEmpty(variation.PictureSource4) ? ImageExtensions.ValidateExtension(variation.PictureSource4) : null;
                        productVariation.PictureSource5 = !string.IsNullOrEmpty(variation.PictureSource5) ? ImageExtensions.ValidateExtension(variation.PictureSource5) : null;
                        productVariation.PictureSource6 = !string.IsNullOrEmpty(variation.PictureSource6) ? ImageExtensions.ValidateExtension(variation.PictureSource6) : null;

                        if (variation.Active)
                            await CreateVariation(productVariation, variation, returnMessage, cancellationToken);
                    }
                    else
                    {
                        variation.Id = long.Parse(variationExist.Id);

                        if (message.UpdatePrice)
                        {
                            var updateVariation = new UpdateVariantPriceRequest()
                            {
                                Id = variation.Id.ToString(),
                                CostPrice = (double)variation.CostPrice,
                                Price = (double)variation.Price,
                                Stock = null
                            };

                            if (message.UpdateStock && variation.Stock != null)
                            {
                                if ((long)variation.Stock != long.Parse(variationExist.Stock))
                                    updateVariation.Stock = (long)variation.Stock;
                            }

                            var response = await apiUpdateVariationPrice(updateVariation);

                            returnMessage.Message = response.Data?.Message;

                            if (response.Result == Result.Error)
                            {
                                returnMessage.Success = false;
                                returnMessage.Message = response.Error?.Message;

                                return returnMessage;
                            }
                        }
                        else if (message.UpdateStock)
                        {
                            var response = await apiUpdateVariationStock(new UpdateVariantStockRequest()
                            {
                                Id = variation.Id.ToString(),
                                Stock = (long)variation.Stock
                            });

                            returnMessage.Message = response.Data?.Message;

                            if (response.Result == Result.Error)
                            {
                                returnMessage.Success = false;
                                returnMessage.Message = response.Error?.Message;

                                return returnMessage;
                            }
                        }
                        else
                        {
                            if (productVariation.HasUpdateVariant(variationExist))
                            {
                                productVariation.From(variationExist);

                                await UpdateVariation(productVariation, variation, returnMessage, cancellationToken);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                returnMessage.Success = false;
                returnMessage.Message = $"Error in ProcessVariationOptions: {ex.Message}";

                //_log.Error($"TrayService - Error in ProcessProcessVariationOptions: {ex}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), message, null));

            }

            return returnMessage;
        }

        public async Task<TrayAppReturnMessage> CreateVariation(ProductVariation productVariation, Variation variation, TrayAppReturnMessage returnMessage, CancellationToken cancellationToken = default)
        {
            try
            {
                async Task<ReturnMessage<CreateVariantResponse>> apiCreateVariation(CreateVariantRequest request)
                   => await _apiActorGroup.Ask<ReturnMessage<CreateVariantResponse>>(request, cancellationToken);

                async Task<ReturnMessage<GetVariantsByFilterResponse>> apiGetVariationByProductId(GetVariantsByFilterRequest request)
                  => await _apiActorGroup.Ask<ReturnMessage<GetVariantsByFilterResponse>>(request, cancellationToken);

                var createVariation = new CreateVariantRequest() { ProductVariation = productVariation };

                var response = await apiCreateVariation(createVariation);

                returnMessage.Message = response.Data?.Message;

                if (response.Result == Result.Error && (response.Error?.Message != null && !response.Error.Message.Contains("This variation already exists")))
                {
                    returnMessage.Success = false;
                    returnMessage.Message = response.Error?.Message;
                }

                var variationId = response?.Data != null ? response.Data.Id : "0";
                if (variationId == "0")
                {
                    var variationExist = await apiGetVariationByProductId(new GetVariantsByFilterRequest() { ProductId = productVariation.ProductId, Reference = productVariation.Reference });
                    if (variationExist?.Data?.Variants != null && variationExist?.Data?.Variants.Count() > 0)
                        variationId = variationExist.Data.Variants.FirstOrDefault()?.Variation?.Id;
                }

                returnMessage.Product.Variations.Add(new TrayAppReturnMessage.ProductIntegration.ProductVariationIntegration()
                {
                    Id = variation.AppTrayProductVariationId,
                    TrayProductVariationId = long.Parse(variationId)
                });

            }
            catch (Exception ex)
            {
                returnMessage.Success = false;
                returnMessage.Message = $"Error in Create Variation: {ex.Message}";

                //_log.Warning($"TrayService - Error in Create Variation: {ex}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), null, null));

            }

            return returnMessage;
        }
        public async Task<TrayAppReturnMessage> UpdateVariation(ProductVariation productVariation, Variation variation, TrayAppReturnMessage returnMessage, CancellationToken cancellationToken = default)
        {
            try
            {
                async Task<ReturnMessage<UpdateVariantResponse>> apiUpdateVariation(UpdateVariantRequest request)
                    => await _apiActorGroup.Ask<ReturnMessage<UpdateVariantResponse>>(request, cancellationToken);

                var updateVariant = new UpdateVariantRequest() { Id = variation.Id.ToString(), ProductVariation = productVariation };

                var response = await apiUpdateVariation(updateVariant);

                returnMessage.Message = response.Data?.Message;

                if (response.Result == Result.Error && (response.Error?.Message != null && !response.Error.Message.Contains("This variation already exists")))
                {
                    returnMessage.Success = false;
                    returnMessage.Message = response.Error?.Message;
                }

                returnMessage.Product.Variations.Add(new TrayAppReturnMessage.ProductIntegration.ProductVariationIntegration()
                {
                    Id = variation.AppTrayProductVariationId,
                    TrayProductVariationId = variation.Id
                });

            }
            catch (Exception ex)
            {
                returnMessage.Success = false;
                returnMessage.Message = $"Error in Update Variation: {ex.Message}";

                //_log.Warning($"TrayService - Error in Update Variation: {ex}", LoggingExtensions.FromService(LoggingExtensions.GetCurrentMethod(), null, null));
            }

            return returnMessage;
        }

        //public async Task<ReturnMessage> ProcessProcessCategoy(TrayProcessCategoyMessage message, CancellationToken cancellationToken = default)
        //{
        //    async Task<ReturnMessage<CreateCategoryResponse>> apiCreateCategory(CreateCategoryRequest request)
        //      => await _apiActorGroup.Ask<ReturnMessage<CreateCategoryResponse>>(request, cancellationToken);

        //    async Task<ReturnMessage<GetCategoriesByFilterResponse>> apiGetCategoryByName(GetCategoriesByFilterRequest request)
        //      => await _apiActorGroup.Ask<ReturnMessage<GetCategoriesByFilterResponse>>(request, cancellationToken);

        //    foreach (var category in message.Categories)
        //    {
        //        var response = await apiGetCategoryByName(new GetCategoriesByFilterRequest() { Name = category.Name });

        //        if (response.Result == Result.Error) throw response.Error;

        //        if (!response.Data.Categories.Any())
        //        {
        //            var responseCreate = await apiCreateCategory(new CreateCategoryRequest()
        //            {
        //                Name = category.Name,
        //                Metatag = new MetatagCategoryModel() { Keywords = "Id", Description = category.Id.ToString() }
        //            });

        //            if (response.Result == Result.Error)
        //                throw response.Error;
        //        }
        //    }

        //    return new ReturnMessage { Result = Result.OK };
        //}
        //public async Task<ReturnMessage> ProcessAttributesProduct(TrayProcessAttributesProductMessage message, CancellationToken cancellationToken = default)
        //{
        //    return new ReturnMessage { Result = Result.OK };
        //}


    }
}

