using Akka.Actor;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Samurai.Integration.APIClient.PluggTo.Models.Results;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.PluggTo;
using Samurai.Integration.Domain.Messages.SellerCenter;
using Samurai.Integration.Domain.Messages.SellerCenter.ProductActor;
using Samurai.Integration.Domain.Models;
using Samurai.Integration.Domain.Queues;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.PluggTo.SellerCenter
{
    public class SellerCenterPluggToProductActor : BasePluggToTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IActorRef _apiActorGroup;
        private readonly CancellationToken _cancellationToken;

        private readonly QueueClient _erpFullProductQueueClient;
        private readonly QueueClient _processVariationOptionsProductQueue;
        private readonly QueueClient _processCategoriesProductQueue;
        private readonly QueueClient _processManufacturersProductQueue;

        public SellerCenterPluggToProductActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, PluggToData pluggToData,
            IActorRef apiActorGroup)
          : base("SellerCenterPluggToProductActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _pluggToData = pluggToData;
            _apiActorGroup = apiActorGroup;

            using (var scope = _serviceProvider.CreateScope())
            {
                var tenantService = scope.ServiceProvider.GetService<TenantService>();

                _erpFullProductQueueClient = tenantService.GetQueueClient(_pluggToData, SellerCenterQueue.CreateProductQueue);
                _processVariationOptionsProductQueue = tenantService.GetQueueClient(_pluggToData, SellerCenterQueue.ProcessVariationOptionsProductQueue);
                _processCategoriesProductQueue = tenantService.GetQueueClient(_pluggToData, SellerCenterQueue.ProcessCategoriesProductQueue);
                _processManufacturersProductQueue = tenantService.GetQueueClient(_pluggToData, SellerCenterQueue.ProcessManufacturersProductQueue);
                               
            }

            ReceiveAsync((Func<PluggToListProductMessage, Task>)(async message =>
            {
                await SendProductToSellerCenterMessage(message, cancellationToken);
            }));

            ReceiveAsync((Func<PluggToListAllProductsMessage, Task>)(async message =>
            {
                await SendAllProductsToSellerCenterMessages(message, cancellationToken);
            }));
        }

        private async Task SendProductToSellerCenterMessage(PluggToListProductMessage message, CancellationToken cancellationToken)
        {
            try
            {
                LogWarning("Starting SendProductToSellerCenterMessage: {0}", JsonConvert.SerializeObject(message));

                ReturnMessage<PluggToApiListProductsResult.Produto> result;

                using (var scope = _serviceProvider.CreateScope())
                {
                    var product = new PluggToApiListProductsResult.Produto();

                    if (!string.IsNullOrEmpty(message.Product))
                        product = JsonConvert.DeserializeObject<PluggToApiListProductsResult.Produto>(message.Product);
                    else
                    {
                        var pluggToService = scope.ServiceProvider.GetService<PluggToService>();
                        pluggToService.Init(_apiActorGroup, GetLog());

                        result = await pluggToService.ListProduct(message, cancellationToken);

                        List<Task> sendMessages = new List<Task>();

                        product = result.Data;
                    }

                    if (product == null)
                        throw new Exception($"Product {message.Sku ?? message.ExternalId} not found");

                    var productMessage = GetProductMessage(product);

                    LogWarning("productMessage: {0}", JsonConvert.SerializeObject(productMessage));

                    var serviceBusMessage = new ServiceBusMessage(productMessage);

                    await _erpFullProductQueueClient.ScheduleMessageAsync(serviceBusMessage.GetMessage(message.Sku ?? message.ExternalId), DateTime.UtcNow.AddSeconds(60));

                    var productSellerCenter = new Product.SummarySellerCenter(new List<Product.Info> { productMessage.ProductInfo });

                    LogWarning("productSellerCenter", JsonConvert.SerializeObject(productSellerCenter));

                    serviceBusMessage = new ServiceBusMessage(productSellerCenter);

                    if (productSellerCenter.Variants.Count > 0)
                        await _processVariationOptionsProductQueue.SendAsync(serviceBusMessage.GetMessage(Guid.NewGuid().ToString()));

                    if (productSellerCenter.Categories.Count > 0)
                        await _processCategoriesProductQueue.SendAsync(serviceBusMessage.GetMessage(Guid.NewGuid().ToString()));

                    if (productSellerCenter.Manufacturers.Count > 0)
                        await _processManufacturersProductQueue.SendAsync(serviceBusMessage.GetMessage(Guid.NewGuid().ToString()));

                }

                LogDebug("Ending SendProductToSellerCenterMessage");
                Sender.Tell(new ReturnMessage());
            }
            catch (Exception ex)
            {
                LogError(ex, "Error in SendProductToSellerCenterMessage");
                Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
            }

        }
        private async Task SendAllProductsToSellerCenterMessages(PluggToListAllProductsMessage message, CancellationToken cancellationToken)
        {
            try
            {
                LogDebug("Starting SendAllProductsToSellerCenterMessages");

                ReturnMessage<List<PluggToApiListProductsResult.Produto>> result;

                using (var scope = _serviceProvider.CreateScope())
                {
                    var pluggToService = scope.ServiceProvider.GetService<PluggToService>();
                    pluggToService.Init(_apiActorGroup, GetLog());

                    result = await pluggToService.ListProducts(message, cancellationToken);

                    List<Task> sendMessages = new List<Task>();

                    var products = result.Data;

                    var itens = new List<Product.Info>();

                    foreach (var product in products)
                    {
                        try
                        {
                            var productMessage = GetProductMessage(product);
                           
                            itens.Add(productMessage.ProductInfo);

                            var serviceBusProductMessage = new ServiceBusMessage(productMessage);

                            sendMessages.Add(_erpFullProductQueueClient.ScheduleMessageAsync(serviceBusProductMessage.GetMessage(productMessage.ProductInfo.ExternalId), DateTime.UtcNow.AddSeconds(60)));

                        }
                        catch (Exception ex)
                        {
                            LogError(ex, "Error in SendAllProductsToSellerCenterMessages");
                        }
                    }

                    var productSellerCenter = new Product.SummarySellerCenter(itens);

                    var serviceBusMessage = new ServiceBusMessage(productSellerCenter);

                    if (productSellerCenter.Variants.Count > 0)
                        await _processVariationOptionsProductQueue.SendAsync(serviceBusMessage.GetMessage(Guid.NewGuid().ToString()));

                    if (productSellerCenter.Categories.Count > 0)
                        await _processCategoriesProductQueue.SendAsync(serviceBusMessage.GetMessage(Guid.NewGuid().ToString()));

                    if (productSellerCenter.Manufacturers.Count > 0)
                        await _processManufacturersProductQueue.SendAsync(serviceBusMessage.GetMessage(Guid.NewGuid().ToString()));

                    await Task.WhenAll(sendMessages);
                }

                LogDebug("Ending SendAllProductsToSellerCenterMessages");
                Sender.Tell(new ReturnMessage());
            }
            catch (Exception ex)
            {
                LogError(ex, "Error in SendAllProductsToSellerCenterMessages");
                Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
            }

        }

        private SellerCenterCreateProductMessage GetProductMessage(PluggToApiListProductsResult.Produto product)
        {
            var optionsName = product.attributes.Select(x => x.label).Distinct().OrderBy(x => x).ToList();
            var variants = GetVariants(product);

            #region variationsImages

            var variationsImages = product.variations?.Select(y => y.photos?.Select(x => new { y.sku, x.url }).ToList()).FirstOrDefault()?
                                                      .Select(x => new ProductImages.SkuImage()
                                                      {
                                                          Sku = x.sku,
                                                          SkuImageUrl = x.url
                                                      }).ToList();
            #endregion

            var isSingle = product.variations.Count == 1;

            var productInfo = new Product.Info
            {
                ExternalId = isSingle ? product.variations.FirstOrDefault().sku : !string.IsNullOrEmpty(product.sku) ? product.sku : string.Empty,
                ExternalCode = !string.IsNullOrEmpty(product.id) ? product.id : string.Empty,
                GroupingReference = product.sku,
                Title = product.name,
                Status = true,
                BodyHtml = product.description,
                Vendor = product.brand,
                Model = product.model,
                OptionsName = optionsName,
                Images = new ProductImages
                {
                    ImageUrls = product.photos?.Select(x => x.url).ToList(),
                    SkuImages = variationsImages
                },
                Categories = new List<Product.Category>
                {
                    new Product.Category
                    {
                        Name = product.categories.FirstOrDefault()?.name
                    }
                },
                Variants = optionsName.Any() ? variants : new List<Product.SkuInfo>(),
                DataSellerCenter = new Product.DataSellerCenter { Variants = GetVariantOptionSellerCenter(variants) }
            };

            return new SellerCenterCreateProductMessage
            {
                ProductInfo = productInfo,
                HasPriceStock = true,
            };
        }
        private List<Product.SkuInfo> GetVariants(PluggToApiListProductsResult.Produto product)
        {
            var response = new List<Product.SkuInfo>();

            //if (product.attributes?.Any() == true)
            //{
            //    var price = Convert.ToDecimal(product.price, new CultureInfo("en-US"));
            //    var specialPrice = product.special_price.HasValue ? Convert.ToDecimal(product.special_price, new CultureInfo("en-US")) : price;

            //    response.Add(new Product.SkuInfo
            //    {
            //        Sku = product.sku,
            //        Barcode = product.ean,
            //        Stock = new Product.SkuStock
            //        {
            //            Quantity = (int)product.quantity,
            //            Sku = product.sku
            //        },
            //        Price = new Product.SkuPrice
            //        {
            //            Price = specialPrice,
            //            CompareAtPrice = price,
            //            Sku = product.sku
            //        },
            //        WeightInKG = Convert.ToDecimal(product.real_dimension?.weight, new CultureInfo("en-US")),
            //        Status = true,
            //        Options = product.attributes.Select(x => x.value.label).ToList(),
            //        OptionsName = product.attributes.Select(x => x.label).ToList(),
            //    });
            //}

            if (product.variations?.Any() == true)
            {
                foreach (var variation in product.variations)
                {
                    var attributes = variation.attributes.OrderBy(x => x.label).Distinct().ToList();

                    var price = Convert.ToDecimal(variation.price, new CultureInfo("en-US"));
                    var specialPrice = variation.special_price.HasValue ? Convert.ToDecimal(variation.special_price, new CultureInfo("en-US")) : price;

                    var dimensionWeight = product.real_dimension?.weight != null ? product.real_dimension?.weight : product.dimension?.weight;                   

                    var responseSkus = new Product.SkuInfo
                    {
                        Sku = variation.sku,
                        Barcode = variation.ean,
                        Stock = new Product.SkuStock
                        {
                            Quantity = (int)variation.quantity,
                            Sku = variation.sku
                        },
                        Price = new Product.SkuPrice
                        {
                            Price = specialPrice,
                            CompareAtPrice = price,
                            Sku = variation.sku
                        },
                        WeightInKG = Convert.ToDecimal(dimensionWeight, new CultureInfo("en-US")),
                        Status = true,
                        Options = attributes.Select(x => x.value.label).ToList(),
                        OptionsName = attributes.Select(x => x.label).ToList(),
                    };

                    response.Add(responseSkus);
                }
            }
            else
            {
                var dimensionWeight = product.real_dimension != null ? product.real_dimension?.weight : product.dimension?.weight;
                var price = Convert.ToDecimal(product.price, new CultureInfo("en-US"));
                var specialPrice = product.special_price.HasValue ? Convert.ToDecimal(product.special_price, new CultureInfo("en-US")) : price;
              
                var responseSku = new Product.SkuInfo
                {
                    Sku = product.sku,
                    Barcode = product.ean,
                    Stock = new Product.SkuStock
                    {
                        Quantity = (int)product.quantity,
                        Sku =product.sku
                    },
                    Price = new Product.SkuPrice
                    {
                        Price = specialPrice,
                        CompareAtPrice = price,
                        Sku = product.sku
                    },
                    WeightInKG = Convert.ToDecimal(dimensionWeight, new CultureInfo("en-US")),
                    Status = true,                   
                };

                response.Add(responseSku);
            }

            return response;
        }
        private List<Product.SkuSellerCenter> GetVariantOptionSellerCenter(List<Product.SkuInfo> variants)
        {
            var response = new List<Product.SkuSellerCenter>();
            foreach (var sku in variants)
            {
                var info = sku.OptionsName?.Select((value, index) => new Product.InfoVariations { NomeVariacao = value, ValorVariacao = sku.Options[index] })?.ToList() ?? new List<Product.InfoVariations>();

                var responseSkus = new Product.SkuSellerCenter
                {
                    Sku = sku.Sku,
                    Weight = sku.WeightInKG,
                    Barcode = sku.Barcode,
                    Options = new List<Product.DetailsVariations>(),
                    InfoVariations = info,
                    SkuPrice = sku.Price,
                    SkuStock = sku.Stock
                };

                response.Add(responseSkus);
            }

            return response;
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, PluggToData pluggToData, IActorRef apiActorGroup)
        {
            return Akka.Actor.Props.Create(() => new SellerCenterPluggToProductActor(serviceProvider, cancellationToken, pluggToData, apiActorGroup));
        }
    }
}
