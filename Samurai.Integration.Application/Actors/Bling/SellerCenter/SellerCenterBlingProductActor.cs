using Akka.Actor;

using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

using Samurai.Integration.APIClient.Bling.Models.Requests;
using Samurai.Integration.APIClient.Bling.Models.Results;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Bling;
using Samurai.Integration.Domain.Messages.SellerCenter;
using Samurai.Integration.Domain.Models;
using Samurai.Integration.Domain.Queues;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.Bling.SellerCenter
{
    public class SellerCenterBlingProductActor : BaseBlingTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IActorRef _apiActorGroup;
        private readonly QueueClient _erpFullProductQueueClient;
        private readonly QueueClient _processVariationOptionsProductQueue;
        private readonly QueueClient _processCategoriesProductQueue;
        private readonly QueueClient _processManufacturersProductQueue;
        private readonly QueueClient _updatePriceQueue;
        private readonly QueueClient _updateStockProductQueue;

        public SellerCenterBlingProductActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, BlingData blingData, IActorRef apiActorGroup)
            : base("SellerCenterBlingProductActor")
        {
            _serviceProvider = serviceProvider;
            _blingData = blingData;
            _apiActorGroup = apiActorGroup;

            using (var scope = _serviceProvider.CreateScope())
            {
                var tenantService = scope.ServiceProvider.GetService<TenantService>();

                _erpFullProductQueueClient = tenantService.GetQueueClient(_blingData, SellerCenterQueue.CreateProductQueue);
                _processVariationOptionsProductQueue = tenantService.GetQueueClient(_blingData, SellerCenterQueue.ProcessVariationOptionsProductQueue);
                _processCategoriesProductQueue = tenantService.GetQueueClient(_blingData, SellerCenterQueue.ProcessCategoriesProductQueue);
                _processManufacturersProductQueue = tenantService.GetQueueClient(_blingData, SellerCenterQueue.ProcessManufacturersProductQueue);
                _updatePriceQueue = tenantService.GetQueueClient(_blingData, SellerCenterQueue.UpdatePriceQueue);
                _updateStockProductQueue = tenantService.GetQueueClient(_blingData, SellerCenterQueue.UpdateStockProductQueue);
            }

            ReceiveAsync((Func<BlingListProductMessage, Task>)(async message =>
            {
                await SendSellerCenterCreateProductMessage(message.ExternalId, cancellationToken);
            }));

            ReceiveAsync((Func<BlingListAllProductsMessage, Task>)(async message =>
            {
                await SendAllSellerCenterCreateProductMessages(message.ProductUpdatedDate, cancellationToken);
            }));
        }

        private async Task SendSellerCenterCreateProductMessage(string productCode, CancellationToken cancellationToken)
        {
            try
            {
                ReturnMessage<BlingApiListProductsResult.Produto> result;
                using (var scope = _serviceProvider.CreateScope())
                {
                    var blingService = scope.ServiceProvider.GetService<BlingService>();
                    blingService.Init(_apiActorGroup, GetLog(), _blingData);
                    result = await blingService.ListProduct(productCode, cancellationToken);

                    var product = result.Data;

                    if (product == null)
                        throw new Exception($"Product {productCode} not found");

                    if (!string.IsNullOrWhiteSpace(product.codigoPai))
                    {
                        var resultPai = await blingService.ListProduct(product.codigoPai, cancellationToken);

                        product = resultPai.Data;
                    }

                    var productMessage = await GetProductMessage(product, blingService, cancellationToken);

                    var serviceBusMessage = new ServiceBusMessage(productMessage);

                    await _erpFullProductQueueClient.ScheduleMessageAsync(serviceBusMessage.GetMessage(productMessage.ProductInfo.ExternalId), DateTime.UtcNow.AddSeconds(60));

                    var message = new Product.SummarySellerCenter(new List<Product.Info> { productMessage.ProductInfo });

                    serviceBusMessage = new ServiceBusMessage(message);

                    if (message.Variants.Count > 0)
                        await _processVariationOptionsProductQueue.SendAsync(serviceBusMessage.GetMessage(Guid.NewGuid().ToString()));

                    if (message.Categories.Count > 0)
                        await _processCategoriesProductQueue.SendAsync(serviceBusMessage.GetMessage(Guid.NewGuid().ToString()));

                    if (message.Manufacturers.Count > 0)
                        await _processManufacturersProductQueue.SendAsync(serviceBusMessage.GetMessage(Guid.NewGuid().ToString()));
                }

                Sender.Tell(new ReturnMessage());
            }
            catch (Exception ex)
            {
                Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
            }
        }

        private async Task SendAllSellerCenterCreateProductMessages(DateTime? ProductUpdatedDate, CancellationToken cancellationToken)
        {
            try
            {
                ReturnMessage<List<BlingApiListProductsResult.Produto>> result;
                using (var scope = _serviceProvider.CreateScope())
                {
                    var blingService = scope.ServiceProvider.GetService<BlingService>();
                    blingService.Init(_apiActorGroup, GetLog(), _blingData);
                    result = await blingService.ListProducts(ProductUpdatedDate, cancellationToken);

                    if (result.Result == Result.Error)
                        throw result.Error;

                    List<Task> sendMessages = new List<Task>();

                    var products = result.Data;

                    products = products.Where(x => string.IsNullOrWhiteSpace(x.codigoPai)).ToList();

                    var itens = new List<Product.Info>();

                    foreach (var product in products)
                    {
                        try
                        {
                            var productMessage = await GetProductMessage(product, blingService, cancellationToken);

                            itens.Add(productMessage.ProductInfo);

                            var serviceBusProductMessage = new ServiceBusMessage(productMessage);

                            sendMessages.Add(_erpFullProductQueueClient.ScheduleMessageAsync(serviceBusProductMessage.GetMessage(productMessage.ProductInfo.ExternalId), DateTime.UtcNow.AddSeconds(60)));
                        }
                        catch(Exception ex)
                        {
                            LogError(ex, "Error in SendAllSellerCenterCreateProductMessages");
                        }
                    }

                    var message = new Product.SummarySellerCenter(itens);

                    var serviceBusMessage = new ServiceBusMessage(message);

                    if (message.Variants.Count > 0)
                        await _processVariationOptionsProductQueue.SendAsync(serviceBusMessage.GetMessage(Guid.NewGuid().ToString()));

                    if (message.Categories.Count > 0)
                        await _processCategoriesProductQueue.SendAsync(serviceBusMessage.GetMessage(Guid.NewGuid().ToString()));

                    if (message.Manufacturers.Count > 0)
                        await _processManufacturersProductQueue.SendAsync(serviceBusMessage.GetMessage(Guid.NewGuid().ToString()));

                    await Task.WhenAll(sendMessages);
                }

                Sender.Tell(new ReturnMessage());
            }
            catch (Exception ex)
            {
                Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
            }
        }

        private async Task<SellerCenterCreateProductMessage> GetProductMessage(BlingApiListProductsResult.Produto product, BlingService blingService, CancellationToken cancellationToken)
        {
            var optionsName = new List<string>();

            if (product?.variacoes?.Any() == true)
            {
                try
                {
                    var varNames = product.variacoes.Select(x => x.variacao.nome).ToList();

                    if (varNames.All(x => x.Contains(";")) || varNames.All(x => x.Contains(":")))
                    {
                        var types = varNames.Select(x => { return x.Split(";"); });

                        var biggerTypes = types.OrderByDescending(x => x.Length).FirstOrDefault();

                        optionsName = biggerTypes.Select(x =>
                        {
                            return x.Split(":")[0].Trim();
                        }).ToList();
                    }
                    else
                    {
                        optionsName.Add("Opção");
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error in GetProductMessage");
                }
            }

            var variants = await GetVariants(optionsName, product, blingService, cancellationToken);

            var productInfo = new Product.Info
            {
                ExternalId = product.codigo,
                ExternalCode = product.id,
                GroupingReference = product.codigoPai,
                Title = product.descricao,
                Status = product.situacao == "Ativo",
                BodyHtml = product.descricaoCurta,
                Vendor = product.marca,
                OptionsName = optionsName,
                Images = new ProductImages
                {
                    ImageUrls = product.imagem?.Select(x => x.link)?.ToList(),
                    SkuImages = new List<ProductImages.SkuImage>()
                },
                Categories = new List<Product.Category>
                {
                    new Product.Category
                    {
                        Name = product.categoria.descricao
                    }
                },
                Variants = optionsName.Any() ? variants : new List<Product.SkuInfo>(),
                DataSellerCenter = new Product.DataSellerCenter { Variants = GetVariantOptionSellerCenter(variants) }
            };

            return new SellerCenterCreateProductMessage
            {
                ProductInfo = productInfo,
                HasPriceStock = true
            };
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

        private async Task<List<Product.SkuInfo>> GetVariants(List<string> optionsName, BlingApiListProductsResult.Produto product, BlingService blingService, CancellationToken cancellationToken)
        {
            var response = new List<Product.SkuInfo>();

            if (product.variacoes?.Any() == true)
            {
                foreach (var sku in product.variacoes)
                {
                    var values = new List<string>();

                    if (sku.variacao.nome.Contains(";") || sku.variacao.nome.Contains(":"))
                    {
                        try
                        {
                            values = sku?.variacao?.nome?.Split(";")?.Select(x => { return x.Split(":"); })?.Select(x => x[1]?.Trim())?.ToList();
                        }
                        catch (Exception ex)
                        {
                            LogError(ex, "Error in GetVariants");
                        }
                    }
                    else
                    {
                        values.Add(sku.variacao.nome);
                    }

                    var productResult = await blingService.ListProduct(sku.variacao.codigo, cancellationToken);

                    var skuProduct = productResult.Data;

                    if (skuProduct == null)
                        throw new Exception($"Sku product {sku.variacao.codigo} not found.");

                    var responseSkus = new Product.SkuInfo
                    {
                        Sku = sku.variacao.codigo,
                        Stock = new Product.SkuStock
                        {
                            Quantity = Decimal.ToInt32(skuProduct.estoqueAtual),
                            Sku = sku.variacao.codigo
                        },
                        Price = new Product.SkuPrice
                        {
                            Price = Convert.ToDecimal(skuProduct.preco, new CultureInfo("en-US")),
                            Sku = sku.variacao.codigo
                        },
                        WeightInKG = Convert.ToDecimal(skuProduct.pesoBruto, new CultureInfo("en-US")),
                        Status = true,
                        Options = values,
                        OptionsName = optionsName
                    };

                    response.Add(responseSkus);
                }
            }
            else
            {
                response.Add(new Product.SkuInfo
                {
                    Sku = product.codigo,
                    Options = new List<string>(),
                    Price = new Product.SkuPrice
                    {
                        Price = Convert.ToDecimal(product.preco, new CultureInfo("en-US")),
                        Sku = product.codigo
                    },
                    WeightInKG = Convert.ToDecimal(product.pesoBruto, new CultureInfo("en-US")),
                    Status = product.situacao == "Ativo",
                    Barcode = product.gtin,
                    Stock = new Product.SkuStock
                    {
                        Quantity = Decimal.ToInt32(product.estoqueAtual)
                    },
                    OptionsName = optionsName
                });
            }
            return response;
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, BlingData blingData, IActorRef apiActorGroup)
        {
            return Akka.Actor.Props.Create(() => new SellerCenterBlingProductActor(serviceProvider, cancellationToken, blingData, apiActorGroup));
        }
    }
}
